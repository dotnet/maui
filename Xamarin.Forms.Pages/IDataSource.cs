using System.Collections.Generic;

namespace Xamarin.Forms.Pages
{
	public interface IDataSource
	{
		IReadOnlyList<IDataItem> Data { get; }

		bool IsLoading { get; }

		object this[string key] { get; set; }

		IEnumerable<string> MaskedKeys { get; }

		void MaskKey(string key);
		void UnmaskKey(string key);
	}
}