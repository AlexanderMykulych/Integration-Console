using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using sf = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ProjectUtils
{
	public class MetaProgrammingProcessor
	{
		public MetaProgrammingProcessor()
		{
		}

		public List<MethodDeclarationSyntax> ProcessMethod(string comment, MethodDeclarationSyntax methodDeclaration, ClassDeclarationSyntax classDeclaration = null)
		{
			if (comment.TrimStart().StartsWith("//Log"))
			{
				return ProcessMethodLog(GetCommentParams(comment), methodDeclaration, classDeclaration);
			}
			return new List<MethodDeclarationSyntax>()
			{
				methodDeclaration
			};
		}

		private Dictionary<string, string> GetCommentParams(string comment)
		{
			var paramIndex = comment.IndexOf(" ", StringComparison.InvariantCulture);
			return comment
				.Substring(paramIndex + 1)
				.Trim()
				.Split(',')
				.Where(x => !string.IsNullOrEmpty(x))
				.Select(x => x
					.Split('=')
					.Where(y => !string.IsNullOrEmpty(y))
					.Take(2)
					.ToList()
				)
				.ToDictionary(x => x.First().Trim().ToLower(), x => x.Last().Trim());
		}

		private List<MethodDeclarationSyntax> ProcessMethodLog(Dictionary<string, string> commentParam, MethodDeclarationSyntax methodDeclaration, ClassDeclarationSyntax classDeclaration = null)
		{
			var logMethod = GetLogMethod(methodDeclaration, commentParam, classDeclaration);
			var bodyMethod = GetBodyMethod(methodDeclaration);
			var mainMethod = GetMainMethod(methodDeclaration, commentParam);
			return new List<MethodDeclarationSyntax>()
			{
				mainMethod,
				bodyMethod,
				logMethod
			};
			
		}

		private MethodDeclarationSyntax GetMainMethod(MethodDeclarationSyntax methodDeclaration, Dictionary<string, string> commentParam)
		{
			return sf.MethodDeclaration(
				methodDeclaration.AttributeLists,
				methodDeclaration.Modifiers,
				methodDeclaration.ReturnType,
				methodDeclaration.ExplicitInterfaceSpecifier,
				methodDeclaration.Identifier,
				methodDeclaration.TypeParameterList,
				methodDeclaration.ParameterList,
				methodDeclaration.ConstraintClauses,
				GetMainMethodBody(
					GetLogMethodName(methodDeclaration),
					GetBodyMethodName(methodDeclaration),
					GetArgumentsList(methodDeclaration.ParameterList),
					commentParam,
					methodDeclaration.ReturnType),
				methodDeclaration.SemicolonToken);
		}

		private MethodDeclarationSyntax GetBodyMethod(MethodDeclarationSyntax methodDeclaration)
		{
			return sf.MethodDeclaration(
				methodDeclaration.AttributeLists,
				PrepareMethodsModifiers(methodDeclaration.Modifiers),
				methodDeclaration.ReturnType,
				methodDeclaration.ExplicitInterfaceSpecifier,
				sf.Identifier(
					methodDeclaration.Identifier.LeadingTrivia,
					GetBodyMethodName(methodDeclaration),
					methodDeclaration.Identifier.TrailingTrivia
				),
				methodDeclaration.TypeParameterList,
				methodDeclaration.ParameterList,
				methodDeclaration.ConstraintClauses,
				methodDeclaration.Body,
				methodDeclaration.SemicolonToken);
		}

		private MethodDeclarationSyntax GetLogMethod(MethodDeclarationSyntax methodDeclaration, Dictionary<string, string> commentParam, ClassDeclarationSyntax classDeclaration = null)
		{
			return sf.MethodDeclaration(
				methodDeclaration.AttributeLists,
				PrepareMethodsModifiers(methodDeclaration.Modifiers),
				methodDeclaration.ReturnType,
				methodDeclaration.ExplicitInterfaceSpecifier,
				sf.Identifier(
					methodDeclaration.Identifier.
						LeadingTrivia,
					GetLogMethodName(methodDeclaration),
					methodDeclaration.Identifier.TrailingTrivia
				),
				methodDeclaration.TypeParameterList,
				methodDeclaration.ParameterList,
				methodDeclaration.ConstraintClauses,
				GetLogMethodTemplate(methodDeclaration.Identifier.Text, GetArgumentsList(methodDeclaration.ParameterList), commentParam, methodDeclaration.ReturnType, classDeclaration),
				methodDeclaration.SemicolonToken);
		}

		public SyntaxTokenList PrepareMethodsModifiers(SyntaxTokenList modifiers)
		{
			return sf.TokenList(modifiers
				.Where(x => x.Kind() != SyntaxKind.OverrideKeyword)
				.Select(x => x.Kind() == SyntaxKind.PublicKeyword ? sf.Token(SyntaxKind.ProtectedKeyword).WithTriviaFrom(x) : x));
		}
		private bool IsVoidType(TypeSyntax type)
		{
			return type.ToString() == "void";
		}

		private ArgumentListSyntax GetArgumentsList(ParameterListSyntax attributes)
		{

			return sf.ArgumentList(sf.SeparatedList(attributes.Parameters.Select(param =>
			{
				var refOrOutKeyword = param.ChildTokens()
					.FirstOrDefault(x => x.Kind() == SyntaxKind.RefKeyword || x.Kind() == SyntaxKind.OutKeyword);
				return sf.Argument(
					null,
					refOrOutKeyword,
					sf.IdentifierName(param.Identifier.Text)
				);
			}).ToArray()));
		}

		private BlockSyntax GetLogMethodTemplate(string methodName, ArgumentListSyntax args, Dictionary<string, string> commentParam, TypeSyntax returnType, ClassDeclarationSyntax classDeclaration = null)
		{
			string className = classDeclaration != null ? classDeclaration.Identifier.ToString() : "<Class>";
			var block = sf.Block()
				.WithOpenBraceToken(sf.Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(sf.CarriageReturnLineFeed).WithLeadingTrivia(sf.Tab, sf.Tab))
				.AddStatements(
					sf.ParseStatement("			Guid oldBlockId = default(Guid);\n"),
					sf.TryStatement(
						sf.Block()
							.WithOpenBraceToken(sf.Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(sf.ElasticCarriageReturnLineFeed).WithLeadingTrivia(sf.Whitespace(" ")))
							.AddStatements(
								sf.ParseStatement($"\t\t\t\toldBlockId = LoggerHelper.CreateBlock(\"Method - {className}.{methodName}\\n{GetLogValue("name", commentParam)}\", TLogObjectType.BlockMethod);").WithTrailingTrivia(sf.ElasticCarriageReturnLineFeed),
								sf.ParseStatement(GetArgsLogString(args, GetLogValue("key", commentParam))),
								WithLogReturn(
									sf.InvocationExpression(
										sf.IdentifierName(sf.Identifier(
											SyntaxTriviaList.Create(sf.Whitespace(" ")), $"{methodName}_Body_{classDeclaration.Identifier.Value}", SyntaxTriviaList.Empty
										)),
										args
									), !IsVoidType(returnType)
								).WithLeadingTrivia(sf.Tab, sf.Tab, sf.Tab, sf.Tab).WithTrailingTrivia(sf.CarriageReturnLineFeed),
								!IsVoidType(returnType) ?
									sf.ParseStatement($"\t\t\t\tIntegrationLogger.InfoReturn(\"{GetLogValue("key", commentParam)}\", result);" +
									                  "\n\t\t\t\treturn result;\n")
									: sf.ParseStatement("\t\t\t\treturn;\n")
							)
							.WithCloseBraceToken(sf.Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(sf.Tab, sf.Tab, sf.Tab)),
						sf.List(new List<CatchClauseSyntax>()
						{
							sf.CatchClause(
								sf.CatchDeclaration(sf.IdentifierName("Exception"), sf.Identifier("integrationError").WithLeadingTrivia(sf.Whitespace(" "))),
								null,
								sf.Block()
									.WithOpenBraceToken(sf.Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(sf.CarriageReturnLineFeed))
									.AddStatements(
										sf.ParseStatement("				IntegrationLogger.Error(integrationError);"),
										AddThrowNext(commentParam).WithLeadingTrivia(sf.CarriageReturnLineFeed, sf.Tab, sf.Tab, sf.Tab, sf.Tab))
									.WithCloseBraceToken(sf.Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(sf.CarriageReturnLineFeed, sf.Tab, sf.Tab, sf.Tab))
							)
						}),
						sf.FinallyClause(
							sf.Block()
								.WithOpenBraceToken(sf.Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(sf.CarriageReturnLineFeed))
								.AddStatements(sf.ParseStatement("				LoggerHelper.FinishTransaction(oldBlockId);"))
								.WithCloseBraceToken(sf.Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(sf.CarriageReturnLineFeed, sf.Tab, sf.Tab, sf.Tab).WithTrailingTrivia(sf.CarriageReturnLineFeed))
						)
					).WithLeadingTrivia(sf.Tab, sf.Tab, sf.Tab)
				);
			if (!IsVoidType(returnType))
			{
				block = block.AddStatements(ReturnDefault(returnType, GetLogValue("defreturn", commentParam)).WithLeadingTrivia(sf.Tab, sf.Tab, sf.Tab).WithTrailingTrivia(sf.CarriageReturnLineFeed));
			}
			return block.WithCloseBraceToken(sf.Token(SyntaxKind.CloseBraceToken).WithTrailingTrivia(sf.CarriageReturnLineFeed).WithLeadingTrivia(sf.Tab, sf.Tab));
		}

		private string GetArgsLogString(ArgumentListSyntax args, string logKey)
		{
			if (!args.Arguments.Any())
			{
				return $"\t\t\t\tIntegrationLogger.InfoArguments(\"{logKey}\", \"No Args\");\n";
			}
			var textFormat = "Args:\\n";
			var i = 0;
			foreach (var argument in args.Arguments)
			{
				textFormat += $"{argument.Expression}: {{{i++}}}\\n";
			}
			var textArgs = args.Arguments.Select(x => x.Expression.ToString()).Aggregate((x, y) => $"{x}, {y}");
			return $"\t\t\t\tIntegrationLogger.InfoArguments(\"{logKey}\", \"{textFormat}\", {textArgs});\n";
		}

		private StatementSyntax AddThrowNext(Dictionary<string, string> commentParam)
		{
			return commentParam.ContainsKey("nothrow") ? sf.ParseStatement("//No Throw Error") : sf.ParseStatement("throw;");
		}

		private ReturnStatementSyntax ReturnDefault(TypeSyntax returnType, string logParam)
		{
			return IsVoidType(returnType) ? null : sf.ReturnStatement(sf.ParseExpression(string.IsNullOrEmpty(logParam) ? $" default({returnType})" : $" {logParam}"));
		}
		private StatementSyntax WithLogReturn(ExpressionSyntax expr, bool withReturn)
		{
			return withReturn
				? sf.LocalDeclarationStatement(
					sf.VariableDeclaration(
						sf.IdentifierName("var "),
						sf.SeparatedList(new List<VariableDeclaratorSyntax>()
						{
							sf.VariableDeclarator(
								sf.Identifier("result "),
								null,
								sf.EqualsValueClause(
									expr
								)
							)
						})
					)
				)
				: (StatementSyntax)sf.ExpressionStatement(expr, sf.Token(SyntaxKind.SemicolonToken).WithTrailingTrivia(sf.CarriageReturnLineFeed));
		}
		private StatementSyntax WithReturn(ExpressionSyntax expr, bool withReturn)
		{
			return withReturn ?
				(StatementSyntax)sf.ReturnStatement(expr) :
				sf.ExpressionStatement(expr, sf.Token(SyntaxKind.SemicolonToken).WithTrailingTrivia(sf.CarriageReturnLineFeed));
		}

		private BlockSyntax GetMainMethodBody(string methodName, string elseMethodName, ArgumentListSyntax args, Dictionary<string, string> commentParam, TypeSyntax returnType)
		{
			return sf.Block()
				.WithOpenBraceToken(sf.Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(sf.CarriageReturnLineFeed).WithLeadingTrivia(sf.Tab, sf.Tab))
				.AddStatements(
					sf.IfStatement(
						sf.ParseExpression($"LoggerHelper.IsActive(\"{GetLogValue("key", commentParam)}\")"),
						sf.Block()
							.WithOpenBraceToken(sf.Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(sf.CarriageReturnLineFeed, sf.Tab))
							.AddStatements(WithReturn(
								sf.InvocationExpression(
									sf.IdentifierName(sf.Identifier(
										SyntaxTriviaList.Create(sf.Whitespace(" ")), methodName, SyntaxTriviaList.Empty
									)),
									args
								), !IsVoidType(returnType)).WithLeadingTrivia(sf.Tab, sf.Tab, sf.Tab).WithTrailingTrivia(sf.CarriageReturnLineFeed)
							)
							.WithCloseBraceToken(sf.Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(sf.Tab, sf.Tab, sf.Tab)),
						sf.ElseClause(
							sf.Block()
								.WithOpenBraceToken(sf.Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(sf.CarriageReturnLineFeed, sf.Tab))
								.AddStatements(WithReturn(
									sf.InvocationExpression(
										sf.IdentifierName(sf.Identifier(
											SyntaxTriviaList.Create(sf.Whitespace(" ")), elseMethodName, SyntaxTriviaList.Empty
										)),
										args
								), !IsVoidType(returnType)).WithLeadingTrivia(sf.Tab, sf.Tab, sf.Tab).WithTrailingTrivia(sf.CarriageReturnLineFeed))
								.WithCloseBraceToken(sf.Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(sf.Tab, sf.Tab, sf.Tab))
						)
					)
					.WithLeadingTrivia(sf.Tab, sf.Tab, sf.Tab)
					.WithTrailingTrivia(sf.CarriageReturnLineFeed)
				)
				.WithCloseBraceToken(sf.Token(SyntaxKind.CloseBraceToken).WithTrailingTrivia(sf.CarriageReturnLineFeed).WithLeadingTrivia(sf.Tab, sf.Tab));
		}

		public string GetLogMethodName(MethodDeclarationSyntax declar)
		{
			var parent = declar.Parent as ClassDeclarationSyntax;
			return $"{declar.Identifier.Text}_Log_{parent.Identifier.Value}";
		}
		public string GetBodyMethodName(MethodDeclarationSyntax declar)
		{
			var parent = declar.Parent as ClassDeclarationSyntax;
			return $"{declar.Identifier.Text}_Body_{parent.Identifier.Value}";
		}

		public string GetLogValue(string key, Dictionary<string, string> commentParam)
		{
			if (key == "key")
			{
				if (commentParam.ContainsKey(key))
				{
					return commentParam[key];
				}
				return commentParam["name"];
			}
			if (commentParam.ContainsKey(key))
			{
				return commentParam[key];
			}
			return string.Empty;
		}
	}
}