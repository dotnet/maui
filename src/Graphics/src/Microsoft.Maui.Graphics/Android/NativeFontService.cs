using System;
using System.Collections.Generic;
using System.IO;
using Android.App;
using Android.Graphics;
using Microsoft.Maui.Graphics.Android;

namespace Microsoft.Maui.Graphics.Native
{
	public class NativeFontService : AbstractFontService
	{
		public static Boolean FontAliasingEnabled { get; set; } = true;

		public const string SystemFont = "System";
		public const string SystemBoldFont = "System-Bold";

		public static NativeFontService Instance = new NativeFontService();

		private IFontFamily[] _fontFamilies;
		
		public NativeFontService() : base()
		{
		}

		public override IFontFamily[] GetFontFamilies()
			=> _fontFamilies ??= new IFontFamily[0];

		public Typeface GetTypeface(string name)
		{
			if (name == null || SystemFont.Equals(name))
				return Typeface.Default;

			if (SystemBoldFont.Equals(name))
				return Typeface.DefaultBold;

			try
			{
				return Typeface.Create(name, TypefaceStyle.Normal);
			}
			catch { }

			return Typeface.Default;
		}
	}
}
