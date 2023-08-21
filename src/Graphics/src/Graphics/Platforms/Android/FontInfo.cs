// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Graphics.Android
{
	internal class FontInfo
	{
		public FontInfo(string path, string family, string style, string fullName)
		{
			Path = path;
			Family = family;
			Style = style;
			FullName = fullName;
		}

		public string Path { get; }
		public string Family { get; }
		public string Style { get; }
		public string FullName { get; }
	}
}
