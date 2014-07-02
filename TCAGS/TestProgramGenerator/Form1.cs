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

namespace TestProgramGenerator
{
	public partial class Form1 : Form
	{
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

			command = outputDir.Substring(0, 2) + "\n";
			command += "cd " + outputDir + "\n";
			command += "c:\\\n";
			//command += "cd c:\\windows\\system32\n";
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
					.Replace("$nunit_dll_path", Application.StartupPath + "\\nunit.framework.dll");

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

	}
}
