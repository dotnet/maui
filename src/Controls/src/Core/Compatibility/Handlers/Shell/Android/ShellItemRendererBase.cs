#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Views;
using AndroidX.Fragment.App;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public abstract class ShellItemRendererBase : Fragment, IShellItemRenderer
	{
		#region ShellItemView

		Fragment IShellItemRenderer.Fragment => this;

		ShellItem IShellItemRenderer.ShellItem
		{
			get { return ShellItem; }
			set { ShellItem = value; }
		}

		public event EventHandler Destroyed;

		#endregion IShellItemRenderer

		readonly Dictionary<Element, IShellObservableFragment> _fragmentMap = new Dictionary<Element, IShellObservableFragment>();
		IShellObservableFragment _currentFragment;
		ShellSection _shellSection;
		Page _displayedPage;

		protected ShellItemRendererBase(IShellContext shellContext)
		{
			ShellContext = shellContext;
		}

		protected ShellSection ShellSection
		{
			get => _shellSection;
			set
			{
				if (_shellSection == value)
					return;

				if (_shellSection != null)
				{
					((IShellSectionController)_shellSection).RemoveDisplayedPageObserver(this);
				}

				_shellSection = value;
				if (value != null)
				{
					OnShellSectionChanged();
					((IShellSectionController)ShellSection).AddDisplayedPageObserver(this, UpdateDisplayedPage);
				}
			}
		}

		protected Page DisplayedPage
		{
			get => _displayedPage;
			set
			{
				if (_displayedPage == value)
					return;

				Page oldPage = _displayedPage;
				_displayedPage = value;
				OnDisplayedPageChanged(_displayedPage, oldPage);
			}
		}

		protected IShellContext ShellContext { get; private set; }

		protected ShellItem ShellItem { get; private set; }

		protected virtual IShellObservableFragment CreateFragmentForPage(Page page)
		{
			return ShellContext.CreateFragmentForPage(page);
		}

		internal void Disconnect()
		{
			ShellSection = null;
			DisplayedPage = null;
			ShellContext = null;
		}

		void Destroy()
		{
			foreach (var item in _fragmentMap)
			{
				RemoveFragment(item.Value.Fragment);
				item.Value.Fragment.Dispose();
			}

			_fragmentMap.Clear();

			ShellSection = null;
			DisplayedPage = null;

			Destroyed?.Invoke(this, EventArgs.Empty);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			Destroy();
		}

		protected abstract ViewGroup GetNavigationTarget();

		protected virtual IShellObservableFragment GetOrCreateFragmentForTab(ShellSection shellSection)
		{
			var renderer = ShellContext.CreateShellSectionRenderer(shellSection);
			renderer.ShellSection = shellSection;
			return renderer;
		}

		protected virtual Task<bool> HandleFragmentUpdate(ShellNavigationSource navSource, ShellSection shellSection, Page page, bool animated)
		{
			// We're using RunContinuationsAsynchronously because we don't want a subsequent navigation
			// to start until the current one has finished.
			// The AnimationFinished event is used to signal when the animation has completed,
			// but that doesn't mean the fragment transaction has completed.
			var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

			bool isForCurrentTab = shellSection == ShellSection;
			bool initialUpdate = _fragmentMap.Count == 0;
			if (!_fragmentMap.ContainsKey(ShellSection))
			{
				_fragmentMap[ShellSection] = GetOrCreateFragmentForTab(ShellSection);
			}

			switch (navSource)
			{
				case ShellNavigationSource.Push:
					if (!_fragmentMap.ContainsKey(page))
						_fragmentMap[page] = CreateFragmentForPage(page);
					if (!isForCurrentTab)
						return Task.FromResult(true);
					break;
				case ShellNavigationSource.Insert:
					if (!isForCurrentTab)
						return Task.FromResult(true);
					break;

				case ShellNavigationSource.Pop:
					if (_fragmentMap.TryGetValue(page, out var frag))
					{
						if (ChildFragmentManager.Contains(frag.Fragment) && !isForCurrentTab)
							RemoveFragment(frag.Fragment);
						_fragmentMap.Remove(page);
					}
					if (!isForCurrentTab)
						return Task.FromResult(true);
					break;

				case ShellNavigationSource.Remove:
					if (_fragmentMap.TryGetValue(page, out var removeFragment))
					{
						if (ChildFragmentManager.Contains(removeFragment.Fragment) && !isForCurrentTab && removeFragment != _currentFragment)
							RemoveFragment(removeFragment.Fragment);
						_fragmentMap.Remove(page);
					}

					if (!isForCurrentTab && removeFragment != _currentFragment)
						return Task.FromResult(true);
					break;

				case ShellNavigationSource.PopToRoot:
					RemoveAllPushedPages(shellSection, isForCurrentTab);
					if (!isForCurrentTab)
						return Task.FromResult(true);
					break;

				case ShellNavigationSource.ShellSectionChanged:
					// We need to handle this after we know what the target is
					// because we might accidentally remove an already added target.
					// Then there would be two transactions in a row, one removing and one adding
					// the same fragment and things get really screwy when you do that.
					break;

				default:
					throw new InvalidOperationException("Unexpected navigation type");
			}

			IReadOnlyList<Page> stack = ShellSection.Stack;
			Element targetElement = null;
			IShellObservableFragment target = null;
			if (stack.Count == 1 || navSource == ShellNavigationSource.PopToRoot)
			{
				target = _fragmentMap[ShellSection];
				targetElement = ShellSection;
			}
			else
			{
				targetElement = stack[stack.Count - 1];
				if (!_fragmentMap.ContainsKey(targetElement))
					_fragmentMap[targetElement] = CreateFragmentForPage(targetElement as Page);
				target = _fragmentMap[targetElement];
			}

			// Down here because of comment above
			if (navSource == ShellNavigationSource.ShellSectionChanged)
				RemoveAllButCurrent(target.Fragment);

			if (target == _currentFragment)
				return Task.FromResult(true);

			var t = ChildFragmentManager.BeginTransactionEx();

			if (animated)
				SetupAnimation(navSource, t, page);

			IShellObservableFragment trackFragment = null;
			switch (navSource)
			{
				case ShellNavigationSource.Push:
					trackFragment = target;

					if (_currentFragment != null)
						t.HideEx(_currentFragment.Fragment);

					if (!ChildFragmentManager.Contains(target.Fragment))
						t.AddEx(GetNavigationTarget().Id, target.Fragment);
					t.ShowEx(target.Fragment);
					break;

				case ShellNavigationSource.ShellSectionChanged:
					if (_currentFragment != null)
						t.HideEx(_currentFragment.Fragment);

					if (!ChildFragmentManager.Contains(target.Fragment))
						t.AddEx(GetNavigationTarget().Id, target.Fragment);

					t.ShowEx(target.Fragment);
					break;
				case ShellNavigationSource.Pop:
				case ShellNavigationSource.PopToRoot:
				case ShellNavigationSource.Remove:
					trackFragment = _currentFragment;

					if (_currentFragment != null)
						t.RemoveEx(_currentFragment.Fragment);

					if (!ChildFragmentManager.Contains(target.Fragment))
						t.AddEx(GetNavigationTarget().Id, target.Fragment);
					t.ShowEx(target.Fragment);
					break;
			}

			if (animated && trackFragment != null)
			{
				GetNavigationTarget().SetBackgroundColor(Colors.Black.ToPlatform());
				void callback(object s, EventArgs e)
				{
					trackFragment.AnimationFinished -= callback;
					result.TrySetResult(true);
					GetNavigationTarget().SetBackground(null);
				}
				trackFragment.AnimationFinished += callback;
			}
			else
			{
				result.TrySetResult(true);
			}

			if (initialUpdate)
			{
				t.SetReorderingAllowedEx(true);
			}

			t.CommitAllowingStateLossEx();
			_currentFragment = target;


			return result.Task;
		}

		protected virtual void HookEvents(ShellItem shellItem)
		{
			shellItem.PropertyChanged += OnShellItemPropertyChanged;
			((IShellItemController)shellItem).ItemsCollectionChanged += OnShellItemsChanged;
			ShellSection = shellItem.CurrentItem;

			foreach (var shellContent in ((IShellItemController)shellItem).GetItems())
			{
				HookChildEvents(shellContent);
			}
		}

		protected virtual void HookChildEvents(ShellSection shellSection)
		{
			((IShellSectionController)shellSection).NavigationRequested += OnNavigationRequested;
			shellSection.PropertyChanged += OnShellSectionPropertyChanged;
		}

		protected virtual void OnShellSectionChanged()
		{
			HandleFragmentUpdate(ShellNavigationSource.ShellSectionChanged, ShellSection, null, false).FireAndForget();
		}

		protected virtual void OnDisplayedPageChanged(Page newPage, Page oldPage)
		{

		}

		protected virtual void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
		{
			e.Task = HandleFragmentUpdate((ShellNavigationSource)e.RequestType, (ShellSection)sender, e.Page, e.Animated);
		}

		protected virtual void OnShellItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellItem.CurrentItemProperty.PropertyName)
				ShellSection = ShellItem.CurrentItem;
		}

		protected virtual void OnShellItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (ShellSection shellSection in e.OldItems)
					UnhookChildEvents(shellSection);
			}

			if (e.NewItems != null)
			{
				foreach (ShellSection shellSection in e.NewItems)
					HookChildEvents(shellSection);
			}
		}

		protected virtual void SetupAnimation(ShellNavigationSource navSource, FragmentTransaction t, Page page)
		{
			switch (navSource)
			{
				case ShellNavigationSource.Push:
					t.SetCustomAnimations(Controls.Resource.Animation.enterfromright, Controls.Resource.Animation.exittoleft);
					break;

				case ShellNavigationSource.Pop:
				case ShellNavigationSource.PopToRoot:
					t.SetCustomAnimations(Controls.Resource.Animation.enterfromleft, Controls.Resource.Animation.exittoright);
					break;

				case ShellNavigationSource.ShellSectionChanged:
					break;
			}
		}

		protected virtual void UnhookEvents(ShellItem shellItem)
		{
			foreach (var shellSection in ((IShellItemController)shellItem).GetItems())
			{
				UnhookChildEvents(shellSection);
			}

			((IShellItemController)shellItem).ItemsCollectionChanged -= OnShellItemsChanged;
			ShellItem.PropertyChanged -= OnShellItemPropertyChanged;
			ShellSection = null;
		}

		protected virtual void UnhookChildEvents(ShellSection shellSection)
		{
			((IShellSectionController)shellSection).NavigationRequested -= OnNavigationRequested;
			shellSection.PropertyChanged -= OnShellSectionPropertyChanged;
		}

		protected virtual void OnShellSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
		}

		void UpdateDisplayedPage(Page page)
		{
			DisplayedPage = page;
		}

		void RemoveAllButCurrent(Fragment skip)
		{
			FragmentTransaction trans = null;
			foreach (var kvp in _fragmentMap)
			{
				var f = kvp.Value.Fragment;
				if (kvp.Value == _currentFragment || kvp.Value.Fragment == skip || !f.IsAdded)
					continue;

				trans ??= ChildFragmentManager.BeginTransactionEx();
				trans.Remove(f);
			}
			;

			trans?.CommitAllowingStateLossEx();
		}

		void RemoveAllPushedPages(ShellSection shellSection, bool keepCurrent)
		{
			if (shellSection.Stack.Count <= 1 || (keepCurrent && shellSection.Stack.Count == 2))
				return;

			var t = ChildFragmentManager.BeginTransactionEx();

			foreach (var kvp in _fragmentMap.ToList())
			{
				if (kvp.Key.Parent != shellSection)
					continue;

				_fragmentMap.Remove(kvp.Key);

				if (keepCurrent && kvp.Value.Fragment == _currentFragment)
					continue;

				t.RemoveEx(kvp.Value.Fragment);
			}

			t.CommitAllowingStateLossEx();
		}

		void RemoveFragment(Fragment fragment)
		{
			var t = ChildFragmentManager.BeginTransactionEx();
			t.RemoveEx(fragment);
			t.CommitAllowingStateLossEx();
		}
	}
}