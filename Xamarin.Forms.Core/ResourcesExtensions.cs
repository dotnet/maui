using System;
using System.Collections.Generic;

namespace Xamarin.Forms
{
	static class ResourcesExtensions
	{
		public static IEnumerable<KeyValuePair<string, object>> GetMergedResources(this IElement element)
		{
			Dictionary<string, object> resources = null;
			while (element != null)
			{
				var ve = element as IResourcesProvider;
				if (ve != null && ve.IsResourcesCreated)
				{
					resources = resources ?? new Dictionary<string, object>();
					foreach (KeyValuePair<string, object> res in ve.Resources.MergedResources)
						if (!resources.ContainsKey(res.Key))
							resources.Add(res.Key, res.Value);
						else if (res.Key.StartsWith(Style.StyleClassPrefix, StringComparison.Ordinal))
						{
							var mergedClassStyles = new List<Style>(resources[res.Key] as List<Style>);
							mergedClassStyles.AddRange(res.Value as List<Style>);
							resources[res.Key] = mergedClassStyles;
						}
				}
				var app = element as Application;
				if (app != null && app.SystemResources != null)
				{
					resources = resources ?? new Dictionary<string, object>(8);
					foreach (KeyValuePair<string, object> res in app.SystemResources)
						if (!resources.ContainsKey(res.Key))
							resources.Add(res.Key, res.Value);
						else if (res.Key.StartsWith(Style.StyleClassPrefix, StringComparison.Ordinal))
						{
							var mergedClassStyles = new List<Style>(resources[res.Key] as List<Style>);
							mergedClassStyles.AddRange(res.Value as List<Style>);
							resources[res.Key] = mergedClassStyles;
						}
				}
				element = element.Parent;
			}
			return resources;
		}

		public static bool TryGetResource(this IElement element, string key, out object value)
		{
			while (element != null)
			{
				var ve = element as IResourcesProvider;
				if (ve != null && ve.IsResourcesCreated && ve.Resources.TryGetValue(key, out value))
					return true;
				var app = element as Application;
				if (app != null && app.SystemResources != null && app.SystemResources.TryGetValue(key, out value))
					return true;
				element = element.Parent;
			}

			//Fallback for the XF previewer
			if (Application.Current != null && ((IResourcesProvider)Application.Current).IsResourcesCreated && Application.Current.Resources.TryGetValue(key, out value))
				return true;

			value = null;
			return false;
		}
	}
}