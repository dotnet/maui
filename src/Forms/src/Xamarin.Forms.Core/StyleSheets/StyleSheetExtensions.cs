using System.Collections.Generic;

namespace Xamarin.Forms.StyleSheets
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