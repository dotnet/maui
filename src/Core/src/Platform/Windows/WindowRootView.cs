using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Platform
{

	public partial class WindowRootView : ContentControl
	{
		public WindowRootView()
		{
		}

		public Image? AppFontIcon { get; private set; }
		public TextBlock? AppTitle { get; private set; }
		public RootNavigationView? NavigationViewControl { get; private set; }
		public FrameworkElement? AppTitleBar { get; private set; } 
		public event TypedEventHandler<NavigationView, NavigationViewBackRequestedEventArgs>? BackRequested;
		bool _hasTitleBarImage = false;
		internal event EventHandler? OnApplyTemplateFinished;
		internal event EventHandler? ContentChanged;
		string? _windowTitle;

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();			
			
			AppTitleBar = (FrameworkElement)GetTemplateChild("AppTitleBar");
			AppFontIcon = (Image)GetTemplateChild("AppFontIcon");
			AppTitle = (TextBlock)GetTemplateChild("AppTitle");

			OnApplyTemplateFinished?.Invoke(this, EventArgs.Empty);

			UpdateAppTitleBarMargins();

			AppFontIcon.ImageOpened += OnImageOpened;
			AppFontIcon.ImageFailed += OnImageFailed;
			SetWindowTitle(_windowTitle);
		}

		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged(oldContent, newContent);
			if(newContent is RootNavigationView mnv)
			{
				NavigationViewControl = mnv;
				NavigationViewControl.DisplayModeChanged += OnNavigationViewControlDisplayModeChanged;
				NavigationViewControl.BackRequested += OnNavigationViewBackRequested;
				NavigationViewControl.RegisterPropertyChangedCallback(NavigationView.IsBackButtonVisibleProperty, AppBarNavigationIconsChanged);
				NavigationViewControl.RegisterPropertyChangedCallback(NavigationView.IsPaneToggleButtonVisibleProperty, AppBarNavigationIconsChanged);

				ContentChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		void OnNavigationViewBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) =>
			BackRequested?.Invoke(sender, args);

		void OnNavigationViewControlDisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
		{
			UpdateAppTitleBarMargins();
		}

		void AppBarNavigationIconsChanged(DependencyObject sender, DependencyProperty dp)
		{
			UpdateAppTitleBarMargins();
		}

		void OnImageOpened(object sender, RoutedEventArgs e)
		{
			_hasTitleBarImage = true;
			UpdateAppTitleBarMargins();
		}

		void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
		{
			_hasTitleBarImage = false;
			UpdateAppTitleBarMargins();
		}

		internal void SetWindowTitle(string? title)
		{
			_windowTitle = title;
			if (AppTitle != null)
				AppTitle.Text = title;
		}

		void UpdateAppTitleBarMargins()
		{
			if (NavigationViewControl == null)
				return;

			if (AppTitleBar == null)
				return;

			//const int topIndent = 16;
			const int expandedIndent = 48;
			int minimalIndent = 0;

			// TODO: Once we implement Left pane navigation we'll probably need to adjust these calculations a bit
			if (!NavigationViewControl.IsBackButtonVisible.Equals(NavigationViewBackButtonVisible.Collapsed))
			{
				minimalIndent += 48;
			}

			if (NavigationViewControl.IsPaneToggleButtonVisible)
			{
				minimalIndent += 48;
			}

			WThickness currMargin = AppTitleBar.Margin;

			// Set the TitleBar margin dependent on NavigationView display mode
			if (NavigationViewControl.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
			{
				AppTitleBar.Margin = new WThickness(minimalIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
			}
			else if(NavigationViewControl.PaneDisplayMode == NavigationViewPaneDisplayMode.Left)
			{
				AppTitleBar.Margin = new WThickness(minimalIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
			}
			else if (NavigationViewControl.DisplayMode == NavigationViewDisplayMode.Minimal)
			{
				AppTitleBar.Margin = new WThickness(minimalIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
			}
			else
			{
				AppTitleBar.Margin = new WThickness(expandedIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
			}
						
			// If the AppIcon loads correctly then we set a margin for the text from the image
			if (_hasTitleBarImage)
			{
				if(AppTitle != null)
					AppTitle.Margin = new WThickness(12, 0, 0, 0);

				if(AppFontIcon != null)
					AppFontIcon.Visibility = UI.Xaml.Visibility.Visible;
			}
			else
			{
				// If there is no AppIcon then we hide the image and the layout already
				// has a margin set

				if (AppTitle != null)
					AppTitle.Margin = new WThickness(0);

				if (AppFontIcon != null)
					AppFontIcon.Visibility = UI.Xaml.Visibility.Collapsed;
			}
		}
	}
}
