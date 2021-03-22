using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.AppThemeGalleries;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.CollectionViewGalleries;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.CollectionViewGalleries.CarouselViewGalleries;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.DateTimePickerGalleries;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.DragAndDropGalleries;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.GradientGalleries;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.PlatformTestsGallery;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.RadioButtonGalleries;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.RefreshViewGalleries;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.ShapesGalleries;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.SwipeViewGalleries;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.VisualStateManagerGalleries;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	[Preserve(AllMembers = true)]
	public static class Messages
	{
		public const string ChangeRoot = "com.xamarin.ChangeRoot";
	}

	[Preserve(AllMembers = true)]
	internal class CoreCarouselPage : CarouselPage
	{
		public CoreCarouselPage()
		{
			AutomationId = "CarouselPageRoot";
			Children.Add(new CoreRootPage(this, NavigationBehavior.PushModalAsync) { Title = "Page 1" });
			Children.Add(new CoreRootPage(this, NavigationBehavior.PushModalAsync) { Title = "Page 2" });
		}
	}

	[Preserve(AllMembers = true)]
	internal class CoreContentPage : ContentPage
	{
		public CoreRootView CoreRootView { get; }
		public CoreContentPage()
		{
			On<iOS>().SetUseSafeArea(true);
			AutomationId = "ContentPageRoot";
			CoreRootView = new CoreRootView();
			Content = new StackLayout { Children = { CoreRootView, new CorePageView(this, NavigationBehavior.PushModalAsync) } };
		}
	}

	[Preserve(AllMembers = true)]
	internal class CoreFlyoutPage : FlyoutPage
	{
		public CoreFlyoutPage()
		{
			AutomationId = "FlyoutPageRoot";

			var toCrashButton = new Button { Text = "Crash Me" };

			var masterPage = new ContentPage { Title = "Menu", IconImageSource = "bank.png", Content = toCrashButton };
			var detailPage = new CoreRootPage(this, NavigationBehavior.PushModalAsync) { Title = "DetailPage" };

			bool toggle = false;
			toCrashButton.Clicked += (sender, args) =>
			{
				if (toggle)
					Detail = new ContentPage { BackgroundColor = Color.Green, };
				else
					Detail = detailPage;

				toggle = !toggle;
			};

			Flyout = masterPage;
			Detail = detailPage;
		}
	}
	[Preserve(AllMembers = true)]
	internal class CoreNavigationPage : NavigationPage
	{
		public CoreNavigationPage()
		{
			AutomationId = "NavigationPageRoot";

			BarBackgroundColor = Color.Maroon;
			BarTextColor = Color.Yellow;

			Device.StartTimer(TimeSpan.FromSeconds(2), () =>
			{
				BarBackgroundColor = Color.Default;
				BarTextColor = Color.Default;

				return false;
			});

			On<iOS>().SetPrefersLargeTitles(true);

			Navigation.PushAsync(new CoreRootPage(this));
		}
	}
	[Preserve(AllMembers = true)]
	public class CoreTabbedPageAsBottomNavigation : CoreTabbedPageBase
	{
		protected override void Init()
		{
			On<Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
			base.Init();
		}
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2456, "StackOverflow after reordering tabs in a TabbedPageView", PlatformAffected.All)]
	public class CoreTabbedPage : CoreTabbedPageBase
	{
	}

	[Preserve(AllMembers = true)]
	public class CoreTabbedPageBase : TestTabbedPage
	{
		protected override void Init()
		{
		}
#if APP
		public CoreTabbedPageBase()
		{
			AutomationId = "TabbedPageRoot";


			Device.StartTimer(TimeSpan.FromSeconds(6), () =>
			{
				BarBackgroundColor = Color.Maroon;
				BarTextColor = Color.Yellow;

				Device.StartTimer(TimeSpan.FromSeconds(6), () =>
				{
					BarBackgroundColor = Color.Default;
					BarTextColor = Color.Default;

					return false;
				});

				return false;
			});

			Children.Add(new CoreRootPage(this, NavigationBehavior.PushModalAsync) { Title = "Tab 1" });
			Children.Add(new CoreRootPage(this, NavigationBehavior.PushModalAsync) { Title = "Tab 2" });
			Children.Add(new NavigationPage(new Page())
			{
				Title = "Rubriques",
				IconImageSource = "coffee.png",
				BarBackgroundColor = Color.Blue,
				BarTextColor = Color.Aqua
			});

			Children.Add(new NavigationPage(new Page())
			{
				Title = "Le Club"
			});

			Children.Add(new NavigationPage(new Page { Title = "Bookmarks" })
			{
				Title = "Bookmarks",
			});

			if (On<Android>().GetMaxItemCount() > 5)
			{
				Children.Add(new NavigationPage(new Page { Title = "Alertes" })
				{
					Title = "Notifications",
				});

				Children.Add(new NavigationPage(new Page { Title = "My account" })
				{
					Title = "My account",
				});

				Children.Add(new NavigationPage(new Page { Title = "About" })
				{
					Title = "About",
				});
			}
		}
#endif

#if UITest
		[Test]
		[Issue (IssueTracker.Github, 2456, "StackOverflow after reordering tabs in a TabbedPageView", PlatformAffected.iOS)]
		public void TestReorderTabs ()
		{
			App.Tap (c => c.Marked("More"));
			App.Tap (c => c.Marked("Edit"));
			var bookmarks = App.Query (c => c.Marked ("Bookmarks"))[0];
			var notifications = App.Query (c => c.Marked ("Notifications"))[0];
			var tab2 = App.Query (c => c.Marked ("Tab 2"))[2];
			var rubriques = App.Query (c => c.Marked ("Rubriques"))[2];
			App.DragCoordinates (bookmarks.Rect.CenterX, bookmarks.Rect.CenterY, rubriques.Rect.CenterX, rubriques.Rect.CenterY);
			App.DragCoordinates (notifications.Rect.CenterX, notifications.Rect.CenterY, tab2.Rect.CenterX, tab2.Rect.CenterY);
			App.Tap (c => c.Marked("Done"));
			App.Tap (c => c.Marked("Tab 1"));
			App.Tap (c => c.Marked("Le Club"));
			App.Tap (c => c.Marked("Bookmarks"));
			App.Tap (c => c.Marked("Notifications"));
		}
#endif
	}

	[Preserve(AllMembers = true)]
	internal class CoreViewContainer
	{
		public string Name { get; private set; }
		public Type PageType { get; private set; }

		public CoreViewContainer(string name, Type pageType)
		{
			Name = name;
			PageType = pageType;
		}
	}
	[Preserve(AllMembers = true)]
	public class CoreRootView : ListView
	{
		public CoreRootView()
		{
			var roots = new[] {
				new CoreViewContainer ("SwapRoot - Tests", typeof(PlatformTestsConsole)),
				new CoreViewContainer ("SwapRoot - CarouselPage", typeof(CoreCarouselPage)),
				new CoreViewContainer ("SwapRoot - ContentPage", typeof(CoreContentPage)),
				new CoreViewContainer ("SwapRoot - FlyoutPage", typeof(CoreFlyoutPage)),
				new CoreViewContainer ("SwapRoot - NavigationPage", typeof(CoreNavigationPage)),
				new CoreViewContainer ("SwapRoot - TabbedPage", typeof(CoreTabbedPage)),
				new CoreViewContainer ("SwapRoot - BottomNavigation TabbedPage", typeof(CoreTabbedPageAsBottomNavigation)),
				new CoreViewContainer ("SwapRoot - Store Shell", typeof(XamStore.StoreShell)),
			};

			var template = new DataTemplate(typeof(TextCell));
			template.SetBinding(TextCell.TextProperty, "Name");

			ItemTemplate = template;
			ItemsSource = roots;

#if PRE_APPLICATION_CLASS
			ItemSelected += (sender, args) => MessagingCenter.Send (this, Messages.ChangeRoot, ((CoreViewContainer)args.SelectedItem).PageType);
#else
			ItemSelected += (sender, args) =>
			{
				var app = Application.Current as App;
				if (app != null)
				{
					var page = (Page)Activator.CreateInstance(((CoreViewContainer)args.SelectedItem).PageType);
					app.SetMainPage(page);
				}
			};
#endif
			SetValue(AutomationProperties.NameProperty, "SwapRoot");
		}
	}


	[Preserve(AllMembers = true)]
	internal class CorePageView : ListView
	{
		[Preserve(AllMembers = true)]
		internal class GalleryPageFactory
		{
			public GalleryPageFactory(Func<Page> create, string title)
			{
				Realize = () =>
				{
					var p = create();
					p.Title = title;
					return p;
				};

				Title = title;
				TitleAutomationId = $"{Title}AutomationId";
			}

			public Func<Page> Realize { get; set; }
			public string Title { get; set; }

			public string TitleAutomationId
			{
				get;
				set;
			}

			public override string ToString()
			{
				// a11y: let Narrator read a friendly string instead of the default ToString()
				return Title;
			}
		}

		List<GalleryPageFactory> _pages = new List<GalleryPageFactory> {
				new GalleryPageFactory(() => new GalleryPages.LayoutGalleries.LayoutGallery(), ".NET MAUI Layouts"),
				new GalleryPageFactory(() => new TabIndexTest.TabIndex(), "Accessibility TabIndex (2)"),
				new GalleryPageFactory(() => new PlatformTestsConsole(), "Platform Automated Tests"),
				new GalleryPageFactory(() => new EmbeddedFonts(), "Embedded Fonts"),
				new GalleryPageFactory(() => new MemoryLeakGallery(), "Memory Leak"),
				new GalleryPageFactory(() => new Issues.A11yTabIndex(), "Accessibility TabIndex"),
				new GalleryPageFactory(() => new RadioButtonGalleries(), "RadioButton Gallery"),
				new GalleryPageFactory(() => new RadioButtonCoreGalleryPage(), "RadioButton Core Gallery"),
				new GalleryPageFactory(() => new FontImageSourceGallery(), "Font ImageSource"),
				new GalleryPageFactory(() => new IndicatorGalleries(), "IndicatorView Gallery"),
				new GalleryPageFactory(() => new CarouselViewGallery(), "CarouselView Gallery"),
				new GalleryPageFactory(() => new CarouselViewCoreGalleryPage(), "CarouselView Core Gallery"),
				new GalleryPageFactory(() => new CollectionViewGallery(), "CollectionView Gallery"),
				new GalleryPageFactory(() => new CollectionViewCoreGalleryPage(), "CollectionView Core Gallery"),
				new GalleryPageFactory(() => new Issues.PerformanceGallery(), "Performance"),
				new GalleryPageFactory(() => new EntryReturnTypeGalleryPage(), "Entry ReturnType "),
				new GalleryPageFactory(() => new VisualStateManagerGallery(), "VisualStateManager Gallery"),
				new GalleryPageFactory(() => new FlowDirectionGalleryLandingPage(), "FlowDirection"),
				new GalleryPageFactory(() => new AutomationPropertiesGallery(), "Accessibility"),
				new GalleryPageFactory(() => new PlatformSpecificsGallery(), "Platform Specifics"),
				new GalleryPageFactory(() => new NativeBindingGalleryPage(), "Native Binding Controls Gallery"),
				new GalleryPageFactory(() => new XamlNativeViews(), "Xaml Native Views Gallery"),
				new GalleryPageFactory(() => new CharacterSpacingGallery(), "CharacterSpacing Views Gallery"),
				new GalleryPageFactory(() => new AppLinkPageGallery(), "App Link Page Gallery"),
				new GalleryPageFactory(() => new NestedNativeControlGalleryPage(), "Nested Native Controls Gallery"),
				new GalleryPageFactory(() => new CellForceUpdateSizeGalleryPage(), "Cell Force Update Size Gallery"),
				new GalleryPageFactory(() => new AppearingGalleryPage(), "Appearing Gallery"),
				new GalleryPageFactory(() => new EntryCoreGalleryPage(), "Entry Gallery"),
				new GalleryPageFactory(() => new MaterialEntryGalleryPage(), "Entry Material Demos"),
				new GalleryPageFactory(() => new NavBarTitleTestPage(), "Titles And Navbar Windows"),
				new GalleryPageFactory(() => new PanGestureGalleryPage(), "Pan gesture Gallery"),
				new GalleryPageFactory(() => new SwipeGestureGalleryPage(), "Swipe gesture Gallery"),
				new GalleryPageFactory(() => new PinchGestureTestPage(), "Pinch gesture Gallery"),
				new GalleryPageFactory(() => new ClickGestureGalleryPage(), "Click gesture Gallery"),
				new GalleryPageFactory(() => new AutomationIdGallery(), "AutomationID Gallery"),
				new GalleryPageFactory(() => new LayoutPerformanceGallery(), "Layout Perf Gallery"),
				new GalleryPageFactory(() => new ListViewSelectionColor(), "ListView SelectionColor Gallery"),
				new GalleryPageFactory(() => new AlertGallery(), "DisplayAlert Gallery"),
				new GalleryPageFactory(() => new ToolbarItems(), "ToolbarItems Gallery"),
				new GalleryPageFactory(() => new ActionSheetGallery(), "ActionSheet Gallery"),
				new GalleryPageFactory(() => new DateTimePickerGallery(), "DateTime Picker Localization Gallery"),
				new GalleryPageFactory(() => new ActivityIndicatorCoreGalleryPage(), "ActivityIndicator Gallery"),
				new GalleryPageFactory(() => new BehaviorsAndTriggers(), "BehaviorsTriggers Gallery"),
				new GalleryPageFactory(() => new ContextActionsGallery(), "ContextActions List Gallery"),
				new GalleryPageFactory(() => new ContextActionsGallery (tableView: true), "ContextActions Table Gallery"),
				new GalleryPageFactory(() => new CoreBoxViewGalleryPage(), "BoxView Gallery"),
				new GalleryPageFactory(() => new ButtonCoreGalleryPage(), "Button Gallery"),
				new GalleryPageFactory(() => new ButtonLayoutGalleryPage(), "Button Layout Gallery"),
				new GalleryPageFactory(() => new ButtonLayoutGalleryPage(VisualMarker.Material), "Button Layout Gallery (Material)"),
				new GalleryPageFactory(() => new ButtonBorderBackgroundGalleryPage(), "Button Border & Background Gallery"),
				new GalleryPageFactory(() => new ButtonBorderBackgroundGalleryPage(VisualMarker.Material), "Button Border & Background Gallery (Material)"),
				new GalleryPageFactory(() => new CheckBoxCoreGalleryPage(), "CheckBox Gallery"),
				new GalleryPageFactory(() => new DatePickerCoreGalleryPage(), "DatePicker Gallery"),
				new GalleryPageFactory(() => new DragAndDropGallery(), "Drag and Drop Gallery"),
				new GalleryPageFactory(() => new EditorCoreGalleryPage(), "Editor Gallery"),
				new GalleryPageFactory(() => new FrameCoreGalleryPage(), "Frame Gallery"),
				new GalleryPageFactory(() => new GradientsGallery(), "Brushes Gallery"),
				new GalleryPageFactory(() => new ImageCoreGalleryPage(), "Image Gallery"),
				new GalleryPageFactory(() => new ImageButtonCoreGalleryPage(), "Image Button Gallery"),
				new GalleryPageFactory(() => new KeyboardCoreGallery(), "Keyboard Gallery"),
				new GalleryPageFactory(() => new LabelCoreGalleryPage(), "Label Gallery"),
				new GalleryPageFactory(() => new ListViewCoreGalleryPage(), "ListView Gallery"),
				new GalleryPageFactory(() => new OpenGLViewCoreGalleryPage(), "OpenGLView Gallery"),
				new GalleryPageFactory(() => new PickerCoreGalleryPage(), "Picker Gallery"),
				new GalleryPageFactory(() => new ProgressBarCoreGalleryPage(), "ProgressBar Gallery"),
				new GalleryPageFactory(() => new MaterialProgressBarGallery(), "ProgressBar & Slider Gallery (Material)"),
				new GalleryPageFactory(() => new MaterialActivityIndicatorGallery(), "ActivityIndicator Gallery (Material)"),
				new GalleryPageFactory(() => new RefreshViewGallery(), "RefreshView Gallery"),
				new GalleryPageFactory(() => new RefreshViewCoreGalleryPage(), "RefreshView Core Gallery"),
				new GalleryPageFactory(() => new ScrollGallery(), "ScrollView Gallery"),
				new GalleryPageFactory(() => new ScrollGallery(ScrollOrientation.Horizontal), "ScrollView Gallery Horizontal"),
				new GalleryPageFactory(() => new ScrollGallery(ScrollOrientation.Both), "ScrollView Gallery 2D"),
				new GalleryPageFactory(() => new SearchBarCoreGalleryPage(), "SearchBar Gallery"),
				new GalleryPageFactory(() => new ShapesGallery(), "Shapes Gallery"),
				new GalleryPageFactory(() => new SliderCoreGalleryPage(), "Slider Gallery"),
				new GalleryPageFactory(() => new StepperCoreGalleryPage(), "Stepper Gallery"),
				new GalleryPageFactory(() => new SwitchCoreGalleryPage(), "Switch Gallery"),
				new GalleryPageFactory(() => new SwipeViewCoreGalleryPage(), "SwipeView Core Gallery"),
				new GalleryPageFactory(() => new SwipeViewGallery(), "SwipeView Gallery"),
				new GalleryPageFactory(() => new TableViewCoreGalleryPage(), "TableView Gallery"),
				new GalleryPageFactory(() => new TimePickerCoreGalleryPage(), "TimePicker Gallery"),
				new GalleryPageFactory(() => new VisualGallery(), "Visual Gallery"),
				new GalleryPageFactory(() => new WebViewCoreGalleryPage(), "WebView Gallery"),
				new GalleryPageFactory(() => new WkWebViewCoreGalleryPage(), "WkWebView Gallery"),
				new GalleryPageFactory(() => new DynamicViewGallery(), "Dynamic ViewGallery"),
				new GalleryPageFactory(() => new AppThemeGallery(), "AppTheme Gallery"),
				//pages
 				new GalleryPageFactory(() => new RootContentPage ("Content"), "RootPages Gallery"),
				new GalleryPageFactory(() => new FlyoutPageTabletPage(), "FlyoutPage Tablet Page"),
				// legacy galleries
				new GalleryPageFactory(() => new AbsoluteLayoutGallery(), "AbsoluteLayout Gallery - Legacy"),
				new GalleryPageFactory(() => new BoundContentPage(), "BoundPage Gallery - Legacy"),
				new GalleryPageFactory(() => new BackgroundImageGallery(), "BackgroundImage gallery"),
				new GalleryPageFactory(() => new ButtonGallery(), "Button Gallery - Legacy"),
				new GalleryPageFactory(() => new CarouselPageGallery(), "CarouselPage Gallery - Legacy"),
				new GalleryPageFactory(() => new CellTypesListPage(), "Cells Gallery - Legacy"),
				new GalleryPageFactory(() => new ClipToBoundsGallery(), "ClipToBounds Gallery - Legacy"),
				new GalleryPageFactory(() => new ControlTemplatePage(), "ControlTemplated Gallery - Legacy"),
				new GalleryPageFactory(() => new ControlTemplateXamlPage(), "ControlTemplated XAML Gallery - Legacy"),
				new GalleryPageFactory(() => new DisposeGallery(), "Dispose Gallery - Legacy"),
				new GalleryPageFactory(() => new EditorGallery(), "Editor Gallery - Legacy"),
				new GalleryPageFactory(() => new EntryGallery(), "Entry Gallery - Legacy"),
				new GalleryPageFactory(() => new FrameGallery (), "Frame Gallery - Legacy"),
				   new GalleryPageFactory(() => new GifGallery(), "Gif Support Gallery"),
				new GalleryPageFactory(() => new GridGallery(), "Grid Gallery - Legacy"),
				new GalleryPageFactory(() => new GroupedListActionsGallery(), "GroupedListActions Gallery - Legacy"),
				new GalleryPageFactory(() => new GroupedListContactsGallery(), "GroupedList Gallery - Legacy"),
				new GalleryPageFactory(() => new ImageGallery (), "Image Gallery - Legacy"),
				new GalleryPageFactory(() => new ImageLoadingGallery (), "ImageLoading Gallery - Legacy"),
				new GalleryPageFactory(() => new InputIntentGallery(), "InputIntent Gallery - Legacy"),
				new GalleryPageFactory(() => new LabelGallery(), "Label Gallery - Legacy"),
				new GalleryPageFactory(() => new LayoutAddPerformance(), "Layout Add Performance - Legacy"),
				new GalleryPageFactory(() => new LayoutOptionsGallery(), "LayoutOptions Gallery - Legacy"),
				new GalleryPageFactory(() => new LineBreakModeGallery(), "LineBreakMode Gallery - Legacy"),
				new GalleryPageFactory(() => new ListPage(), "ListView Gallery - Legacy"),
				new GalleryPageFactory(() => new ListScrollTo(), "ListView.ScrollTo"),
				new GalleryPageFactory(() => new ListRefresh(), "ListView.PullToRefresh"),
				new GalleryPageFactory(() => new ListViewDemoPage(), "ListView Demo Gallery - Legacy"),
				new GalleryPageFactory(() => new MapGallery(), "Map Gallery - Legacy"),
				new GalleryPageFactory(() => new MapWithItemsSourceGallery(), "Map With ItemsSource Gallery - Legacy"),
				new GalleryPageFactory(() => new MapElementsGallery(), "Map Elements Gallery - Legacy"),
				new GalleryPageFactory(() => new MinimumSizeGallery(), "MinimumSize Gallery - Legacy"),
				new GalleryPageFactory(() => new MultiGallery(), "Multi Gallery - Legacy"),
				new GalleryPageFactory(() => new NavigationPropertiesGallery(), "Navigation Properties"),
#if HAVE_OPENTK
				new GalleryPageFactory(() => new BasicOpenGLGallery(), "Basic OpenGL Gallery - Legacy"),
				new GalleryPageFactory(() => new AdvancedOpenGLGallery(), "Advanced OpenGL Gallery - Legacy"),
#endif
				new GalleryPageFactory(() => new PickerGallery(), "Picker Gallery - Legacy"),
				new GalleryPageFactory(() => new ProgressBarGallery(), "ProgressBar Gallery - Legacy"),
				new GalleryPageFactory(() => new RelativeLayoutGallery(), "RelativeLayout Gallery - Legacy"),
				new GalleryPageFactory(() => new ScaleRotate(), "Scale Rotate Gallery - Legacy"),
				new GalleryPageFactory(() => new SearchBarGallery(), "SearchBar Gallery - Legacy"),
				new GalleryPageFactory(() => new SettingsPage(), "Settings Page - Legacy"),
				new GalleryPageFactory(() => new SliderGallery(), "Slider Gallery - Legacy"),
				new GalleryPageFactory(() => new StackLayoutGallery(), "StackLayout Gallery - Legacy"),
				new GalleryPageFactory(() => new StepperGallery(), "Stepper Gallery - Legacy"),
				new GalleryPageFactory(() => new StyleGallery(), "Style Gallery"),
				new GalleryPageFactory(() => new StyleXamlGallery(), "Style Gallery in Xaml"),
				new GalleryPageFactory(() => new SwitchGallery(), "Switch Gallery - Legacy"),
				new GalleryPageFactory(() => new TableViewGallery(), "TableView Gallery - Legacy"),
				new GalleryPageFactory(() => new TemplatedCarouselGallery(), "TemplatedCarouselPage Gallery - Legacy"),
				new GalleryPageFactory(() => new TemplatedTabbedGallery(), "TemplatedTabbedPage Gallery - Legacy"),
				new GalleryPageFactory(() => new UnevenViewCellGallery(), "UnevenViewCell Gallery - Legacy"),
				new GalleryPageFactory(() => new UnevenListGallery(), "UnevenList Gallery - Legacy"),
				new GalleryPageFactory(() => new ViewCellGallery(), "ViewCell Gallery - Legacy"),
				new GalleryPageFactory(() => new WebViewGallery(), "WebView Gallery - Legacy"),
				new GalleryPageFactory(() => new BindableLayoutGalleryPage(), "BindableLayout Gallery - Legacy"),
				new GalleryPageFactory(() => new ShowModalWithTransparentBkgndGalleryPage(), "Modal With Transparent Background Gallery - Legacy"),
			};

		public CorePageView(Page rootPage, NavigationBehavior navigationBehavior = NavigationBehavior.PushAsync)
		{
			var galleryFactory = DependencyService.Get<IPlatformSpecificCoreGalleryFactory>();

			var platformPages = galleryFactory?.GetPages();
			if (platformPages != null)
				_pages.AddRange(platformPages.Select(p => new GalleryPageFactory(p.Create, p.Title + " (Platform Specifc)")));

			_titleToPage = _pages.ToDictionary(o => o.Title);

			// avoid NRE for root pages without NavigationBar
			if (navigationBehavior == NavigationBehavior.PushAsync && rootPage.GetType() == typeof(CoreNavigationPage))
			{
				_pages.Insert(0, new GalleryPageFactory(() => new NavigationBarGallery((NavigationPage)rootPage), "NavigationBar Gallery - Legacy"));
				_pages.Insert(1, new GalleryPageFactory(() => new TitleView(true), "TitleView"));
			}

			_pages.Sort((x, y) => string.Compare(x.Title, y.Title, true));

			var template = new DataTemplate(() =>
			{
				var cell = new TextCell();
				cell.ContextActions.Add(new MenuItem
				{
					Text = "Select Visual",
					Command = new Command(async () =>
					{
						var buttons = typeof(VisualMarker).GetProperties().Select(p => p.Name);
						var selection = await rootPage.DisplayActionSheet("Select Visual", "Cancel", null, buttons.ToArray());
						if (cell.BindingContext is GalleryPageFactory pageFactory)
						{
							var page = pageFactory.Realize();
							if (typeof(VisualMarker).GetProperty(selection)?.GetValue(null) is IVisual visual)
								page.Visual = visual;
							await PushPage(page);
						}
					})
				});

				return cell;
			});

			template.SetBinding(TextCell.TextProperty, "Title");
			template.SetBinding(TextCell.AutomationIdProperty, "TitleAutomationId");

			BindingContext = _pages;
			ItemTemplate = template;
			ItemsSource = _pages;

			ItemSelected += async (sender, args) =>
			{
				if (SelectedItem == null)
					return;

				var item = args.SelectedItem;
				var page = item as GalleryPageFactory;
				if (page != null)
				{
					var realize = page.Realize();
					if (realize is Shell)
						Application.Current.MainPage = realize;
					else
						await PushPage(realize);
				}

				SelectedItem = null;
			};

			SetValue(AutomationProperties.NameProperty, "Core Pages");
		}


		async Task PushPage(Page contentPage)
		{
			await Navigation.PushAsync(contentPage);
		}

		readonly Dictionary<string, GalleryPageFactory> _titleToPage;
		public async Task<bool> PushPage(string pageTitle)
		{

			GalleryPageFactory pageFactory = null;
			if (!_titleToPage.TryGetValue(pageTitle, out pageFactory))
				return false;

			var page = pageFactory.Realize();

			await PushPage(page);
			return true;
		}

		public void FilterPages(string filter)
		{
			if (string.IsNullOrWhiteSpace(filter))
				ItemsSource = _pages;
			else
				ItemsSource = _pages.Where(p => p.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1);
		}
	}
	[Preserve(AllMembers = true)]
	internal class CoreRootPage : ContentPage
	{
		CoreRootView CoreRootView { get; }

		public CoreRootPage(Page rootPage, NavigationBehavior navigationBehavior = NavigationBehavior.PushAsync)
		{
			ValidateRegistrar();

			var galleryFactory = DependencyService.Get<IPlatformSpecificCoreGalleryFactory>();

			Title = galleryFactory?.Title ?? "Core Gallery";

			var corePageView = new CorePageView(rootPage, navigationBehavior);

			var searchBar = new SearchBar()
			{
				AutomationId = "SearchBar"
			};

			searchBar.TextChanged += (sender, e) =>
			{
				corePageView.FilterPages(e.NewTextValue);
			};

			var testCasesButton = new Button
			{
				Text = "Go to Test Cases",
				AutomationId = "GoToTestButton",
				TabIndex = -2,
				Command = new Command(async () =>
				{
					if (!string.IsNullOrEmpty(searchBar.Text))
					{
						if (!(await corePageView.PushPage(searchBar.Text)))
						{
							foreach (CoreViewContainer item in CoreRootView.ItemsSource)
							{
								if (item.Name == searchBar.Text)
								{
									CoreRootView.SelectedItem = item;
									break;
								}
							}
						}
					}
					else
						await Navigation.PushModalAsync(TestCases.GetTestCases());
				})
			};

			var stackLayout = new StackLayout()
			{
				Children = {
					testCasesButton,
					searchBar,
					new Button {
						Text = "Click to Force GC",
						TabIndex = -2,
						Command = new Command(() => {
							GC.Collect ();
							GC.WaitForPendingFinalizers ();
							GC.Collect ();
						})
					}
				}
			};

			var secondaryWindowService = DependencyService.Get<ISecondaryWindowService>();
			if (secondaryWindowService != null)
			{
				var openSecondWindowButton = new Button() { Text = "Open Secondary Window" };
				openSecondWindowButton.Clicked += (obj, args) => { secondaryWindowService.OpenSecondaryWindow(typeof(Issue2482)); };
				stackLayout.Children.Add(openSecondWindowButton);
			}

			this.SetAutomationPropertiesName("Gallery");
			this.SetAutomationPropertiesHelpText("Lists all gallery pages");

			CoreRootView = new CoreRootView();
			Content = new AbsoluteLayout
			{
				Children = {
					{ CoreRootView, new Rectangle(0, 0.0, 1, 0.35), AbsoluteLayoutFlags.All },
					{ stackLayout, new Rectangle(0, 0.5, 1, 0.30), AbsoluteLayoutFlags.All },
					{ corePageView, new Rectangle(0, 1.0, 1.0, 0.35), AbsoluteLayoutFlags.All },
				}
			};
		}

		void ValidateRegistrar()
		{
			foreach (var view in Issues.Helpers.ViewHelper.GetAllViews())
			{
				if (!DependencyService.Get<IRegistrarValidationService>().Validate(view, out string message))
					throw new InvalidOperationException(message);
			}

			foreach (var page in Issues.Helpers.ViewHelper.GetAllPages())
			{
				page.Visual = VisualMarker.Default;
				if (!DependencyService.Get<IRegistrarValidationService>().Validate(page, out string message))
					throw new InvalidOperationException(message);
			}
		}
	}

	[Preserve(AllMembers = true)]
	public interface IPlatformSpecificCoreGalleryFactory
	{
		string Title { get; }

		IEnumerable<(Func<Page> Create, string Title)> GetPages();
	}
	[Preserve(AllMembers = true)]
	public static class CoreGallery
	{
		public static Page GetMainPage()
		{
			return new CoreNavigationPage();
		}
	}
}
