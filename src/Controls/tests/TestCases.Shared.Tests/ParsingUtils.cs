﻿using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.TestCases.Tests
{
	internal static class ParsingUtils
	{
		public static Font ParseUIFont(string font)
		{
			FontAttributes fontAttrs = FontAttributes.None;

			if (font.IndexOf("font-weight: bold;", StringComparison.Ordinal) != -1)
			{
				// Logger.LogLine ("Found Bold");
				fontAttrs = FontAttributes.Bold;
			}

			return new Font().WithAttributes(fontAttrs);
		}

		public static Color ParseUIColor(string backgroundColor)
		{
			var delimiters = new char[] { ' ' };
			string[] words = backgroundColor.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			return new Color(float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3]), float.Parse(words[4]));
		}

		public static Point ParseCGPoint(object CGPoint)
		{
			var point = new Point { X = 0, Y = 0 };
			return point;
		}

		public static Matrix ParseCATransform3D(string CATransform3D)
		{
			char[] delimiters = { '[', ' ', ']', ';' };
			string[] words = CATransform3D.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

			List<double> numbers = new List<double>();

			// Each number is represented by 2 blocks returned by server
			for (int i = 0; i < words.Length; i++)
			{
				numbers.Add(Convert.ToDouble(words[i]));
			}

			var transformationMatrix = new Matrix();
			transformationMatrix.M00 = numbers[0];
			transformationMatrix.M01 = numbers[1];
			transformationMatrix.M02 = numbers[2];
			transformationMatrix.M03 = numbers[3];
			transformationMatrix.M10 = numbers[4];
			transformationMatrix.M11 = numbers[5];
			transformationMatrix.M12 = numbers[6];
			transformationMatrix.M13 = numbers[7];
			transformationMatrix.M20 = numbers[8];
			transformationMatrix.M21 = numbers[9];
			transformationMatrix.M22 = numbers[10];
			transformationMatrix.M23 = numbers[11];
			transformationMatrix.M30 = numbers[12];
			transformationMatrix.M31 = numbers[13];
			transformationMatrix.M32 = numbers[14];
			transformationMatrix.M33 = numbers[15];

			return transformationMatrix;
		}

	}
}