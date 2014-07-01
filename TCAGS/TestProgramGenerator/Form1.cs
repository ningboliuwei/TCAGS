using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
			string outputDir = Application.StartupPath;
			string className = GetClassName(textBox1.Text);

			GenerateCSFile(textBox1.Text, outputDir + "\\" + className + ".cs");
			GenerateBuildConfig(className, outputDir);

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


			templatePath = outputDir + "\\default_build_template.xml";
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
					.Replace("$report_output_dir", outputDir);

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
	}
}
