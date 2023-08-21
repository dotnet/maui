// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries
{
	internal static class IndexParser
	{
		static int ParseToken(string value)
		{
			if (!int.TryParse(value, out int index))
			{
				return -1;
			}

			return index;
		}

		public static bool ParseIndexes(string text, int count, out int[] indexes)
		{
			indexes = text.Split(',').Select(v => ParseToken(v.Trim())).ToArray();
			return indexes.Length == count;
		}
	}
}