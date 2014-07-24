using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Mono.Cecil;
using Nova.Analysis;
using Nova.CodeDOM;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;

namespace TestProgramGenerator
{
	public partial class Form1 : Form
	{
		private static string _baseDirectory;
		private static string _executionDirectory;
		private SyntaxTree syntaxTree;

		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			string outputDir = "r:\\";
			//string className = GetClassName(textBox1.Text);
			string className = "Triangle";

			LoadSytanxTree(textBox1.Text); //将源代码解析为语法树
			textBox1.Clear();
			TraverseASTNode(syntaxTree);

			GenerateCSFile(textBox1.Text, outputDir + "\\" + className + ".cs");
			GenerateBuildConfig(className, outputDir);
			GenerateUnitTestFile(outputDir + "\\" + className + "test.cs");

			ExecuteNANT(outputDir);

			//string methodName = GetMethodNames(textBox1.Text)[0];
			textBox1.Text = Regex.Replace(textBox1.Text, "[\f\n\r\t\v]", "");
			textBox1.Text = Regex.Replace(textBox1.Text, " {2,}", " ");
			textBox1.Text = textBox1.Text.Replace(" {", "{");
			//codeText = sourceCode.Replace("\r", "").Replace("\n", "").Replace(" ","").ToLower();


			MessageBox.Show("输出成功");
		}

		private void ExecuteNANT(string outputDir)
		{
			string command = outputDir.Substring(0, 2) + "\n";
			command += "cd " + outputDir + "\n";
			command += "c:\\\n";
			command += "NANT.BAT";

			ExecuteCommand(command);
		}


		private string GetNodeName(AstNode node)
		{
			string className = "";





			bool hasProperties = false;
			bool isClassNode = false;

			foreach (PropertyInfo p in node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{

			}
			foreach (PropertyInfo p in node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (p.Name == "NodeType" || p.Name == "IsNull" || p.Name == "IsFrozen" || p.Name == "HasChildren")
				{
					continue;
				}
				if ((p.PropertyType == typeof(string) || p.PropertyType.IsEnum || p.PropertyType == typeof(bool)))
				{
					if (p.Name == "ClassType")
					{
						foreach (PropertyInfo p2 in node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
						{
							if (p2.Name == "NodeType" || p2.Name == "IsNull" || p2.Name == "IsFrozen" || p2.Name == "HasChildren")
							{
								continue;
							}
							if ((p2.PropertyType == typeof(string) || p2.PropertyType.IsEnum || p2.PropertyType == typeof(bool)))
							{
								if (p2.Name.ToUpper() == "NAME") //如果当前是Class节点
								{
									MessageBox.Show(p2.GetValue(node, null).ToString());

								}
							}
						}
					}

				
				}

			}


			return className;
		}

		private void TraverseASTNode(AstNode node)
		{
			AstNode childNode;

			childNode = node.FirstChild;

			string name = "";

			if (node != null)
			{
				name = GetNodeName(node);
				if (name != "")
				{
					textBox1.Text += name + Environment.NewLine;
				}
			}

			while (childNode != null)
			{
				TraverseASTNode(childNode);
				childNode = childNode.NextSibling;
			}
		}


		private List<string> GetMethodNames(string sourceCode)
		{
			string codeText = "";
			int startPos;
			int endPos;
			List<string> methodNames;

			codeText = Regex.Replace(sourceCode, "[\f\n\r\t\v]", "");
			//codeText = sourceCode.Replace("\r", "").Replace("\n", "").Replace(" ","").ToLower();
			textBox1.Text = codeText;

			return new List<string>();
			//startPos = codeText.IndexOf("class") + 5;
			//endPos = codeText.IndexOf("{", startPos);
			//className = codeText.Substring(startPos, endPos - startPos - 1);

			//return className;
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
					.Replace("$nunit_dll_path", Application.StartupPath + "\\nunit.framework.dll"); //获得NUnit dll的地址

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

		private void LoadSytanxTree(string sourceCode)
		{
			CSharpParser csParser = new CSharpParser();
			syntaxTree = csParser.Parse(sourceCode);
		}
	}
}