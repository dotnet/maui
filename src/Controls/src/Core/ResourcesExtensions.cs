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

		internal static IEnumerable<KeyValuePair<string, object>> GetMergedResourcesForKeys(this IElementDefinition element, IEnumerable<string> keys)
		{
			if (element == null || keys == null)
				return null;

			HashSet<string> requestedKeys = null;
			foreach (var key in keys)
			{
				if (string.IsNullOrEmpty(key))
					continue;

				requestedKeys ??= new HashSet<string>(StringComparer.Ordinal);
				requestedKeys.Add(key);
			}

			if (requestedKeys == null || requestedKeys.Count == 0)
				return null;

			var filteredResources = GetFilteredMergedResources(element, requestedKeys);
			return filteredResources.Count == 0 ? null : filteredResources;
		}

		static List<KeyValuePair<string, object>> GetFilteredMergedResources(IElementDefinition element, HashSet<string> requestedKeys)
		{
			// This is equivalent to filtering GetMergedResources() by requestedKeys, but stores only
			// matching entries. Keep the traversal and replacement rules in sync with GetMergedResources:
			// dynamic-resource callbacks can observe both value order and style-class merge order.
			var filteredResources = new List<KeyValuePair<string, object>>(requestedKeys.Count);
			Dictionary<string, int> filteredIndexes = null;

			while (element != null)
			{
				var ve = element as IResourcesProvider;
				if (ve != null && ve.IsResourcesCreated)
					AppendMatchingMergedResources(ve.Resources, requestedKeys, filteredResources, ref filteredIndexes);

				var app = element as Application;
				if (app != null && app.SystemResources != null)
					AppendMatchingResources(app.SystemResources, requestedKeys, filteredResources, ref filteredIndexes);

				if (app != null && requestedKeys.Contains(AppThemeBinding.AppThemeResource))
					AddOrUpdateFilteredResource(AppThemeBinding.AppThemeResource, app.RequestedTheme, filteredResources, ref filteredIndexes);

				element = element.Parent;
			}

			return filteredResources;
		}

		static void AppendMatchingMergedResources(ResourceDictionary source, HashSet<string> requestedKeys, List<KeyValuePair<string, object>> filteredResources, ref Dictionary<string, int> filteredIndexes)
		{
			foreach (KeyValuePair<string, object> res in source.MergedResources)
			{
				if (!requestedKeys.Contains(res.Key))
					continue;

				if (!TryGetFilteredResourceIndex(filteredIndexes, res.Key, out var index))
				{
					// MergedResources may include an overridden entry before the final one. Match
					// GetMergedResources by resolving the current effective value on first insert.
					if (source.TryGetValue(res.Key, out object value))
						AddFilteredResource(res.Key, value, filteredResources, ref filteredIndexes);
				}
				else if (res.Key.StartsWith(Style.StyleClassPrefix, StringComparison.Ordinal))
				{
					AppendStyleClassResource(filteredResources, index, res.Value);
				}
			}
		}

		static void AppendMatchingResources(IEnumerable<KeyValuePair<string, object>> source, HashSet<string> requestedKeys, List<KeyValuePair<string, object>> filteredResources, ref Dictionary<string, int> filteredIndexes)
		{
			foreach (KeyValuePair<string, object> res in source)
			{
				if (!requestedKeys.Contains(res.Key))
					continue;

				if (!TryGetFilteredResourceIndex(filteredIndexes, res.Key, out var index))
				{
					AddFilteredResource(res.Key, res.Value, filteredResources, ref filteredIndexes);
				}
				else if (res.Key.StartsWith(Style.StyleClassPrefix, StringComparison.Ordinal))
				{
					AppendStyleClassResource(filteredResources, index, res.Value);
				}
			}
		}

		static bool TryGetFilteredResourceIndex(Dictionary<string, int> filteredIndexes, string key, out int index)
		{
			if (filteredIndexes != null && filteredIndexes.TryGetValue(key, out index))
			{
				return true;
			}

			index = -1;
			return false;
		}

		static void AddFilteredResource(string key, object value, List<KeyValuePair<string, object>> filteredResources, ref Dictionary<string, int> filteredIndexes)
		{
			filteredIndexes ??= new Dictionary<string, int>(StringComparer.Ordinal);
			filteredIndexes.Add(key, filteredResources.Count);
			filteredResources.Add(new KeyValuePair<string, object>(key, value));
		}

		static void AddOrUpdateFilteredResource(string key, object value, List<KeyValuePair<string, object>> filteredResources, ref Dictionary<string, int> filteredIndexes)
		{
			if (TryGetFilteredResourceIndex(filteredIndexes, key, out var index))
			{
				// GetMergedResources uses dictionary indexer replacement for synthetic app theme resources.
				// Replace in place so the filtered payload keeps the same enumeration position.
				filteredResources[index] = new KeyValuePair<string, object>(key, value);
				return;
			}

			AddFilteredResource(key, value, filteredResources, ref filteredIndexes);
		}

		static void AppendStyleClassResource(List<KeyValuePair<string, object>> filteredResources, int index, object value)
		{
			var mergedClassStyles = new List<Style>(filteredResources[index].Value as List<Style>);
			mergedClassStyles.AddRange(value as List<Style>);
			filteredResources[index] = new KeyValuePair<string, object>(filteredResources[index].Key, mergedClassStyles);
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