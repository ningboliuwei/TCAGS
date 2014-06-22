using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
			GenerateCSFile(textBox1.Text, "r:\\source.cs");


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
