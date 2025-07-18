#nullable disable
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackagePropertySetView.xml" path="Type[@FullName='Microsoft.Maui.Controls.DataPackagePropertySetView']/Docs/*" />
	public class DataPackagePropertySetView : IReadOnlyDictionary<string, object>
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackagePropertySetView.xml" path="//Member[@MemberName='_dataPackagePropertySet']/Docs/*" />
		public DataPackagePropertySet _dataPackagePropertySet;

		/// <param name="dataPackagePropertySet">The dataPackagePropertySet parameter.</param>
		public DataPackagePropertySetView(DataPackagePropertySet dataPackagePropertySet)
		{
			_ = dataPackagePropertySet ?? throw new global::System.ArgumentNullException(nameof(dataPackagePropertySet));
			_dataPackagePropertySet = dataPackagePropertySet;
		}

		public object this[string key] => _dataPackagePropertySet[key];

		/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackagePropertySetView.xml" path="//Member[@MemberName='Keys']/Docs/*" />
		public IEnumerable<string> Keys => _dataPackagePropertySet.Keys;

		/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackagePropertySetView.xml" path="//Member[@MemberName='Values']/Docs/*" />
		public IEnumerable<object> Values => _dataPackagePropertySet.Values;

		/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackagePropertySetView.xml" path="//Member[@MemberName='Count']/Docs/*" />
		public int Count => _dataPackagePropertySet.Count;

		/// <param name="key">The key parameter.</param>
		public bool ContainsKey(string key) => _dataPackagePropertySet.ContainsKey(key);

		/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackagePropertySetView.xml" path="//Member[@MemberName='GetEnumerator']/Docs/*" />
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _dataPackagePropertySet.GetEnumerator();

		/// <param name="key">The key parameter.</param>
		/// <param name="value">The value to set.</param>
		public bool TryGetValue(string key, out object value) => _dataPackagePropertySet.TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator() => _dataPackagePropertySet.GetEnumerator();
	}
}
