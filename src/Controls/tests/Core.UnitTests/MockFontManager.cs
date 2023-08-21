// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	internal class MockFontManager : IFontManager
	{
		public MockFontManager()
		{
		}

		public double DefaultFontSize => 10.0d;
	}

	internal class MockFontRegistrar : IFontRegistrar
	{
		public string GetFont(string font)
			=> fonts?[font];

		Dictionary<string, string> fonts = new();

		public void Register(string filename, string alias, Assembly assembly)
		{
			fonts[alias ?? filename] = filename;
		}

		public void Register(string filename, string alias)
		{
			fonts[alias ?? filename] = filename;
		}
	}
}