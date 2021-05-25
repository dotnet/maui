// Based on: https://github.com/dotnet/runtime/blob/776dd1d3318760bf2328f66a544ed377d78b2099/src/libraries/Common/src/System/Drawing/ColorTable.cs

#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	static class ColorTable
	{
		static readonly Lazy<Dictionary<string, SKColor>> ColorConstants = new Lazy<Dictionary<string, SKColor>>(GetColors);

		static Dictionary<string, SKColor> GetColors()
		{
			var colors = new Dictionary<string, SKColor>(StringComparer.OrdinalIgnoreCase);
			FillWithProperties(colors, typeof(SKColors));
			return colors;
		}

		static void FillWithProperties(Dictionary<string, SKColor> dictionary, Type typeWithColors)
		{
			foreach (FieldInfo field in typeWithColors.GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (field.FieldType == typeof(SKColor))
					dictionary[field.Name] = (SKColor)field.GetValue(null)!;
			}
		}

		static Dictionary<string, SKColor> Colors => ColorConstants.Value;

		public static bool TryGetNamedColor(string name, out SKColor result) => Colors.TryGetValue(name, out result);

		public static bool IsKnownNamedColor(string name) => Colors.TryGetValue(name, out _);
	}
}