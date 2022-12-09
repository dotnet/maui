using System;
using System.Collections.Generic;
using System.IO;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using AApplication = Android.App.Application;
//using System.Reflection.Metadata.Ecma335;

namespace Microsoft.Maui.Graphics.Platform
{
	public static class FontExtensions
	{
		public static Typeface ToTypeface(this IFont font)
		{

			TypefaceStyle GetStyle(IFont font)
			{
				if (font.Weight >= FontWeights.Bold)
					return font.StyleType == FontStyleType.Normal ? TypefaceStyle.Bold : TypefaceStyle.BoldItalic;
				else
					return font.StyleType == FontStyleType.Normal ? TypefaceStyle.Normal : TypefaceStyle.Italic;
			}

			if (font == null)
				return Typeface.Default;

			if (string.IsNullOrEmpty(font.Name))
				return Typeface.DefaultFromStyle(GetStyle(font));

			var context = AApplication.Context;
			Typeface typeface = null;

			// Fonts can be resources in API 26+
			if (OperatingSystem.IsAndroidVersionAtLeast(26))
			{
				var id = context.Resources.GetIdentifier(font.Name, "font", context.PackageName);

				if (id > 0)
					typeface = context.Resources?.GetFont(id);
			}

			// Next, we can try to load from an asset
			if (typeface == null)
			{
				// First check filename as is
				if (!TryLoadTypefaceFromAsset(font.Name, out typeface))
				{
					var sepChar = Java.IO.File.PathSeparatorChar;

					// Also try any *fonts*/ subfolders
					foreach (var a in context.Assets.List(""))
					{
						var file = new Java.IO.File(a);
						if (file.IsDirectory && file.Name.Contains("fonts", StringComparison.InvariantCultureIgnoreCase))
						{
							if (TryLoadTypefaceFromAsset(file.AbsolutePath.TrimEnd(sepChar) + font.Name.TrimStart(sepChar), out typeface))
								break;
						}
					}
				}
			}

			if (typeface == null)
				return Typeface.Create(font.Name, GetStyle(font));

			return typeface;
		}

		static bool TryLoadTypefaceFromAsset(string filename, out Typeface typeface)
		{
			try
			{
				typeface = Typeface.CreateFromAsset(AApplication.Context.Assets, filename);

				return typeface != null;
			}
			catch
			{
				typeface = null;
			}

			return false;
		}
	}
}
