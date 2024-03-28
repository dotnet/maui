#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	[Microsoft.UI.Xaml.Data.Bindable]
	public class ShellView : RootNavigationView, IAppearanceObserver, IFlyoutBehaviorObserver
	{
		internal static readonly global::Windows.UI.Color DefaultBackgroundColor = global::Windows.UI.Color.FromArgb(255, 3, 169, 244);
		internal static readonly global::Windows.UI.Color DefaultForegroundColor = Microsoft.UI.Colors.White;
		internal static readonly global::Windows.UI.Color DefaultTitleColor = Microsoft.UI.Colors.White;
		internal static readonly global::Windows.UI.Color DefaultUnselectedColor = global::Windows.UI.Color.FromArgb(180, 255, 255, 255);
		double _flyoutHeight = -1d;
		double _flyoutWidth = -1d;

		List<List<Element>> _flyoutGrouping;
		ShellItemHandler ItemRenderer { get; set; }
		IShellController ShellController => (IShellController)Element;
		ObservableCollection<object> FlyoutItems = new ObservableCollection<object>();
		IMauiContext MauiContext => Element.Handler.MauiContext;
		ShellSplitView _shellSplitView;
		Brush _flyoutBackdrop;

		public ShellView()
		{
			IsPaneOpen = false;
			MenuItemTemplateSelector = CreateShellFlyoutTemplateSelector();
			MenuItemsSource = FlyoutItems;
			this.Loaded += OnLoaded;
		}

		void OnLoaded(object sender, RoutedEventArgs e)
		{
			// We can't reliably set IsPaneOpen to true until the control has loaded
			// If we set it earlier than this then WinUI will transition it back to false
			if (IsPaneOpen != Element.FlyoutIsPresented)
				IsPaneOpen = Element.FlyoutIsPresented;

			UpdateFlyoutBackdrop();
		}

		private protected override void UpdateFlyoutCustomContent()
		{
			base.UpdateFlyoutCustomContent();

			if (MauiNavigationView.FlyoutCustomContent == null)
				MenuItemsSource = FlyoutItems;
			else
				MenuItemsSource = null;
		}

		internal void SetElement(VisualElement element)
		{
			if (Element != null && element != null)
				throw new NotSupportedException("Reuse of the Shell Renderer is not supported");

			Element = (Shell)element;
			ShellController.AddAppearanceObserver(this, Element);
		}

		internal Shell Element { get; set; }

		private protected override void OnApplyTemplateCore()
		{
			_shellSplitView = new ShellSplitView(RootSplitView);
			_shellSplitView.FlyoutBackdrop = _flyoutBackdrop;
			TogglePaneButton?.SetAutomationPropertiesAutomationId("OK");

			base.OnApplyTemplateCore();
			UpdateFlyoutBackdrop();
		}

		internal void UpdateFlyoutPosition()
		{
			if (Element.FlyoutBehavior == FlyoutBehavior.Disabled)
				return;

			var splitView = _shellSplitView;
			if (splitView != null)
			{
				_shellSplitView.SetFlyoutSizes(_flyoutHeight, _flyoutWidth);
				if (IsPaneOpen)
					_shellSplitView.RefreshFlyoutPosition();
			}
		}

		internal void UpdateFlyoutBackdrop()
		{
			if (RootSplitView != null && Element.FlyoutBehavior != FlyoutBehavior.Flyout)
				return;

			var splitView = _shellSplitView;
			if (splitView != null)
			{
				if (IsPaneOpen)
					_shellSplitView.RefreshFlyoutBackdrop();
			}
		}

		internal Brush FlyoutBackdrop
		{
			set
			{
				_flyoutBackdrop = value;

				if (_shellSplitView != null)
					_shellSplitView.FlyoutBackdrop = value;
			}
		}

		internal void UpdateMenuItemSource()
		{
			var newGrouping = ((IShellController)Element).GenerateFlyoutGrouping();
			if (_flyoutGrouping != newGrouping)
			{
				_flyoutGrouping = newGrouping;
				var newItems = IterateItems(newGrouping).ToList();

				foreach (var item in newItems)
				{
					if (!FlyoutItems.Contains(item))
					{
						FlyoutItems.Add(item);
					}
				}

				for (var i = FlyoutItems.Count - 1; i >= 0; i--)
				{
					var item = FlyoutItems[i];
					if (!newItems.Contains(item))
						FlyoutItems.RemoveAt(i);
				}
			}

			if (!FlyoutItems.Contains(SelectedItem))
				SelectedItem = null;
		}

		IEnumerable<object> IterateItems(List<List<Element>> groups)
		{
			int separatorNumber = 0;
			foreach (var group in groups)
			{
				if (group.Count > 0 && group != groups[0])
				{
					yield return new FlyoutItemMenuSeparator(separatorNumber++); // Creates a separator
				}

				foreach (var item in group)
				{
					bool foundExistingVM = false;

					// Check to see if this element already has a VM counter part
					foreach (var navItem in FlyoutItems)
					{
						if (navItem is NavigationViewItemViewModel viewModel && viewModel.Data == item)
						{
							foundExistingVM = true;
							yield return viewModel;
						}
					}

					if (!foundExistingVM)
					{
						yield return new NavigationViewItemViewModel()
						{
							Data = item
						};
					}
				}
			}
		}

		class FlyoutItemMenuSeparator : Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator
		{
			public FlyoutItemMenuSeparator(int separatorNumber)
			{
				Id = separatorNumber;
			}

			public int Id { get; set; }
			public override int GetHashCode() => Id.GetHashCode();
			public override bool Equals(object obj) =>
				obj is FlyoutItemMenuSeparator fim && fim.Id == Id;
		}

		internal void SwitchShellItem(ShellItem newItem, bool animate = true)
		{
			var navItems = FlyoutItems.OfType<NavigationViewItemViewModel>();

			// Implicit items aren't items that are surfaced to the user 
			// or data structures. So, we just want to find the element
			// the user defined on Shell
			if (Routing.IsImplicit(newItem))
			{
				if (Routing.IsImplicit(newItem.CurrentItem))
					SelectedItem = navItems.GetWithData(newItem.CurrentItem.CurrentItem);
				else
					SelectedItem = navItems.GetWithData(newItem.CurrentItem);
			}
			else
			{
				if (navItems.TryGetWithData(newItem, out NavigationViewItemViewModel vm1))
					SelectedItem = vm1;
				else if (newItem.CurrentItem is not null)
				{
					if (navItems.TryGetWithData(newItem.CurrentItem, out NavigationViewItemViewModel vm2))
						SelectedItem = vm2;
					else if (navItems.TryGetWithData(newItem.CurrentItem.CurrentItem, out NavigationViewItemViewModel vm3))
						SelectedItem = vm3;
				}
			}

			var handler = CreateShellItemView();
			if (handler.VirtualView != newItem)
				handler.SetVirtualView(newItem);
		}

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			global::Windows.UI.Color backgroundColor = DefaultBackgroundColor;
			global::Windows.UI.Color titleColor = DefaultTitleColor;

			if (appearance != null)
			{
				if (!appearance.BackgroundColor.IsDefault())
					backgroundColor = appearance.BackgroundColor.ToWindowsColor();
				if (!appearance.TitleColor.IsDefault())
					titleColor = appearance.TitleColor.ToWindowsColor();

				_flyoutWidth = appearance.FlyoutWidth;
				_flyoutHeight = appearance.FlyoutHeight;
			}

			UpdateFlyoutBackdrop();
			UpdateFlyoutPosition();
		}

		#endregion IAppearanceObserver

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
		}

		public virtual ShellFlyoutTemplateSelector CreateShellFlyoutTemplateSelector() => new ShellFlyoutTemplateSelector();

		ShellItemHandler CreateShellItemView()
		{
			if (ItemRenderer == null)
			{
				ItemRenderer = (ShellItemHandler)Element.CurrentItem.ToHandler(MauiContext);
				if (ItemRenderer.PlatformView is NavigationView nv)
				{
					nv.SelectionChanged += TabSelectionChanged;
				}
			}

			if (ItemRenderer.PlatformView != (Content as FrameworkElement))
				Content = ItemRenderer.PlatformView;

			if (ItemRenderer.VirtualView != Element.CurrentItem)
				ItemRenderer.SetVirtualView(Element.CurrentItem);

			return ItemRenderer;
		}

		void TabSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) =>
			Element.Handler.UpdateValue(nameof(Shell.CurrentItem));
	}
}
