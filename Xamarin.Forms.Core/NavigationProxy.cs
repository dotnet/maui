using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NavigationProxy : INavigation
	{
		INavigation _inner;
		Lazy<List<Page>> _modalStack = new Lazy<List<Page>>(() => new List<Page>());

		Lazy<List<Page>> _pushStack = new Lazy<List<Page>>(() => new List<Page>());

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

				if (ReferenceEquals(_inner, null))
				{
					_pushStack = new Lazy<List<Page>>(() => new List<Page>());
					_modalStack = new Lazy<List<Page>>(() => new List<Page>());
				}
				else
				{
					if (_pushStack != null && _pushStack.IsValueCreated)
					{
						foreach (Page page in _pushStack.Value)
						{
							_inner.PushAsync(page);
						}
					}

					if (_modalStack != null && _modalStack.IsValueCreated)
					{
						foreach (Page page in _modalStack.Value)
						{
							_inner.PushModalAsync(page);
						}
					}

					_pushStack = null;
					_modalStack = null;
				}
			}
		}

		public void InsertPageBefore(Page page, Page before)
		{
			OnInsertPageBefore(page, before);
		}

		public IReadOnlyList<Page> ModalStack
		{
			get { return GetModalStack(); }
		}

		public IReadOnlyList<Page> NavigationStack
		{
			get { return GetNavigationStack(); }
		}

		public Task<Page> PopAsync()
		{
			return OnPopAsync(true);
		}

		public Task<Page> PopAsync(bool animated)
		{
			return OnPopAsync(animated);
		}

		public Task<Page> PopModalAsync()
		{
			return OnPopModal(true);
		}

		public Task<Page> PopModalAsync(bool animated)
		{
			return OnPopModal(animated);
		}

		public Task PopToRootAsync()
		{
			return OnPopToRootAsync(true);
		}

		public Task PopToRootAsync(bool animated)
		{
			return OnPopToRootAsync(animated);
		}

		public Task PushAsync(Page root)
		{
			return PushAsync(root, true);
		}

		public Task PushAsync(Page root, bool animated)
		{
			if (root.RealParent != null)
				throw new InvalidOperationException("Page must not already have a parent.");
			return OnPushAsync(root, animated);
		}

		public Task PushModalAsync(Page modal)
		{
			return PushModalAsync(modal, true);
		}

		public Task PushModalAsync(Page modal, bool animated)
		{
			if (modal.RealParent != null)
				throw new InvalidOperationException("Page must not already have a parent.");
			return OnPushModal(modal, animated);
		}

		public void RemovePage(Page page)
		{
			OnRemovePage(page);
		}

		protected virtual IReadOnlyList<Page> GetModalStack()
		{
			INavigation currentInner = Inner;
			return currentInner == null ? _modalStack.Value : currentInner.ModalStack;
		}

		protected virtual IReadOnlyList<Page> GetNavigationStack()
		{
			INavigation currentInner = Inner;
			return currentInner == null ? _pushStack.Value : currentInner.NavigationStack;
		}

		protected virtual void OnInsertPageBefore(Page page, Page before)
		{
			INavigation currentInner = Inner;
			if (currentInner == null)
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
			return inner == null ? Task.FromResult(Pop()) : inner.PopAsync(animated);
		}

		protected virtual Task<Page> OnPopModal(bool animated)
		{
			INavigation innerNav = Inner;
			return innerNav == null ? Task.FromResult(PopModal()) : innerNav.PopModalAsync(animated);
		}

		protected virtual Task OnPopToRootAsync(bool animated)
		{
			INavigation currentInner = Inner;
			if (currentInner == null)
			{
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
			if (currentInner == null)
			{
				_pushStack.Value.Add(page);
				return Task.FromResult(page);
			}
			return currentInner.PushAsync(page, animated);
		}

		protected virtual Task OnPushModal(Page modal, bool animated)
		{
			INavigation currentInner = Inner;
			if (currentInner == null)
			{
				_modalStack.Value.Add(modal);
				return Task.FromResult<object>(null);
			}
			return currentInner.PushModalAsync(modal, animated);
		}

		protected virtual void OnRemovePage(Page page)
		{
			INavigation currentInner = Inner;
			if (currentInner == null)
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
			Page result = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return result;
		}

		Page PopModal()
		{
			List<Page> list = _modalStack.Value;
			Page result = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return result;
		}
	}
}