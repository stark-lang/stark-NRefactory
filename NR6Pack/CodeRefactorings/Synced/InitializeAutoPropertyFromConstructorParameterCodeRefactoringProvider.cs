﻿//
// InitializeAutoPropertyFromConstructorParameterCodeRefactoringProvider.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Formatting;
using System;

namespace ICSharpCode.NRefactory6.CSharp.CodeRefactorings
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name="Initialize auto property from constructor parameter")]
	public class InitializeAutoPropertyFromConstructorParameterCodeRefactoringProvider : CodeRefactoringProvider
	{
		public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			var document = context.Document;
			if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
				return;
			var span = context.Span;
			if (!span.IsEmpty)
				return;
			var cancellationToken = context.CancellationToken;
			if (cancellationToken.IsCancellationRequested)
				return;
			var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			if (model.IsFromGeneratedCode(cancellationToken))
				return;
			var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
			var token = root.FindToken(span.Start);
			var parameter = token.Parent as ParameterSyntax;

			if (parameter != null) {
				var ctor = parameter.Parent.Parent as ConstructorDeclarationSyntax;
				if (ctor == null) 
					return;

				context.RegisterRefactoring(
					CodeActionFactory.Create(
						parameter.Span,
						DiagnosticSeverity.Info, 
						GettextCatalog.GetString ("Initialize auto-property from parameter"),
						t2 => {
							var propertyName = GetPropertyName (parameter.Identifier.ToString ());
							var accessorDeclList = new SyntaxList<AccessorDeclarationSyntax> ().Add (SyntaxFactory.AccessorDeclaration (SyntaxKind.GetAccessorDeclaration).WithSemicolonToken (SyntaxFactory.Token (SyntaxKind.SemicolonToken))).Add (SyntaxFactory.AccessorDeclaration (SyntaxKind.SetAccessorDeclaration).WithSemicolonToken (SyntaxFactory.Token (SyntaxKind.SemicolonToken)));
							var newProperty = SyntaxFactory.PropertyDeclaration (parameter.Type, propertyName)
								.WithAccessorList (SyntaxFactory.AccessorList (accessorDeclList))
								.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
								.WithAdditionalAnnotations(Formatter.Annotation);

							var assignmentStatement = SyntaxFactory.ExpressionStatement(
								SyntaxFactory.AssignmentExpression(
									SyntaxKind.SimpleAssignmentExpression, 
									propertyName != parameter.Identifier.ToString () ? (ExpressionSyntax)SyntaxFactory.IdentifierName(propertyName) : SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression (), SyntaxFactory.IdentifierName(parameter.Identifier)),
									SyntaxFactory.IdentifierName(parameter.Identifier)
								)
							).WithAdditionalAnnotations(Formatter.Annotation);

							root = root.TrackNodes (ctor);
							var newRoot = root.InsertNodesBefore (root.GetCurrentNode (ctor), new List<SyntaxNode> () {
								newProperty
							});
							newRoot = newRoot.ReplaceNode (newRoot.GetCurrentNode (ctor), ctor.WithBody (
								ctor.Body.WithStatements (SyntaxFactory.List<StatementSyntax> (new [] { assignmentStatement }.Concat (ctor.Body.Statements)))
							));

							return Task.FromResult(document.WithSyntaxRoot (newRoot));
						})
				);
			}
		}

		static string GetPropertyName (string v)
		{
			return char.ToUpper (v [0]) + v.Substring (1);
        }
	}
}