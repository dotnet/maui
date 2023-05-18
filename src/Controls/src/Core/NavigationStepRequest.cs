using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	internal class NavigationStepRequest
	{
		public NavigationStepRequest(Page page, bool isModal, bool isAnimated)
		{
			IsModal = isModal;
			IsAnimated = isAnimated;
			Page = page;
		}

		public bool IsModal { get; }
		public bool IsAnimated { get; }
		public Page Page { get; }
	}

	class NavigatingStepRequestList : IList<NavigationStepRequest>
	{
		List<NavigationStepRequest> _innerList = new List<NavigationStepRequest>();
		List<Page> _pages = new List<Page>();

		public IReadOnlyList<Page> Pages => _pages;
		public int Count => _pages.Count;

		public bool IsReadOnly => false;

		public NavigationStepRequest this[int index]
		{
			get => _innerList[index];
			set => _innerList.Insert(index, value);
		}

		public void Add(NavigationStepRequest args)
		{
			_pages.Add(args.Page);
			_innerList.Add(args);
		}

		public void Remove(Page page)
		{
			if (TryGetValue(page, out var request))
				Remove(request);
		}

		public void Remove(NavigationStepRequest args)
		{
			_innerList.Remove(args);
			_pages.Remove(args.Page);
		}

		public void Clear()
		{
			_innerList.Clear();
			_pages.Clear();
		}

		public int IndexOf(NavigationStepRequest item)
		{
			return _innerList.IndexOf(item);
		}

		public void Insert(int index, NavigationStepRequest item)
		{
			_innerList.Insert(index, item);
			_pages.Insert(index, item.Page);
		}

		public void RemoveAt(int index)
		{
			_innerList.RemoveAt(index);
			_pages.RemoveAt(index);
		}

		public bool Contains(NavigationStepRequest item)
		{
			return _innerList.Contains(item);
		}

		public void CopyTo(NavigationStepRequest[] array, int arrayIndex)
		{
			_innerList.CopyTo(array, arrayIndex);

			var pages = new Page[array.Length];

			for (var i = 0; i < array.Length; i++)
			{
				NavigationStepRequest arg = array[i];
				pages[i] = arg.Page;
			}

			_pages.CopyTo(pages, arrayIndex);
		}

		bool ICollection<NavigationStepRequest>.Remove(NavigationStepRequest item)
		{
			if (!_innerList.Contains(item))
				return false;

			Remove(item);
			return true;
		}

		public IEnumerator<NavigationStepRequest> GetEnumerator()
		{
			return _innerList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _innerList.GetEnumerator();
		}

		internal bool TryGetValue(Page page, [NotNullWhen(true)] out NavigationStepRequest? request)
		{
			for (var i = 0; i < _innerList.Count; i++)
			{
				NavigationStepRequest? item = _innerList[i];

				if (item.Page == page)
				{
					request = item;
					return true;
				}
			}

			request = null;

			return false;
		}
	}
}