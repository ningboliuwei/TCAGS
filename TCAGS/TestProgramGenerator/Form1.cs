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
using Mono.CSharp;
using Nova.Analysis;
using Nova.CodeDOM;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using CSharpParser = ICSharpCode.NRefactory.CSharp.CSharpParser;

namespace TestProgramGenerator
{
	public partial class Form1 : Form
	{
		private static string _baseDirectory;
		private static string _executionDirectory;
		private SyntaxTree syntaxTree;
		private string result = string.Empty; //用于遍历函数

		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			string outputDir = "r:\\";
			string className = string.Empty;
			string methodName = string.Empty;
			string namespaceName = string.Empty;

			LoadSytanxTree(textBox1.Text); //将源代码解析为语法树
			FindNodeName(syntaxTree, "CLASS"); //得到被测的类名
			className = result;

			result = string.Empty;
			FindNodeName(syntaxTree, "METHOD"); //得到被测类中的方法
			methodName = result;

			result = string.Empty;
			FindNodeName(syntaxTree, "NAMESPACE"); //得到命名空间
			namespaceName = result;

			GenerateCSFile(textBox1.Text, outputDir + "\\" + className + ".cs");
			GenerateBuildConfig(className, outputDir);
			GenerateUnitTestFile(outputDir + "\\" + className + "Test.cs", className, namespaceName, methodName);

			ExecuteNANT(outputDir);

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


		private string GetNodeName(AstNode node, string nodeType)
		{
			foreach (PropertyInfo p in node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (p.Name == "NodeType" || p.Name == "IsNull" || p.Name == "IsFrozen" || p.Name == "HasChildren")
				{
					continue;
				}
				if ((p.PropertyType == typeof (string) || p.PropertyType.IsEnum || p.PropertyType == typeof (bool)))
				{
					if (nodeType == "CLASS")
					{
						if (p.Name == "ClassType")
						{
							foreach (PropertyInfo p2 in node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
							{
								if (p2.Name == "NodeType" || p2.Name == "IsNull" || p2.Name == "IsFrozen" || p2.Name == "HasChildren")
								{
									continue;
								}
								if ((p2.PropertyType == typeof (string) || p2.PropertyType.IsEnum || p2.PropertyType == typeof (bool)))
								{
									if (p2.Name.ToUpper() == "NAME") //如果当前是Class节点
									{
										return p2.GetValue(node, null).ToString();
									}
								}
							}
						}
					}
					else if (nodeType == "METHOD")
					{
						if (p.Name == "EntityType" && p.GetValue(node, null).ToString() == "Method")
						{
							foreach (PropertyInfo p2 in node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
							{
								if (p2.Name == "NodeType" || p2.Name == "IsNull" || p2.Name == "IsFrozen" || p2.Name == "HasChildren")
								{
									continue;
								}
								if ((p2.PropertyType == typeof (string) || p2.PropertyType.IsEnum || p2.PropertyType == typeof (bool)))
								{
									if (p2.Name.ToUpper() == "NAME") //如果当前是Method节点
									{
										return p2.GetValue(node, null).ToString();
									}
								}
							}
						}
					}
					else if (nodeType == "NAMESPACE")
					{
						if (node.GetType().Name == "NamespaceDeclaration")
						{
							foreach (PropertyInfo p2 in node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
							{
								if (p2.Name == "NodeType" || p2.Name == "IsNull" || p2.Name == "IsFrozen" || p2.Name == "HasChildren")
								{
									continue;
								}
								if ((p2.PropertyType == typeof(string) || p2.PropertyType.IsEnum || p2.PropertyType == typeof(bool)))
								{
									if (p2.Name.ToUpper() == "NAME") //如果当前是Method节点
									{
										return p2.GetValue(node, null).ToString();
									}
								}
							}
						}
					}
					else
					{
					}
				}
			}

			return string.Empty;
		}

		private void FindNodeName(AstNode node, string nodeType)
		{
			AstNode childNode;


			childNode = node.FirstChild;

			string currentName = GetNodeName(node, nodeType);

			if (currentName != string.Empty && result == string.Empty)
			{
				result = currentName;
			}


			while (childNode != null)
			{
				FindNodeName(childNode, nodeType);
				childNode = childNode.NextSibling;
			}
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

		public void GenerateUnitTestFile(string targetPath, string className, string namespaceName, string methodName)
		{
			string templatePath;
			string fileContent;

			templatePath = Application.StartupPath + "\\unit_test_template.txt";
			try
			{
				StreamReader streamReader = new StreamReader(templatePath, new UTF8Encoding());
				fileContent = streamReader.ReadToEnd();
				streamReader.Close();

				fileContent = fileContent.Replace("$namespace_name", namespaceName)
					.Replace("$test_class_name", className + "Test")
					.Replace("$class_name", className)
					.Replace("$instance_name", className.ToLower())
					.Replace("$method_name", methodName);

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