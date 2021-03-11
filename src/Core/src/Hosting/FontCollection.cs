using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Hosting
{
	class FontCollection : IFontCollection
	{
		readonly List<FontDescriptor> _descriptors = new List<FontDescriptor>();

		public int Count => _descriptors.Count;

		public bool IsReadOnly => false;

		public FontDescriptor this[int index]
		{
			get => _descriptors[index];
			set => _descriptors[index] = value;
		}

		public void Add(FontDescriptor item)
		{
			if (!_descriptors.Contains(item))
				_descriptors.Add(item);
		}

		public void Clear() => _descriptors.Clear();

		public bool Contains(FontDescriptor item) => _descriptors.Contains(item);

		public IEnumerator<FontDescriptor> GetEnumerator() => _descriptors.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _descriptors.GetEnumerator();

		public bool Remove(FontDescriptor item) => _descriptors.Remove(item);

		public void CopyTo(FontDescriptor[] array, int arrayIndex) => _descriptors.CopyTo(array, arrayIndex);

		public int IndexOf(FontDescriptor item) => _descriptors.IndexOf(item);

		public void Insert(int index, FontDescriptor item) => _descriptors.Insert(index, item);

		public void RemoveAt(int index) => _descriptors.RemoveAt(index);
	}
}
