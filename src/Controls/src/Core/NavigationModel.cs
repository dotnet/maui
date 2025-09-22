#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	internal class NavigationModel
	{
		readonly List<Page> _modalStack = new List<Page>();
		readonly List<List<Page>> _navTree = new List<List<Page>>();

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
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


		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public IReadOnlyList<Page> Modals
		{
			get { return _modalStack.AsReadOnly(); }
		}

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
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

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public IReadOnlyList<IReadOnlyList<Page>> Tree
		{
			get { return _navTree; }
		}

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public void Clear()
		{
			_navTree.Clear();
			_modalStack.Clear();
		}

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <param name="page">Internal parameter for platform use.</param>
		/// <param name="before">Internal parameter for platform use.</param>
		public void InsertPageBefore(Page page, Page before)
		{
			List<Page> currentStack = _navTree.Last();
			int index = currentStack.IndexOf(before);

			if (index == -1)
				throw new ArgumentException("before must be in the current navigation context");

			currentStack.Insert(index, page);
		}

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <param name="ancestralNav">Internal parameter for platform use.</param>
		/// <returns>For internal use by the Microsoft.Maui.Controls platform.</returns>
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

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <returns>For internal use by the Microsoft.Maui.Controls platform.</returns>
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
				previousPage.SendNavigatingFrom(new NavigatingFromEventArgs(CurrentPage, NavigationType.Pop));
				previousPage.SendDisappearing();
				CurrentPage.SendAppearing();
			}

			return modal;
		}

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <returns>For internal use by the Microsoft.Maui.Controls platform.</returns>
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

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <param name="ancestralNav">Internal parameter for platform use.</param>
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

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <param name="page">Internal parameter for platform use.</param>
		/// <param name="ancestralNav">Internal parameter for platform use.</param>
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

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <param name="page">Internal parameter for platform use.</param>
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
				previousPage.SendNavigatingFrom(new NavigatingFromEventArgs(page, NavigationType.Push));
				previousPage.SendDisappearing();
				page.SendAppearing();
			}
		}

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <param name="page">Internal parameter for platform use.</param>
		/// <returns>For internal use by the Microsoft.Maui.Controls platform.</returns>
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
