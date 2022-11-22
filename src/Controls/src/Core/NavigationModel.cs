using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	internal class NavigationModel
	{
		readonly List<Page> _modalStack = new List<Page>();
		readonly List<List<Page>> _navTree = new List<List<Page>>();

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/NavigationModel.xml" path="//Member[@MemberName='CurrentPage']/Docs/*" />
		public Page CurrentPage
		{
			get
			{
				if (_navTree.Count > 0)
					return _navTree.Last().Last();
				return null;
			}
		}

		public Page LastRoot
		{
			get
			{
				if (_navTree.Count == 0)
					return null;

				return _navTree.Last()[0];
			}
		}


		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/NavigationModel.xml" path="//Member[@MemberName='Modals']/Docs/*" />
		public IReadOnlyList<Page> Modals
		{
			get { return _modalStack.AsReadOnly(); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/NavigationModel.xml" path="//Member[@MemberName='Roots']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/NavigationModel.xml" path="//Member[@MemberName='Tree']/Docs/*" />
		public IReadOnlyList<IReadOnlyList<Page>> Tree
		{
			get { return _navTree; }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/NavigationModel.xml" path="//Member[@MemberName='Clear']/Docs/*" />
		public void Clear()
		{
			_navTree.Clear();
			_modalStack.Clear();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/NavigationModel.xml" path="//Member[@MemberName='InsertPageBefore']/Docs/*" />
		public void InsertPageBefore(Page page, Page before)
		{
			List<Page> currentStack = _navTree.Last();
			int index = currentStack.IndexOf(before);

			if (index == -1)
				throw new ArgumentException("before must be in the current navigation context");

			currentStack.Insert(index, page);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/NavigationModel.xml" path="//Member[@MemberName='Pop']/Docs/*" />
		public Page Pop(Page ancestralNav)
		{
			ancestralNav = ancestralNav.AncestorToRoot();
			foreach (List<Page> stack in _navTree)
			{
				if (stack.Contains(ancestralNav))
				{
					if (stack.Count <= 1)
						throw new InvalidNavigationException("Cannot pop final item in stack");
					Page result = stack.Last();
					stack.Remove(result);
					return result;
				}
			}

			throw new InvalidNavigationException("Popped from unpushed item?");
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/NavigationModel.xml" path="//Member[@MemberName='PopModal']/Docs/*" />
		public Page PopModal()
		{
			if (_navTree.Count <= 1)
				throw new InvalidNavigationException("Can't pop modal without any modals pushed");

			var previousPage = CurrentPage;
			Page modal = _navTree.Last()[0];
			_modalStack.Remove(modal);
			_navTree.Remove(_navTree.Last());

			// Shell handles its own page life cycle events
			// because you can pop multiple pages in a single
			// request
			if (_navTree.Count > 0 &&
				_navTree[0].Count > 0 &&
				_navTree[0][0] is not Shell)
			{
				previousPage.SendNavigatingFrom(new NavigatingFromEventArgs());
				previousPage.SendDisappearing();
				CurrentPage.SendAppearing();
			}

			return modal;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/NavigationModel.xml" path="//Member[@MemberName='PopTopPage']/Docs/*" />
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
			if (_navTree.Last().Count == 0)
			{
				_navTree.RemoveAt(_navTree.Count - 1);
			}
			return itemToRemove;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/NavigationModel.xml" path="//Member[@MemberName='PopToRoot']/Docs/*" />
		public void PopToRoot(Page ancestralNav)
		{
			ancestralNav = ancestralNav.AncestorToRoot();
			foreach (List<Page> stack in _navTree)
			{
				if (stack.Contains(ancestralNav))
				{
					if (stack.Count <= 1)
						throw new InvalidNavigationException("Cannot pop final item in stack");
					stack.RemoveRange(1, stack.Count - 1);
					return;
				}
			}

			throw new InvalidNavigationException("Popped from unpushed item?");
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/NavigationModel.xml" path="//Member[@MemberName='Push']/Docs/*" />
		public void Push(Page page, Page ancestralNav)
		{
			if (ancestralNav == null)
			{
				if (_navTree.Count > 0)
					throw new InvalidNavigationException("Ancestor must be provided for all pushes except first");
				_navTree.Add(new List<Page> { page });
				return;
			}

			ancestralNav = ancestralNav.AncestorToRoot();

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

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/NavigationModel.xml" path="//Member[@MemberName='PushModal']/Docs/*" />
		public void PushModal(Page page)
		{
			var previousPage = CurrentPage;
			_navTree.Add(new List<Page> { page });
			_modalStack.Add(page);

			// Shell handles its own page life cycle events
			// because you can push multiple pages in a single
			// request
			if (_navTree.Count > 0 &&
				_navTree[0].Count > 0 &&
				_navTree[0][0] is not Shell)
			{
				previousPage.SendNavigatingFrom(new NavigatingFromEventArgs());
				previousPage.SendDisappearing();
				page.SendAppearing();
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/NavigationModel.xml" path="//Member[@MemberName='RemovePage']/Docs/*" />
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
	}
}
