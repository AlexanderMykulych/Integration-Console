using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProjectUtilsClassLibrary
{
	public class ReverseProjectBuilder
	{
		private string _inputFile;
		private string _outputFolder;
		private Dictionary<string, List<BaseTypeDeclarationSyntax>> _fileMapp = new Dictionary<string, List<BaseTypeDeclarationSyntax>>();
		private NamespaceDeclarationSyntax _namespaceDeclarationRoot;
		private IEnumerable<UsingDirectiveSyntax> _usings;

		public ReverseProjectBuilder(string inputFile, string outputFolder)
		{
			_inputFile = inputFile;
			_outputFolder = outputFolder;
		}

		public virtual void Run()
		{
			var fileContent = GetFileContent(_inputFile);
			if (fileContent != null)
			{
				var syntaxTree = CSharpSyntaxTree.ParseText(fileContent);
				var root = (CompilationUnitSyntax)syntaxTree.GetRoot();
				SaveUsings(root);
				if (root.Members[0] is NamespaceDeclarationSyntax namespaceDeclarationRoot)
				{
					SaveNamespaceDeclaration(namespaceDeclarationRoot);
					foreach (var member in namespaceDeclarationRoot.Members)
					{
						if (member is BaseTypeDeclarationSyntax classDeclaration)
						{
							ProcessClassDeclaration(classDeclaration);
						}
					}
				}
				SaveFiles();
			}
		}

		private void SaveNamespaceDeclaration(NamespaceDeclarationSyntax namespaceDeclarationRoot)
		{
			_namespaceDeclarationRoot = namespaceDeclarationRoot;
		}

		private void SaveUsings(CompilationUnitSyntax root)
		{
			_usings = root
				.ChildNodes()
				.OfType<UsingDirectiveSyntax>();
		}

		private void SaveFiles()
		{
			foreach (var mapp in _fileMapp)
			{
				var folderPath = CreateFolderIfNotExist(mapp.Key);
				var extension = Path.GetExtension(mapp.Key);
				if (folderPath != null)
				{
					mapp.Value.ForEach(classDeclar =>
					{
						var identToken = classDeclar.ChildNodesAndTokens().FirstOrDefault(token => token.Kind() == SyntaxKind.IdentifierToken);
						if (identToken != null)
						{
							var fileName = identToken.ToString() + $"{extension}";
							var resultPath = Path.Combine(folderPath, fileName);
							var tree = SyntaxFactory.CompilationUnit()
								.AddUsings(_usings.ToArray())
								.AddMembers(SyntaxFactory
									.NamespaceDeclaration(SyntaxFactory.IdentifierName($" {_namespaceDeclarationRoot.Name.ToString()}"))
									.AddMembers(ClearClassDeclaration(classDeclar))
								);
							File.WriteAllText(resultPath, tree.ToString());
						}
					});
				}
			}
		}

		private BaseTypeDeclarationSyntax ClearClassDeclaration(BaseTypeDeclarationSyntax classDeclar)
		{
			return classDeclar.WithoutLeadingTrivia().WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia("\n\t"));
		}

		private string CreateFolderIfNotExist(string filePath)
		{
			var path = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
				return path;
			}
			return path;
		}

		private void ProcessClassDeclaration(BaseTypeDeclarationSyntax classDeclaration)
		{
			var firstNodeOrToken = classDeclaration.ChildNodesAndTokens().First();
			SyntaxTriviaList triviaList;
			if (firstNodeOrToken.IsNode)
			{
				triviaList = firstNodeOrToken.AsNode().GetLeadingTrivia();
			}
			else
			{
				triviaList = firstNodeOrToken.AsToken().LeadingTrivia;
			}
			var trivia = triviaList.First(syntaxTrivia => syntaxTrivia.Kind() == SyntaxKind.MultiLineCommentTrivia);
			var relativeFilePath = GetRelativeFilePath(trivia.ToString());
			var absolutePath = GetAbsolutePath(relativeFilePath);
			Console.WriteLine(absolutePath);
			AddMappFile(absolutePath, classDeclaration);
		}

		private void AddMappFile(string absolutePath, BaseTypeDeclarationSyntax classDeclaration)
		{
			if (!_fileMapp.ContainsKey(absolutePath))
			{
				_fileMapp.Add(absolutePath, new List<BaseTypeDeclarationSyntax>()
				{
					classDeclaration
				});
				return;
			}
			_fileMapp[absolutePath].Add(classDeclaration);
		}

		private string GetAbsolutePath(string relativeFilePath)
		{
			return Path.GetFullPath(Path.Combine(_outputFolder, relativeFilePath));
		}

		private string GetRelativeFilePath(string toString)
		{
			var projectString = "Project Path: ";
			var extString = ".cs";
			var projectIndex = toString.IndexOf(projectString);
			if (projectIndex > -1)
			{
				projectIndex += projectString.Length;
				var escIndex = toString.IndexOf(extString, projectIndex);
				if (escIndex < -1)
				{
					escIndex = toString.Length;
				}
				else
				{
					escIndex += extString.Length;
				}
				var res = toString.Substring(projectIndex, escIndex - projectIndex);
				return res;
			}
			return null;
		}

		private string GetFileContent(string inputFile)
		{
			try
			{
				using (var reader = new StreamReader(new FileStream(inputFile, FileMode.Open, FileAccess.Read)))
				{
					return reader.ReadToEnd();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
		}
	}
}
