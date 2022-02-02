using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation.Metadata;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WButton = Microsoft.UI.Xaml.Controls.Button;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using Microsoft.UI;
using System.Collections.Specialized;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Handlers;

namespace Microsoft.Maui.Controls.Platform
{
	[Microsoft.UI.Xaml.Data.Bindable]
	public class ShellView : RootNavigationView, IAppearanceObserver, IFlyoutBehaviorObserver
	{
		internal static readonly global::Windows.UI.Color DefaultBackgroundColor = global::Windows.UI.Color.FromArgb(255, 3, 169, 244);
		internal static readonly global::Windows.UI.Color DefaultForegroundColor = Microsoft.UI.Colors.White;
		internal static readonly global::Windows.UI.Color DefaultTitleColor = Microsoft.UI.Colors.White;
		internal static readonly global::Windows.UI.Color DefaultUnselectedColor = global::Windows.UI.Color.FromArgb(180, 255, 255, 255);
		Control TogglePaneButton { get; set; }
		WButton NavigationViewBackButton { get; set; }
		Shell _shell;
		Brush _flyoutBackdrop;
		double _flyoutHeight = -1d;
		double _flyoutWidth = -1d;

		FlyoutBehavior _flyoutBehavior;
		List<List<Element>> _flyoutGrouping;
		ShellItemHandler ItemRenderer { get; set; }
		IShellController ShellController => (IShellController)_shell;
		ObservableCollection<object> FlyoutItems = new ObservableCollection<object>();
		IMauiContext MauiContext => Shell.Handler.MauiContext;

		public ShellView()
		{
			_flyoutBackdrop = Brush.Default;
			IsPaneOpen = false;
			MenuItemTemplateSelector = CreateShellFlyoutTemplateSelector();
			ItemInvoked += OnMenuItemInvoked;
			BackRequested += OnBackRequested;

			MenuItemsSource = FlyoutItems;
		}

		async void OnBackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
		{
			try
			{
				await _shell.Navigation.PopAsync();
			}
			catch (Exception exc)
			{
				_shell?.FindMauiContext()?.CreateLogger<ShellView>()?.LogWarning(exc, "Failed to Navigate Back");
			}

			UpdateToolBar();
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			TogglePaneButton = (Control)GetTemplateChild("TogglePaneButton");
			NavigationViewBackButton = (WButton)GetTemplateChild("NavigationViewBackButton");

			UpdatePaneButtonColor(TogglePaneButton, !IsPaneOpen);
			UpdatePaneButtonColor(NavigationViewBackButton, !IsPaneOpen);
			TogglePaneButton?.SetAutomationPropertiesAutomationId("OK");
		}

		void OnPaneOpening(Microsoft.UI.Xaml.Controls.NavigationView sender, object args)
		{
			if (Shell != null)
				Shell.FlyoutIsPresented = true;

			UpdatePaneButtonColor(TogglePaneButton, false);
			UpdatePaneButtonColor(NavigationViewBackButton, false);
			UpdateFlyoutBackgroundColor();
			UpdateFlyoutBackdrop();
			UpdateFlyoutPosition();
			UpdateFlyoutVerticalScrollMode();
		}


		void OnPaneOpened(Microsoft.UI.Xaml.Controls.NavigationView sender, object args)
		{
			// UWP likes to sometimes set the back drop back to the
			// default color
			UpdateFlyoutBackdrop();
		}

		void OnPaneClosing(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewPaneClosingEventArgs args)
		{
			args.Cancel = true;
			if (Shell != null)
				Shell.FlyoutIsPresented = false;
			UpdatePaneButtonColor(TogglePaneButton, true);
			UpdatePaneButtonColor(NavigationViewBackButton, true);
		}

		void OnMenuItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
		{
			var item = args.InvokedItemContainer?.DataContext as Element;
			if (item != null)
				ShellController.OnFlyoutItemSelected(item);
		}


		public void SetElement(VisualElement element)
		{
			if (Element != null && element != null)
				throw new NotSupportedException("Reuse of the Shell Renderer is not supported");

			if (element != null)
			{

				if (ApiInformation.IsEventPresent("Microsoft.UI.Xaml.Controls.NavigationView", "PaneClosing"))
					PaneClosing += OnPaneClosing;
				if (ApiInformation.IsEventPresent("Microsoft.UI.Xaml.Controls.NavigationView", "PaneOpening"))
					PaneOpening += OnPaneOpening;
				if (ApiInformation.IsEventPresent("Microsoft.UI.Xaml.Controls.NavigationView", "PaneOpened"))
					PaneOpened += OnPaneOpened;

				ItemInvoked += OnMenuItemInvoked;
				BackRequested += OnBackRequested;

				Element = (Shell)element;
				Element.SizeChanged += OnElementSizeChanged;
				OnElementSet(Element);
				Element.PropertyChanged += OnElementPropertyChanged;
			}
			else if (Element != null)
			{
				Element.SizeChanged -= OnElementSizeChanged;
				Element.PropertyChanged -= OnElementPropertyChanged;

				if (ApiInformation.IsEventPresent("Microsoft.UI.Xaml.Controls.NavigationView", "PaneClosing"))
					PaneClosing -= OnPaneClosing;
				if (ApiInformation.IsEventPresent("Microsoft.UI.Xaml.Controls.NavigationView", "PaneOpening"))
					PaneOpening -= OnPaneOpening;
				if (ApiInformation.IsEventPresent("Microsoft.UI.Xaml.Controls.NavigationView", "PaneOpened"))
					PaneOpened -= OnPaneOpened;

				ItemInvoked -= OnMenuItemInvoked;
				BackRequested -= OnBackRequested;
			}
		}

		ShellSplitView ShellSplitView => GetTemplateChild("RootSplitView") as ShellSplitView;
		ScrollViewer ShellLeftNavScrollViewer => (ScrollViewer)GetTemplateChild("LeftNavScrollViewer");
		protected internal Shell Element { get; set; }

		internal Shell Shell => Element;

		void OnElementSizeChanged(object sender, EventArgs e)
		{
			InvalidateMeasure();
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.FlyoutVerticalScrollModeProperty.PropertyName)
			{
				UpdateFlyoutVerticalScrollMode();
			}
		}

		void UpdateFlyoutVerticalScrollMode()
		{
			var scrollViewer = ShellLeftNavScrollViewer;
			if (scrollViewer != null)
			{
				switch (Shell.FlyoutVerticalScrollMode)
				{
					case ScrollMode.Disabled:
						scrollViewer.VerticalScrollMode = WScrollMode.Disabled;
						scrollViewer.VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Hidden;
						break;
					case ScrollMode.Enabled:
						scrollViewer.VerticalScrollMode = WScrollMode.Enabled;
						scrollViewer.VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Visible;
						break;
					default:
						scrollViewer.VerticalScrollMode = WScrollMode.Auto;
						scrollViewer.VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Auto;
						break;
				}
			}
		}

		void UpdateFlyoutPosition()
		{
			if (_flyoutBehavior == FlyoutBehavior.Disabled)
				return;

			var splitView = ShellSplitView;
			if (splitView != null)
			{
				splitView.SetFlyoutSizes(_flyoutHeight, _flyoutWidth);
				if (IsPaneOpen)
					ShellSplitView?.RefreshFlyoutPosition();
			}
		}

		void UpdateFlyoutBackdrop()
		{
			if (ShellSplitView != null && _flyoutBehavior != FlyoutBehavior.Flyout)
				return;

			var splitView = ShellSplitView;
			if (splitView != null)
			{
				splitView.FlyoutBackdrop = _flyoutBackdrop;
				if (IsPaneOpen)
					ShellSplitView?.RefreshFlyoutBackdrop();
			}
		}

		protected virtual void OnElementSet(Shell shell)
		{
			if (_shell != null)
			{
				ShellController.ItemsCollectionChanged -= OnItemsCollectionChanged;
			}

			_shell = shell;

			if (shell == null)
				return;

			var shr = CreateShellHeaderView(shell);
			PaneCustomContent = shr;
			PaneFooter = CreateShellFooterView(shell);

			UpdateMenuItemSource();
			SwitchShellItem(shell.CurrentItem, false);
			IsPaneOpen = Shell.FlyoutIsPresented;
			ShellController.AddFlyoutBehaviorObserver(this);
			ShellController.AddAppearanceObserver(this, shell);
			ShellController.ItemsCollectionChanged += OnItemsCollectionChanged;
			ShellController.FlyoutItemsChanged += OnFlyoutItemsChanged;

			_shell.Navigated += OnShellNavigated;
			UpdateToolBar();
		}

		void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
		{
			UpdateToolBar();
		}

		internal void UpdateToolBar()
		{
			if (SelectedItem == null)
				return;


			this.UpdateFlyoutBehavior(Shell);
		}

		void OnFlyoutItemsChanged(object sender, EventArgs e)
		{
			UpdateMenuItemSource();
		}

		void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateMenuItemSource();
		}

		void UpdateMenuItemSource()
		{
			var newGrouping = ((IShellController)Shell).GenerateFlyoutGrouping();
			if (_flyoutGrouping != newGrouping)
			{
				_flyoutGrouping = newGrouping;
				var newItems = IterateItems(newGrouping).ToList();

				foreach (var item in newItems)
				{
					if (!FlyoutItems.Contains(item))
						FlyoutItems.Add(item);
				}

				for (var i = FlyoutItems.Count - 1; i >= 0; i--)
				{
					var item = FlyoutItems[i];
					if (!newItems.Contains(item))
						FlyoutItems.RemoveAt(i);
				}
			}
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
					yield return item;
				}
			}
		}

		class FlyoutItemMenuSeparator : MenuFlyoutSeparator
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
			SelectedItem = newItem;
			var handler = CreateShellItemView();
			if (handler.VirtualView != newItem)
				handler.SetVirtualView(newItem);
		}

		void UpdatePaneButtonColor(Control control, bool overrideColor)
		{
			if (NavigationViewBackButton != null)
			{
				// TODO WINUI
				//var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
				//if (overrideColor)
				//	toggleButton.Foreground = new WSolidColorBrush(titleBar.ButtonForegroundColor.Value);
				//else
				//	toggleButton.ClearValue(Control.ForegroundProperty);
			}
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

				_flyoutBackdrop = appearance.FlyoutBackdrop;

				_flyoutWidth = appearance.FlyoutWidth;
				_flyoutHeight = appearance.FlyoutHeight;
			}

			UpdatePaneButtonColor(TogglePaneButton, !IsPaneOpen);
			UpdatePaneButtonColor(NavigationViewBackButton, !IsPaneOpen);
			UpdateFlyoutBackdrop();
			UpdateFlyoutPosition();
		}

		#endregion IAppearanceObserver

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			_flyoutBehavior = behavior;
			UpdateToolBar();
		}

		public virtual ShellFlyoutTemplateSelector CreateShellFlyoutTemplateSelector() => new ShellFlyoutTemplateSelector();
		public virtual ShellHeaderView CreateShellHeaderView(Shell shell) => new ShellHeaderView(shell);
		public virtual ShellFooterView CreateShellFooterView(Shell shell) => new ShellFooterView(shell);
		ShellItemHandler CreateShellItemView()
		{
			ItemRenderer ??= (ShellItemHandler)Shell.CurrentItem.ToHandler(MauiContext);

			if (ItemRenderer.NativeView != (Content as FrameworkElement))
				Content = ItemRenderer.NativeView;

			if (ItemRenderer.VirtualView != Shell.CurrentItem)
				ItemRenderer.SetVirtualView(Shell.CurrentItem);

			return ItemRenderer;
		}
		//MauiNavigationView CreateShellSectionView() => new ShellSectionView();
	}
}
