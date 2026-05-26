#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface INavigationProxy
	{
		NavigationProxy NavigationProxy { get; }
	}

	/// <summary>Represents an object capable of handling stack-based navigation via proxying.</summary>
	/// <remarks>
	///		<para>Elements may use navigation proxies to delegate navigation capabilities to their parents if they themselves can't handle it.</para>
	///		<para>For internal use for .NET MAUI.</para>
	///	</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NavigationProxy : INavigation
	{
		INavigation _inner;
		Lazy<NavigatingStepRequestList> _modalStack = new Lazy<NavigatingStepRequestList>(() => new NavigatingStepRequestList());

		Lazy<List<Page>> _pushStack = new Lazy<List<Page>>(() => new List<Page>());

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public INavigation Inner
		{
			get { return _inner; }
			set
			{
				if (_inner == value)
					return;
				_inner = value;
				// reverse so that things go into the new stack in the same order
				// null out to release memory that will likely never be needed again

				if (_inner is null)
				{
					_pushStack = new Lazy<List<Page>>(() => new List<Page>());
					_modalStack = new Lazy<NavigatingStepRequestList>(() => new NavigatingStepRequestList());
				}
				else
				{
					if (_pushStack is not null && _pushStack.IsValueCreated)
					{
						foreach (Page page in _pushStack.Value)
						{
							_inner.PushAsync(page);
						}
					}

					if (_modalStack is not null && _modalStack.IsValueCreated)
					{
						foreach (var request in _modalStack.Value)
						{
							_inner.PushModalAsync(request.Page, request.IsAnimated);
						}
					}

					_pushStack = null;
					_modalStack = null;
				}
			}
		}

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <param name="page">Internal parameter for platform use.</param>
		/// <param name="before">Internal parameter for platform use.</param>
		public void InsertPageBefore(Page page, Page before)
		{
			OnInsertPageBefore(page, before);
		}

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public IReadOnlyList<Page> ModalStack
		{
			get { return GetModalStack(); }
		}

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public IReadOnlyList<Page> NavigationStack
		{
			get { return GetNavigationStack(); }
		}

		/// <summary>Pops the current page from the navigation stack with animation.</summary>
		/// <returns>The page that was popped.</returns>
		public Task<Page> PopAsync()
		{
			return OnPopAsync(true);
		}

		/// <summary>Pops the current page from the navigation stack.</summary>
		/// <param name="animated">Whether to animate the transition.</param>
		/// <returns>The page that was popped.</returns>
		public Task<Page> PopAsync(bool animated)
		{
			return OnPopAsync(animated);
		}

		/// <summary>Pops the current modal page with animation.</summary>
		/// <returns>The modal page that was popped.</returns>
		public Task<Page> PopModalAsync()
		{
			return OnPopModal(true);
		}

		/// <summary>Pops the current modal page.</summary>
		/// <param name="animated">Whether to animate the transition.</param>
		/// <returns>The modal page that was popped.</returns>
		public Task<Page> PopModalAsync(bool animated)
		{
			return OnPopModal(animated);
		}

		/// <summary>Pops all pages except the root page with animation.</summary>
		/// <returns>A task representing the operation.</returns>
		public Task PopToRootAsync()
		{
			return OnPopToRootAsync(true);
		}

		/// <summary>Pops all pages except the root page.</summary>
		/// <param name="animated">Whether to animate the transition.</param>
		/// <returns>A task representing the operation.</returns>
		public Task PopToRootAsync(bool animated)
		{
			return OnPopToRootAsync(animated);
		}

		/// <summary>Pushes a page onto the navigation stack with animation.</summary>
		/// <param name="root">The page to push.</param>
		/// <returns>A task representing the operation.</returns>
		public Task PushAsync(Page root)
		{
			return PushAsync(root, true);
		}

		/// <summary>Pushes a page onto the navigation stack.</summary>
		/// <param name="root">The page to push.</param>
		/// <param name="animated">Whether to animate the transition.</param>
		/// <returns>A task representing the operation.</returns>
		public Task PushAsync(Page root, bool animated)
		{
			if (root.RealParent is not null)
				throw new InvalidOperationException("Page must not already have a parent.");
			return OnPushAsync(root, animated);
		}

		/// <summary>Presents a page modally with animation.</summary>
		/// <param name="modal">The page to present modally.</param>
		/// <returns>A task representing the operation.</returns>
		public Task PushModalAsync(Page modal)
		{
			return PushModalAsync(modal, true);
		}

		/// <summary>Presents a page modally.</summary>
		/// <param name="modal">The page to present modally.</param>
		/// <param name="animated">Whether to animate the transition.</param>
		/// <returns>A task representing the operation.</returns>
		public Task PushModalAsync(Page modal, bool animated)
		{
			if (modal.RealParent is not null && modal.RealParent is not IWindow)
				throw new InvalidOperationException("Page must not already have a parent.");
			return OnPushModal(modal, animated);
		}

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <param name="page">Internal parameter for platform use.</param>
		public void RemovePage(Page page)
		{
			OnRemovePage(page);
		}

		protected virtual IReadOnlyList<Page> GetModalStack()
		{
			INavigation currentInner = Inner;
			return currentInner is null ? _modalStack.Value.Pages : currentInner.ModalStack;
		}

		protected virtual IReadOnlyList<Page> GetNavigationStack()
		{
			INavigation currentInner = Inner;
			return currentInner is null ? _pushStack.Value : currentInner.NavigationStack;
		}

		protected virtual void OnInsertPageBefore(Page page, Page before)
		{
			INavigation currentInner = Inner;
			if (currentInner is null)
			{
				int index = _pushStack.Value.IndexOf(before);
				if (index == -1)
					throw new ArgumentException("before must be in the pushed stack of the current context");
				_pushStack.Value.Insert(index, page);
			}
			else
			{
				currentInner.InsertPageBefore(page, before);
			}
		}

		protected virtual Task<Page> OnPopAsync(bool animated)
		{
			INavigation inner = Inner;
			return inner is null ? Task.FromResult(Pop()) : inner.PopAsync(animated);
		}

		protected virtual Task<Page> OnPopModal(bool animated)
		{
			INavigation innerNav = Inner;
			return innerNav is null ? Task.FromResult(PopModal()) : innerNav.PopModalAsync(animated);
		}

		protected virtual Task OnPopToRootAsync(bool animated)
		{
			INavigation currentInner = Inner;
			if (currentInner is null)
			{
				if (_pushStack.Value.Count == 0)
					return Task.FromResult<Page>(null);

				Page root = _pushStack.Value.Last();
				_pushStack.Value.Clear();
				_pushStack.Value.Add(root);
				return Task.FromResult(root);
			}
			return currentInner.PopToRootAsync(animated);
		}

		protected virtual Task OnPushAsync(Page page, bool animated)
		{
			INavigation currentInner = Inner;
			if (currentInner is null)
			{
				_pushStack.Value.Add(page);
				return Task.FromResult(page);
			}
			return currentInner.PushAsync(page, animated);
		}

		protected virtual Task OnPushModal(Page modal, bool animated)
		{
			INavigation currentInner = Inner;
			if (currentInner is null)
			{
				_modalStack.Value.Add(new NavigationStepRequest(modal, true, animated));
				return Task.FromResult<object>(null);
			}
			return currentInner.PushModalAsync(modal, animated);
		}

		protected virtual void OnRemovePage(Page page)
		{
			INavigation currentInner = Inner;
			if (currentInner is null)
			{
				_pushStack.Value.Remove(page);
			}
			else
			{
				currentInner.RemovePage(page);
			}
		}

		Page Pop()
		{
			List<Page> list = _pushStack.Value;
			if (list.Count == 0)
				return null;
			Page result = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return result;
		}

		Page PopModal()
		{
			var list = _modalStack.Value;
			if (list.Count == 0)
				return null;
			var result = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return result.Page;
		}
	}
}