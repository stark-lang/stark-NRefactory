﻿//
// ObjectCreationContextHandler.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.


using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace ICSharpCode.NRefactory6.CSharp.Completion
{
	class ObjectCreationContextHandler : CompletionContextHandler
	{
		public override bool IsTriggerCharacter (Microsoft.CodeAnalysis.Text.SourceText text, int position)
		{
			return IsTriggerAfterSpaceOrStartOfWordCharacter (text, position);
		}

		public override bool IsCommitCharacter (ICompletionData completionItem, char ch, string textTypedSoFar)
		{
			return ch == ' ' || ch == '(' || ch == '{' || ch == '[';
		}

		public async override Task<IEnumerable<ICompletionData>> GetCompletionDataAsync (CompletionResult result, CompletionEngine engine, CompletionContext completionContext, CompletionTriggerInfo info, CancellationToken cancellationToken)
		{
			var ctx = await completionContext.GetSyntaxContextAsync (engine.Workspace, cancellationToken).ConfigureAwait (false);
			var list = new List<ICompletionData> ();

			var newExpression = GetObjectCreationNewExpression (ctx.SyntaxTree, completionContext.Position, cancellationToken);
			if (newExpression == null) {
				foreach (var inferredType in SyntaxContext.InferenceService.InferTypes (ctx.CSharpSyntaxContext.SemanticModel, completionContext.Position, cancellationToken)) {
					foreach (var symbol in await GetPreselectedSymbolsWorker(ctx.CSharpSyntaxContext, inferredType, completionContext.Position - 1, cancellationToken)) {
						var symbolCompletionData = engine.Factory.CreateObjectCreation (this, symbol, completionContext.Position, false);
						list.Add (symbolCompletionData);
						if (string.IsNullOrEmpty (result.DefaultCompletionString))
							result.DefaultCompletionString = symbolCompletionData.DisplayText;
					}	
				}
				return list;
			}

			var type = SyntaxContext.InferenceService.InferType (ctx.CSharpSyntaxContext.SemanticModel, newExpression, objectAsDefault: false, cancellationToken: cancellationToken);

			foreach (var symbol in await GetPreselectedSymbolsWorker(ctx.CSharpSyntaxContext, type, completionContext.Position, cancellationToken)) {
				var symbolCompletionData = engine.Factory.CreateObjectCreation (this, symbol, newExpression.SpanStart, true);
				list.Add (symbolCompletionData);
				if (string.IsNullOrEmpty (result.DefaultCompletionString))
					result.DefaultCompletionString = symbolCompletionData.DisplayText;
			}
			return list;
		}


		static Task<IEnumerable<ISymbol>> GetPreselectedSymbolsWorker2 (CSharpSyntaxContext context, ITypeSymbol type, CancellationToken cancellationToken)
		{
			// Unwrap an array type fully.  We only want to offer the underlying element type in the
			// list of completion items.
			bool isArray = false;
			while (type is IArrayTypeSymbol) {
				isArray = true;
				type = ((IArrayTypeSymbol)type).ElementType;
			}

			if (type == null) {
				return Task.FromResult (Enumerable.Empty<ISymbol> ());
			}

			// Unwrap nullable
			if (type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T) {
				type = type.GetTypeArguments ().FirstOrDefault ();
			}

			if (type.SpecialType == SpecialType.System_Void) {
				return Task.FromResult (Enumerable.Empty<ISymbol> ());
			}

			if (type.ContainsAnonymousType ()) {
				return Task.FromResult (Enumerable.Empty<ISymbol> ());
			}

			if (!type.CanBeReferencedByName) {
				return Task.FromResult (Enumerable.Empty<ISymbol> ());
			}

			// Normally the user can't say things like "new IList".  Except for "IList[] x = new |".
			// In this case we do want to allow them to preselect certain types in the completion
			// list even if they can't new them directly.
			if (!isArray) {
				if (type.TypeKind == TypeKind.Interface ||
				    type.TypeKind == TypeKind.Pointer ||
				    type.TypeKind == TypeKind.Dynamic ||
				    type.IsAbstract) {
					return Task.FromResult (Enumerable.Empty<ISymbol> ());
				}

				if (type.TypeKind == TypeKind.TypeParameter &&
				    !((ITypeParameterSymbol)type).HasConstructorConstraint) {
					return Task.FromResult (Enumerable.Empty<ISymbol> ());
				}
			}

//			if (!type.IsEditorBrowsable(options.GetOption(RecommendationOptions.HideAdvancedMembers, context.SemanticModel.Language), context.SemanticModel.Compilation))
//			{
//				return SpecializedTasks.EmptyEnumerable<ISymbol>();
//			}
//
			return Task.FromResult (SpecializedCollections.SingletonEnumerable ((ISymbol)type));
		}

		static async Task<IEnumerable<ISymbol>> GetPreselectedSymbolsWorker (CSharpSyntaxContext context, ITypeSymbol inferredType, int position, CancellationToken cancellationToken)
		{
			var result = await GetPreselectedSymbolsWorker2 (context, inferredType, cancellationToken).ConfigureAwait (false);
			if (result.Any ()) {
				var type = (ITypeSymbol)result.Single ();
				var alias = await type.FindApplicableAlias (position, context.SemanticModel, cancellationToken).ConfigureAwait (false);
				if (alias != null) {
					return SpecializedCollections.SingletonEnumerable (alias);
				}
			}

			return result;
		}

		static SyntaxNode GetObjectCreationNewExpression (SyntaxTree tree, int position, CancellationToken cancellationToken)
		{
			if (tree != null) {
				if (!tree.IsInNonUserCode (position, cancellationToken)) {
					var tokenOnLeftOfPosition = tree.FindTokenOnLeftOfPosition (position, cancellationToken);
					var newToken = tokenOnLeftOfPosition.GetPreviousTokenIfTouchingWord (position);

					// Only after 'new'.
					if (newToken.Kind () == SyntaxKind.NewKeyword) {
						// Only if the 'new' belongs to an object creation expression (and isn't a 'new'
						// modifier on a member).
						if (tree.IsObjectCreationTypeContext (position, tokenOnLeftOfPosition, cancellationToken)) {
							return newToken.Parent as ExpressionSyntax;
						}
					}
				}
			}

			return null;
		}

	}
}
