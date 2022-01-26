using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;
using GraphicsTester.Scenarios;
using System.Linq;
using System.IO;

namespace GraphicsTester.Skia.ConsoleApp
{
	class Program
	{
		static void Main()
		{
			string outputFolder = System.IO.Path.GetFullPath("TestImages");
			if (!Directory.Exists(outputFolder))
				Directory.CreateDirectory(outputFolder);

			foreach (AbstractScenario scenario in ScenarioList.Scenarios)
			{
				using BitmapExportContext bmp = new SkiaBitmapExportContext((int)scenario.Width, (int)scenario.Height, 1f);
				bmp.Canvas.FillColor = Colors.White;
				bmp.Canvas.FillRectangle(0, 0, scenario.Width, scenario.Height);

				scenario.Draw(bmp.Canvas);

				string fileName = GetSafeFilename(scenario.ToString()) + ".png";
				string filePath = Path.Combine(outputFolder, fileName);
				bmp.WriteToFile(filePath);
				Console.WriteLine(filePath);
			}
		}

		static string GetSafeFilename(string text)
		{
			char[] allowedSpecialChars = { '_', '-' };
			char[] chars = text.ToCharArray();
			for (int i = 0; i < chars.Length; i++)
			{
				if (allowedSpecialChars.Contains(chars[i]))
					continue;
				else if (char.IsLetterOrDigit(chars[i]))
					chars[i] = char.ToLowerInvariant(chars[i]);
				else
					chars[i] = '-';
			}

			string safe = new string(chars);
			while (safe.Contains("--"))
				safe = safe.Replace("--", "-");
			return safe.Trim('-');
		}
	}
}
