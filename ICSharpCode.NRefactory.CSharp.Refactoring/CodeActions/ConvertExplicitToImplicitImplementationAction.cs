﻿// 
// ConvertExplicitToImplicitImplementation.cs
// 
// Author:
//      Mansheng Yang <lightyang0@gmail.com>
// 
// Copyright (c) 2012 Mansheng Yang <lightyang0@gmail.com>
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
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ICSharpCode.NRefactory6.CSharp.Refactoring;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace ICSharpCode.NRefactory6.CSharp.Refactoring
{
	[NRefactoryCodeRefactoringProvider(Description = "Convert explicit implementation of an interface method to implicit implementation")]
	[ExportCodeRefactoringProvider("Convert explicit to implict implementation", LanguageNames.CSharp)]
	public class ConvertExplicitToImplicitImplementationAction : CodeRefactoringProvider
	{
		public override async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(CodeRefactoringContext context)
		{
			var document = context.Document;
			var span = context.Span;
			var cancellationToken = context.CancellationToken;
			var model = await document.GetSemanticModelAsync(cancellationToken);
			var root = await model.SyntaxTree.GetRootAsync(cancellationToken);
			var node = root.FindNode(span);
			while (node != null && !(node is MemberDeclarationSyntax))
				node = node.Parent;
			if (node == null)
				return Enumerable.Empty<CodeAction>();

			if (!node.IsKind(SyntaxKind.MethodDeclaration) &&
				!node.IsKind(SyntaxKind.PropertyDeclaration) &&
				!node.IsKind(SyntaxKind.IndexerDeclaration) &&
				!node.IsKind(SyntaxKind.EventDeclaration))
				return Enumerable.Empty<CodeAction>();

			var memberDeclaration = node as MemberDeclarationSyntax;
			var explicitSyntax = memberDeclaration.GetExplicitInterfaceSpecifierSyntax();
			if (explicitSyntax == null || !explicitSyntax.Span.Contains(span))
				return Enumerable.Empty<CodeAction>();

			var enclosingSymbol = model.GetDeclaredSymbol(memberDeclaration, cancellationToken);
			if (enclosingSymbol == null)
				return Enumerable.Empty<CodeAction>();
			var containingType = enclosingSymbol.ContainingType;


			foreach (var member in containingType.GetMembers()) {
				if (member == enclosingSymbol ||
					member.Kind != enclosingSymbol.Kind)
					continue;

				switch (member.Kind) {
					case SymbolKind.Property:
						var property1 = (IPropertySymbol)enclosingSymbol;
						var property2 = (IPropertySymbol)member;

						foreach (var explictProperty in property1.ExplicitInterfaceImplementations) {
							if (explictProperty.Name == property2.Name) {
								if (SignatureComparer.HaveSameSignature(property1.Parameters, property2.Parameters))
									return Enumerable.Empty<CodeAction>();
							}
						}
						break;
					case SymbolKind.Method:
						var method1 = (IMethodSymbol)enclosingSymbol;
						var method2 = (IMethodSymbol)member;
						foreach (var explictMethod in method1.ExplicitInterfaceImplementations) {
							if (explictMethod.Name == method2.Name) {
								if (SignatureComparer.HaveSameSignature(method1.Parameters, method2.Parameters))
									return Enumerable.Empty<CodeAction>();
							}
						}
						break;
					case SymbolKind.Event:
						var evt1 = (IEventSymbol)enclosingSymbol;
						var evt2 = (IEventSymbol)member;
						foreach (var explictProperty in evt1.ExplicitInterfaceImplementations) {
							if (explictProperty.Name == evt2.Name)
								return Enumerable.Empty<CodeAction>();
						}
						break;
				}
			}

			return new[] {
				CodeActionFactory.Create(
					span, 
					DiagnosticSeverity.Info, 
					"Convert explict to implicit implementation", 
					t2 => {
						var newNode = memberDeclaration;
						switch (newNode.CSharpKind()) {
							case SyntaxKind.MethodDeclaration:
								var method = (MethodDeclarationSyntax)memberDeclaration;
								newNode = method
									.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
									.WithExplicitInterfaceSpecifier(null);
								break;
							case SyntaxKind.PropertyDeclaration:
								var property = (PropertyDeclarationSyntax)memberDeclaration;
								newNode = property
									.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
									.WithExplicitInterfaceSpecifier(null);
								break;
							case SyntaxKind.IndexerDeclaration:
								var indexer = (IndexerDeclarationSyntax)memberDeclaration;
								newNode = indexer
									.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
									.WithExplicitInterfaceSpecifier(null);
								break;
							case SyntaxKind.EventDeclaration:
								var evt = (EventDeclarationSyntax)memberDeclaration;
								newNode = evt
									.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
									.WithExplicitInterfaceSpecifier(null);
								break;
						}
						var newRoot = root.ReplaceNode(
							memberDeclaration,
							newNode.WithAdditionalAnnotations(Formatter.Annotation)
						);
						return Task.FromResult(document.WithSyntaxRoot(newRoot));
					}
				) 
			};
		}
	}
}