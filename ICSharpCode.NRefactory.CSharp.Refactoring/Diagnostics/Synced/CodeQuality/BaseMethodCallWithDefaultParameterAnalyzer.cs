//
// BaseMethodCallWithDefaultParameterAnalyzer.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://xamarin.com)
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class BaseMethodCallWithDefaultParameterAnalyzer : DiagnosticAnalyzer
	{
		static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor (
			NRefactoryDiagnosticIDs.BaseMethodCallWithDefaultParameterDiagnosticID, 
			GettextCatalog.GetString("Call to base member with implicit default parameters"),
			GettextCatalog.GetString("Call to base member with implicit default parameters"), 
			DiagnosticAnalyzerCategories.CodeQualityIssues, 
			DiagnosticSeverity.Warning, 
			isEnabledByDefault: true,
			helpLinkUri: HelpLink.CreateFor(NRefactoryDiagnosticIDs.ConvertClosureToMethodDiagnosticID)
		);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create (descriptor);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(
				(nodeContext) => {
					Diagnostic diagnostic;
					if (TryGetDiagnostic(nodeContext, out diagnostic)) 
						nodeContext.ReportDiagnostic (diagnostic);
				}, 
				new SyntaxKind[] { SyntaxKind.InvocationExpression, SyntaxKind.ElementAccessExpression }
			);
		}

		static bool TryGetDiagnostic (SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
		{
			diagnostic = default(Diagnostic);

			var invocationExpr = nodeContext.Node as InvocationExpressionSyntax;
			if (invocationExpr != null) {
				var mr = invocationExpr.Expression as MemberAccessExpressionSyntax;
				if (mr == null || !mr.Expression.IsKind (SyntaxKind.BaseExpression))
					return false;

				var invocationRR = nodeContext.SemanticModel.GetSymbolInfo (invocationExpr);
				if (invocationRR.Symbol == null)
					return false;

				var parentEntity = invocationExpr.FirstAncestorOrSelf<MethodDeclarationSyntax> ();
				if (parentEntity == null)
					return false;
				var rr = nodeContext.SemanticModel.GetDeclaredSymbol (parentEntity);
				if (rr == null || rr.OverriddenMethod != invocationRR.Symbol)
					return false;

				var parameters = invocationRR.Symbol.GetParameters ();
				if (invocationExpr.ArgumentList.Arguments.Count >= parameters.Length ||
						parameters.Length == 0 ||
						!parameters.Last ().IsOptional)
					return false;

				diagnostic = Diagnostic.Create (
					descriptor,
					invocationExpr.GetLocation ()
				);
				return true;
			}

			var elementAccessExpr = nodeContext.Node as ElementAccessExpressionSyntax;
			if (elementAccessExpr != null) {
				var mr = elementAccessExpr.Expression;
				if (mr == null || !mr.IsKind (SyntaxKind.BaseExpression))
					return false;

				var invocationRR = nodeContext.SemanticModel.GetSymbolInfo (elementAccessExpr);
				if (invocationRR.Symbol == null)
					return false;

				var parentEntity = elementAccessExpr.FirstAncestorOrSelf<IndexerDeclarationSyntax> ();
				if (parentEntity == null)
					return false;
				
				var rr = nodeContext.SemanticModel.GetDeclaredSymbol (parentEntity);
				if (rr == null || rr.OverriddenProperty != invocationRR.Symbol)
					return false;

				var parameters = invocationRR.Symbol.GetParameters ();
				if (elementAccessExpr.ArgumentList.Arguments.Count >= parameters.Length ||
						parameters.Length == 0 ||
						!parameters.Last ().IsOptional)
					return false;
				
				diagnostic = Diagnostic.Create (
					descriptor,
					elementAccessExpr.GetLocation ()
				);
				return true;
			}
			return false;
		}
	}
}