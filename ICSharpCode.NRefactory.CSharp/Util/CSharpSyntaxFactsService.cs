﻿//
// CSharpSyntaxFactsService.cs
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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;

namespace ICSharpCode.NRefactory6.CSharp
{
	public static class CSharpSyntaxFactsService
	{
		public static bool IsAwaitKeyword(this SyntaxToken token)
		{
			return token.IsKind(SyntaxKind.AwaitKeyword);
		}

		public static bool IsIdentifier(this SyntaxToken token)
		{
			return token.IsKind(SyntaxKind.IdentifierToken);
		}

		public static bool IsGlobalNamespaceKeyword(this SyntaxToken token)
		{
			return token.IsKind(SyntaxKind.GlobalKeyword);
		}

		public static bool IsVerbatimIdentifier(this SyntaxToken token)
		{
			return token.IsVerbatimIdentifier();
		}

		public static bool IsOperator(this SyntaxToken token)
		{
			var kind = token.Kind();

			return
				(SyntaxFacts.IsAnyUnaryExpression(kind) &&
					(token.Parent is PrefixUnaryExpressionSyntax || token.Parent is PostfixUnaryExpressionSyntax)) ||
				(SyntaxFacts.IsBinaryExpression(kind) && token.Parent is BinaryExpressionSyntax) ||
				(SyntaxFacts.IsAssignmentExpressionOperatorToken(kind) && token.Parent is AssignmentExpressionSyntax);
		}

		public static bool IsKeyword(this SyntaxToken token)
		{
			var kind = (SyntaxKind)token.RawKind;
			return
				SyntaxFacts.IsKeywordKind(kind); // both contextual and reserved keywords
		}
//
//		public bool IsContextualKeyword(SyntaxToken token)
//		{
//			var kind = (SyntaxKind)token.RawKind;
//			return
//				SyntaxFacts.IsContextualKeyword(kind);
//		}
//
//		public bool IsPreprocessorKeyword(SyntaxToken token)
//		{
//			var kind = (SyntaxKind)token.RawKind;
//			return
//				SyntaxFacts.IsPreprocessorKeyword(kind);
//		}
//
//		public bool IsHashToken(SyntaxToken token)
//		{
//			return (SyntaxKind)token.RawKind == SyntaxKind.HashToken;
//		}
//
//		public bool IsInInactiveRegion(SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
//		{
//			var csharpTree = syntaxTree as SyntaxTree;
//			if (csharpTree == null)
//			{
//				return false;
//			}
//
//			return csharpTree.IsInInactiveRegion(position, cancellationToken);
//		}
//
//		public bool IsInNonUserCode(SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
//		{
//			var csharpTree = syntaxTree as SyntaxTree;
//			if (csharpTree == null)
//			{
//				return false;
//			}
//
//			return csharpTree.IsInNonUserCode(position, cancellationToken);
//		}
//
//		public bool IsEntirelyWithinStringOrCharLiteral(SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
//		{
//			var csharpTree = syntaxTree as SyntaxTree;
//			if (csharpTree == null)
//			{
//				return false;
//			}
//
//			return csharpTree.IsEntirelyWithinStringOrCharLiteral(position, cancellationToken);
//		}
//
//		public bool IsDirective(SyntaxNode node)
//		{
//			return node is DirectiveTriviaSyntax;
//		}
//
//		public bool TryGetExternalSourceInfo(SyntaxNode node, out ExternalSourceInfo info)
//		{
//			var lineDirective = node as LineDirectiveTriviaSyntax;
//			if (lineDirective != null)
//			{
//				if (lineDirective.Line.Kind() == SyntaxKind.DefaultKeyword)
//				{
//					info = new ExternalSourceInfo(null, ends: true);
//					return true;
//				}
//				else if (lineDirective.Line.Kind() == SyntaxKind.NumericLiteralToken &&
//					lineDirective.Line.Value is int)
//				{
//					info = new ExternalSourceInfo((int)lineDirective.Line.Value, false);
//					return true;
//				}
//			}
//
//			info = default(ExternalSourceInfo);
//			return false;
//		}

		public static bool IsRightSideOfQualifiedName(this SyntaxNode node)
		{
			var name = node as SimpleNameSyntax;
			return name.IsRightSideOfQualifiedName();
		}

		public static bool IsMemberAccessExpressionName(this SyntaxNode node)
		{
			var name = node as SimpleNameSyntax;
			return name.IsMemberAccessExpressionName();
		}

		public static bool IsObjectCreationExpressionType(this SyntaxNode node)
		{
			return node.IsParentKind(SyntaxKind.ObjectCreationExpression) &&
				((ObjectCreationExpressionSyntax)node.Parent).Type == node;
		}

		public static bool IsAttributeName(this SyntaxNode node)
		{
			return SyntaxFacts.IsAttributeName(node);
		}

//		public bool IsInvocationExpression(SyntaxNode node)
//		{
//			return node is InvocationExpressionSyntax;
//		}
//
//		public bool IsAnonymousFunction(SyntaxNode node)
//		{
//			return node is ParenthesizedLambdaExpressionSyntax ||
//				node is SimpleLambdaExpressionSyntax ||
//				node is AnonymousMethodExpressionSyntax;
//		}
//
//		public bool IsGenericName(SyntaxNode node)
//		{
//			return node is GenericNameSyntax;
//		}

		public static bool IsNamedParameter(this SyntaxNode node)
		{
			return node.CheckParent<NameColonSyntax>(p => p.Name == node);
		}

//		public bool IsSkippedTokensTrivia(SyntaxNode node)
//		{
//			return node is SkippedTokensTriviaSyntax;
//		}

		public static bool HasIncompleteParentMember(this SyntaxNode node)
		{
			return node.IsParentKind(SyntaxKind.IncompleteMember);
		}

//		public SyntaxToken GetIdentifierOfGenericName(SyntaxNode genericName)
//		{
//			var csharpGenericName = genericName as GenericNameSyntax;
//			return csharpGenericName != null
//				? csharpGenericName.Identifier
//					: default(SyntaxToken);
//		}
//
//		public bool IsCaseSensitive
//		{
//			get
//			{
//				return true;
//			}
//		}
//
//		public bool IsUsingDirectiveName(SyntaxNode node)
//		{
//			return
//				node.IsParentKind(SyntaxKind.UsingDirective) &&
//				((UsingDirectiveSyntax)node.Parent).Name == node;
//		}
//
//		public bool IsForEachStatement(SyntaxNode node)
//		{
//			return node is ForEachStatementSyntax;
//		}
//
//		public bool IsLockStatement(SyntaxNode node)
//		{
//			return node is LockStatementSyntax;
//		}
//
//		public bool IsUsingStatement(SyntaxNode node)
//		{
//			return node is UsingStatementSyntax;
//		}
//
//		public bool IsThisConstructorInitializer(SyntaxToken token)
//		{
//			return token.Parent.IsKind(SyntaxKind.ThisConstructorInitializer) &&
//				((ConstructorInitializerSyntax)token.Parent).ThisOrBaseKeyword == token;
//		}
//
//		public bool IsBaseConstructorInitializer(SyntaxToken token)
//		{
//			return token.Parent.IsKind(SyntaxKind.BaseConstructorInitializer) &&
//				((ConstructorInitializerSyntax)token.Parent).ThisOrBaseKeyword == token;
//		}
//
//		public bool IsQueryExpression(SyntaxNode node)
//		{
//			return node is QueryExpressionSyntax;
//		}
//
//		public bool IsPredefinedType(SyntaxToken token)
//		{
//			PredefinedType actualType;
//			return TryGetPredefinedType(token, out actualType) && actualType != PredefinedType.None;
//		}
//
//		public bool IsPredefinedType(SyntaxToken token, PredefinedType type)
//		{
//			PredefinedType actualType;
//			return TryGetPredefinedType(token, out actualType) && actualType == type;
//		}
//
//		public bool TryGetPredefinedType(SyntaxToken token, out PredefinedType type)
//		{
//			type = GetPredefinedType(token);
//			return type != PredefinedType.None;
//		}
//
//		private PredefinedType GetPredefinedType(SyntaxToken token)
//		{
//			switch ((SyntaxKind)token.RawKind)
//			{
//			case SyntaxKind.BoolKeyword:
//				return PredefinedType.Boolean;
//			case SyntaxKind.ByteKeyword:
//				return PredefinedType.Byte;
//			case SyntaxKind.SByteKeyword:
//				return PredefinedType.SByte;
//			case SyntaxKind.IntKeyword:
//				return PredefinedType.Int32;
//			case SyntaxKind.UIntKeyword:
//				return PredefinedType.UInt32;
//			case SyntaxKind.ShortKeyword:
//				return PredefinedType.Int16;
//			case SyntaxKind.UShortKeyword:
//				return PredefinedType.UInt16;
//			case SyntaxKind.LongKeyword:
//				return PredefinedType.Int64;
//			case SyntaxKind.ULongKeyword:
//				return PredefinedType.UInt64;
//			case SyntaxKind.FloatKeyword:
//				return PredefinedType.Single;
//			case SyntaxKind.DoubleKeyword:
//				return PredefinedType.Double;
//			case SyntaxKind.DecimalKeyword:
//				return PredefinedType.Decimal;
//			case SyntaxKind.StringKeyword:
//				return PredefinedType.String;
//			case SyntaxKind.CharKeyword:
//				return PredefinedType.Char;
//			case SyntaxKind.ObjectKeyword:
//				return PredefinedType.Object;
//			case SyntaxKind.VoidKeyword:
//				return PredefinedType.Void;
//			default:
//				return PredefinedType.None;
//			}
//		}
//
//		public bool IsPredefinedOperator(SyntaxToken token)
//		{
//			PredefinedOperator actualOperator;
//			return TryGetPredefinedOperator(token, out actualOperator) && actualOperator != PredefinedOperator.None;
//		}
//
//		public bool IsPredefinedOperator(SyntaxToken token, PredefinedOperator op)
//		{
//			PredefinedOperator actualOperator;
//			return TryGetPredefinedOperator(token, out actualOperator) && actualOperator == op;
//		}
//
//		public bool TryGetPredefinedOperator(SyntaxToken token, out PredefinedOperator op)
//		{
//			op = GetPredefinedOperator(token);
//			return op != PredefinedOperator.None;
//		}
//
//		private PredefinedOperator GetPredefinedOperator(SyntaxToken token)
//		{
//			switch ((SyntaxKind)token.RawKind)
//			{
//			case SyntaxKind.PlusToken:
//			case SyntaxKind.PlusEqualsToken:
//				return PredefinedOperator.Addition;
//
//			case SyntaxKind.MinusToken:
//			case SyntaxKind.MinusEqualsToken:
//				return PredefinedOperator.Subtraction;
//
//			case SyntaxKind.AmpersandToken:
//			case SyntaxKind.AmpersandEqualsToken:
//				return PredefinedOperator.BitwiseAnd;
//
//			case SyntaxKind.BarToken:
//			case SyntaxKind.BarEqualsToken:
//				return PredefinedOperator.BitwiseOr;
//
//			case SyntaxKind.MinusMinusToken:
//				return PredefinedOperator.Decrement;
//
//			case SyntaxKind.PlusPlusToken:
//				return PredefinedOperator.Increment;
//
//			case SyntaxKind.SlashToken:
//			case SyntaxKind.SlashEqualsToken:
//				return PredefinedOperator.Division;
//
//			case SyntaxKind.EqualsEqualsToken:
//				return PredefinedOperator.Equality;
//
//			case SyntaxKind.CaretToken:
//			case SyntaxKind.CaretEqualsToken:
//				return PredefinedOperator.ExclusiveOr;
//
//			case SyntaxKind.GreaterThanToken:
//				return PredefinedOperator.GreaterThan;
//
//			case SyntaxKind.GreaterThanEqualsToken:
//				return PredefinedOperator.GreaterThanOrEqual;
//
//			case SyntaxKind.ExclamationEqualsToken:
//				return PredefinedOperator.Inequality;
//
//			case SyntaxKind.LessThanLessThanToken:
//			case SyntaxKind.LessThanLessThanEqualsToken:
//				return PredefinedOperator.LeftShift;
//
//			case SyntaxKind.LessThanEqualsToken:
//				return PredefinedOperator.LessThanOrEqual;
//
//			case SyntaxKind.AsteriskToken:
//			case SyntaxKind.AsteriskEqualsToken:
//				return PredefinedOperator.Multiplication;
//
//			case SyntaxKind.PercentToken:
//			case SyntaxKind.PercentEqualsToken:
//				return PredefinedOperator.Modulus;
//
//			case SyntaxKind.ExclamationToken:
//			case SyntaxKind.TildeToken:
//				return PredefinedOperator.Complement;
//
//			case SyntaxKind.GreaterThanGreaterThanToken:
//			case SyntaxKind.GreaterThanGreaterThanEqualsToken:
//				return PredefinedOperator.RightShift;
//			}
//
//			return PredefinedOperator.None;
//		}
//
//		public string GetText(int kind)
//		{
//			return SyntaxFacts.GetText((SyntaxKind)kind);
//		}
//
//		public bool IsIdentifierStartCharacter(char c)
//		{
//			return SyntaxFacts.IsIdentifierStartCharacter(c);
//		}
//
//		public bool IsIdentifierPartCharacter(char c)
//		{
//			return SyntaxFacts.IsIdentifierPartCharacter(c);
//		}
//
//		public bool IsIdentifierEscapeCharacter(char c)
//		{
//			return c == '@';
//		}
//
//		public bool IsValidIdentifier(string identifier)
//		{
//			var token = SyntaxFactory.ParseToken(identifier);
//			return IsIdentifier(token) && !token.ContainsDiagnostics && token.ToString().Length == identifier.Length;
//		}
//
//		public bool IsVerbatimIdentifier(string identifier)
//		{
//			var token = SyntaxFactory.ParseToken(identifier);
//			return IsIdentifier(token) && !token.ContainsDiagnostics && token.ToString().Length == identifier.Length && token.IsVerbatimIdentifier();
//		}
//
//		public bool IsTypeCharacter(char c)
//		{
//			return false;
//		}
//
//		public bool IsStartOfUnicodeEscapeSequence(char c)
//		{
//			return c == '\\';
//		}
//
//		public bool IsLiteral(SyntaxToken token)
//		{
//			switch (token.Kind())
//			{
//			case SyntaxKind.NumericLiteralToken:
//			case SyntaxKind.CharacterLiteralToken:
//			case SyntaxKind.StringLiteralToken:
//			case SyntaxKind.NullKeyword:
//			case SyntaxKind.TrueKeyword:
//			case SyntaxKind.FalseKeyword:
//				return true;
//			}
//
//			return false;
//		}
//
//		public bool IsStringLiteral(SyntaxToken token)
//		{
//			return token.IsKind(SyntaxKind.StringLiteralToken);
//		}
//
//		public bool IsTypeNamedVarInVariableOrFieldDeclaration(SyntaxToken token, SyntaxNode parent)
//		{
//			var typedToken = token;
//			var typedParent = parent;
//
//			if (typedParent.IsKind(SyntaxKind.IdentifierName))
//			{
//				TypeSyntax declaredType = null;
//				if (typedParent.IsParentKind(SyntaxKind.VariableDeclaration))
//				{
//					declaredType = ((VariableDeclarationSyntax)typedParent.Parent).Type;
//				}
//				else if (typedParent.IsParentKind(SyntaxKind.FieldDeclaration))
//				{
//					declaredType = ((FieldDeclarationSyntax)typedParent.Parent).Declaration.Type;
//				}
//
//				return declaredType == typedParent && typedToken.ValueText == "var";
//			}
//
//			return false;
//		}
//
//		public bool IsTypeNamedDynamic(SyntaxToken token, SyntaxNode parent)
//		{
//			var typedParent = parent as ExpressionSyntax;
//
//			if (typedParent != null)
//			{
//				if (SyntaxFacts.IsInTypeOnlyContext(typedParent) &&
//					typedParent.IsKind(SyntaxKind.IdentifierName) &&
//					token.ValueText == "dynamic")
//				{
//					return true;
//				}
//			}
//
//			return false;
//		}
//
//		public bool IsBindableToken(SyntaxToken token)
//		{
//			if (this.IsWord(token) || this.IsLiteral(token) || this.IsOperator(token))
//			{
//				switch ((SyntaxKind)token.RawKind)
//				{
//				case SyntaxKind.DelegateKeyword:
//				case SyntaxKind.VoidKeyword:
//					return false;
//				}
//
//				return true;
//			}
//
//			return false;
//		}

		public static bool IsMemberAccessExpression(this SyntaxNode node)
		{
			return node is MemberAccessExpressionSyntax &&
				((MemberAccessExpressionSyntax)node).Kind() == SyntaxKind.SimpleMemberAccessExpression;
		}

		public static bool IsConditionalMemberAccessExpression(this SyntaxNode node)
		{
			return node is ConditionalAccessExpressionSyntax;
		}

		public static bool IsPointerMemberAccessExpression(this SyntaxNode node)
		{
			return node is MemberAccessExpressionSyntax &&
				((MemberAccessExpressionSyntax)node).Kind() == SyntaxKind.PointerMemberAccessExpression;
		}

		public static void GetNameAndArityOfSimpleName(this SyntaxNode node, out string name, out int arity)
		{
			name = null;
			arity = 0;

			var simpleName = node as SimpleNameSyntax;
			if (simpleName != null)
			{
				name = simpleName.Identifier.ValueText;
				arity = simpleName.Arity;
			}
		}

		public static SyntaxNode GetExpressionOfMemberAccessExpression(this SyntaxNode node)
		{
			if (node.IsKind(SyntaxKind.MemberBindingExpression))
			{
				if (node.IsParentKind(SyntaxKind.ConditionalAccessExpression))
				{
					return GetExpressionOfConditionalMemberAccessExpression(node.Parent);
				}
				if (node.IsParentKind(SyntaxKind.InvocationExpression) &&
					node.Parent.IsParentKind(SyntaxKind.ConditionalAccessExpression))
				{
					return GetExpressionOfConditionalMemberAccessExpression(node.Parent.Parent);
				}
			}

			return (node as MemberAccessExpressionSyntax)?.Expression;
		}

		public static SyntaxNode GetExpressionOfConditionalMemberAccessExpression(this SyntaxNode node)
		{
			return (node as ConditionalAccessExpressionSyntax)?.Expression;
		}


		public static bool IsInNamespaceOrTypeContext(this SyntaxNode node)
		{
			return SyntaxFacts.IsInNamespaceOrTypeContext(node as ExpressionSyntax);
		}

		public static SyntaxNode GetExpressionOfArgument(this SyntaxNode node)
		{
			return ((ArgumentSyntax)node).Expression;
		}

		public static RefKind GetRefKindOfArgument(this SyntaxNode node)
		{
			return (node as ArgumentSyntax).GetRefKind();
		}

		public static bool IsInConstantContext(this SyntaxNode node)
		{
			return (node as ExpressionSyntax).IsInConstantContext();
		}

		public static bool IsInConstructor(this SyntaxNode node)
		{
			return node.GetAncestor<ConstructorDeclarationSyntax>() != null;
		}

//		public bool IsUnsafeContext(SyntaxNode node)
//		{
//			return node.IsUnsafeContext();
//		}

		public static SyntaxNode GetNameOfAttribute(this SyntaxNode node)
		{
			return ((AttributeSyntax)node).Name;
		}

		public static bool IsAttribute(this SyntaxNode node)
		{
			return node is AttributeSyntax;
		}

		public static bool IsAttributeNamedArgumentIdentifier(this SyntaxNode node)
		{
			var identifier = node as IdentifierNameSyntax;
			return
				identifier != null &&
				identifier.IsParentKind(SyntaxKind.NameEquals) &&
				identifier.Parent.IsParentKind(SyntaxKind.AttributeArgument);
		}

		public static SyntaxNode GetContainingTypeDeclaration(this SyntaxNode root, int position)
		{
			if (root == null)
			{
				throw new ArgumentNullException("root");
			}

			if (position < 0 || position > root.Span.End)
			{
				throw new ArgumentOutOfRangeException("position");
			}

			return root
				.FindToken(position)
				.GetAncestors<SyntaxNode>()
				.FirstOrDefault(n => n is BaseTypeDeclarationSyntax || n is DelegateDeclarationSyntax);
		}
//
//		public SyntaxNode GetContainingVariableDeclaratorOfFieldDeclaration(SyntaxNode node)
//		{
//			throw ExceptionUtilities.Unreachable;
//		}
//
//		public SyntaxToken FindTokenOnLeftOfPosition(
//			SyntaxNode node, int position, bool includeSkipped, bool includeDirectives, bool includeDocumentationComments)
//		{
//			return node.FindTokenOnLeftOfPosition(position, includeSkipped, includeDirectives, includeDocumentationComments);
//		}
//
//		public SyntaxToken FindTokenOnRightOfPosition(
//			SyntaxNode node, int position, bool includeSkipped, bool includeDirectives, bool includeDocumentationComments)
//		{
//			return node.FindTokenOnRightOfPosition(position, includeSkipped, includeDirectives, includeDocumentationComments);
//		}

		public static bool IsObjectCreationExpression(this SyntaxNode node)
		{
			return node is ObjectCreationExpressionSyntax;
		}

		public static bool IsObjectInitializerNamedAssignmentIdentifier(this SyntaxNode node)
		{
			var identifier = node as IdentifierNameSyntax;
			return
				identifier != null &&
				identifier.IsLeftSideOfAssignExpression() &&
				identifier.Parent.IsParentKind(SyntaxKind.ObjectInitializerExpression);
		}

		public static bool IsElementAccessExpression(this SyntaxNode node)
		{
			return node.Kind() == SyntaxKind.ElementAccessExpression;
		}
//
//		public SyntaxNode ConvertToSingleLine(SyntaxNode node)
//		{
//			return node.ConvertToSingleLine();
//		}
//
//		public SyntaxToken ToIdentifierToken(string name)
//		{
//			return name.ToIdentifierToken();
//		}

		public static SyntaxNode Parenthesize(this SyntaxNode expression, bool includeElasticTrivia=true)
		{
			return ((ExpressionSyntax)expression).Parenthesize(includeElasticTrivia);
		}

//		public bool IsIndexerMemberCRef(SyntaxNode node)
//		{
//			return node.Kind() == SyntaxKind.IndexerMemberCref;
//		}
//
//		public SyntaxNode GetContainingMemberDeclaration(SyntaxNode root, int position)
//		{
//			Contract.ThrowIfNull(root, "root");
//			Contract.ThrowIfTrue(position < 0 || position > root.FullSpan.End, "position");
//
//			var end = root.FullSpan.End;
//			if (end == 0)
//			{
//				// empty file
//				return null;
//			}
//
//			// make sure position doesn't touch end of root
//			position = Math.Min(position, end - 1);
//
//			var node = root.FindToken(position).Parent;
//			while (node != null)
//			{
//				if (node is MemberDeclarationSyntax)
//				{
//					return node;
//				}
//
//				node = node.Parent;
//			}
//
//			return null;
//		}
//
//		public bool IsMethodLevelMember(SyntaxNode node)
//		{
//			return node is BaseMethodDeclarationSyntax || node is BasePropertyDeclarationSyntax || node is EnumMemberDeclarationSyntax || node is BaseFieldDeclarationSyntax;
//		}
//
//		public bool IsTopLevelNodeWithMembers(SyntaxNode node)
//		{
//			return node is NamespaceDeclarationSyntax ||
//				node is TypeDeclarationSyntax ||
//				node is EnumDeclarationSyntax;
//		}
//
		public static bool TryGetDeclaredSymbolInfo(this SyntaxNode node, out DeclaredSymbolInfo declaredSymbolInfo)
		{
			switch (node.Kind())
			{
			case SyntaxKind.ClassDeclaration:
				var classDecl = (ClassDeclarationSyntax)node;
				declaredSymbolInfo = new DeclaredSymbolInfo(node,
					classDecl.Identifier.ValueText,
					GetContainerDisplayName(node.Parent),
					GetFullyQualifiedContainerName(node.Parent),
					DeclaredSymbolInfoKind.Class, classDecl.Identifier.Span);
				return true;
			case SyntaxKind.ConstructorDeclaration:
				var ctorDecl = (ConstructorDeclarationSyntax)node;
				declaredSymbolInfo = new DeclaredSymbolInfo(
					node,
					ctorDecl.Identifier.ValueText,
					GetContainerDisplayName(node.Parent),
					GetFullyQualifiedContainerName(node.Parent),
					DeclaredSymbolInfoKind.Constructor,
					ctorDecl.Identifier.Span,
					parameterCount: (ushort)(ctorDecl.ParameterList?.Parameters.Count ?? 0));
				return true;
			case SyntaxKind.DelegateDeclaration:
				var delegateDecl = (DelegateDeclarationSyntax)node;
				declaredSymbolInfo = new DeclaredSymbolInfo(node,
					delegateDecl.Identifier.ValueText,
					GetContainerDisplayName(node.Parent),
					GetFullyQualifiedContainerName(node.Parent),
					DeclaredSymbolInfoKind.Delegate, delegateDecl.Identifier.Span);
				return true;
			case SyntaxKind.EnumDeclaration:
				var enumDecl = (EnumDeclarationSyntax)node;
				declaredSymbolInfo = new DeclaredSymbolInfo(node,
					enumDecl.Identifier.ValueText,
					GetContainerDisplayName(node.Parent),
					GetFullyQualifiedContainerName(node.Parent),
					DeclaredSymbolInfoKind.Enum, enumDecl.Identifier.Span);
				return true;
			case SyntaxKind.EnumMemberDeclaration:
				var enumMember = (EnumMemberDeclarationSyntax)node;
				declaredSymbolInfo = new DeclaredSymbolInfo(node,
					enumMember.Identifier.ValueText,
					GetContainerDisplayName(node.Parent),
					GetFullyQualifiedContainerName(node.Parent),
					DeclaredSymbolInfoKind.EnumMember, enumMember.Identifier.Span);
				return true;
			case SyntaxKind.EventDeclaration:
				var eventDecl = (EventDeclarationSyntax)node;
				declaredSymbolInfo = new DeclaredSymbolInfo(node,
					ExpandExplicitInterfaceName(eventDecl.Identifier.ValueText, eventDecl.ExplicitInterfaceSpecifier),
					GetContainerDisplayName(node.Parent),
					GetFullyQualifiedContainerName(node.Parent),
					DeclaredSymbolInfoKind.Event, eventDecl.Identifier.Span);
				return true;
			case SyntaxKind.IndexerDeclaration:
				var indexerDecl = (IndexerDeclarationSyntax)node;
				declaredSymbolInfo = new DeclaredSymbolInfo(node,
					WellKnownMemberNames.Indexer,
					GetContainerDisplayName(node.Parent),
					GetFullyQualifiedContainerName(node.Parent),
					DeclaredSymbolInfoKind.Indexer, indexerDecl.ThisKeyword.Span);
				return true;
			case SyntaxKind.InterfaceDeclaration:
				var interfaceDecl = (InterfaceDeclarationSyntax)node;
				declaredSymbolInfo = new DeclaredSymbolInfo(node,
					interfaceDecl.Identifier.ValueText,
					GetContainerDisplayName(node.Parent),
					GetFullyQualifiedContainerName(node.Parent),
					DeclaredSymbolInfoKind.Interface, interfaceDecl.Identifier.Span);
				return true;
			case SyntaxKind.MethodDeclaration:
				var method = (MethodDeclarationSyntax)node;
				declaredSymbolInfo = new DeclaredSymbolInfo(node,
					ExpandExplicitInterfaceName(method.Identifier.ValueText, method.ExplicitInterfaceSpecifier),
					GetContainerDisplayName(node.Parent),
					GetFullyQualifiedContainerName(node.Parent),
					DeclaredSymbolInfoKind.Method,
					method.Identifier.Span,
					parameterCount: (ushort)(method.ParameterList?.Parameters.Count ?? 0),
					typeParameterCount: (ushort)(method.TypeParameterList?.Parameters.Count ?? 0));
				return true;
			case SyntaxKind.PropertyDeclaration:
				var property = (PropertyDeclarationSyntax)node;
				declaredSymbolInfo = new DeclaredSymbolInfo(node,
					ExpandExplicitInterfaceName(property.Identifier.ValueText, property.ExplicitInterfaceSpecifier),
					GetContainerDisplayName(node.Parent),
					GetFullyQualifiedContainerName(node.Parent),
					DeclaredSymbolInfoKind.Property, property.Identifier.Span);
				return true;
			case SyntaxKind.StructDeclaration:
				var structDecl = (StructDeclarationSyntax)node;
				declaredSymbolInfo = new DeclaredSymbolInfo(node,
					structDecl.Identifier.ValueText,
					GetContainerDisplayName(node.Parent),
					GetFullyQualifiedContainerName(node.Parent),
					DeclaredSymbolInfoKind.Struct, structDecl.Identifier.Span);
				return true;
			case SyntaxKind.VariableDeclarator:
				// could either be part of a field declaration or an event field declaration
				var variableDeclarator = (VariableDeclaratorSyntax)node;
				var variableDeclaration = variableDeclarator.Parent as VariableDeclarationSyntax;
				var fieldDeclaration = variableDeclaration?.Parent as BaseFieldDeclarationSyntax;
				if (fieldDeclaration != null)
				{
					var kind = fieldDeclaration is EventFieldDeclarationSyntax
						? DeclaredSymbolInfoKind.Event
						: fieldDeclaration.Modifiers.Any(m => m.Kind() == SyntaxKind.ConstKeyword)
						? DeclaredSymbolInfoKind.Constant
						: DeclaredSymbolInfoKind.Field;

					declaredSymbolInfo = new DeclaredSymbolInfo(node,
						variableDeclarator.Identifier.ValueText,
						GetContainerDisplayName(fieldDeclaration.Parent),
						GetFullyQualifiedContainerName(fieldDeclaration.Parent),
						kind, variableDeclarator.Identifier.Span);
					return true;
				}

				break;
			}

			declaredSymbolInfo = default(DeclaredSymbolInfo);
			return false;
		}

		public static bool DescentIntoSymbolForDeclarationSearch(SyntaxNode node)
		{
			var b = !(node is BlockSyntax);
			return b;
		}

		private static string ExpandExplicitInterfaceName(string identifier, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier)
		{
			if (explicitInterfaceSpecifier == null)
			{
				return identifier;
			}
			else
			{
				var builder = new StringBuilder();
				ExpandTypeName(explicitInterfaceSpecifier.Name, builder);
				builder.Append('.');
				builder.Append(identifier);
				return builder.ToString();
			}
		}

		private static void ExpandTypeName(TypeSyntax type, StringBuilder builder)
		{
			switch (type.Kind())
			{
			case SyntaxKind.AliasQualifiedName:
				var alias = (AliasQualifiedNameSyntax)type;
				builder.Append(alias.Alias.Identifier.ValueText);
				break;
			case SyntaxKind.ArrayType:
				var array = (ArrayTypeSyntax)type;
				ExpandTypeName(array.ElementType, builder);
				for (int i = 0; i < array.RankSpecifiers.Count; i++)
				{
					var rankSpecifier = array.RankSpecifiers[i];
					builder.Append(rankSpecifier.OpenBracketToken.Text);
					for (int j = 1; j < rankSpecifier.Sizes.Count; j++)
					{
						builder.Append(',');
					}

					builder.Append(rankSpecifier.CloseBracketToken.Text);
				}

				break;
			case SyntaxKind.GenericName:
				var generic = (GenericNameSyntax)type;
				builder.Append(generic.Identifier.ValueText);
				if (generic.TypeArgumentList != null)
				{
					var arguments = generic.TypeArgumentList.Arguments;
					builder.Append(generic.TypeArgumentList.LessThanToken.Text);
					for (int i = 0; i < arguments.Count; i++)
					{
						if (i != 0)
						{
							builder.Append(',');
						}

						ExpandTypeName(arguments[i], builder);
					}

					builder.Append(generic.TypeArgumentList.GreaterThanToken.Text);
				}

				break;
			case SyntaxKind.IdentifierName:
				var identifierName = (IdentifierNameSyntax)type;
				builder.Append(identifierName.Identifier.ValueText);
				break;
			case SyntaxKind.NullableType:
				var nullable = (NullableTypeSyntax)type;
				ExpandTypeName(nullable.ElementType, builder);
				builder.Append(nullable.QuestionToken.Text);
				break;
			case SyntaxKind.OmittedTypeArgument:
				// do nothing since it was omitted, but don't reach the default block
				break;
			case SyntaxKind.PointerType:
				var pointer = (PointerTypeSyntax)type;
				ExpandTypeName(pointer.ElementType, builder);
				builder.Append(pointer.AsteriskToken.Text);
				break;
			case SyntaxKind.PredefinedType:
				var predefined = (PredefinedTypeSyntax)type;
				builder.Append(predefined.Keyword.Text);
				break;
			case SyntaxKind.QualifiedName:
				var qualified = (QualifiedNameSyntax)type;
				ExpandTypeName(qualified.Left, builder);
				builder.Append(qualified.DotToken.Text);
				ExpandTypeName(qualified.Right, builder);
				break;
			default:
				Debug.Assert(false, "Unexpected type syntax " + type.Kind());
				break;
			}
		}

		private static string GetContainerDisplayName(SyntaxNode node)
		{
			return GetContainer(node, immediate: true);
		}

		private static string GetFullyQualifiedContainerName(SyntaxNode node)
		{
			return GetContainer(node, immediate: false);
		}

		private static string GetContainer(SyntaxNode node, bool immediate)
		{
			var name = GetNodeName(node, includeTypeParameters: immediate);
			var names = new List<string> { name };

			// check for nested classes and always add that to the container name.
			var parent = node.Parent;
			while (parent is TypeDeclarationSyntax)
			{
				var currentParent = (TypeDeclarationSyntax)parent;
				names.Add(currentParent.Identifier.ValueText + (immediate ? ExpandTypeParameterList(currentParent.TypeParameterList) : ""));
				parent = currentParent.Parent;
			}

			// If they're just asking for the immediate parent, then we're done. Otherwise keep 
			// walking all the way to the root, adding the names.
			if (!immediate)
			{
				while (parent != null && parent.Kind() != SyntaxKind.CompilationUnit)
				{
					names.Add(GetNodeName(parent, includeTypeParameters: false));
					parent = parent.Parent;
				}
			}

			names.Reverse();
			return string.Join(".", names);
		}

		private static string GetNodeName(SyntaxNode node, bool includeTypeParameters)
		{
			string name;
			TypeParameterListSyntax typeParameterList;
			switch (node.Kind())
			{
			case SyntaxKind.ClassDeclaration:
				var classDecl = (ClassDeclarationSyntax)node;
				name = classDecl.Identifier.ValueText;
				typeParameterList = classDecl.TypeParameterList;
				break;
			case SyntaxKind.CompilationUnit:
				return string.Empty;
			case SyntaxKind.DelegateDeclaration:
				var delegateDecl = (DelegateDeclarationSyntax)node;
				name = delegateDecl.Identifier.ValueText;
				typeParameterList = delegateDecl.TypeParameterList;
				break;
			case SyntaxKind.EnumDeclaration:
				return ((EnumDeclarationSyntax)node).Identifier.ValueText;
			case SyntaxKind.IdentifierName:
				return ((IdentifierNameSyntax)node).Identifier.ValueText;
			case SyntaxKind.InterfaceDeclaration:
				var interfaceDecl = (InterfaceDeclarationSyntax)node;
				name = interfaceDecl.Identifier.ValueText;
				typeParameterList = interfaceDecl.TypeParameterList;
				break;
			case SyntaxKind.MethodDeclaration:
				var methodDecl = (MethodDeclarationSyntax)node;
				name = methodDecl.Identifier.ValueText;
				typeParameterList = methodDecl.TypeParameterList;
				break;
			case SyntaxKind.NamespaceDeclaration:
				return GetNodeName(((NamespaceDeclarationSyntax)node).Name, includeTypeParameters: false);
			case SyntaxKind.QualifiedName:
				var qualified = (QualifiedNameSyntax)node;
				return GetNodeName(qualified.Left, includeTypeParameters: false) + "." + GetNodeName(qualified.Right, includeTypeParameters: false);
			case SyntaxKind.StructDeclaration:
				var structDecl = (StructDeclarationSyntax)node;
				name = structDecl.Identifier.ValueText;
				typeParameterList = structDecl.TypeParameterList;
				break;
			default:
				Debug.Assert(false, "Unexpected node type " + node.Kind());
				return null;
			}

			return name + (includeTypeParameters ? ExpandTypeParameterList(typeParameterList) : "");
		}

		private static string ExpandTypeParameterList(TypeParameterListSyntax typeParameterList)
		{
			if (typeParameterList != null && typeParameterList.Parameters.Count > 0)
			{
				var builder = new StringBuilder();
				builder.Append('<');
				builder.Append(typeParameterList.Parameters[0].Identifier.ValueText);
				for (int i = 1; i < typeParameterList.Parameters.Count; i++)
				{
					builder.Append(',');
					builder.Append(typeParameterList.Parameters[i].Identifier.ValueText);
				}

				builder.Append('>');
				return builder.ToString();
			}
			else
			{
				return null;
			}
		}
//
//		public List<SyntaxNode> GetMethodLevelMembers(SyntaxNode root)
//		{
//			var list = new List<SyntaxNode>();
//			AppendMethodLevelMembers(root, list);
//			return list;
//		}
//
//		private void AppendMethodLevelMembers(SyntaxNode node, List<SyntaxNode> list)
//		{
//			foreach (var member in node.GetMembers())
//			{
//				if (IsTopLevelNodeWithMembers(member))
//				{
//					AppendMethodLevelMembers(member, list);
//					continue;
//				}
//
//				if (IsMethodLevelMember(member))
//				{
//					list.Add(member);
//				}
//			}
//		}
//
//		public TextSpan GetMemberBodySpanForSpeculativeBinding(SyntaxNode node)
//		{
//			if (node.Span.IsEmpty)
//			{
//				return default(TextSpan);
//			}
//
//			var member = GetContainingMemberDeclaration(node, node.SpanStart);
//			if (member == null)
//			{
//				return default(TextSpan);
//			}
//
//			// TODO: currently we only support method for now
//			var method = member as BaseMethodDeclarationSyntax;
//			if (method != null)
//			{
//				if (method.Body == null)
//				{
//					return default(TextSpan);
//				}
//
//				return GetBlockBodySpan(method.Body);
//			}
//
//			return default(TextSpan);
//		}
//
//		public bool ContainsInMemberBody(SyntaxNode node, TextSpan span)
//		{
//			var constructor = node as ConstructorDeclarationSyntax;
//			if (constructor != null)
//			{
//				return (constructor.Body != null && GetBlockBodySpan(constructor.Body).Contains(span)) ||
//					(constructor.Initializer != null && constructor.Initializer.Span.Contains(span));
//			}
//
//			var method = node as BaseMethodDeclarationSyntax;
//			if (method != null)
//			{
//				return method.Body != null && GetBlockBodySpan(method.Body).Contains(span);
//			}
//
//			var property = node as BasePropertyDeclarationSyntax;
//			if (property != null)
//			{
//				return property.AccessorList != null && property.AccessorList.Span.Contains(span);
//			}
//
//			var @enum = node as EnumMemberDeclarationSyntax;
//			if (@enum != null)
//			{
//				return @enum.EqualsValue != null && @enum.EqualsValue.Span.Contains(span);
//			}
//
//			var field = node as BaseFieldDeclarationSyntax;
//			if (field != null)
//			{
//				return field.Declaration != null && field.Declaration.Span.Contains(span);
//			}
//
//			return false;
//		}
//
//		private TextSpan GetBlockBodySpan(BlockSyntax body)
//		{
//			return TextSpan.FromBounds(body.OpenBraceToken.Span.End, body.CloseBraceToken.SpanStart);
//		}
//
//		public int GetMethodLevelMemberId(SyntaxNode root, SyntaxNode node)
//		{
//			Contract.Requires(root.SyntaxTree == node.SyntaxTree);
//
//			int currentId = 0;
//			SyntaxNode currentNode;
//			Contract.ThrowIfFalse(TryGetMethodLevelMember(root, (n, i) => n == node, ref currentId, out currentNode));
//
//			Contract.ThrowIfFalse(currentId >= 0);
//			CheckMemberId(root, node, currentId);
//			return currentId;
//		}
//
//		public SyntaxNode GetMethodLevelMember(SyntaxNode root, int memberId)
//		{
//			int currentId = 0;
//			SyntaxNode currentNode;
//			if (!TryGetMethodLevelMember(root, (n, i) => i == memberId, ref currentId, out currentNode))
//			{
//				return null;
//			}
//
//			Contract.ThrowIfNull(currentNode);
//			CheckMemberId(root, currentNode, memberId);
//			return currentNode;
//		}
//
//		private bool TryGetMethodLevelMember(
//			SyntaxNode node, Func<SyntaxNode, int, bool> predicate, ref int currentId, out SyntaxNode currentNode)
//		{
//			foreach (var member in node.GetMembers())
//			{
//				if (IsTopLevelNodeWithMembers(member))
//				{
//					if (TryGetMethodLevelMember(member, predicate, ref currentId, out currentNode))
//					{
//						return true;
//					}
//
//					continue;
//				}
//
//				if (IsMethodLevelMember(member))
//				{
//					if (predicate(member, currentId))
//					{
//						currentNode = member;
//						return true;
//					}
//
//					currentId++;
//				}
//			}
//
//			currentNode = null;
//			return false;
//		}
//
//		[Conditional("DEBUG")]
//		private void CheckMemberId(SyntaxNode root, SyntaxNode node, int memberId)
//		{
//			var list = GetMethodLevelMembers(root);
//			var index = list.IndexOf(node);
//
//			Contract.ThrowIfFalse(index == memberId);
//		}
//
//		public SyntaxNode GetBindableParent(SyntaxToken token)
//		{
//			var node = token.Parent;
//			while (node != null)
//			{
//				var parent = node.Parent;
//
//				// If this node is on the left side of a member access expression, don't ascend 
//				// further or we'll end up binding to something else.
//				var memberAccess = parent as MemberAccessExpressionSyntax;
//				if (memberAccess != null)
//				{
//					if (memberAccess.Expression == node)
//					{
//						break;
//					}
//				}
//
//				// If this node is on the left side of a qualified name, don't ascend 
//				// further or we'll end up binding to something else.
//				var qualifiedName = parent as QualifiedNameSyntax;
//				if (qualifiedName != null)
//				{
//					if (qualifiedName.Left == node)
//					{
//						break;
//					}
//				}
//
//				// If this node is on the left side of a alias-qualified name, don't ascend 
//				// further or we'll end up binding to something else.
//				var aliasQualifiedName = parent as AliasQualifiedNameSyntax;
//				if (aliasQualifiedName != null)
//				{
//					if (aliasQualifiedName.Alias == node)
//					{
//						break;
//					}
//				}
//
//				// If this node is the type of an object creation expression, return the
//				// object creation expression.
//				var objectCreation = parent as ObjectCreationExpressionSyntax;
//				if (objectCreation != null)
//				{
//					if (objectCreation.Type == node)
//					{
//						node = parent;
//						break;
//					}
//				}
//
//				// If this node is not parented by a name, we're done.
//				var name = parent as NameSyntax;
//				if (name == null)
//				{
//					break;
//				}
//
//				node = parent;
//			}
//
//			return node;
//		}
//
//		public IEnumerable<SyntaxNode> GetConstructors(SyntaxNode root, CancellationToken cancellationToken)
//		{
//			var compilationUnit = root as CompilationUnitSyntax;
//			if (compilationUnit == null)
//			{
//				return SpecializedCollections.EmptyEnumerable<SyntaxNode>();
//			}
//
//			var constructors = new List<SyntaxNode>();
//			AppendConstructors(compilationUnit.Members, constructors, cancellationToken);
//			return constructors;
//		}
//
//		private void AppendConstructors(SyntaxList<MemberDeclarationSyntax> members, List<SyntaxNode> constructors, CancellationToken cancellationToken)
//		{
//			foreach (var member in members)
//			{
//				cancellationToken.ThrowIfCancellationRequested();
//
//				var constructor = member as ConstructorDeclarationSyntax;
//				if (constructor != null)
//				{
//					constructors.Add(constructor);
//					continue;
//				}
//
//				var @namespace = member as NamespaceDeclarationSyntax;
//				if (@namespace != null)
//				{
//					AppendConstructors(@namespace.Members, constructors, cancellationToken);
//				}
//
//				var @class = member as ClassDeclarationSyntax;
//				if (@class != null)
//				{
//					AppendConstructors(@class.Members, constructors, cancellationToken);
//				}
//
//				var @struct = member as StructDeclarationSyntax;
//				if (@struct != null)
//				{
//					AppendConstructors(@struct.Members, constructors, cancellationToken);
//				}
//			}
//		}
//
//		public bool TryGetCorrespondingOpenBrace(SyntaxToken token, out SyntaxToken openBrace)
//		{
//			if (token.Kind() == SyntaxKind.CloseBraceToken)
//			{
//				var tuple = token.Parent.GetBraces();
//
//				openBrace = tuple.Item1;
//				return openBrace.Kind() == SyntaxKind.OpenBraceToken;
//			}
//
//			openBrace = default(SyntaxToken);
//			return false;
//		}
	}
}
