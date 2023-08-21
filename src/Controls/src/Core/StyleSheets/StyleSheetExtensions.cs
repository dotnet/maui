// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.StyleSheets
{
	static class StyleSheetExtensions
	{
		public static IEnumerable<StyleSheet> GetStyleSheets(this IResourcesProvider resourcesProvider)
		{
			if (!resourcesProvider.IsResourcesCreated)
				yield break;
			if (resourcesProvider.Resources.StyleSheets == null)
				yield break;
			foreach (var styleSheet in resourcesProvider.Resources.StyleSheets)
				yield return styleSheet;
		}
	}
}