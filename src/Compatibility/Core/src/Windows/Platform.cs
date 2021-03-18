using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Microsoft.Maui.Controls.Internals;
using NativeAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;
using WFlowDirection = Microsoft.UI.Xaml.FlowDirection;
using WImage = Microsoft.UI.Xaml.Controls.Image;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public abstract class Platform : INavigation
	{
		static Task<bool> s_currentAlert;
		static Task<string> s_currentPrompt;

		internal static readonly BindableProperty RendererProperty = BindableProperty.CreateAttached("Renderer",
			typeof(IVisualElementRenderer), typeof(Windows.Foundation.Metadata.Platform), default(IVisualElementRenderer));

		public static IVisualElementRenderer GetRenderer(VisualElement element)
		{
			return (IVisualElementRenderer)element.GetValue(RendererProperty);
		}

		public static void SetRenderer(VisualElement element, IVisualElementRenderer value)
		{
			element.SetValue(RendererProperty, value);
			element.IsPlatformEnabled = value != null;
		}

		public static IVisualElementRenderer CreateRenderer(VisualElement element)
		{
			if (element == null)
				throw new ArgumentNullException(nameof(element));

			IVisualElementRenderer renderer = null;

			// temporary hack to fix the following issues
			// https://github.com/xamarin/Microsoft.Maui.Controls.Compatibility/issues/13261
			// https://github.com/xamarin/Microsoft.Maui.Controls.Compatibility/issues/12484
			if (element is RadioButton tv && tv.ResolveControlTemplate() != null)
			{
				renderer = new DefaultRenderer();
			}

			if (renderer == null)
			{
				renderer = Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(element) ??
												  new DefaultRenderer();
			}

			renderer.SetElement(element);
			return renderer;
		}

		internal static Platform Current
		{
			get
			{
				var frame = Window.Current?.Content as Microsoft.UI.Xaml.Controls.Frame;
				var wbp = frame?.Content as WindowsBasePage;
				return wbp?.Platform;
			}
		}

		internal Platform(Microsoft.UI.Xaml.Window page)
		{
			if (page == null)
				throw new ArgumentNullException(nameof(page));

			_page = page;

			var current = Microsoft.UI.Xaml.Application.Current;

			if (!current.Resources.ContainsKey("RootContainerStyle"))
			{
				Microsoft.UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(Forms.GetTabletResources());
			}

			if (!current.Resources.ContainsKey(ShellRenderer.ShellStyle))
			{
				var myResourceDictionary = new Microsoft.UI.Xaml.ResourceDictionary();
				myResourceDictionary.Source = new Uri("ms-appx:///Microsoft.Maui.Controls.Compatibility.Platform.UAP/Shell/ShellStyles.xbf");
				Microsoft.UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(myResourceDictionary);
			}

			_container = new Canvas
			{
				Style = (Microsoft.UI.Xaml.Style)current.Resources["RootContainerStyle"]
			};

			_page.Content = _container;

			_container.SizeChanged += OnRendererSizeChanged;

			MessagingCenter.Subscribe(this, Page.BusySetSignalName, (Page sender, bool enabled) =>
			{
				Microsoft.UI.Xaml.Controls.ProgressBar indicator = GetBusyIndicator();
				indicator.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
			});

			_toolbarTracker.CollectionChanged += OnToolbarItemsChanged;

			UpdateBounds();

			InitializeStatusBar();

			SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
			Microsoft.UI.Xaml.Application.Current.Resuming += OnResumingAsync;
		}

		async void OnResumingAsync(object sender, object e)
		{
			try
			{
				await UpdateToolbarItems();
			}
			catch (Exception exception)
			{
				Log.Warning("Update toolbar items after app resume",
					$"UpdateToolbarItems failed after app resume: {exception.Message}");

			}
		}

		internal void SetPage(Page newRoot)
		{
			if (newRoot == null)
				throw new ArgumentNullException(nameof(newRoot));

			_navModel.Clear();

			_navModel.Push(newRoot, null);
			SetCurrent(newRoot, true);
			Application.Current.NavigationProxy.Inner = this;
		}

		public IReadOnlyList<Page> NavigationStack
		{
			get { return _navModel.Tree.Last(); }
		}

		public IReadOnlyList<Page> ModalStack
		{
			get { return _navModel.Modals.ToList(); }
		}

		Task INavigation.PushAsync(Page root)
		{
			return ((INavigation)this).PushAsync(root, true);
		}

		Task<Page> INavigation.PopAsync()
		{
			return ((INavigation)this).PopAsync(true);
		}

		Task INavigation.PopToRootAsync()
		{
			return ((INavigation)this).PopToRootAsync(true);
		}

		Task INavigation.PushAsync(Page root, bool animated)
		{
			throw new InvalidOperationException("PushAsync is not supported globally on Windows, please use a NavigationPage.");
		}

		Task<Page> INavigation.PopAsync(bool animated)
		{
			throw new InvalidOperationException("PopAsync is not supported globally on Windows, please use a NavigationPage.");
		}

		Task INavigation.PopToRootAsync(bool animated)
		{
			throw new InvalidOperationException(
				"PopToRootAsync is not supported globally on Windows, please use a NavigationPage.");
		}

		void INavigation.RemovePage(Page page)
		{
			throw new InvalidOperationException("RemovePage is not supported globally on Windows, please use a NavigationPage.");
		}

		void INavigation.InsertPageBefore(Page page, Page before)
		{
			throw new InvalidOperationException(
				"InsertPageBefore is not supported globally on Windows, please use a NavigationPage.");
		}

		Task INavigation.PushModalAsync(Page page)
		{
			return ((INavigation)this).PushModalAsync(page, true);
		}

		Task<Page> INavigation.PopModalAsync()
		{
			return ((INavigation)this).PopModalAsync(true);
		}

		Task INavigation.PushModalAsync(Page page, bool animated)
		{
			if (page == null)
				throw new ArgumentNullException(nameof(page));

			var tcs = new TaskCompletionSource<bool>();
			_navModel.PushModal(page);
			SetCurrent(page, false, true, () => tcs.SetResult(true));
			return tcs.Task;
		}

		Task<Page> INavigation.PopModalAsync(bool animated)
		{
			var tcs = new TaskCompletionSource<Page>();
			Page result = _navModel.PopModal();
			SetCurrent(_navModel.CurrentPage, true, true, () => tcs.SetResult(result));
			return tcs.Task;
		}

		public static SizeRequest GetNativeSize(VisualElement element, double widthConstraint, double heightConstraint)
		{
			// Hack around the fact that Canvas ignores the child constraints.
			// It is entirely possible using Canvas as our base class is not wise.
			// FIXME: This should not be an if statement. Probably need to define an interface here.
			if (widthConstraint > 0 && heightConstraint > 0)
			{
				IVisualElementRenderer elementRenderer = GetRenderer(element);
				if (elementRenderer != null)
					return elementRenderer.GetDesiredSize(widthConstraint, heightConstraint);
			}

			return new SizeRequest();
		}

		internal virtual Rectangle ContainerBounds
		{
			get { return _bounds; }
		}

		internal void UpdatePageSizes()
		{
			Rectangle bounds = ContainerBounds;
			if (bounds.IsEmpty)
				return;
			foreach (Page root in _navModel.Roots)
			{
				root.Layout(bounds);
				IVisualElementRenderer renderer = GetRenderer(root);
				if (renderer != null)
				{
					renderer.ContainerElement.Width = _container.ActualWidth;
					renderer.ContainerElement.Height = _container.ActualHeight;
				}
			}
		}

		Rectangle _bounds;
		readonly Panel _container;
		readonly Microsoft.UI.Xaml.Window _page;
		Microsoft.UI.Xaml.Controls.ProgressBar _busyIndicator;
		Page _currentPage;
		Page _modalBackgroundPage;
		readonly NavigationModel _navModel = new NavigationModel();
		readonly ToolbarTracker _toolbarTracker = new ToolbarTracker();
		readonly ImageConverter _imageConverter = new ImageConverter();
		readonly ImageSourceIconElementConverter _imageSourceIconElementConverter = new ImageSourceIconElementConverter();

		Microsoft.UI.Xaml.Controls.ProgressBar GetBusyIndicator()
		{
			if (_busyIndicator == null)
			{
				_busyIndicator = new Microsoft.UI.Xaml.Controls.ProgressBar
				{
					IsIndeterminate = true,
					Visibility = Visibility.Collapsed,
					VerticalAlignment = VerticalAlignment.Top
				};

				Canvas.SetZIndex(_busyIndicator, 1);
				_container.Children.Add(_busyIndicator);
			}

			return _busyIndicator;
		}

		internal bool BackButtonPressed()
		{
			Page lastRoot = _navModel.Roots.LastOrDefault();

			if (lastRoot == null)
				return false;

			bool handled = lastRoot.SendBackButtonPressed();

			if (!handled && _navModel.Tree.Count > 1)
			{
				Page removed = _navModel.PopModal();
				if (removed != null)
				{
					SetCurrent(_navModel.CurrentPage, true, true);
					handled = true;
				}
			}

			return handled;
		}

		void OnRendererSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			UpdateBounds();
			UpdatePageSizes();
		}

		async void SetCurrent(Page newPage, bool popping = false, bool modal = false, Action completedCallback = null)
		{
			try
			{
				if (newPage == _currentPage)
					return;

				if (_currentPage != null)
				{
					Page previousPage = _currentPage;

					if (modal && !popping && !newPage.BackgroundColor.IsDefault)
						_modalBackgroundPage = previousPage;
					else
					{
						RemovePage(previousPage);

						if (!modal && _modalBackgroundPage != null)
						{
							RemovePage(_modalBackgroundPage);
							_modalBackgroundPage.Cleanup();
							_modalBackgroundPage.Parent = null;
						}

						_modalBackgroundPage = null;
					}

					if (popping)
					{
						previousPage.Cleanup();
						// Un-parent the page; otherwise the Resources Changed Listeners won't be unhooked and the 
						// page will leak 
						previousPage.Parent = null;
					}
				}

				newPage.Layout(ContainerBounds);

				AddPage(newPage);

				completedCallback?.Invoke();

				_currentPage = newPage;

				UpdateToolbarTracker();

				await UpdateToolbarItems();
			}
			catch (Exception error)
			{
				//This exception prevents the Main Page from being changed in a child 
				//window or a different thread, except on the Main thread. 
				//HEX 0x8001010E 
				if (error.HResult == -2147417842)
					throw new InvalidOperationException("Changing the current page is only allowed if it's being called from the same UI thread." +
						"Please ensure that the new page is in the same UI thread as the current page.");
				throw;
			}
		}

		void RemovePage(Page page)
		{
			if (_container == null || page == null)
				return;

			if (_modalBackgroundPage != null)
				_modalBackgroundPage.GetCurrentPage()?.SendAppearing();

			IVisualElementRenderer pageRenderer = GetRenderer(page);

			if (_container.Children.Contains(pageRenderer.ContainerElement))
				_container.Children.Remove(pageRenderer.ContainerElement);
		}

		void AddPage(Page page)
		{
			if (_container == null || page == null)
				return;

			if (_modalBackgroundPage != null)
				_modalBackgroundPage.GetCurrentPage()?.SendDisappearing();



			IVisualElementRenderer pageRenderer = page.GetOrCreateRenderer();

			if (!_container.Children.Contains(pageRenderer.ContainerElement))
				_container.Children.Add(pageRenderer.ContainerElement);

			pageRenderer.ContainerElement.Width = _container.ActualWidth;
			pageRenderer.ContainerElement.Height = _container.ActualHeight;
		}

		async void OnToolbarItemsChanged(object sender, EventArgs e)
		{
			await UpdateToolbarItems();
		}

		void UpdateToolbarTracker()
		{
			Page last = _navModel.Roots.Last();
			if (last != null)
				_toolbarTracker.Target = last;
		}

		void UpdateBounds()
		{
			_bounds = new Rectangle(0, 0, _page.Bounds.Width, _page.Bounds.Height);

			if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
			{
				// TODO WINUI
				//StatusBar statusBar = StatusBar.GetForCurrentView();
				//if (statusBar != null)
				//{
				//	bool landscape = Device.Info.CurrentOrientation.IsLandscape();
				//	bool titleBar = CoreApplication.GetCurrentView().TitleBar.IsVisible;
				//	double offset = landscape ? statusBar.OccludedRect.Width : statusBar.OccludedRect.Height;

				//	_bounds = new Rectangle(0, 0, _page.ActualWidth - (landscape ? offset : 0), _page.ActualHeight - (landscape ? 0 : offset));

				//	// Even if the MainPage is a ContentPage not inside of a NavigationPage, the calculated bounds
				//	// assume the TitleBar is there even if it isn't visible. When UpdatePageSizes is called,
				//	// _container.ActualWidth is correct because it's aware that the TitleBar isn't there, but the
				//	// bounds aren't, and things can subsequently run under the StatusBar.
				//	if (!titleBar)
				//	{
				//		_bounds.Width -= (_bounds.Width - _container.ActualWidth);
				//	}
				//}
			}
		}

		void InitializeStatusBar()
		{
			if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
			{
				//StatusBar statusBar = StatusBar.GetForCurrentView();
				//if (statusBar != null)
				//{
				//	statusBar.Showing += (sender, args) => UpdateBounds();
				//	statusBar.Hiding += (sender, args) => UpdateBounds();

				//	// UWP 14393 Bug: If RequestedTheme is Light (which it is by default), then the 
				//	// status bar uses White Foreground with White Background. 
				//	// UWP 10586 Bug: If RequestedTheme is Light (which it is by default), then the 
				//	// status bar uses Black Foreground with Black Background. 
				//	// Since the Light theme should have a Black on White status bar, we will set it explicitly. 
				//	// This can be overriden by setting the status bar colors in App.xaml.cs OnLaunched.

				//	if (statusBar.BackgroundColor == null && statusBar.ForegroundColor == null && Microsoft.UI.Xaml.Application.Current.RequestedTheme == ApplicationTheme.Light)
				//	{
				//		statusBar.BackgroundColor = Colors.White;
				//		statusBar.ForegroundColor = Colors.Black;
				//		statusBar.BackgroundOpacity = 1;
				//	}
				//}
			}
		}

		internal async Task UpdateToolbarItems()
		{
			var toolbarProvider = GetToolbarProvider();

			if (toolbarProvider == null)
			{
				return;
			}

			CommandBar commandBar = await toolbarProvider.GetCommandBarAsync();

			if (commandBar == null)
			{
				return;
			}

			commandBar.PrimaryCommands.Clear();
			commandBar.SecondaryCommands.Clear();

			var toolBarForegroundBinder = GetToolbarProvider() as IToolBarForegroundBinder;

			foreach (ToolbarItem item in _toolbarTracker.ToolbarItems)
			{
				toolBarForegroundBinder?.BindForegroundColor(commandBar);

				var button = new AppBarButton();
				button.SetBinding(AppBarButton.LabelProperty, "Text");

				if (commandBar.IsDynamicOverflowEnabled && item.Order == ToolbarItemOrder.Secondary)
				{
					button.SetBinding(AppBarButton.IconProperty, "IconImageSource", _imageSourceIconElementConverter);
				}
				else
				{
					var img = new WImage();
					img.SetBinding(WImage.SourceProperty, "Value");
					img.SetBinding(WImage.DataContextProperty, "IconImageSource", _imageConverter);
					button.Content = img;
				}

				// WINUUI FIX
				//button.Command = new MenuItemCommand(item);
				button.DataContext = item;
				button.SetValue(NativeAutomationProperties.AutomationIdProperty, item.AutomationId);
				button.SetAutomationPropertiesName(item);
				button.SetAutomationPropertiesAccessibilityView(item);
				button.SetAutomationPropertiesHelpText(item);
				button.SetAutomationPropertiesLabeledBy(item);

				ToolbarItemOrder order = item.Order == ToolbarItemOrder.Default ? ToolbarItemOrder.Primary : item.Order;
				if (order == ToolbarItemOrder.Primary)
				{
					toolBarForegroundBinder?.BindForegroundColor(button);
					commandBar.PrimaryCommands.Add(button);
				}
				else
				{
					commandBar.SecondaryCommands.Add(button);
				}
			}
		}

		internal IToolbarProvider GetToolbarProvider()
		{
			IToolbarProvider provider = null;

			Page element = _currentPage;
			while (element != null)
			{
				provider = GetRenderer(element) as IToolbarProvider;
				if (provider != null)
					break;

				var pageContainer = element as IPageContainer<Page>;
				element = pageContainer?.CurrentPage;
			}

			return provider;
		}

		internal static void SubscribeAlertsAndActionSheets()
		{
			MessagingCenter.Subscribe<Page, AlertArguments>(Forms.MainWindow, Page.AlertSignalName, OnPageAlert);
			MessagingCenter.Subscribe<Page, ActionSheetArguments>(Forms.MainWindow, Page.ActionSheetSignalName, OnPageActionSheet);
			MessagingCenter.Subscribe<Page, PromptArguments>(Forms.MainWindow, Page.PromptSignalName, OnPagePrompt);
		}

		static void OnPageActionSheet(Page sender, ActionSheetArguments options)
		{
			bool userDidSelect = false;

			if (options.FlowDirection == FlowDirection.MatchParent)
			{
				if ((sender as IVisualElementController).EffectiveFlowDirection.IsRightToLeft())
				{
					options.FlowDirection = FlowDirection.RightToLeft;
				}
				else if ((sender as IVisualElementController).EffectiveFlowDirection.IsLeftToRight())
				{
					options.FlowDirection = FlowDirection.LeftToRight;
				}
			}

			var flyoutContent = new FormsFlyout(options);

			var actionSheet = new Flyout
			{
				FlyoutPresenterStyle = (Microsoft.UI.Xaml.Style)Microsoft.UI.Xaml.Application.Current.Resources["FormsFlyoutPresenterStyle"],
				Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Full,
				Content = flyoutContent
			};

			flyoutContent.OptionSelected += (s, e) =>
			{
				userDidSelect = true;
				actionSheet.Hide();
			};

			actionSheet.Closed += (s, e) =>
			{
				if (!userDidSelect)
					options.SetResult(null);
			};

			try
			{
				actionSheet.ShowAt(((Page)sender).GetOrCreateRenderer().ContainerElement);
			}
			catch (ArgumentException) // if the page is not in the visual tree
			{
				if (Forms.MainWindow.Content is FrameworkElement mainPage)
					actionSheet.ShowAt(mainPage);
			}
		}

		static async void OnPagePrompt(Page sender, PromptArguments options)
		{
			var promptDialog = new PromptDialog
			{
				Title = options.Title ?? string.Empty,
				Message = options.Message ?? string.Empty,
				Input = options.InitialValue ?? string.Empty,
				Placeholder = options.Placeholder ?? string.Empty,
				MaxLength = options.MaxLength >= 0 ? options.MaxLength : 0,
				InputScope = options.Keyboard.ToInputScope()
			};

			if (options.Cancel != null)
				promptDialog.SecondaryButtonText = options.Cancel;

			if (options.Accept != null)
				promptDialog.PrimaryButtonText = options.Accept;

			var currentAlert = s_currentPrompt;
			while (currentAlert != null)
			{
				await currentAlert;
				currentAlert = s_currentPrompt;
			}

			s_currentPrompt = ShowPrompt(promptDialog);
			options.SetResult(await s_currentPrompt.ConfigureAwait(false));
			s_currentPrompt = null;
		}

		static async Task<string> ShowPrompt(PromptDialog prompt)
		{
			ContentDialogResult result = await prompt.ShowAsync();

			if (result == ContentDialogResult.Primary)
				return prompt.Input;
			return null;
		}

		static async void OnPageAlert(Page sender, AlertArguments options)
		{
			string content = options.Message ?? string.Empty;
			string title = options.Title ?? string.Empty;

			var alertDialog = new AlertDialog
			{
				Content = content,
				Title = title,
				VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Auto
			};

			if (options.FlowDirection == FlowDirection.RightToLeft)
			{
				alertDialog.FlowDirection = Microsoft.UI.Xaml.FlowDirection.RightToLeft;
			}
			else if (options.FlowDirection == FlowDirection.LeftToRight)
			{
				alertDialog.FlowDirection = Microsoft.UI.Xaml.FlowDirection.LeftToRight;
			}
			else
			{
				if ((sender as IVisualElementController).EffectiveFlowDirection.IsRightToLeft())
				{
					alertDialog.FlowDirection = WFlowDirection.RightToLeft;
				}
				else if ((sender as IVisualElementController).EffectiveFlowDirection.IsLeftToRight())
				{
					alertDialog.FlowDirection = WFlowDirection.LeftToRight;
				}
			}

			if (options.Cancel != null)
				alertDialog.SecondaryButtonText = options.Cancel;

			if (options.Accept != null)
				alertDialog.PrimaryButtonText = options.Accept;

			var currentAlert = s_currentAlert;
			while (currentAlert != null)
			{
				await currentAlert;
				currentAlert = s_currentAlert;
			}

			s_currentAlert = ShowAlert(alertDialog);
			options.SetResult(await s_currentAlert.ConfigureAwait(false));
			s_currentAlert = null;
		}

		static async Task<bool> ShowAlert(ContentDialog alert)
		{
			ContentDialogResult result = await alert.ShowAsync();

			return result == ContentDialogResult.Primary;
		}

		void OnBackRequested(object sender, BackRequestedEventArgs e)
		{
			Application app = Application.Current;
			Page page = app?.MainPage;
			if (page == null)
				return;
			e.Handled = BackButtonPressed();
		}
	}
}