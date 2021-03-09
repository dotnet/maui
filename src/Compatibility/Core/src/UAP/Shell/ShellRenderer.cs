using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation.Metadata;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using Microsoft.UI;
using System.Collections.Specialized;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[Microsoft.UI.Xaml.Data.Bindable]
	public class ShellRenderer : Microsoft.UI.Xaml.Controls.NavigationView, IVisualElementRenderer, IAppearanceObserver, IFlyoutBehaviorObserver
	{
		public static readonly DependencyProperty FlyoutBackgroundColorProperty = DependencyProperty.Register(
			nameof(FlyoutBackgroundColor), typeof(Brush), typeof(ShellRenderer),
			new PropertyMetadata(default(Brush)));

		internal static readonly Windows.UI.Color DefaultBackgroundColor = Windows.UI.Color.FromArgb(255, 3, 169, 244);
		internal static readonly Windows.UI.Color DefaultForegroundColor = Microsoft.UI.Colors.White;
		internal static readonly Windows.UI.Color DefaultTitleColor = Microsoft.UI.Colors.White;
		internal static readonly Windows.UI.Color DefaultUnselectedColor = Windows.UI.Color.FromArgb(180, 255, 255, 255);
		const string TogglePaneButton = "TogglePaneButton";
		const string NavigationViewBackButton = "NavigationViewBackButton";
		internal const string ShellStyle = "ShellNavigationView";
		Shell _shell;
		Brush _flyoutBackdrop;
		double _flyoutHeight = -1d;
		double _flyoutWidth = -1d;

		FlyoutBehavior _flyoutBehavior;
		List<List<Element>> _flyoutGrouping;
		ShellItemRenderer ItemRenderer { get; }
		IShellController ShellController => (IShellController)_shell;
		ObservableCollection<object> FlyoutItems = new ObservableCollection<object>();

		public ShellRenderer()
		{
			Microsoft.Maui.Controls.Shell.VerifyShellUWPFlagEnabled(nameof(ShellRenderer));
			_flyoutBackdrop = Brush.Default;
			IsSettingsVisible = false;
			PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;
			IsPaneOpen = false;
			Content = ItemRenderer = CreateShellItemRenderer();
			MenuItemTemplateSelector = CreateShellFlyoutTemplateSelector();
			ItemInvoked += OnMenuItemInvoked;
			BackRequested += OnBackRequested;
			//Style = Microsoft.UI.Xaml.Application.Current.Resources["ShellNavigationView"] as Microsoft.UI.Xaml.Style;
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
				Internals.Log.Warning(nameof(Shell), $"Failed to Navigate Back: {exc}");
			}

			UpdateToolBar();
		}

		public WBrush FlyoutBackgroundColor
		{
			get => (WBrush)GetValue(FlyoutBackgroundColorProperty);
			set => SetValue(FlyoutBackgroundColorProperty, value);
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			UpdatePaneButtonColor(TogglePaneButton, !IsPaneOpen);
			UpdatePaneButtonColor(NavigationViewBackButton, !IsPaneOpen);
			(GetTemplateChild(TogglePaneButton) as FrameworkElement)?.SetAutomationPropertiesAutomationId("OK");
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

		#region IVisualElementRenderer

		event EventHandler<VisualElementChangedEventArgs> _elementChanged;

		event EventHandler<VisualElementChangedEventArgs> IVisualElementRenderer.ElementChanged
		{
			add { _elementChanged += value; }
			remove { _elementChanged -= value; }
		}

		FrameworkElement IVisualElementRenderer.ContainerElement => this;

		VisualElement IVisualElementRenderer.Element => Element;

		SizeRequest IVisualElementRenderer.GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var constraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);

			double oldWidth = Width;
			double oldHeight = Height;

			Height = double.NaN;
			Width = double.NaN;

			Measure(constraint);
			var result = new Size(Math.Ceiling(DesiredSize.Width), Math.Ceiling(DesiredSize.Height));

			Width = oldWidth;
			Height = oldHeight;

			return new SizeRequest(result);
		}

		public UIElement GetNativeElement() => null;

		public void Dispose()
		{
			SetElement(null);
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
				ItemRenderer.SetShellContext(this);
				_elementChanged?.Invoke(this, new VisualElementChangedEventArgs(null, Element));
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

		#endregion IVisualElementRenderer
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
			if (e.PropertyName == Shell.CurrentItemProperty.PropertyName)
			{
				SwitchShellItem(Element.CurrentItem);
			}
			else if (e.PropertyName == Shell.FlyoutIsPresentedProperty.PropertyName)
			{
				IsPaneOpen = Shell.FlyoutIsPresented;
			}
			else if (e.PropertyName == Shell.FlyoutBackgroundColorProperty.PropertyName)
			{
				UpdateFlyoutBackgroundColor();
			}
			else if (e.PropertyName == Shell.FlyoutVerticalScrollModeProperty.PropertyName)
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
						scrollViewer.VerticalScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode.Disabled;
						scrollViewer.VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Hidden;
						break;
					case ScrollMode.Enabled:
						scrollViewer.VerticalScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode.Enabled;
						scrollViewer.VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Visible;
						break;
					default:
						scrollViewer.VerticalScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode.Auto;
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

		protected virtual void UpdateFlyoutBackgroundColor()
		{
			if (_shell.FlyoutBackgroundColor == Color.Default)
			{
				object color = null;
				if (IsPaneOpen)
					color = Resources["NavigationViewExpandedPaneBackground"];
				else
					color = Resources["NavigationViewDefaultPaneBackground"];


				if (color is WBrush brush)
					FlyoutBackgroundColor = brush;
				else if (color is Windows.UI.Color uiColor)
					new WSolidColorBrush(uiColor);
			}
			else
				FlyoutBackgroundColor = _shell.FlyoutBackgroundColor.ToBrush();
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

			var shr = CreateShellHeaderRenderer(shell);
			PaneCustomContent = shr;
			PaneFooter = CreateShellFooterRenderer(shell);

			UpdateMenuItemSource();
			SwitchShellItem(shell.CurrentItem, false);
			IsPaneOpen = Shell.FlyoutIsPresented;
			ShellController.AddFlyoutBehaviorObserver(this);
			ShellController.AddAppearanceObserver(this, shell);
			ShellController.ItemsCollectionChanged += OnItemsCollectionChanged;
			ShellController.FlyoutItemsChanged += OnFlyoutItemsChanged;
			UpdateFlyoutBackgroundColor();

			_shell.Navigated += OnShellNavigated;
			UpdateToolBar();
		}
		
		void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
		{
			UpdateToolBar();
		}

		void UpdateToolBar()
		{
			if (SelectedItem == null)
				return;

			if(_shell.Navigation.NavigationStack.Count > 1)
			{
				IsBackEnabled = true;
				IsBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Visible;
			}
			else
			{
				IsBackEnabled = false;
				IsBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed;
			}

			switch (_flyoutBehavior)
			{
				case FlyoutBehavior.Disabled:
					IsPaneVisible = IsBackEnabled;
					IsPaneToggleButtonVisible = !IsBackEnabled;
					PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;
					IsPaneOpen = false;
					break;

				case FlyoutBehavior.Flyout:
					IsPaneVisible = true;
					IsPaneToggleButtonVisible = !IsBackEnabled;
					bool shouldOpen = Shell.FlyoutIsPresented;
					PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal; //This will trigger opening the flyout
					IsPaneOpen = shouldOpen;
					break;

				case FlyoutBehavior.Locked:
					IsPaneVisible = true;
					IsPaneToggleButtonVisible = false;
					PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Left;
					break;
			}
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

		void SwitchShellItem(ShellItem newItem, bool animate = true)
		{
			SelectedItem = newItem;
			ItemRenderer.NavigateToShellItem(newItem, animate);
		}

		void UpdatePaneButtonColor(string name, bool overrideColor)
		{
			var toggleButton = GetTemplateChild(name) as Control;
			if (toggleButton != null)
			{
				var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
				if (overrideColor)
					toggleButton.Foreground = new WSolidColorBrush(titleBar.ButtonForegroundColor.Value);
				else
					toggleButton.ClearValue(Control.ForegroundProperty);
			}
		}

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			Windows.UI.Color backgroundColor = DefaultBackgroundColor;
			Windows.UI.Color titleColor = DefaultTitleColor;
			if (appearance != null)
			{
				if (!appearance.BackgroundColor.IsDefault)
					backgroundColor = appearance.BackgroundColor.ToWindowsColor();
				if (!appearance.TitleColor.IsDefault)
					titleColor = appearance.TitleColor.ToWindowsColor();

				_flyoutBackdrop = appearance.FlyoutBackdrop;

				_flyoutWidth = appearance.FlyoutWidth;
				_flyoutHeight = appearance.FlyoutHeight;
			}

			var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
			titleBar.BackgroundColor = titleBar.ButtonBackgroundColor = backgroundColor;
			titleBar.ForegroundColor = titleBar.ButtonForegroundColor = titleColor;
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
		public virtual ShellHeaderRenderer CreateShellHeaderRenderer(Shell shell) => new ShellHeaderRenderer(shell);
		public virtual ShellFooterRenderer CreateShellFooterRenderer(Shell shell) => new ShellFooterRenderer(shell);
		public virtual ShellItemRenderer CreateShellItemRenderer() => new ShellItemRenderer(this);
		public virtual ShellSectionRenderer CreateShellSectionRenderer() => new ShellSectionRenderer();
	}
}
