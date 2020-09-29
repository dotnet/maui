using System.Collections;
using System.Collections.Generic;

namespace Xamarin.Forms
{
	public class DataPackagePropertySet : IEnumerable
	{
		Dictionary<string, object> _propertyBag;

		public DataPackagePropertySet()
		{
			_propertyBag = new Dictionary<string, object>();
		}

		public object this[string key]
		{
			get => _propertyBag[key];
			set => _propertyBag[key] = value;
		}

		public int Count => _propertyBag.Count;

		public IEnumerable<string> Keys => _propertyBag.Keys;
		public IEnumerable<object> Values => _propertyBag.Values;

		public void Add(string key, object value)
		{
			_propertyBag.Add(key, value);
		}

		public bool ContainsKey(string key) => _propertyBag.ContainsKey(key);

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _propertyBag.GetEnumerator();

		public bool TryGetValue(string key, out object value) => _propertyBag.TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _propertyBag.GetEnumerator();
		}
	}
}