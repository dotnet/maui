#nullable disable
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A read-only view of a <see cref="DataPackagePropertySet"/>.
	/// </summary>
	public class DataPackagePropertySetView : IReadOnlyDictionary<string, object>
	{
		/// <summary>
		/// The underlying property set.
		/// </summary>
		public DataPackagePropertySet _dataPackagePropertySet;

		/// <summary>
		/// Initializes a new instance of the <see cref="DataPackagePropertySetView"/> class.
		/// </summary>
		/// <param name="dataPackagePropertySet">The property set to wrap.</param>
		public DataPackagePropertySetView(DataPackagePropertySet dataPackagePropertySet)
		{
			_ = dataPackagePropertySet ?? throw new global::System.ArgumentNullException(nameof(dataPackagePropertySet));
			_dataPackagePropertySet = dataPackagePropertySet;
		}

		/// <summary>
		/// Gets the property value associated with the specified key.
		/// </summary>
		/// <param name="key">The key of the property.</param>
		public object this[string key] => _dataPackagePropertySet[key];

		/// <summary>
		/// Gets the collection of property keys.
		/// </summary>
		public IEnumerable<string> Keys => _dataPackagePropertySet.Keys;

		/// <summary>
		/// Gets the collection of property values.
		/// </summary>
		public IEnumerable<object> Values => _dataPackagePropertySet.Values;

		/// <summary>
		/// Gets the number of properties in the view.
		/// </summary>
		public int Count => _dataPackagePropertySet.Count;

		/// <summary>
		/// Determines whether the view contains a property with the specified key.
		/// </summary>
		/// <param name="key">The key to locate.</param>
		public bool ContainsKey(string key) => _dataPackagePropertySet.ContainsKey(key);

		/// <summary>
		/// Returns an enumerator that iterates through the properties.
		/// </summary>
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _dataPackagePropertySet.GetEnumerator();

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key of the value to get.</param>
		/// <param name="value">When this method returns, contains the value if the key was found; otherwise, the default value.</param>
		public bool TryGetValue(string key, out object value) => _dataPackagePropertySet.TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator() => _dataPackagePropertySet.GetEnumerator();
	}
}
