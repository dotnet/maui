using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.UWP
{
	public abstract class Platform : IPlatform, INavigation, IToolbarProvider
	{
		IToolbarProvider _toolbarProvider;
		static Task<bool> s_currentAlert;

		internal static StatusBar MobileStatusBar => ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar") ? StatusBar.GetForCurrentView() : null;

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

			IVisualElementRenderer renderer = Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(element) ??
			                                  new DefaultRenderer();
			renderer.SetElement(element);
			return renderer;
		}

		internal Platform(Windows.UI.Xaml.Controls.Page page)
		{
			if (page == null)
				throw new ArgumentNullException(nameof(page));

			_page = page;

			_container = new Canvas
			{
				Style = (Windows.UI.Xaml.Style)Windows.UI.Xaml.Application.Current.Resources["RootContainerStyle"]
			};

			_page.Content = _container;

			_container.SizeChanged += OnRendererSizeChanged;

			MessagingCenter.Subscribe(this, Page.BusySetSignalName, (Page sender, bool enabled) =>
			{
				Windows.UI.Xaml.Controls.ProgressBar indicator = GetBusyIndicator();
				indicator.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
			});

			_toolbarTracker.CollectionChanged += OnToolbarItemsChanged;

			UpdateBounds();

			InitializeStatusBar();
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

		internal void SetPlatformDisconnected(VisualElement visualElement)
		{
			visualElement.Platform = this;
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
			SetCurrent(page, completedCallback: () => tcs.SetResult(true));
			return tcs.Task;
		}

		Task<Page> INavigation.PopModalAsync(bool animated)
		{
			var tcs = new TaskCompletionSource<Page>();
			Page result = _navModel.PopModal();
			SetCurrent(_navModel.CurrentPage, true, () => tcs.SetResult(result));
			return tcs.Task;
		}

		SizeRequest IPlatform.GetNativeSize(VisualElement element, double widthConstraint, double heightConstraint)
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
		readonly Canvas _container;
		readonly Windows.UI.Xaml.Controls.Page _page;
		Windows.UI.Xaml.Controls.ProgressBar _busyIndicator;
		Page _currentPage;
		readonly NavigationModel _navModel = new NavigationModel();
		readonly ToolbarTracker _toolbarTracker = new ToolbarTracker();
		readonly FileImageSourcePathConverter _fileImageSourcePathConverter = new FileImageSourcePathConverter();
		Windows.UI.Xaml.Controls.ProgressBar GetBusyIndicator()
		{
			if (_busyIndicator == null)
			{
				_busyIndicator = new Windows.UI.Xaml.Controls.ProgressBar
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
			Page lastRoot = _navModel.Roots.Last();

			bool handled = lastRoot.SendBackButtonPressed();

			if (!handled && _navModel.Tree.Count > 1)
			{
				Page removed = _navModel.PopModal();
				if (removed != null)
				{
					SetCurrent(_navModel.CurrentPage, true);
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

		async void SetCurrent(Page newPage, bool popping = false, Action completedCallback = null)
		{
			if (newPage == _currentPage)
				return;

			newPage.Platform = this;

			if (_currentPage != null)
			{
				Page previousPage = _currentPage;
				IVisualElementRenderer previousRenderer = GetRenderer(previousPage);
				_container.Children.Remove(previousRenderer.ContainerElement);

				if (popping)
				{
					previousPage.Cleanup();
					// Un-parent the page; otherwise the Resources Changed Listeners won't be unhooked and the 
					// page will leak 
					previousPage.Parent = null;
				}
			}

			newPage.Layout(ContainerBounds);

			IVisualElementRenderer pageRenderer = newPage.GetOrCreateRenderer();
			_container.Children.Add(pageRenderer.ContainerElement);

			pageRenderer.ContainerElement.Width = _container.ActualWidth;
			pageRenderer.ContainerElement.Height = _container.ActualHeight;

			completedCallback?.Invoke();

			_currentPage = newPage;

			UpdateToolbarTracker();

			UpdateToolbarTitle(newPage);

			await UpdateToolbarItems();
		}

		Task<CommandBar> IToolbarProvider.GetCommandBarAsync()
		{
			return GetCommandBarAsync();
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
			_bounds = new Rectangle(0, 0, _page.ActualWidth, _page.ActualHeight);

			StatusBar statusBar = MobileStatusBar;
			if (statusBar != null)
			{
				bool landscape = Device.Info.CurrentOrientation.IsLandscape();
				bool titleBar = CoreApplication.GetCurrentView().TitleBar.IsVisible;
				double offset = landscape ? statusBar.OccludedRect.Width : statusBar.OccludedRect.Height;

				_bounds = new Rectangle(0, 0, _page.ActualWidth - (landscape ? offset : 0), _page.ActualHeight - (landscape ? 0 : offset));

				// Even if the MainPage is a ContentPage not inside of a NavigationPage, the calculated bounds
				// assume the TitleBar is there even if it isn't visible. When UpdatePageSizes is called,
				// _container.ActualWidth is correct because it's aware that the TitleBar isn't there, but the
				// bounds aren't, and things can subsequently run under the StatusBar.
				if (!titleBar)
				{
					_bounds.Width -= (_bounds.Width - _container.ActualWidth);
				}
			}
		}

		void InitializeStatusBar()
		{
			StatusBar statusBar = MobileStatusBar;
			if (statusBar != null)
			{
				statusBar.Showing += (sender, args) => UpdateBounds();
				statusBar.Hiding += (sender, args) => UpdateBounds();

				// UWP 14393 Bug: If RequestedTheme is Light (which it is by default), then the 
				// status bar uses White Foreground with White Background. 
				// UWP 10586 Bug: If RequestedTheme is Light (which it is by default), then the 
				// status bar uses Black Foreground with Black Background. 
				// Since the Light theme should have a Black on White status bar, we will set it explicitly. 
				// This can be overriden by setting the status bar colors in App.xaml.cs OnLaunched.

				if (statusBar.BackgroundColor == null && statusBar.ForegroundColor == null && Windows.UI.Xaml.Application.Current.RequestedTheme == ApplicationTheme.Light)
				{
					statusBar.BackgroundColor = Colors.White;
					statusBar.ForegroundColor = Colors.Black;
					statusBar.BackgroundOpacity = 1;
				}
			}
		}

		void UpdateToolbarTitle(Page page)
		{
			if (_toolbarProvider == null)
				return;

			((ToolbarProvider)_toolbarProvider).CommandBar.Content = page.Title;
		}

		internal async Task UpdateToolbarItems()
		{
			CommandBar commandBar = await GetCommandBarAsync();
			if (commandBar != null)
			{
				commandBar.PrimaryCommands.Clear();
				commandBar.SecondaryCommands.Clear();

				if (_page.BottomAppBar != null || _page.TopAppBar != null)
				{
					_page.BottomAppBar = null;
					_page.TopAppBar = null;
					_page.InvalidateMeasure();
				}
			}

			var toolBarProvider = GetToolbarProvider() as IToolBarForegroundBinder;

			foreach (ToolbarItem item in _toolbarTracker.ToolbarItems.OrderBy(ti => ti.Priority))
			{
				if (commandBar == null)
					commandBar = CreateCommandBar();

				toolBarProvider?.BindForegroundColor(commandBar);

				var button = new AppBarButton();
				button.SetBinding(AppBarButton.LabelProperty, "Text");
				button.SetBinding(AppBarButton.IconProperty, "Icon", _fileImageSourcePathConverter);
				button.Command = new MenuItemCommand(item);
				button.DataContext = item;

				ToolbarItemOrder order = item.Order == ToolbarItemOrder.Default ? ToolbarItemOrder.Primary : item.Order;
				if (order == ToolbarItemOrder.Primary)
				{
					toolBarProvider?.BindForegroundColor(button);
					commandBar.PrimaryCommands.Add(button);
				}
				else
				{
					commandBar.SecondaryCommands.Add(button);
				}
			}

			if (commandBar?.PrimaryCommands.Count + commandBar?.SecondaryCommands.Count == 0)
				ClearCommandBar();
		}

		void ClearCommandBar()
		{
			if (_toolbarProvider != null)
			{
				_toolbarProvider = null;
				if (Device.Idiom == TargetIdiom.Phone)
					_page.BottomAppBar = null;
				else
					_page.TopAppBar = null;
			}
		}

		CommandBar CreateCommandBar()
		{
			var bar = new FormsCommandBar();
			if (Device.Idiom != TargetIdiom.Phone)
				bar.Style = (Windows.UI.Xaml.Style)Windows.UI.Xaml.Application.Current.Resources["TitleToolbar"];

			_toolbarProvider = new ToolbarProvider(bar);

			if (Device.Idiom == TargetIdiom.Phone)
				_page.BottomAppBar = bar;
			else
				_page.TopAppBar = bar;

			return bar;
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

			if (provider != null && _toolbarProvider == null)
				ClearCommandBar();

			return provider;
		}

		async Task<CommandBar> GetCommandBarAsync()
		{
			IToolbarProvider provider = GetToolbarProvider();
			if (provider == null)
			{
				return null;
			}

			return await provider.GetCommandBarAsync();
		}

		internal static void SubscribeAlertsAndActionSheets()
		{
			MessagingCenter.Subscribe<Page, AlertArguments>(Window.Current, Page.AlertSignalName, OnPageAlert);
			MessagingCenter.Subscribe<Page, ActionSheetArguments>(Window.Current, Page.ActionSheetSignalName, OnPageActionSheet);
		}

		static void OnPageActionSheet(object sender, ActionSheetArguments options)
		{
			bool userDidSelect = false;
			var flyoutContent = new FormsFlyout(options);

			var actionSheet = new Flyout
			{
				FlyoutPresenterStyle = (Windows.UI.Xaml.Style)Windows.UI.Xaml.Application.Current.Resources["FormsFlyoutPresenterStyle"],
				Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Full,
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

			actionSheet.ShowAt(((Page)sender).GetOrCreateRenderer().ContainerElement);
		}

		static async void OnPageAlert(Page sender, AlertArguments options)
		{
			string content = options.Message ?? string.Empty;
			string title = options.Title ?? string.Empty;

			var alertDialog = new AlertDialog
			{
				Content = content,
				Title = title,
				VerticalScrollBarVisibility = Windows.UI.Xaml.Controls.ScrollBarVisibility.Auto
			};

			if (options.Cancel != null)
				alertDialog.SecondaryButtonText = options.Cancel;

			if (options.Accept != null)
				alertDialog.PrimaryButtonText = options.Accept;

			while (s_currentAlert != null)
			{
				await s_currentAlert;
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

		class ToolbarProvider : IToolbarProvider
		{
			readonly Task<CommandBar> _commandBar;

			public ToolbarProvider(CommandBar commandBar)
			{
				_commandBar = Task.FromResult(commandBar);
			}

			public CommandBar CommandBar => _commandBar.Result;

			public Task<CommandBar> GetCommandBarAsync()
			{
				return _commandBar;
			}
		}
	}
}