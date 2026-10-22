#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>Event arguments for resource dictionary changes.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ResourcesChangedEventArgs : EventArgs
	{
		/// <summary>Singleton instance used for stylesheet changes.</summary>
		public static readonly ResourcesChangedEventArgs StyleSheets = new ResourcesChangedEventArgs((IEnumerable<string>)null, null);

		readonly Func<string, object> _resolver;

		/// <summary>Creates a new <see cref="ResourcesChangedEventArgs"/> with the specified keys and resolver.</summary>
		/// <param name="keys">The changed resource keys.</param>
		/// <param name="resolver">Function to resolve a value by key (called on-demand).</param>
		public ResourcesChangedEventArgs(IEnumerable<string> keys, Func<string, object> resolver)
		{
			Keys = keys;
			_resolver = resolver;
		}

		/// <summary>Creates a new <see cref="ResourcesChangedEventArgs"/> with the specified values.</summary>
		/// <param name="values">The changed resource values.</param>
		[Obsolete("Use the constructor with keys and resolver to avoid resolving lazy resources.")]
		public ResourcesChangedEventArgs(IEnumerable<KeyValuePair<string, object>> values)
		{
			Keys = values?.Select(kvp => kvp.Key);
			_resolver = key => values?.FirstOrDefault(kvp => kvp.Key == key).Value;
		}

		/// <summary>Gets the changed resource keys.</summary>
		public IEnumerable<string> Keys { get; }

		/// <summary>Gets the changed resource values. Enumerating this will resolve lazy resources.</summary>
		public IEnumerable<KeyValuePair<string, object>> Values => 
			Keys?.Select(k => new KeyValuePair<string, object>(k, _resolver?.Invoke(k)));

		/// <summary>Resolves a single resource value by key.</summary>
		/// <param name="key">The resource key.</param>
		/// <returns>The resolved value, or null if not found.</returns>
		public object ResolveValue(string key) => _resolver?.Invoke(key);
	}
}