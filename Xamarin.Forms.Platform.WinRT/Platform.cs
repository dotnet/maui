using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Xamarin.Forms.Internals;

#if WINDOWS_UWP
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
#endif

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public abstract class Platform : IPlatform, INavigation, IToolbarProvider
	{
		internal static readonly BindableProperty RendererProperty = BindableProperty.CreateAttached("Renderer", typeof(IVisualElementRenderer), typeof(Platform), default(IVisualElementRenderer));

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
				throw new ArgumentNullException("element");

			IVisualElementRenderer renderer = Registrar.Registered.GetHandler<IVisualElementRenderer>(element.GetType()) ?? new DefaultRenderer();
			renderer.SetElement(element);
			return renderer;
		}

		internal Platform(Windows.UI.Xaml.Controls.Page page)
		{
			if (page == null)
				throw new ArgumentNullException("page");

			_page = page;

			_container = new Canvas { Style = (Windows.UI.Xaml.Style)Windows.UI.Xaml.Application.Current.Resources["RootContainerStyle"] };

			_page.Content = _container;

			_container.SizeChanged += OnRendererSizeChanged;

			MessagingCenter.Subscribe(this, Page.BusySetSignalName, (Page sender, bool enabled) =>
			{
				Windows.UI.Xaml.Controls.ProgressBar indicator = GetBusyIndicator();
				indicator.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
			});

			_toolbarTracker.CollectionChanged += OnToolbarItemsChanged;

			MessagingCenter.Subscribe<Page, AlertArguments>(this, Page.AlertSignalName, OnPageAlert);
			MessagingCenter.Subscribe<Page, ActionSheetArguments>(this, Page.ActionSheetSignalName, OnPageActionSheet);

			UpdateBounds();


#if WINDOWS_UWP
			if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
			{
				StatusBar statusBar = StatusBar.GetForCurrentView();
				statusBar.Showing += (sender, args) => UpdateBounds();
				statusBar.Hiding += (sender, args) => UpdateBounds();
			}
#endif
		}

		internal void SetPage(Page newRoot)
		{
			if (newRoot == null)
				throw new ArgumentNullException("newRoot");

			_navModel.Clear();

			_navModel.Push(newRoot, null);
			SetCurrent(newRoot, false, true);
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
			throw new InvalidOperationException("PopToRootAsync is not supported globally on Windows, please use a NavigationPage.");
		}

		void INavigation.RemovePage(Page page)
		{
			throw new InvalidOperationException("RemovePage is not supported globally on Windows, please use a NavigationPage.");
		}

		void INavigation.InsertPageBefore(Page page, Page before)
		{
			throw new InvalidOperationException("InsertPageBefore is not supported globally on Windows, please use a NavigationPage.");
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
				throw new ArgumentNullException("page");

			var tcs = new TaskCompletionSource<bool>();
			_navModel.PushModal(page);
			SetCurrent(page, animated, completedCallback: () => tcs.SetResult(true));
			return tcs.Task;
		}

		Task<Page> INavigation.PopModalAsync(bool animated)
		{
			var tcs = new TaskCompletionSource<Page>();
			Page result = _navModel.PopModal();
			SetCurrent(_navModel.CurrentPage, animated, true, () => tcs.SetResult(result));
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

		internal virtual Rectangle WindowBounds
		{
			get { return _bounds; }
		}

		internal void UpdatePageSizes()
		{
			Rectangle bounds = WindowBounds;
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

		internal async Task UpdateToolbarItems()
		{
			CommandBar commandBar = await GetCommandBarAsync();
			if (commandBar != null)
			{
				commandBar.PrimaryCommands.Clear();
				commandBar.SecondaryCommands.Clear();
#if WINDOWS_UWP
				if (_page.BottomAppBar != null || _page.TopAppBar != null)
				{
					_page.BottomAppBar = null;
					_page.TopAppBar = null;
					_page.InvalidateMeasure();
				}
#endif
			}

#if !WINDOWS_UWP
			commandBar = AddOpenMasterButton(commandBar);
#endif

#if WINDOWS_UWP
			var toolBarProvider = GetToolbarProvider() as IToolBarForegroundBinder;
#endif

			foreach (ToolbarItem item in _toolbarTracker.ToolbarItems.OrderBy(ti => ti.Priority))
			{
				if (commandBar == null)
					commandBar = CreateCommandBar();

#if WINDOWS_UWP
				toolBarProvider?.BindForegroundColor(commandBar);
#endif

				var button = new AppBarButton();
				button.SetBinding(AppBarButton.LabelProperty, "Text");
				button.SetBinding(AppBarButton.IconProperty, "Icon", _fileImageSourcePathConverter);
				button.Command = new MenuItemCommand(item);
				button.DataContext = item;

#if WINDOWS_UWP
				toolBarProvider?.BindForegroundColor(button);
#endif

				ToolbarItemOrder order = item.Order == ToolbarItemOrder.Default ? ToolbarItemOrder.Primary : item.Order;
				if (order == ToolbarItemOrder.Primary)
					commandBar.PrimaryCommands.Add(button);
				else
					commandBar.SecondaryCommands.Add(button);
			}

			if (commandBar?.PrimaryCommands.Count + commandBar?.SecondaryCommands.Count == 0)
				ClearCommandBar();
		}

#if !WINDOWS_UWP
		CommandBar AddOpenMasterButton(CommandBar commandBar)
		{
			if (!_toolbarTracker.HaveMasterDetail)
			{
				return commandBar;
			}

			if (commandBar == null)
			{
				commandBar = CreateCommandBar();
			}

			Page target = _toolbarTracker.Target;
			var mdp = target as MasterDetailPage;
			while (mdp == null)
			{
				var container = target as IPageContainer<Page>;
				if (container == null)
				{
					break;
				}

				target = container.CurrentPage;
				mdp = container.CurrentPage as MasterDetailPage;
			}

			if (mdp == null || !mdp.ShouldShowToolbarButton())
			{
				return commandBar;
			}

			var openMaster = new AppBarButton { DataContext = mdp };
			openMaster.SetBinding(AppBarButton.LabelProperty, "Master.Title");
			openMaster.SetBinding(AppBarButton.IconProperty, "Master.Icon", _fileImageSourcePathConverter);
			openMaster.Click += (s, a) => { mdp.IsPresented = !mdp.IsPresented; };

			commandBar.PrimaryCommands.Add(openMaster);

			return commandBar;
		}
#endif

		Rectangle _bounds;
		readonly Canvas _container;
		readonly Windows.UI.Xaml.Controls.Page _page;
		Windows.UI.Xaml.Controls.ProgressBar _busyIndicator;
		Page _currentPage;
		readonly NavigationModel _navModel = new NavigationModel();
		readonly ToolbarTracker _toolbarTracker = new ToolbarTracker();
		readonly FileImageSourcePathConverter _fileImageSourcePathConverter = new FileImageSourcePathConverter();

#pragma warning disable 649
        IToolbarProvider _toolbarProvider;
#pragma warning restore 649

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

		Windows.UI.Xaml.Controls.ProgressBar GetBusyIndicator()
		{
			if (_busyIndicator == null)
			{
				_busyIndicator = new Windows.UI.Xaml.Controls.ProgressBar { IsIndeterminate = true, Visibility = Visibility.Collapsed, VerticalAlignment = VerticalAlignment.Top };

				Canvas.SetZIndex(_busyIndicator, 1);
				_container.Children.Add(_busyIndicator);
			}

			return _busyIndicator;
		}

		internal bool BackButtonPressed()
		{
			if (_currentActionSheet != null)
			{
				CancelActionSheet();
				return true;
			}

			Page lastRoot = _navModel.Roots.Last();

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

		void CancelActionSheet()
		{
			if (_currentActionSheet == null)
				return;

			_actionSheetOptions.SetResult(null);
			_actionSheetOptions = null;
			_currentActionSheet.IsOpen = false;
			_currentActionSheet = null;
		}

		void UpdateBounds()
		{
			_bounds = new Rectangle(0, 0, _page.ActualWidth, _page.ActualHeight);
#if WINDOWS_UWP
			if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
			{
				StatusBar statusBar = StatusBar.GetForCurrentView();

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
#endif
		}

		void OnRendererSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			UpdateBounds();
			UpdatePageSizes();
		}

		async void SetCurrent(Page newPage, bool animated, bool popping = false, Action completedCallback = null)
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
					previousPage.Cleanup();
			}

			newPage.Layout(new Rectangle(0, 0, _page.ActualWidth, _page.ActualHeight));

			IVisualElementRenderer pageRenderer = newPage.GetOrCreateRenderer();
			_container.Children.Add(pageRenderer.ContainerElement);

			pageRenderer.ContainerElement.Width = _container.ActualWidth;
			pageRenderer.ContainerElement.Height = _container.ActualHeight;

			if (completedCallback != null)
				completedCallback();

			_currentPage = newPage;

			UpdateToolbarTracker();
			UpdateToolbarTitle(newPage);
			await UpdateToolbarItems();
		}

		void UpdateToolbarTitle(Page page)
		{
			if (_toolbarProvider == null)
				return;

			((ToolbarProvider)_toolbarProvider).CommandBar.Content = page.Title;
		}

		Task<CommandBar> IToolbarProvider.GetCommandBarAsync()
		{
			return GetCommandBarAsync();
		}

#pragma warning disable 1998 // considered for removal
		async Task<CommandBar> GetCommandBarAsync()
#pragma warning restore 1998
		{
#if !WINDOWS_UWP
			return _page.BottomAppBar as CommandBar;
#else
			IToolbarProvider provider = GetToolbarProvider();
			//var titleProvider = provider as ITitleProvider; 
			if (provider == null) // || (titleProvider != null && !titleProvider.ShowTitle))
				return null;

			return await provider.GetCommandBarAsync();
#endif
		}

		CommandBar CreateCommandBar()
		{
#if !WINDOWS_UWP
			var commandBar = new CommandBar();
			_page.BottomAppBar = commandBar;
			return commandBar;
#else

			var bar = new FormsCommandBar();
			if (Device.Idiom != TargetIdiom.Phone)
				bar.Style = (Windows.UI.Xaml.Style)Windows.UI.Xaml.Application.Current.Resources["TitleToolbar"];

			_toolbarProvider = new ToolbarProvider(bar);

			if (Device.Idiom == TargetIdiom.Phone)
				_page.BottomAppBar = bar;
			else
				_page.TopAppBar = bar;

			return bar;
#endif
		}

		void ClearCommandBar()
		{
#if !WINDOWS_UWP
			_page.BottomAppBar = null;
#else
			if (_toolbarProvider != null)
			{
				_toolbarProvider = null;
				if (Device.Idiom == TargetIdiom.Phone)
					_page.BottomAppBar = null;
				else
					_page.TopAppBar = null;
			}
#endif
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

		ActionSheetArguments _actionSheetOptions;
		Popup _currentActionSheet;

#if WINDOWS_UWP
		async void OnPageActionSheet(Page sender, ActionSheetArguments options)
		{
			List<string> buttons = options.Buttons.ToList();

			var list = new Windows.UI.Xaml.Controls.ListView
			{
				Style = (Windows.UI.Xaml.Style)Windows.UI.Xaml.Application.Current.Resources["ActionSheetList"],
				ItemsSource = buttons,
				IsItemClickEnabled = true
			};

			var dialog = new ContentDialog
			{
				Template = (Windows.UI.Xaml.Controls.ControlTemplate)Windows.UI.Xaml.Application.Current.Resources["MyContentDialogControlTemplate"],
				Content = list,
				Style = (Windows.UI.Xaml.Style)Windows.UI.Xaml.Application.Current.Resources["ActionSheetStyle"]
			};

			if (options.Title != null)
				dialog.Title = options.Title;

			list.ItemClick += (s, e) =>
			{
				dialog.Hide();
				options.SetResult((string)e.ClickedItem);
			};

			TypedEventHandler<CoreWindow, CharacterReceivedEventArgs> onEscapeButtonPressed = delegate(CoreWindow window, CharacterReceivedEventArgs args)
			{
				if (args.KeyCode == 27)
				{
					dialog.Hide();
					options.SetResult(ContentDialogResult.None.ToString());
				}
			};

			Window.Current.CoreWindow.CharacterReceived += onEscapeButtonPressed;

			_actionSheetOptions = options;

			if (options.Cancel != null)
				dialog.SecondaryButtonText = options.Cancel;

			if (options.Destruction != null)
				dialog.PrimaryButtonText = options.Destruction;

			ContentDialogResult result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Secondary)
				options.SetResult(options.Cancel);
			else if (result == ContentDialogResult.Primary)
				options.SetResult(options.Destruction);

			Window.Current.CoreWindow.CharacterReceived -= onEscapeButtonPressed;
		}
#else
		void OnPageActionSheet(Page sender, ActionSheetArguments options)
		{
			var finalArguments = new List<string>();
			if (options.Destruction != null)
				finalArguments.Add(options.Destruction);
			if (options.Buttons != null)
				finalArguments.AddRange(options.Buttons);
			if (options.Cancel != null)
				finalArguments.Add(options.Cancel);

			var list = new Windows.UI.Xaml.Controls.ListView
			{
				Style = (Windows.UI.Xaml.Style)Windows.UI.Xaml.Application.Current.Resources["ActionSheetList"],
				ItemsSource = finalArguments,
				IsItemClickEnabled = true
			};

			list.ItemClick += (s, e) =>
			{
				_currentActionSheet.IsOpen = false;
				options.SetResult((string)e.ClickedItem);
			};

			_actionSheetOptions = options;

			Size size = Device.Info.ScaledScreenSize;

			var stack = new StackPanel
			{
				MinWidth = 100,
				Children =
				{
					new TextBlock
					{
						Text = options.Title ?? string.Empty,
						Style = (Windows.UI.Xaml.Style)Windows.UI.Xaml.Application.Current.Resources["TitleTextBlockStyle"],
						Margin = new Windows.UI.Xaml.Thickness(0, 0, 0, 10),
						Visibility = options.Title != null ? Visibility.Visible : Visibility.Collapsed
					},
					list
				}
			};

			var border = new Border
			{
				Child = stack,
				BorderBrush = new SolidColorBrush(Colors.White),
				BorderThickness = new Windows.UI.Xaml.Thickness(1),
				Padding = new Windows.UI.Xaml.Thickness(15),
				Background = (Brush)Windows.UI.Xaml.Application.Current.Resources["AppBarBackgroundThemeBrush"]
			};

			Windows.UI.Xaml.Controls.Grid.SetRow(border, 1);
			Windows.UI.Xaml.Controls.Grid.SetColumn(border, 1);

			var container = new Windows.UI.Xaml.Controls.Grid
			{
				RowDefinitions =
				{
					new Windows.UI.Xaml.Controls.RowDefinition { Height = new Windows.UI.Xaml.GridLength(1, Windows.UI.Xaml.GridUnitType.Star) },
					new Windows.UI.Xaml.Controls.RowDefinition { Height = new Windows.UI.Xaml.GridLength(0, Windows.UI.Xaml.GridUnitType.Auto) },
					new Windows.UI.Xaml.Controls.RowDefinition { Height = new Windows.UI.Xaml.GridLength(1, Windows.UI.Xaml.GridUnitType.Star) }
				},
				ColumnDefinitions =
				{
					new Windows.UI.Xaml.Controls.ColumnDefinition { Width = new Windows.UI.Xaml.GridLength(1, Windows.UI.Xaml.GridUnitType.Star) },
					new Windows.UI.Xaml.Controls.ColumnDefinition { Width = new Windows.UI.Xaml.GridLength(0, Windows.UI.Xaml.GridUnitType.Auto) },
					new Windows.UI.Xaml.Controls.ColumnDefinition { Width = new Windows.UI.Xaml.GridLength(1, Windows.UI.Xaml.GridUnitType.Star) }
				},
				Height = size.Height,
				Width = size.Width,
				Children = { border }
			};

			var bgPopup = new Popup { Child = new Canvas { Width = size.Width, Height = size.Height, Background = new SolidColorBrush(new Windows.UI.Color { A = 128, R = 0, G = 0, B = 0 }) } };

			bgPopup.IsOpen = true;

			_currentActionSheet = new Popup { ChildTransitions = new TransitionCollection { new PopupThemeTransition() }, IsLightDismissEnabled = true, Child = container };

			_currentActionSheet.Closed += (s, e) =>
			{
				bgPopup.IsOpen = false;
				CancelActionSheet();
			};

			if (Device.Idiom == TargetIdiom.Phone)
			{
				double height = WindowBounds.Height;
				stack.Height = height;
				stack.Width = size.Width;
				border.BorderThickness = new Windows.UI.Xaml.Thickness(0);

				_currentActionSheet.Height = height;
				_currentActionSheet.VerticalOffset = size.Height - height;
			}

			_currentActionSheet.IsOpen = true;
		}
#endif

		async void OnPageAlert(Page sender, AlertArguments options)
		{
			string content = options.Message ?? options.Title ?? string.Empty;

			MessageDialog dialog;
			if (options.Message == null || options.Title == null)
				dialog = new MessageDialog(content);
			else
				dialog = new MessageDialog(options.Message, options.Title);

			if (options.Accept != null)
			{
				dialog.Commands.Add(new UICommand(options.Accept));
				dialog.DefaultCommandIndex = 0;
			}

			if (options.Cancel != null)
			{
				dialog.Commands.Add(new UICommand(options.Cancel));
				dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
			}

			IUICommand command = await dialog.ShowAsync();
			options.SetResult(command.Label == options.Accept);
		}
	}
}