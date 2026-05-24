#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A collection of custom properties for a <see cref="DataPackage"/>.
	/// </summary>
	public class DataPackagePropertySet : IEnumerable
	{
		Dictionary<string, object> _propertyBag;

		/// <summary>
		/// Initializes a new instance of the <see cref="DataPackagePropertySet"/> class.
		/// </summary>
		public DataPackagePropertySet()
		{
			_propertyBag = new(StringComparer.Ordinal);
		}

		/// <summary>
		/// Gets or sets the property value associated with the specified key.
		/// </summary>
		/// <param name="key">The key of the property.</param>
		public object this[string key]
		{
			get => _propertyBag[key];
			set => _propertyBag[key] = value;
		}

		/// <summary>
		/// Gets the number of properties in the set.
		/// </summary>
		public int Count => _propertyBag.Count;

		/// <summary>
		/// Gets the collection of property keys.
		/// </summary>
		public IEnumerable<string> Keys => _propertyBag.Keys;

		/// <summary>
		/// Gets the collection of property values.
		/// </summary>
		public IEnumerable<object> Values => _propertyBag.Values;

		/// <summary>
		/// Adds a property with the specified key and value.
		/// </summary>
		/// <param name="key">The key of the property to add.</param>
		/// <param name="value">The value of the property to add.</param>
		public void Add(string key, object value)
		{
			_propertyBag.Add(key, value);
		}

		/// <summary>
		/// Determines whether the set contains a property with the specified key.
		/// </summary>
		/// <param name="key">The key to locate.</param>
		public bool ContainsKey(string key) => _propertyBag.ContainsKey(key);

		/// <summary>
		/// Returns an enumerator that iterates through the properties.
		/// </summary>
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _propertyBag.GetEnumerator();

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key of the value to get.</param>
		/// <param name="value">When this method returns, contains the value if the key was found; otherwise, the default value.</param>
		public bool TryGetValue(string key, out object value) => _propertyBag.TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _propertyBag.GetEnumerator();
		}
	}
}
