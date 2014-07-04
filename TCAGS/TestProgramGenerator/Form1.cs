using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Mono.Cecil;
using Nova.Analysis;
using Nova.CodeDOM;

namespace TestProgramGenerator
{
	public partial class Form1 : Form
	{
		private static string _baseDirectory;
		private static string _executionDirectory;

		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			string outputDir = "r:\\";
			string className = GetClassName(textBox1.Text);
			string command;

			GenerateCSFile(textBox1.Text, outputDir + "\\" + className + ".cs");
			GenerateBuildConfig(className, outputDir);
			GenerateUnitTestFile(outputDir + "\\" + className + "test.cs");

			command = outputDir.Substring(0, 2) + "\n";
			command += "cd " + outputDir + "\n";
			command += "c:\\\n";
			command += "NANT.BAT";

			ExecuteCommand(command);


			MessageBox.Show("输出成功");
		}

		private string GetClassName(string sourceCode)
		{
			string codeText = "";
			int startPos;
			int endPos;
			string className;

			codeText = sourceCode.Replace(" ", "").Replace("\r", "").Replace("\n", "").ToLower();
			startPos = codeText.IndexOf("class") + 5;
			endPos = codeText.IndexOf("{", startPos);
			className = codeText.Substring(startPos, endPos - startPos - 1);

			return className;
		}

		private void GenerateBuildConfig(string className, string outputDir)
		{
			string originalConfigText;
			string newConfigText;
			string templatePath;
			string targetPath;


			templatePath = Application.StartupPath + "\\default_build_template.xml";
			targetPath = outputDir + "\\default.build";

			XmlDocument doc = new XmlDocument();

			try
			{
				doc.Load(templatePath);
				originalConfigText = doc.OuterXml;

				newConfigText = originalConfigText.Replace("$build_dir", outputDir)
					.Replace("$source_dll_name", className + ".dll")
					.Replace("$source_cs_name", className + ".cs")
					.Replace("$test_dll_name", className + "test.dll")
					.Replace("$test_cs_name", className + "test.cs")
					.Replace("$report_output_dir", outputDir)
					.Replace("$nunit_dll_path", Application.StartupPath + "\\nunit.framework.dll");//获得NUnit dll的地址

				doc.LoadXml(newConfigText);
				doc.Save(targetPath);
			}
			catch (Exception ex)
			{

				throw new Exception(ex.Message);
			}

		}

		public void GenerateCSFile(string sourceCode, string targetFilePath)
		{
			try
			{
				StreamWriter streamWriter = new StreamWriter(targetFilePath, false, new UTF8Encoding());
				streamWriter.Write(sourceCode);
				streamWriter.Close();
			}
			catch (Exception ex)
			{

				throw new Exception(ex.Message);
			}

		}

		public void GenerateUnitTestFile(string targetPath)
		{
			string templatePath;
			string fileContent;

			templatePath = Application.StartupPath + "\\unit_test_template.txt";
			try
			{
				StreamReader streamReader = new StreamReader(templatePath, new UTF8Encoding());
				fileContent = streamReader.ReadToEnd();
				streamReader.Close();

				StreamWriter streamWriter = new StreamWriter(targetPath, false, new UTF8Encoding());
				streamWriter.Write(fileContent);
				streamWriter.Close();
			}
			catch (Exception ex)
			{

				throw new Exception(ex.Message);
			}

		}


		private void ExecuteCommand(string command)
		{
			Process p = new Process();
			p.StartInfo.FileName = "cmd.exe";

			string[] commands = command.Split('\n');

			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardInput = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.CreateNoWindow = true;

			p.Start();

			foreach (string cmd in commands)
			{
				p.StandardInput.WriteLine(cmd);
			}
			p.StandardInput.WriteLine("exit");
			p.WaitForExit();

			p.Close();
		}

		private void LoadCode()
		{

		}

		public static void LoadCodeUnits(string sourceCode)
		{
			// To load one or more CodeUnits with external references resolved, create a 'dummy' parent Project for the CodeUnits,
			// and add any desired assembly references to it.
			//Project project = new Project("Miscellaneous Files", null);
			//CodeUnit codeUnit1 = project.CreateCodeUnit("Program.cs");  // Add files like this
			//CodeUnit codeUnit2 = project.CreateCodeUnit("MyVisitor.cs");
			//const string fragment = "using System; Console.WriteLine(\"TEST\");";
			//CodeUnit codeUnit3 = project.CreateCodeUnit(fragment, "Fragment");  // Add code fragments like this

			// We want external references resolved in this case, so add assembly references and load their types
			//project.AddDefaultAssemblyReferences();  // Add commonly used assembly references
			//// Add additional assembly references like this, with an optional "hint path":
			//project.AddAssemblyReference("Nova.CodeDOM", GetFilePath("Nova.CodeDOM.dll"));
			//// You may also set 'project.ReferencePath' to a string of semi-colon separated search paths.

			//project.LoadReferencedAssembliesAndTypes();  // Load the referenced assemblies, and their public types
			//project.ParseAndResolveCodeUnits();          // Parse and resolve all code units in the project

			// In this example, there is no Solution, so we can't benefit from it's single CodeAnnotations collection
			// of all messages for all source files.  We could add a dummy Solution to the Project to get this, or look
			// at the ListedAnnotations collection on each CodeUnit.  In this case, we'll just call a helper method on
			// each CodeUnit that displays message counts along with any errors or warnings.
			//codeUnit1.LogMessageCounts(true);
			//codeUnit2.LogMessageCounts(true);
			//codeUnit3.LogMessageCounts(true);

			Project project = new Project("tempProjectForTest", null);
			string fragment = sourceCode;
			CodeUnit codeUnit3 = project.CreateCodeUnit(fragment, "Fragment");  // Add code fragments like this
			project.AddDefaultAssemblyReferences();
			project.AddAssemblyReference("Nova.CodeDOM", GetFilePath("Nova.CodeDOM.dll"));
			project.LoadReferencedAssembliesAndTypes();  // Load the referenced assemblies, and their public types
			project.ParseAndResolveCodeUnits();          // Parse and resolve all code units in the project

			Namespace namespace_Examples = project.FindNamespace("Nova.Examples");

			if (namespace_Examples != null)
			{
				var findMethodDecls = new FindByType(typeof(MethodDecl), project);
				findMethodDecls.Find();
			}

		}

		private static string GetFilePath(string name)
		{
			string filePath = _executionDirectory + @"\" + name;
			if (!File.Exists(filePath))
				filePath = _baseDirectory + @"\" + name;
			return filePath;
		}

		#region /* FINDING CODE OBJECTS EXAMPLES */

		/// <summary>
		/// Finding code objects in a particular scope.
		/// </summary>
		//public static void FindCodeObjects()
		//{
		//	// Various 'Find' methods are provided on various CodeDOM types to find immediate child code objects at a particular
		//	// point in a code tree, examples of which are shown below.  Global finds or searches are addressed in later examples.

		//	Solution solution = Solution.Load("Nova.Examples.sln");
		//	if (solution != null)
		//	{
		//		// Find a Project in a Solution by name
		//		Project project = solution.FindProject("Nova.Examples");
		//		Log.WriteLine(project != null ? "Found " + project : "ERROR - Project not found");
		//		if (project != null)
		//		{
		//			// Find a CodeUnit in a Project by name
		//			CodeUnit codeUnit = project.FindCodeUnit("Program.cs");
		//			Log.WriteLine(codeUnit != null ? "Found " + codeUnit : "ERROR - CodeUnit not found");

		//			// Find a Namespace in a Project by full name
		//			Namespace namespace_Examples = project.FindNamespace("Nova.Examples");
		//			Log.WriteLine(namespace_Examples != null ? "Found " + namespace_Examples : "ERROR - Namespace not found");
		//			if (namespace_Examples != null)
		//			{
		//				// Find a type in a Project as a TypeRef (to a TypeDecl or TypeDefinition or Type) when
		//				// you don't know if the type is external or not.
		//				var typeRef_Console = project.FindRef("System.Console") as TypeRef;  // Treat UnresolvedRef as null
		//				Log.WriteLine(typeRef_Console != null ? "Found " + typeRef_Console : "ERROR - TypeRef not found");

		//				// Find a TypeDecl in a Project when you know the type is local (as opposed to external).
		//				var typeDecl_Program1 = project.Find("Nova.Examples.Program") as TypeDecl;
		//				Log.WriteLine(typeDecl_Program1 != null ? "Found " + typeDecl_Program1 : "ERROR - TypeDecl not found");

		//				// Find a TypeDefinition (if using Mono Cecil - the default) or a Type (if using Reflection) in a Project
		//				// when you know the type is external.
		//				if (ApplicationContext.UseMonoCecilLoads)
		//				{
		//					var typeDefinition = project.Find("System.Console") as TypeDefinition;
		//					Log.WriteLine(typeDefinition != null ? "Found TypeDefinition: " + typeDefinition : "ERROR - TypeDefinition not found");
		//				}
		//				else
		//				{
		//					var type = project.Find("System.Console") as Type;
		//					Log.WriteLine(type != null ? "Found Type: " + type : "ERROR - Type not found");
		//				}

		//				// Find a type in a Namespace as a TypeRef
		//				var typeRef_Program = TypeRef.Find(namespace_Examples, "Program") as TypeRef;
		//				Log.WriteLine(typeRef_Program != null ? "Found " + typeRef_Program : "ERROR - TypeRef not found");

		//				// Find a TypeDecl in a Namespace
		//				var typeDecl_Program = namespace_Examples.Find("Program") as TypeDecl;
		//				Log.WriteLine(typeDecl_Program != null ? "Found " + typeDecl_Program : "ERROR - TypeDecl not found");
		//				if (typeDecl_Program != null)
		//				{
		//					// Find a MethodDecl in a TypeDecl by name
		//					var methodDecl = typeDecl_Program.FindFirst<MethodDecl>("FindCodeObjects");
		//					Log.WriteLine(methodDecl != null ? "Found " + methodDecl : "ERROR - MethodDecl not found");
		//					if (methodDecl != null)
		//					{
		//						// Find a LocalDecl in a MethodDecl by name
		//						var localDecl = methodDecl.FindFirst<LocalDecl>("solution");
		//						Log.WriteLine(localDecl != null ? "Found " + localDecl : "ERROR - LocalDecl not found");
		//						if (localDecl != null)
		//						{
		//							// Find the Parent MethodDecl of a code object
		//							var parentMethod = localDecl.FindParentMethod();
		//							Log.WriteLine(parentMethod != null ? "Found " + parentMethod : "ERROR - parent Method not found");

		//							// Find the Parent NamespaceDecl of a code object
		//							var parentNamespaceDecl = localDecl.FindParent<NamespaceDecl>();
		//							Log.WriteLine(parentNamespaceDecl != null ? "Found " + parentNamespaceDecl : "ERROR - parent NamespaceDecl not found");
		//						}

		//						// Find the first 'if' statement in a MethodDecl
		//						var @if = methodDecl.FindFirst<If>();
		//						Log.WriteLine(@if != null ? "Found " + @if : "ERROR - If statement not found");
		//						if (@if != null)
		//						{
		//							// Find all child 'if' statements in the body of a parent 'if' statement
		//							foreach (var statement in @if.Body)
		//							{
		//								if (statement is If)
		//									Log.WriteLine("Found child " + statement);
		//							}
		//						}
		//					}
		//				}
		//			}
		//		}
		//	}

			// Helper methods are provided on various SymbolicRef types to find types when manually generating code
			// which conveniently return an UnresolvedRef instead of null if the item isn't found.  They can also
			// be passed an UnresolvedRef as input, so the output of one call can be passed as the input of another
			// without any checks for errors.  For examples, see ManualTests.GenerateFullTest().
			// These methods include the following signature patterns:
			//
			// Get a reference to a type or namespace in the current project instance:
			//     SymbolicRef Project.FindRef(string name)
			//
			// Find a type in a namespace or parent type, where NorT is a Namespace or Type/TypeDecl, or an Alias to one
			//     SymbolicRef TypeRef.Find(NorT type, string name)
			//
			// Find a member of a type, where TYPE is a Type, TypeDecl, TypeRefBase, or type Alias:
			//     SymbolicRef MethodRef.Find(TYPE type, string name)
			//     SymbolicRef ConstructorRef.Find(TYPE type, params TypeRefBase[] parameterTypes)
			//     SymbolicRef PropertyRef.Find(TYPE type, string name)
			//     SymbolicRef FieldRef.Find(TYPE type, string name)
			//
			// Find a parameter of a method, where METHOD is a MethodInfo or MethodDeclBase
			//     SymbolicRef ParameterRef.Find(METHOD method, string name)
		}

		/// <summary>
		/// Find all code objects of a certain type.
		/// </summary>
		public static void FindByType()
		{
			Solution solution = Solution.Load("Nova.Examples.sln");
			if (solution != null)
			{
				// Find all MethodDecls in the solution
				var findMethodDecls = new FindByType(typeof(MethodDecl), solution);
				findMethodDecls.Find();
				Log.WriteLine("Found " + findMethodDecls.Results.Count + " MethodDecls in the solution");

				Project project = solution.FindProject("Nova.Examples");
				if (project != null)
				{
					// Find all ClassDecls in the project
					var findClassDecls = new FindByType(typeof(ClassDecl), project);
					findClassDecls.Find();
					Log.WriteLine("Found " + findClassDecls.Results.Count + " ClassDecls in the project");

					// Find all If statements in Program.cs
					CodeUnit programFile = project.FindCodeUnit("Program.cs");
					var findIfs = new FindByType(typeof(If), programFile);
					findIfs.Find();
					Log.WriteLine("Found " + findIfs.Results.Count + " 'if' statements in " + programFile.Name);
				}
			}
		}

		/// <summary>
		/// Find code objects by text string or regular expression.
		/// </summary>
		public static void FindByText()
		{
			Solution solution = Solution.Load("Nova.Examples.sln");
			if (solution != null)
			{
				// Find all code objects in the solution with the text 'Find' in their name or content
				// (match case, but don't require it to be a whole word).
				var findText1 = new FindByText("Find", solution, true, false);
				findText1.Find();
				Log.WriteLine("Found " + findText1.Results.Count + " objects containing the text 'Find' in the solution");

				// Find all code objects in the solution that have a name of 'findText1' OR 'findText2', or contain
				// such a name in their text content (using regular expressions).  Match case and whole words only.
				var findText2 = new FindByText("findText1|findText2", solution, true, true, true);
				findText2.Find();
				Log.WriteLine("Found " + findText2.Results.Count + " objects containing the text 'findText1' or 'findText2' in the solution");

				// Note that the scope of FindByText can be any sub-tree, and in addition to the options to MatchCase, MatchWholeWord,
				// and UseRegularExpressions, there are result filtering options to MatchDeclarations, MatchReferences, MatchLiterals,
				// MatchComments, and MatchMessages.
			}
		}

		#endregion



	}
}
