using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgramsForTesting
{
	public class Triangle
	{
		
		public string GetTypeOfTriangle(int a, int b, int c)
		{
		
			if (a + b > c && b + c > a && a + c > b)
			{
				if (a == b && a == c)
				{
					return "等边三角形";
				}
				else if (a == b || a == c || b == c)
				{
					return "等腰三角形";
				}
				else
				{
					return "一般三角形";
				}
			}
			else
			{
				return "非三角形";
			}
		}
	}
}
