// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System.Reflection;

namespace Microsoft.Maui.Hosting
{
	public class FontDescriptor
	{
		public FontDescriptor(string filename, string? alias, Assembly? assembly)
		{
			Filename = filename;
			Alias = alias;
			Assembly = assembly;
		}

		public string Filename { get; }

		public string? Alias { get; }

		public Assembly? Assembly { get; }
	}
}