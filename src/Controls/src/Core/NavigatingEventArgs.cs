using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	internal class NavigatingEventArgs
	{

		public NavigatingEventArgs(Page page, bool isModal, bool isAnimated)
		{
			IsModal = isModal;
			IsAnimated = isAnimated;
			Page = page;
		}

		public bool IsModal { get; }
		public bool IsAnimated { get; }
		public Page Page { get; }
	}

	class NavigatingEventArgsList : IList<NavigatingEventArgs>
	{
		List<NavigatingEventArgs> _innerList = new List<NavigatingEventArgs>();
		List<Page> _pages = new List<Page>();

		public IReadOnlyList<Page> Pages => _pages;
		public int Count => _pages.Count;

		public bool IsReadOnly => false;

		public NavigatingEventArgs this[int index]
		{
			get => _innerList[index];
			set => _innerList.Insert(index, value);
		}

		public void Add(NavigatingEventArgs args)
		{
			_pages.Add(args.Page);
			_innerList.Add(args);
		}

		public void Remove(Page page)
		{
			if (TryGetValue(page, out var request))
				Remove(request);
		}

		public void Remove(NavigatingEventArgs args)
		{
			_innerList.Remove(args);
			_pages.Remove(args.Page);
		}

		public void Clear()
		{
			_innerList.Clear();
			_pages.Clear();
		}

		public int IndexOf(NavigatingEventArgs item)
		{
			return _innerList.IndexOf(item);
		}

		public void Insert(int index, NavigatingEventArgs item)
		{
			_innerList.Insert(index, item);
			_pages.Insert(index, item.Page);
		}

		public void RemoveAt(int index)
		{
			_innerList.RemoveAt(index);
			_pages.RemoveAt(index);
		}

		public bool Contains(NavigatingEventArgs item)
		{
			return _innerList.Contains(item);
		}

		public void CopyTo(NavigatingEventArgs[] array, int arrayIndex)
		{
			_innerList.CopyTo(array, arrayIndex);

			var pages = new Page[array.Length];

			for (var i = 0; i < array.Length; i++)
			{
				NavigatingEventArgs arg = array[i];
				pages[i] = arg.Page;
			}

			_pages.CopyTo(pages, arrayIndex);
		}

		bool ICollection<NavigatingEventArgs>.Remove(NavigatingEventArgs item)
		{
			if (!_innerList.Contains(item))
				return false;

			Remove(item);
			return true;
		}

		public IEnumerator<NavigatingEventArgs> GetEnumerator()
		{
			return _innerList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _innerList.GetEnumerator();
		}

#if NETSTANDARD2_1_OR_GREATER
		internal bool TryGetValue(Page page, [NotNullWhen(true)]out NavigatingEventArgs? request)
#else
		internal bool TryGetValue(Page page, out NavigatingEventArgs request)
#endif
		{
			for (var i = 0; i < _innerList.Count; i++)
			{
				NavigatingEventArgs? item = _innerList[i];

				if (item.Page == page)
				{
					request = item;
					return true;
				}
			}

#if NETSTANDARD2_1_OR_GREATER
			request = null;
#else
			request = null!;
#endif
			return false;
		}
	}
}