// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System.IO;

namespace Microsoft.Maui
{
	/// <inheritdoc/>
	public partial class FontRegistrar : IFontRegistrar
	{
		// Return the filename as-is, as we load the font directly in FontManager
		string? LoadNativeAppFont(string font, string filename, string? alias) => filename;
	}
}