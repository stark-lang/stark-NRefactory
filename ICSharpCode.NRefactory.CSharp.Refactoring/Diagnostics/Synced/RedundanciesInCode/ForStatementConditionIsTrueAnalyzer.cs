// 
// ForStatementConditionIsTrueAnalyzer.cs
// 
// Author:
//      Ji Kun <jikun.nus@gmail.com>
// 
// Copyright (c) 2013 Ji Kun <jikun.nus@gmail.com>
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ForStatementConditionIsTrueAnalyzer : DiagnosticAnalyzer
	{
		static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor (
			NRefactoryDiagnosticIDs.ForStatementConditionIsTrueAnalyzerID,
			GettextCatalog.GetString ("'true' is redundant as for statement condition"),
			GettextCatalog.GetString ("true is redundant as for statement condition, thus can be safely ommited"),
			DiagnosticAnalyzerCategories.RedundanciesInCode,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			helpLinkUri: HelpLink.CreateFor (NRefactoryDiagnosticIDs.ForStatementConditionIsTrueAnalyzerID),
			customTags: DiagnosticCustomTags.Unnecessary
		);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create (descriptor);

		public override void Initialize (AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction (
				nodeContext => {
					Diagnostic diagnostic;
					if (TryGetDiagnostic (nodeContext, out diagnostic)) {
						nodeContext.ReportDiagnostic (diagnostic);
					}
				},
				new SyntaxKind [] { SyntaxKind.ForStatement }
			);
		}

		static bool TryGetDiagnostic (SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
		{
			diagnostic = default(Diagnostic);
			var node = nodeContext.Node as ForStatementSyntax;
			if (!node.Condition.IsKind (SyntaxKind.TrueLiteralExpression))
				return false;
			diagnostic = Diagnostic.Create (descriptor, node.Condition.GetLocation ());
			return true;
		}
	}
}