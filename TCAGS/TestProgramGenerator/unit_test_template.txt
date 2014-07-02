using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ProgramsForTesting
{
	class TriangleTest
	{
		private int a;
		private int b;
		private int c;

		public Triangle triangle = new Triangle();

		
		[Test] //断言
		public void GetTriangleTypeTest()
		{
			Assert.AreEqual("非三角形", triangle.GetTypeOfTriangle(1,2,3));
			Assert.AreEqual("等边三角形", triangle.GetTypeOfTriangle(3, 3, 3));
			Assert.AreEqual("等腰三角形", triangle.GetTypeOfTriangle(3, 3, 4));
			Assert.AreEqual("一般三角形", triangle.GetTypeOfTriangle(3, 4, 5));


		}

		
	}
}
