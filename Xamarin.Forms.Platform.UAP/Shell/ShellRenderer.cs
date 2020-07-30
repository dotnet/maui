using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WBrush = Windows.UI.Xaml.Media.Brush;
using WSolidColorBrush = Windows.UI.Xaml.Media.SolidColorBrush;

namespace Xamarin.Forms.Platform.UWP
{
	[Windows.UI.Xaml.Data.Bindable]
	public class ShellRenderer : Microsoft.UI.Xaml.Controls.NavigationView, IVisualElementRenderer, IAppearanceObserver, IFlyoutBehaviorObserver
	{
		public static readonly DependencyProperty FlyoutBackgroundColorProperty = DependencyProperty.Register(
			nameof(FlyoutBackgroundColor), typeof(Brush), typeof(ShellRenderer),
			new PropertyMetadata(default(Brush)));

		internal static readonly Windows.UI.Color DefaultBackgroundColor = Windows.UI.Color.FromArgb(255, 3, 169, 244);
		internal static readonly Windows.UI.Color DefaultForegroundColor = Windows.UI.Colors.White;
		internal static readonly Windows.UI.Color DefaultTitleColor = Windows.UI.Colors.White;
		internal static readonly Windows.UI.Color DefaultUnselectedColor = Windows.UI.Color.FromArgb(180, 255, 255, 255);
		const string TogglePaneButton = "TogglePaneButton";
		const string NavigationViewBackButton = "NavigationViewBackButton";
		internal const string ShellStyle = "ShellNavigationView";
		Shell _shell;
		Brush _flyoutBackdrop;
		FlyoutBehavior _flyoutBehavior;
		ShellItemRenderer ItemRenderer { get; }
		IShellController ShellController => (IShellController)_shell;

		public ShellRenderer()
		{
			Xamarin.Forms.Shell.VerifyShellUWPFlagEnabled(nameof(ShellRenderer));
			_flyoutBackdrop = Brush.Default;
			IsSettingsVisible = false;
			PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;
			IsPaneOpen = false;
			Content = ItemRenderer = CreateShellItemRenderer();
			MenuItemTemplateSelector = CreateShellFlyoutTemplateSelector();
			if (ApiInformation.IsEventPresent("Windows.UI.Xaml.Controls.NavigationView", "PaneClosing"))
				PaneClosing += (s, e) => OnPaneClosed();
			if (ApiInformation.IsEventPresent("Windows.UI.Xaml.Controls.NavigationView", "PaneOpening"))
				PaneOpening += (s, e) => OnPaneOpening();
			ItemInvoked += OnMenuItemInvoked;
			BackRequested += OnBackRequested;
			Style = Windows.UI.Xaml.Application.Current.Resources["ShellNavigationView"] as Windows.UI.Xaml.Style;
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
		}

		void OnPaneOpening()
		{
			if (Shell != null)
				Shell.FlyoutIsPresented = true;
			UpdatePaneButtonColor(TogglePaneButton, false);
			UpdatePaneButtonColor(NavigationViewBackButton, false);
			UpdateFlyoutBackgroundColor();
			UpdateFlyoutBackdrop();

			if(_flyoutBehavior == FlyoutBehavior.Flyout)
				ShellSplitView.UpdateFlyoutBackdrop();
		}

		void OnPaneClosed()
		{
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
			}
		}

		#endregion IVisualElementRenderer


		ShellSplitView ShellSplitView => (ShellSplitView)GetTemplateChild("RootSplitView");
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
		}

		void UpdateFlyoutBackdrop()
		{
			if (_flyoutBehavior != FlyoutBehavior.Flyout)
				return;

			var splitView = ShellSplitView;
			if (splitView != null)
			{
				splitView.FlyoutBackdrop = _flyoutBackdrop;
				if (IsPaneOpen)
					ShellSplitView.UpdateFlyoutBackdrop();
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
			MenuItemsSource = IterateItems();
			SwitchShellItem(shell.CurrentItem, false);
			IsPaneOpen = Shell.FlyoutIsPresented;
			ShellController.AddFlyoutBehaviorObserver(this);
			ShellController.AddAppearanceObserver(this, shell);
			ShellController.ItemsCollectionChanged += OnItemsCollectionChanged;
			ShellController.StructureChanged += OnStructureChanged;
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
					IsPaneToggleButtonVisible = false;
					IsPaneVisible = false;
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

		void OnStructureChanged(object sender, EventArgs e)
		{
			MenuItemsSource = IterateItems();
		}

		void OnItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			MenuItemsSource = IterateItems();
		}

		IEnumerable<object> IterateItems()
		{
			var groups = ((IShellController)Shell).GenerateFlyoutGrouping();
			foreach (var group in groups)
			{
				if (group.Count > 0 && group != groups[0])
				{
					yield return new MenuFlyoutSeparator(); // Creates a separator
				}
				foreach (var item in group)
				{
					yield return item;
				}
			}
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
			}

			var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
			titleBar.BackgroundColor = titleBar.ButtonBackgroundColor = backgroundColor;
			titleBar.ForegroundColor = titleBar.ButtonForegroundColor = titleColor;
			UpdatePaneButtonColor(TogglePaneButton, !IsPaneOpen);
			UpdatePaneButtonColor(NavigationViewBackButton, !IsPaneOpen);

			UpdateFlyoutBackdrop();
		}

		#endregion IAppearanceObserver
		
		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			_flyoutBehavior = behavior;
			UpdateToolBar();
		}

		public virtual ShellFlyoutTemplateSelector CreateShellFlyoutTemplateSelector() => new ShellFlyoutTemplateSelector();
		public virtual ShellHeaderRenderer CreateShellHeaderRenderer(Shell shell) => new ShellHeaderRenderer(shell);
		public virtual ShellItemRenderer CreateShellItemRenderer() => new ShellItemRenderer(this);
		public virtual ShellSectionRenderer CreateShellSectionRenderer() => new ShellSectionRenderer();
	}
}
