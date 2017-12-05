using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NavigationModel
	{
		readonly List<Page> _modalStack = new List<Page>();
		readonly List<List<Page>> _navTree = new List<List<Page>>();

		public Page CurrentPage
		{
			get
			{
				if (_navTree.Any())
					return _navTree.Last().Last();
				return null;
			}
		}

		public IEnumerable<Page> Modals
		{
			get { return _modalStack; }
		}

		public IEnumerable<Page> Roots
		{
			get
			{
				foreach (List<Page> list in _navTree)
				{
					yield return list[0];
				}
			}
		}

		public IReadOnlyList<IReadOnlyList<Page>> Tree
		{
			get { return _navTree; }
		}

		public void Clear()
		{
			_navTree.Clear();
			_modalStack.Clear();
		}

		public void InsertPageBefore(Page page, Page before)
		{
			List<Page> currentStack = _navTree.Last();
			int index = currentStack.IndexOf(before);

			if (index == -1)
				throw new ArgumentException("before must be in the current navigation context");

			currentStack.Insert(index, page);
		}

		public Page Pop(Page ancestralNav)
		{
			ancestralNav = AncestorToRoot(ancestralNav);
			foreach (List<Page> stack in _navTree)
			{
				if (stack.Contains(ancestralNav))
				{
					if (stack.Count <= 1)
						throw new InvalidNavigationException("Can not pop final item in stack");
					Page result = stack.Last();
					stack.Remove(result);
					return result;
				}
			}

			throw new InvalidNavigationException("Popped from unpushed item?");
		}

		public Page PopModal()
		{
			if (_navTree.Count <= 1)
				throw new InvalidNavigationException("Can't pop modal without any modals pushed");
			Page modal = _navTree.Last().First();
			_modalStack.Remove(modal);
			_navTree.Remove(_navTree.Last());
			return modal;
		}

		public Page PopTopPage()
		{
			Page itemToRemove;
			if (_navTree.Count == 1)
			{
				if (_navTree[0].Count > 1)
				{
					itemToRemove = _navTree[0].Last();
					_navTree[0].Remove(itemToRemove);
					return itemToRemove;
				}
				return null;
			}
			itemToRemove = _navTree.Last().Last();
			_navTree.Last().Remove(itemToRemove);
			if (!_navTree.Last().Any())
			{
				_navTree.RemoveAt(_navTree.Count - 1);
			}
			return itemToRemove;
		}

		public void PopToRoot(Page ancestralNav)
		{
			ancestralNav = AncestorToRoot(ancestralNav);
			foreach (List<Page> stack in _navTree)
			{
				if (stack.Contains(ancestralNav))
				{
					if (stack.Count <= 1)
						throw new InvalidNavigationException("Can not pop final item in stack");
					stack.RemoveRange(1, stack.Count - 1);
					return;
				}
			}

			throw new InvalidNavigationException("Popped from unpushed item?");
		}

		public void Push(Page page, Page ancestralNav)
		{
			if (ancestralNav == null)
			{
				if (_navTree.Any())
					throw new InvalidNavigationException("Ancestor must be provided for all pushes except first");
				_navTree.Add(new List<Page> { page });
				return;
			}

			ancestralNav = AncestorToRoot(ancestralNav);

			foreach (List<Page> stack in _navTree)
			{
				if (stack.Contains(ancestralNav))
				{
					stack.Add(page);
					return;
				}
			}

			throw new InvalidNavigationException("Invalid ancestor passed");
		}

		public void PushModal(Page page)
		{
			_navTree.Add(new List<Page> { page });
			_modalStack.Add(page);
		}

		public bool RemovePage(Page page)
		{
			bool found;
			List<Page> currentStack = _navTree.Last();
			var i = 0;
			while (!(found = currentStack.Remove(page)) && i < _navTree.Count - 1)
			{
				currentStack = _navTree[i++];
			}

			return found;
		}

		Page AncestorToRoot(Page ancestor)
		{
			Page result = ancestor;
			while (!Application.IsApplicationOrNull(result.RealParent))
				result = (Page)result.RealParent;
			return result;
		}
	}
}