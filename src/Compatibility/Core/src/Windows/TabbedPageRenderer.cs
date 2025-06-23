
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using PageSpecifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Page;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.TabbedPage;
using VisualElementSpecifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.VisualElement;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WStackPanel = Microsoft.UI.Xaml.Controls.StackPanel;
using WTextAlignment = Microsoft.UI.Xaml.TextAlignment;
using WTextBlock = Microsoft.UI.Xaml.Controls.TextBlock;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class TabbedPageRenderer : IVisualElementRenderer, ITitleProvider, IToolbarProvider,
		IToolBarForegroundBinder
	{
		const string TabBarHeaderStackPanelName = "TabbedPageHeaderStackPanel";
		const string TabBarHeaderImageName = "TabbedPageHeaderImage";
		const string TabBarHeaderTextBlockName = "TabbedPageHeaderTextBlock";
		const string TabBarHeaderGridName = "TabbedPageHeaderGrid";

		Color _barBackgroundColor;
		Brush _barBackground;
		Color _barTextColor;
		bool _disposed;
		bool _showTitle;
		WBrush _defaultSelectedColor;
		WBrush _defaultUnselectedColor;

		WTextAlignment _oldBarTextBlockTextAlignment = WTextAlignment.Center;
		WHorizontalAlignment _oldBarTextBlockHorinzontalAlignment = WHorizontalAlignment.Center;

		VisualElementTracker<Page, Pivot> _tracker;

		ITitleProvider TitleProvider => this;

		public FormsPivot Control { get; private set; }

		public TabbedPage Element { get; private set; }

		protected VisualElementTracker<Page, Pivot> Tracker
		{
			get { return _tracker; }
			set
			{
				if (_tracker == value)
					return;

				if (_tracker != null)
					_tracker.Dispose();

				_tracker = value;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		WBrush ITitleProvider.BarBackgroundBrush
		{
			set { Control.ToolbarBackground = value; }
		}

		WBrush ITitleProvider.BarForegroundBrush
		{
			set { Control.ToolbarForeground = value; }
		}

		bool ITitleProvider.ShowTitle
		{
			get { return _showTitle; }

			set
			{
				if (_showTitle == value)
					return;
				_showTitle = value;

				UpdateTitleVisibility();
			}
		}

		string ITitleProvider.Title
		{
			get { return (string)Control?.Title; }

			set
			{
				if (Control != null && _showTitle)
					Control.Title = value;
			}
		}

		public Task<CommandBar> GetCommandBarAsync()
		{
			return (Control as IToolbarProvider)?.GetCommandBarAsync();
		}

		public FrameworkElement ContainerElement
		{
			get { return Control; }
		}

		VisualElement IVisualElementRenderer.Element
		{
			get { return Element; }
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var constraint = new global::Windows.Foundation.Size(widthConstraint, heightConstraint);

			double oldWidth = Control.Width;
			double oldHeight = Control.Height;

			Control.Height = double.NaN;
			Control.Width = double.NaN;

			Control.Measure(constraint);
			var result = new Size(Math.Ceiling(Control.DesiredSize.Width), Math.Ceiling(Control.DesiredSize.Height));

			Control.Width = oldWidth;
			Control.Height = oldHeight;

			return new SizeRequest(result);
		}

		UIElement IVisualElementRenderer.GetNativeElement()
		{
			return Control;
		}

		public void SetElement(VisualElement element)
		{
			if (element != null && !(element is TabbedPage))
				throw new ArgumentException("Element must be a TabbedPage", "element");

			TabbedPage oldElement = Element;
			Element = (TabbedPage)element;

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
				((INotifyCollectionChanged)oldElement.Children).CollectionChanged -= OnPagesChanged;
				Control?.GetDescendantsByName<TextBlock>(TabBarHeaderTextBlockName).ForEach(t => { t.AccessKeyInvoked -= AccessKeyInvokedForTab; });
			}

			if (element != null)
			{
				if (Control == null)
				{
					Control = new FormsPivot
					{
						Style = (Microsoft.UI.Xaml.Style)Microsoft.UI.Xaml.Application.Current.Resources["TabbedPageStyle"],
					};

					Control.SelectionChanged += OnSelectionChanged;

					Tracker = new BackgroundTracker<Pivot>(Microsoft.UI.Xaml.Controls.Control.BackgroundProperty)
					{
						Element = (Page)element,
						Control = Control,
						Container = Control
					};

					Control.Loaded += OnLoaded;
					Control.Unloaded += OnUnloaded;
				}

				Control.DataContext = Element;
				OnPagesChanged(Element.Children,
					new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

				UpdateCurrentPage();
				UpdateToolbarPlacement();
				UpdateToolbarDynamicOverflowEnabled();

				((INotifyCollectionChanged)Element.Children).CollectionChanged += OnPagesChanged;
				element.PropertyChanged += OnElementPropertyChanged;

				if (!string.IsNullOrEmpty(element.AutomationId))
					Control.SetValue(Microsoft.UI.Xaml.Automation.AutomationProperties.AutomationIdProperty, element.AutomationId);
			}

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || _disposed)
				return;

			_disposed = true;
			Element?.SendDisappearing();
			SetElement(null);
			Tracker = null;
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			ElementChanged?.Invoke(this, e);
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(TabbedPage.CurrentPage))
			{
				UpdateCurrentPage();
				UpdateBarTextColor();
				UpdateBarBackgroundColor();
				UpdateBarBackground();
			}
			else if (e.PropertyName == TabbedPage.BarTextColorProperty.PropertyName)
				UpdateBarTextColor();
			else if (e.PropertyName == TabbedPage.BarBackgroundColorProperty.PropertyName)
				UpdateBarBackgroundColor();
			else if (e.PropertyName == TabbedPage.BarBackgroundProperty.PropertyName)
				UpdateBarBackground();
			else if (e.PropertyName == PlatformConfiguration.WindowsSpecific.Page.ToolbarPlacementProperty.PropertyName)
				UpdateToolbarPlacement();
			else if (e.PropertyName == PlatformConfiguration.WindowsSpecific.Page.ToolbarDynamicOverflowEnabledProperty.PropertyName)
				UpdateToolbarDynamicOverflowEnabled();
			else if (e.PropertyName == Specifics.HeaderIconsEnabledProperty.PropertyName)
				UpdateBarIcons();
			else if (e.PropertyName == Specifics.HeaderIconsSizeProperty.PropertyName)
				UpdateBarIcons();
			else if (e.PropertyName == PageSpecifics.ToolbarPlacementProperty.PropertyName)
				UpdateToolbarPlacement();
			else if (e.PropertyName == TabbedPage.SelectedTabColorProperty.PropertyName || e.PropertyName == TabbedPage.UnselectedTabColorProperty.PropertyName)
				UpdateSelectedTabColors();
		}

		void OnLoaded(object sender, RoutedEventArgs args)
		{
			Element?.SendAppearing();

			UpdateBarTextColor();
			UpdateBarBackgroundColor();
			UpdateBarBackground();
			UpdateBarIcons();
			UpdateAccessKeys();
			UpdateSelectedTabColors();
		}

		void OnPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			e.Apply(Element.Children, Control.Items);
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
				case NotifyCollectionChangedAction.Remove:
				case NotifyCollectionChangedAction.Replace:
					if (e.NewItems != null)
						for (int i = 0; i < e.NewItems.Count; i++)
							((Page)e.NewItems[i]).PropertyChanged += OnChildPagePropertyChanged;
					if (e.OldItems != null)
						for (int i = 0; i < e.OldItems.Count; i++)
							((Page)e.OldItems[i]).PropertyChanged -= OnChildPagePropertyChanged;
					break;
				case NotifyCollectionChangedAction.Reset:
					foreach (var page in Element.Children)
						page.PropertyChanged += OnChildPagePropertyChanged;
					break;
			}

			Control.UpdateLayout();
			EnsureBarColors(e.Action);
		}

		void EnsureBarColors(NotifyCollectionChangedAction action)
		{
			switch (action)
			{
				case NotifyCollectionChangedAction.Add:
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
					// Need to make sure any new items have the correct fore/background colors
					ApplyBarBackgroundColor(true);
					ApplyBarTextColor(true);
					UpdateSelectedTabColors();
					break;
			}
		}

		void OnChildPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var page = sender as Page;
			if (page != null)
			{
				// If AccessKeys properties are updated on a child (tab) we want to
				// update the access key on the native control.
				if (e.PropertyName == VisualElementSpecifics.AccessKeyProperty.PropertyName ||
					e.PropertyName == VisualElementSpecifics.AccessKeyPlacementProperty.PropertyName ||
					e.PropertyName == VisualElementSpecifics.AccessKeyHorizontalOffsetProperty.PropertyName ||
					e.PropertyName == VisualElementSpecifics.AccessKeyVerticalOffsetProperty.PropertyName)
					UpdateAccessKeys();
			}
		}

		void OnSelectionChanged(object sender, WSelectionChangedEventArgs e)
		{
			if (Element == null)
				return;

			Page page = e.AddedItems.Count > 0 ? (Page)e.AddedItems[0] : null;
			Page currentPage = Element.CurrentPage;
			if (currentPage == page)
				return;
			currentPage?.SendDisappearing();
			Element.CurrentPage = page;

			UpdateSelectedTabColors();

			page?.SendAppearing();
		}

		void OnUnloaded(object sender, RoutedEventArgs args)
		{
			Element?.SendDisappearing();
		}

		WBrush GetBarBackgroundBrush()
		{
			object defaultColor = new WSolidColorBrush(Microsoft.UI.Colors.Transparent);

			if (Element.BarBackgroundColor.IsDefault() && defaultColor != null)
				return (WBrush)defaultColor;
			return Element.BarBackgroundColor.ToPlatform();
		}

		WBrush GetBarForegroundBrush()
		{
			object defaultColor = Microsoft.UI.Xaml.Application.Current.Resources["ApplicationForegroundThemeBrush"];
			if (Element.BarTextColor.IsDefault() && defaultColor != null)
				return (WBrush)defaultColor;
			return Element.BarTextColor.ToPlatform();
		}

		void UpdateBarBackgroundColor()
		{
			if (Element == null)
				return;
			var barBackgroundColor = Element.BarBackgroundColor;

			if (barBackgroundColor == _barBackgroundColor)
				return;
			_barBackgroundColor = barBackgroundColor;

			ApplyBarBackgroundColor();
		}

		void UpdateBarBackground()
		{
			if (Element == null)
				return;

			var barBackground = Element.BarBackground;

			if (barBackground == _barBackground)
				return;

			_barBackground = barBackground;

			ApplyBarBackground();
		}

		void ApplyBarBackgroundColor(bool force = false)
		{
			var controlToolbarBackground = Control.ToolbarBackground;
			if (controlToolbarBackground == null && _barBackgroundColor.IsDefault())
				return;

			var brush = GetBarBackgroundBrush();
			if (brush == controlToolbarBackground && !force)
				return;

			TitleProvider.BarBackgroundBrush = brush;

			foreach (WGrid tabBarGrid in Control.GetDescendantsByName<WGrid>(TabBarHeaderGridName))
			{
				tabBarGrid.Background = brush;
			}
		}

		void ApplyBarBackground()
		{
			var controlToolbarBackground = Control.ToolbarBackground;
			var barBackground = Element.BarBackground;

			if (Brush.IsNullOrEmpty(barBackground))
				return;

			var brush = barBackground.ToBrush();

			if (brush == controlToolbarBackground)
				return;

			TitleProvider.BarBackgroundBrush = brush;

			foreach (WGrid tabBarGrid in Control.GetDescendantsByName<WGrid>(TabBarHeaderGridName))
			{
				tabBarGrid.Background = brush;
			}
		}

		void UpdateBarTextColor()
		{
			if (Element == null)
				return;
			var barTextColor = Element.BarTextColor;

			if (barTextColor == _barTextColor)
				return;
			_barTextColor = barTextColor;

			ApplyBarTextColor();
		}

		void ApplyBarTextColor(bool force = false)
		{
			var controlToolbarForeground = Control.ToolbarForeground;
			if (controlToolbarForeground == null && _barTextColor.IsDefault())
				return;

			var brush = GetBarForegroundBrush();
			if (brush == controlToolbarForeground && !force)
				return;

			TitleProvider.BarForegroundBrush = brush;

			foreach (WTextBlock tabBarTextBlock in Control.GetDescendantsByName<WTextBlock>(TabBarHeaderTextBlockName))
			{
				tabBarTextBlock.Foreground = brush;
			}
		}

		void UpdateTitleVisibility()
		{
			Control.TitleVisibility = _showTitle ? WVisibility.Visible : WVisibility.Collapsed;
		}

		void UpdateCurrentPage()
		{
			Page page = Element.CurrentPage;

			var nav = page as NavigationPage;
			TitleProvider.ShowTitle = nav != null;

			// Enforce consistency rules on toolbar (show toolbar if visible Tab is Navigation Page)
			Control.ShouldShowToolbar = nav != null;

			if (page == null)
				return;

			Control.SelectedItem = page;
		}

		void UpdateBarIcons()
		{
			if (Control == null || Element == null)
				return;

			bool headerIconsEnabled = Element.OnThisPlatform().GetHeaderIconsEnabled();
			bool invalidateMeasure = false;

			// Get all stack panels affected by update.
			var stackPanels = Control.GetDescendantsByName<WStackPanel>(TabBarHeaderStackPanelName);
			foreach (var stackPanel in stackPanels)
			{
#pragma warning disable RS0030 // Do not use banned APIs; Panel.Children is banned for performance reasons.
				int stackPanelChildCount = stackPanel.Children.Count;
#pragma warning restore RS0030 // Do not use banned APIs

				for (int i = 0; i < stackPanelChildCount; i++)
				{
#pragma warning disable RS0030 // Do not use banned APIs; Panel.Children is banned for performance reasons.
					var stackPanelItem = stackPanel.Children[i];
#pragma warning restore RS0030 // Do not use banned APIs

					if (stackPanelItem is WImage tabBarImage)
					{
						// Update icon image.
						if (tabBarImage.GetValue(FrameworkElement.NameProperty).ToString() == TabBarHeaderImageName)
						{
							if (headerIconsEnabled && tabBarImage.DataContext != null)
							{
								if (Element.IsSet(Specifics.HeaderIconsSizeProperty))
								{
									Size iconSize = Element.OnThisPlatform().GetHeaderIconsSize();
									tabBarImage.Height = iconSize.Height;
									tabBarImage.Width = iconSize.Width;
								}
								tabBarImage.HorizontalAlignment = WHorizontalAlignment.Center;
								tabBarImage.Visibility = WVisibility.Visible;
							}
							else
							{
								tabBarImage.Visibility = WVisibility.Collapsed;
							}

							invalidateMeasure = true;
						}
					}
					else if (stackPanelItem is WTextBlock tabBarTextblock)
					{
						// Update text block.
						if (tabBarTextblock.GetValue(FrameworkElement.NameProperty).ToString() == TabBarHeaderTextBlockName)
						{
							if (headerIconsEnabled)
							{
								// Remember old values so we can restore them if icons are collapsed.
								// NOTE, since all Textblock instances in this stack panel comes from the same
								// style, we just keep one copy of the value (since they should be identical).
								if (tabBarTextblock.TextAlignment != WTextAlignment.Center)
								{
									_oldBarTextBlockTextAlignment = tabBarTextblock.TextAlignment;
									tabBarTextblock.TextAlignment = WTextAlignment.Center;
								}

								if (tabBarTextblock.HorizontalAlignment != WHorizontalAlignment.Center)
								{
									_oldBarTextBlockHorinzontalAlignment = tabBarTextblock.HorizontalAlignment;
									tabBarTextblock.HorizontalAlignment = WHorizontalAlignment.Center;
								}
							}
							else
							{
								// Restore old values.
								tabBarTextblock.TextAlignment = _oldBarTextBlockTextAlignment;
								tabBarTextblock.HorizontalAlignment = _oldBarTextBlockHorinzontalAlignment;
							}
						}
					}
				}
			}

			// If items have been made visible or collapsed in panel, invalidate current control measures.
			if (invalidateMeasure)
				Control.InvalidateMeasure();
		}

		void UpdateToolbarPlacement()
		{
			Control.ToolbarPlacement = Element.OnThisPlatform().GetToolbarPlacement();
		}

		void UpdateToolbarDynamicOverflowEnabled()
		{
			Control.ToolbarDynamicOverflowEnabled = Element.OnThisPlatform().GetToolbarDynamicOverflowEnabled();
		}

		protected void UpdateAccessKeys()
		{
			Control?.GetDescendantsByName<TextBlock>(TabBarHeaderTextBlockName).ForEach(UpdateAccessKey);
		}

		void AccessKeyInvokedForTab(UIElement sender, AccessKeyInvokedEventArgs arg)
		{
			var tab = sender as TextBlock;
			if (tab != null && tab.DataContext is Page page)
				Element.CurrentPage = page;
		}

		protected void UpdateAccessKey(TextBlock control)
		{

			if (control != null && control.DataContext is Page page)
			{
				var windowsElement = page.On<PlatformConfiguration.Windows>();
				if (page.IsSet(VisualElementSpecifics.AccessKeyProperty))
				{
					control.AccessKeyInvoked += AccessKeyInvokedForTab;
				}
				AccessKeyHelper.UpdateAccessKey(control, page);
			}
		}

		public void BindForegroundColor(AppBar appBar)
		{
			SetAppBarForegroundBinding(appBar);
		}

		public void BindForegroundColor(AppBarButton button)
		{
			SetAppBarForegroundBinding(button);
		}

		void SetAppBarForegroundBinding(FrameworkElement element)
		{
			element.SetBinding(Microsoft.UI.Xaml.Controls.Control.ForegroundProperty,
				new Microsoft.UI.Xaml.Data.Binding
				{
					Path = new PropertyPath("ToolbarForeground"),
					Source = Control,
					RelativeSource = new RelativeSource { Mode = RelativeSourceMode.TemplatedParent }
				});
		}

		void UpdateSelectedTabColors()
		{
			// Retrieve all tab header textblocks
			var allTabHeaderTextBlocks = Control.GetDescendantsByName<WTextBlock>(TabBarHeaderTextBlockName).ToArray();
			if (allTabHeaderTextBlocks.Length != Control.Items.Count)
				return;

			// Loop through all pages in the Pivot control
			foreach (Page page in Control.Items)
			{
				// Fetch just the textblock for the current page
				var tabBarTextBlock = allTabHeaderTextBlocks[Control.Items.IndexOf(page)];

				// Apply selected or unselected style to the current textblock
				if (page == Element.CurrentPage)
				{
					if (_defaultSelectedColor == null)
						_defaultSelectedColor = tabBarTextBlock.Foreground;

					if (Element.IsSet(TabbedPage.SelectedTabColorProperty) && Element.SelectedTabColor != null)
						tabBarTextBlock.Foreground = Element.SelectedTabColor.ToPlatform();
					else
						tabBarTextBlock.Foreground = _defaultSelectedColor;
				}
				else
				{
					if (_defaultUnselectedColor == null)
						_defaultUnselectedColor = tabBarTextBlock.Foreground;

					if (Element.IsSet(TabbedPage.SelectedTabColorProperty) && Element.UnselectedTabColor != null)
						tabBarTextBlock.Foreground = Element.UnselectedTabColor.ToPlatform();
					else
						tabBarTextBlock.Foreground = _defaultUnselectedColor;
				}
			}
		}
	}
}
