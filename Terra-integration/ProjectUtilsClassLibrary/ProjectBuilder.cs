using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MethodDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax;

namespace ProjectUtils
{
	public class ProjectBuilder
	{
		private string OutputPath;
		private string SourcePath;
		private static Dictionary<string, string> ReplacedRules = new Dictionary<string, string>() {
			{ "Terrasoft.TsConfiguration", "Terrasoft.Configuration" }
		};

		public ProjectBuilder(string sourcePath, string outputPath, bool metaProgramingEnabled = false)
		{
			SourcePath = sourcePath;
			OutputPath = outputPath;
			_metaProgramingEnabled = metaProgramingEnabled;
		}

		public List<string> Run()
		{
			DirSearch(SourcePath);
			foreach (var path in FilePath)
			{
				CreateTestCompilation(path);
			}
			return GenerateFiles();
		}

		private List<string> FilePath = new List<string>();
		public static Dictionary<string, List<NodeProjectInfo>> NamespaceText = new Dictionary<string, List<NodeProjectInfo>>();
		public static Dictionary<string, List<string>> Usings = new Dictionary<string, List<string>>();
		private bool _metaProgramingEnabled;

		private void DirSearch(string sDir)
		{
			try
			{
				foreach (string f in Directory.GetFiles(sDir, "*.cs"))
				{
					FilePath.Add(f);
				}
				foreach (string d in Directory.GetDirectories(sDir))
				{
					DirSearch(d);
				}
			}
			catch (System.Exception excpt)
			{
				Console.WriteLine(excpt.Message);
			}
		}

		private void CreateTestCompilation(string programPath)
		{
			string programText = File.ReadAllText(programPath);
			SyntaxTree programTree =
						   CSharpSyntaxTree.ParseText(programText)
										   .WithFilePath(programPath);
			var root = (CompilationUnitSyntax)programTree.GetRoot();
			if (root.Members.Count() == 0)
			{
				return;
			}

			var nameSpace = (root.Members[0] as NamespaceDeclarationSyntax).Name.ToFullString().Trim();
			var chiledNodes = root.Members[0].ChildNodes().ToList();
			var resultNodes = new List<NodeProjectInfo>();
			foreach (var node in chiledNodes)
			{
				if ((node is ClassDeclarationSyntax) || (node is InterfaceDeclarationSyntax) || (node is EnumDeclarationSyntax))
				{
					string type = "";
					string name = "";
					if (node is ClassDeclarationSyntax)
					{
						type = "Class";
						
						name = ((ClassDeclarationSyntax)node).Identifier.ValueText;
					}
					else if (node is InterfaceDeclarationSyntax)
					{
						type = "Interface";
						name = ((InterfaceDeclarationSyntax)node).Identifier.ValueText;
					}
					else if (node is EnumDeclarationSyntax)
					{
						type = "Enum";
						name = ((EnumDeclarationSyntax)node).Identifier.ValueText;
					}
					if (_metaProgramingEnabled && node is ClassDeclarationSyntax)
					{
						var genNode = ProcessMetaPrograming((ClassDeclarationSyntax) node);
						resultNodes.Add(new NodeProjectInfo()
						{
							Node = genNode,
							Path = programPath,
							Name = name,
							Type = type
						});
					}
					else
					{
						resultNodes.Add(new NodeProjectInfo()
						{
							Node = node,
							Path = programPath,
							Name = name,
							Type = type
						});
					}
					
				}
			}
			foreach (var usingName in root.Usings)
			{
				if (!Usings.ContainsKey(nameSpace))
				{
					Usings.Add(nameSpace, new List<string>());
				}
				var usingStr = usingName.ToFullString();
				if (!Usings[nameSpace].Contains(usingStr))
				{
					Usings[nameSpace].Add(usingStr);
				}
			}
			if (NamespaceText.ContainsKey(nameSpace))
			{
				NamespaceText[nameSpace].AddRange(resultNodes);
			}
			else
			{
				NamespaceText.Add(nameSpace, resultNodes);
			}
		}

		private ClassDeclarationSyntax ProcessMetaPrograming(ClassDeclarationSyntax classDeclaration)
		{
			var metaProcessor = new MetaProgrammingProcessor();

			var nodes = classDeclaration.ChildNodesAndTokens();
			var modifiedList = new List<System.Tuple<MethodDeclarationSyntax, List<MethodDeclarationSyntax>>>();
			foreach (SyntaxNodeOrToken nodeOrToken in nodes)
			{
				if (nodeOrToken.IsNode)
				{
					var node = nodeOrToken.AsNode();
					if (node is MethodDeclarationSyntax methodDeclaration)
					{
						var commentTrivia = node.GetLeadingTrivia().FirstOrDefault(x => x.Kind() == SyntaxKind.SingleLineCommentTrivia);
						var comment = commentTrivia.ToString();
						if (!string.IsNullOrEmpty(comment))
						{
							var res = metaProcessor.ProcessMethod(comment, methodDeclaration, classDeclaration);
							if (res.Count() > 1)
							{
								modifiedList.Add(new Tuple<MethodDeclarationSyntax, List<MethodDeclarationSyntax>>(methodDeclaration, res));
							}
						}
					}
				}
			}
			return ProcessModifiedNodes(classDeclaration, modifiedList);
		}

		private ClassDeclarationSyntax ProcessModifiedNodes(ClassDeclarationSyntax classDeclaration, List<Tuple<MethodDeclarationSyntax, List<MethodDeclarationSyntax>>> modifiedList)
		{
			if (modifiedList.Any())
			{
				classDeclaration =
					classDeclaration.RemoveNodes(modifiedList.Select(x => x.Item1), SyntaxRemoveOptions.KeepNoTrivia);
				classDeclaration.Members.AddRange(modifiedList.SelectMany(x => x.Item2));
				return SyntaxFactory.ClassDeclaration(
					classDeclaration.AttributeLists,
					classDeclaration.Modifiers,
					classDeclaration.Keyword,
					classDeclaration.Identifier,
					classDeclaration.TypeParameterList,
					classDeclaration.BaseList,
					classDeclaration.ConstraintClauses,
					classDeclaration.OpenBraceToken,
					classDeclaration.Members.AddRange(modifiedList.SelectMany(x => x.Item2).ToList()),
					classDeclaration.CloseBraceToken,
					classDeclaration.SemicolonToken);
			}
			return classDeclaration;
		}

		private ClassDeclarationSyntax CreateClasClone(ClassDeclarationSyntax classDeclaration)
		{
			return SyntaxFactory.ClassDeclaration(
				classDeclaration.AttributeLists,
				classDeclaration.Modifiers,
				classDeclaration.Identifier,
				classDeclaration.TypeParameterList,
				classDeclaration.BaseList,
				classDeclaration.ConstraintClauses,
				classDeclaration.Members
			);
		}

		private List<string> GenerateFiles()
		{
			var filePaths = new List<string>();
			foreach (var dictKeyValue in NamespaceText)
			{
				var namespaceName = dictKeyValue.Key;
				var usings = GetUsingsByNamespace(namespaceName);
				var strBuilder = new StringBuilder();
				foreach (var node in dictKeyValue.Value.OrderBy(x => x.Type))
				{
					strBuilder.AppendFormat("\n\n\t#region {3}: {0}\n\t/*\n\t\tProject Path: {2}\n\t\t\n\t*/\n{1}\n\t#endregion\n", node.Name, node.Node.ToFullString(), node.Path, node.Type);
				}
				string fullText = string.Format("{4}\nnamespace {0} {2}{1}\n{3}", namespaceName, strBuilder.ToString(), "{", "}", usings);
				string fileName = string.Format("namespace_{0}.cs", namespaceName);
				foreach (char c in System.IO.Path.GetInvalidFileNameChars())
				{
					fileName = fileName.Replace(c, '_');
				}
				var path = string.Format(@"{0}\{1}", OutputPath, fileName);
				foreach (var rule in ReplacedRules)
				{
					fullText = fullText.Replace(rule.Key, rule.Value);
				}
				File.WriteAllText(path, fullText);
				filePaths.Add(path);
			}
			return filePaths;
		}

		private string GetUsingsByNamespace(string nameSpace)
		{
			if (Usings.ContainsKey(nameSpace))
			{
				var strBuilder = new StringBuilder();
				foreach (var Using in Usings[nameSpace].OrderBy(x => x))
				{
					strBuilder.AppendFormat("{0}", Using);
				}
				return strBuilder.ToString();
			}
			return "";
		}
	}
}
