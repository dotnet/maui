using System.Collections;
using System.Collections.Generic;

namespace Xamarin.Forms
{
	public class DataPackagePropertySetView : IReadOnlyDictionary<string, object>
	{
		public DataPackagePropertySet _dataPackagePropertySet;

		public DataPackagePropertySetView(DataPackagePropertySet dataPackagePropertySet)
		{
			_ = dataPackagePropertySet ?? throw new System.ArgumentNullException(nameof(dataPackagePropertySet));
			_dataPackagePropertySet = dataPackagePropertySet;
		}

		public object this[string key] => _dataPackagePropertySet[key];

		public IEnumerable<string> Keys => _dataPackagePropertySet.Keys;

		public IEnumerable<object> Values => _dataPackagePropertySet.Values;

		public int Count => _dataPackagePropertySet.Count;

		public bool ContainsKey(string key) => _dataPackagePropertySet.ContainsKey(key);

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _dataPackagePropertySet.GetEnumerator();

		public bool TryGetValue(string key, out object value) => _dataPackagePropertySet.TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator() => _dataPackagePropertySet.GetEnumerator();
	}
}