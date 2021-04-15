#nullable enable
using System;
using System.Reflection;

namespace Microsoft.Maui.Hosting
{
	public static class FontCollectionExtensions
	{
		public static IFontCollection AddFont(this IFontCollection fontCollection, string filename, string? alias = null)
		{
			_ = filename ?? throw new ArgumentNullException(nameof(filename));
			if (string.IsNullOrWhiteSpace(filename))
				throw new ArgumentException("Filename was not a valid file name.", nameof(filename));

			fontCollection.Add(new FontDescriptor(filename, alias, null));
			return fontCollection;
		}

		public static IFontCollection AddEmeddedResourceFont(this IFontCollection fontCollection, Assembly assembly, string filename, string? alias = null)
		{
			_ = assembly ?? throw new ArgumentNullException(nameof(assembly));
			_ = filename ?? throw new ArgumentNullException(nameof(filename));
			if (string.IsNullOrWhiteSpace(filename))
				throw new ArgumentException("Filename was not a valid file name.", nameof(filename));

			fontCollection.Add(new FontDescriptor(filename, alias, assembly));
			return fontCollection;
		}
	}
}