#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	static class ResourcesExtensions
	{
		public static IEnumerable<KeyValuePair<string, object>> GetMergedResources(this IElementDefinition element)
		{
			Dictionary<string, object> resources = null;
			while (element != null)
			{
				var ve = element as IResourcesProvider;
				if (ve != null && ve.IsResourcesCreated)
				{
					resources = resources ?? new(StringComparer.Ordinal);
					foreach (KeyValuePair<string, object> res in ve.Resources.MergedResources)
					{
						// If a MergedDictionary value is overridden for a DynamicResource, 
						// it comes out later in the enumeration of MergedResources
						// TryGetValue ensures we pull the up-to-date value for the key
						if (!resources.ContainsKey(res.Key) && ve.Resources.TryGetValue(res.Key, out object value))
							resources.Add(res.Key, value);
						else if (res.Key.StartsWith(Style.StyleClassPrefix, StringComparison.Ordinal))
						{
							var mergedClassStyles = new List<Style>(resources[res.Key] as List<Style>);
							mergedClassStyles.AddRange(res.Value as List<Style>);
							resources[res.Key] = mergedClassStyles;
						}
					}
				}
				var app = element as Application;
				if (app != null && app.SystemResources != null)
				{
					resources = resources ?? new Dictionary<string, object>(8, StringComparer.Ordinal);
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
				if (app != null)
				{
					resources = resources ?? new(StringComparer.Ordinal);
					resources[AppThemeBinding.AppThemeResource] = app.RequestedTheme;
				}

				element = element.Parent;
			}
			return resources;
		}

		/// <summary>
		/// Gets all merged resource keys without resolving lazy values.
		/// Used for resource change propagation where values are looked up on-demand.
		/// </summary>
		public static IEnumerable<string> GetMergedResourceKeys(this IElementDefinition element)
		{
			HashSet<string> keys = null;
			while (element != null)
			{
				var ve = element as IResourcesProvider;
				if (ve != null && ve.IsResourcesCreated)
				{
					keys = keys ?? new(StringComparer.Ordinal);
					foreach (string key in ve.Resources.MergedResourcesKeys)
					{
						keys.Add(key);
					}
				}
				var app = element as Application;
				if (app != null && app.SystemResources != null)
				{
					keys = keys ?? new(StringComparer.Ordinal);
					foreach (var kvp in app.SystemResources)
						keys.Add(kvp.Key);
				}
				if (app != null)
				{
					keys = keys ?? new(StringComparer.Ordinal);
					keys.Add(AppThemeBinding.AppThemeResource);
				}

				element = element.Parent;
			}
			return keys;
		}

		public static bool TryGetResource(this IElementDefinition element, string key, out object value)
		{
			while (element != null)
			{
				if (element is IResourcesProvider ve && ve.IsResourcesCreated && ve.Resources.TryGetValue(key, out value))
					return true;
				if (element is Application app && app.SystemResources != null && app.SystemResources.TryGetValue(key, out value))
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