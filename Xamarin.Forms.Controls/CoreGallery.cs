using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Controls.GalleryPages;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Controls.GalleryPages.VisualStateManagerGalleries;

namespace Xamarin.Forms.Controls
{
	public static class Messages
	{
		public const string ChangeRoot = "com.xamarin.ChangeRoot";
	}

	internal class CoreCarouselPage : CarouselPage
	{
		public CoreCarouselPage ()
		{
			AutomationId = "CarouselPageRoot";
			Children.Add (new CoreRootPage (this, NavigationBehavior.PushModalAsync) { Title = "Page 1" });
			Children.Add (new CoreRootPage (this, NavigationBehavior.PushModalAsync) { Title = "Page 2" });
		}
	}

	internal class CoreContentPage : ContentPage
	{
		public CoreContentPage ()
		{
			On<iOS>().SetUseSafeArea(true);
			AutomationId = "ContentPageRoot";
			Content = new StackLayout { Children = { new CoreRootView (), new CorePageView (this, NavigationBehavior.PushModalAsync) } };
		}
	}

	internal class CoreMasterDetailPage : MasterDetailPage
	{
		public CoreMasterDetailPage ()
		{
			AutomationId = "MasterDetailPageRoot";

			var toCrashButton = new Button {Text = "Crash Me"};

			var masterPage = new ContentPage {Title = "Menu", Icon = "bank.png", Content = toCrashButton};
			var detailPage = new CoreRootPage (this, NavigationBehavior.PushModalAsync) { Title = "DetailPage" };

			bool toggle = false;
			toCrashButton.Clicked += (sender, args) => {
				if (toggle)
					Detail = new ContentPage { BackgroundColor = Color.Green, };
				else
					Detail = detailPage;

				toggle = !toggle;
			};

			Master = masterPage;
			Detail = detailPage;
		}
	}

	internal class CoreNavigationPage : NavigationPage
	{
		public CoreNavigationPage ()
		{
			AutomationId = "NavigationPageRoot";

			BarBackgroundColor = Color.Maroon;
			BarTextColor = Color.Yellow;

			Device.StartTimer(TimeSpan.FromSeconds(2), () => {
				BarBackgroundColor = Color.Default;
				BarTextColor = Color.Default;

				return false;
			});

			On<iOS>().SetPrefersLargeTitles(true);
		
			Navigation.PushAsync (new CoreRootPage (this));
		}
	}

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2456, "StackOverflow after reordering tabs in a TabbedPageView", PlatformAffected.All)]
	public class CoreTabbedPage : TestTabbedPage
	{
		protected override void Init ()
		{
		}
#if APP
		public CoreTabbedPage ()
		{
			AutomationId = "TabbedPageRoot";


			Device.StartTimer(TimeSpan.FromSeconds(6), () => {
				BarBackgroundColor = Color.Maroon;
				BarTextColor = Color.Yellow;

				Device.StartTimer(TimeSpan.FromSeconds(6), () => {
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
					Icon = "coffee.png",
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

	[Preserve (AllMembers = true)]
	internal class CoreViewContainer
	{
		public string Name { get; private set; }
		public Type PageType { get; private set; }

		public CoreViewContainer (string name, Type pageType)
		{
			Name = name;
			PageType = pageType;
		}
	}

	public class CoreRootView : ListView
	{
		public CoreRootView ()
		{
			var roots = new [] {
				new CoreViewContainer ("SwapRoot - CarouselPage", typeof(CoreCarouselPage)), 
				new CoreViewContainer ("SwapRoot - ContentPage", typeof(CoreContentPage)),
				new CoreViewContainer ("SwapRoot - MasterDetailPage", typeof(CoreMasterDetailPage)),
				new CoreViewContainer ("SwapRoot - NavigationPage", typeof(CoreNavigationPage)),
				new CoreViewContainer ("SwapRoot - TabbedPage", typeof(CoreTabbedPage)),
			};

			var template = new DataTemplate (typeof(TextCell));
			template.SetBinding (TextCell.TextProperty, "Name");

			ItemTemplate = template;
			ItemsSource = roots;

#if PRE_APPLICATION_CLASS
			ItemSelected += (sender, args) => MessagingCenter.Send (this, Messages.ChangeRoot, ((CoreViewContainer)args.SelectedItem).PageType);
#else			
			ItemSelected += (sender, args) => {
				var app = Application.Current as App;
				if (app != null) {
					var page = (Page)Activator.CreateInstance (((CoreViewContainer)args.SelectedItem).PageType);
					app.SetMainPage (page);
				}		
			};
#endif
			SetValue(AutomationProperties.NameProperty, "SwapRoot");
		}
	}

	

	internal class CorePageView : ListView
	{
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
			}

			public Func<Page> Realize { get; set; }
			public string Title { get; set; }

			public override string ToString()
			{	
				// a11y: let Narrator read a friendly string instead of the default ToString()
				return Title;
			}
		}

		List<GalleryPageFactory> _pages = new List<GalleryPageFactory> {
				new GalleryPageFactory(() => new Issues.PerformanceGallery(), "Performance"),
				new GalleryPageFactory(() => new VisualStateManagerGallery(), "VisualStateManager Gallery"),
				new GalleryPageFactory(() => new FlowDirectionGalleryLandingPage(), "FlowDirection"),
				new GalleryPageFactory(() => new AutomationPropertiesGallery(), "Accessibility"),
				new GalleryPageFactory(() => new PlatformSpecificsGallery(), "Platform Specifics"),
				new GalleryPageFactory(() => new NativeBindingGalleryPage(), "Native Binding Controls Gallery"),
				new GalleryPageFactory(() => new XamlNativeViews(), "Xaml Native Views Gallery"),
				new GalleryPageFactory(() => new AppLinkPageGallery(), "App Link Page Gallery"),
				new GalleryPageFactory(() => new NestedNativeControlGalleryPage(), "Nested Native Controls Gallery"),
				new GalleryPageFactory(() => new CellForceUpdateSizeGalleryPage(), "Cell Force Update Size Gallery"),
				new GalleryPageFactory(() => new AppearingGalleryPage(), "Appearing Gallery"),
				new GalleryPageFactory(() => new EntryCoreGalleryPage(), "Entry Gallery"),
				new GalleryPageFactory(() => new NavBarTitleTestPage(), "Titles And Navbar Windows"),
				new GalleryPageFactory(() => new PanGestureGalleryPage(), "Pan gesture Gallery"),
				new GalleryPageFactory(() => new PinchGestureTestPage(), "Pinch gesture Gallery"),
				new GalleryPageFactory(() => new AutomationIdGallery(), "AutomationID Gallery"),
				new GalleryPageFactory(() => new LayoutPerformanceGallery(), "Layout Perf Gallery"),
				new GalleryPageFactory(() => new ListViewSelectionColor(), "ListView SelectionColor Gallery"),
				new GalleryPageFactory(() => new AlertGallery(), "DisplayAlert Gallery"),
				new GalleryPageFactory(() => new ToolbarItems(), "ToolbarItems Gallery"),
				new GalleryPageFactory(() => new ActionSheetGallery(), "ActionSheet Gallery"),
				new GalleryPageFactory(() => new ActivityIndicatorCoreGalleryPage(), "ActivityIndicator Gallery"),
				new GalleryPageFactory(() => new BehaviorsAndTriggers(), "BehaviorsTriggers Gallery"),
				new GalleryPageFactory(() => new ContextActionsGallery(), "ContextActions List Gallery"),
				new GalleryPageFactory(() => new ContextActionsGallery (tableView: true), "ContextActions Table Gallery"),
				new GalleryPageFactory(() => new CoreBoxViewGalleryPage(), "BoxView Gallery"),
				new GalleryPageFactory(() => new ButtonCoreGalleryPage(), "Button Gallery"),
				new GalleryPageFactory(() => new DatePickerCoreGalleryPage(), "DatePicker Gallery"),
				new GalleryPageFactory(() => new EditorCoreGalleryPage(), "Editor Gallery"),
				new GalleryPageFactory(() => new FrameCoreGalleryPage(), "Frame Gallery"),
				new GalleryPageFactory(() => new ImageCoreGalleryPage(), "Image Gallery"),
				new GalleryPageFactory(() => new KeyboardCoreGallery(), "Keyboard Gallery"),
				new GalleryPageFactory(() => new LabelCoreGalleryPage(), "Label Gallery"),
				new GalleryPageFactory(() => new ListViewCoreGalleryPage(), "ListView Gallery"),
				new GalleryPageFactory(() => new OpenGLViewCoreGalleryPage(), "OpenGLView Gallery"),
				new GalleryPageFactory(() => new PickerCoreGalleryPage(), "Picker Gallery"),
				new GalleryPageFactory(() => new ProgressBarCoreGalleryPage(), "ProgressBar Gallery"),
				new GalleryPageFactory(() => new ScrollGallery(), "ScrollView Gallery"),
				new GalleryPageFactory(() => new ScrollGallery(ScrollOrientation.Horizontal), "ScrollView Gallery Horizontal"),
				new GalleryPageFactory(() => new ScrollGallery(ScrollOrientation.Both), "ScrollView Gallery 2D"),
				new GalleryPageFactory(() => new SearchBarCoreGalleryPage(), "SearchBar Gallery"),
				new GalleryPageFactory(() => new SliderCoreGalleryPage(), "Slider Gallery"),
				new GalleryPageFactory(() => new StepperCoreGalleryPage(), "Stepper Gallery"),
				new GalleryPageFactory(() => new SwitchCoreGalleryPage(), "Switch Gallery"),
				new GalleryPageFactory(() => new TableViewCoreGalleryPage(), "TableView Gallery"),
				new GalleryPageFactory(() => new TimePickerCoreGalleryPage(), "TimePicker Gallery"),
				new GalleryPageFactory(() => new WebViewCoreGalleryPage(), "WebView Gallery"),
				//pages
 				new GalleryPageFactory(() => new RootContentPage ("Content"), "RootPages Gallery"),
				new GalleryPageFactory(() => new MasterDetailPageTabletPage(), "MasterDetailPage Tablet Page"),
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
				new GalleryPageFactory(() => new MinimumSizeGallery(), "MinimumSize Gallery - Legacy"),
				new GalleryPageFactory(() => new MultiGallery(), "Multi Gallery - Legacy"),
				new GalleryPageFactory(() => new NavigationMenuGallery(), "NavigationMenu Gallery - Legacy"),
				new GalleryPageFactory(() => new NavigationPropertiesGallery(), "Navigation Properties"),
#if HAVE_OPENTK
				new GalleryPageFactory(() => new OpenGLGallery(), "OpenGLGallery - Legacy"),
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
			};

		public CorePageView (Page rootPage, NavigationBehavior navigationBehavior = NavigationBehavior.PushAsync)
		{
			_titleToPage = _pages.ToDictionary (o => o.Title);

			// avoid NRE for root pages without NavigationBar
			if (navigationBehavior == NavigationBehavior.PushAsync && rootPage.GetType () == typeof (CoreNavigationPage)) {
				_pages.Add (new GalleryPageFactory(() => new NavigationBarGallery((NavigationPage)rootPage), "NavigationBar Gallery - Legacy"));
			}

			var template = new DataTemplate (typeof(TextCell));
			template.SetBinding (TextCell.TextProperty, "Title");

			BindingContext = _pages;
			ItemTemplate = template;
			ItemsSource = _pages;

			ItemSelected += async (sender, args) => {
				if (SelectedItem == null)
					return;

				var item = args.SelectedItem;
				var page = item as GalleryPageFactory;
				if (page != null)
					await PushPage (page.Realize());

				SelectedItem = null;
			};

			SetValue(AutomationProperties.NameProperty, "Core Pages");
		}

		NavigationBehavior navigationBehavior;

		async Task PushPage (Page contentPage)
		{
			if (navigationBehavior == NavigationBehavior.PushModalAsync) {
				await Navigation.PushModalAsync (contentPage);
			} else {
				await Navigation.PushAsync (contentPage);
			}
		}

		readonly Dictionary<string, GalleryPageFactory> _titleToPage;
		public async Task PushPage (string pageTitle)
		{

			GalleryPageFactory pageFactory = null;
			if (!_titleToPage.TryGetValue (pageTitle, out pageFactory))
				return;

			var page = pageFactory.Realize();

			await PushPage (page);
		}
	}

	internal class CoreRootPage : ContentPage
	{
		public CoreRootPage (Page rootPage, NavigationBehavior navigationBehavior = NavigationBehavior.PushAsync)
		{
			IStringProvider stringProvider = DependencyService.Get<IStringProvider> ();

			Title = stringProvider.CoreGalleryTitle;

			var corePageView = new CorePageView (rootPage, navigationBehavior);

			var searchBar = new SearchBar () {
				AutomationId = "SearchBar"
			};

			var testCasesButton = new Button {
				Text = "Go to Test Cases",
				AutomationId = "GoToTestButton",
				Command = new Command (async () => {
					if (!string.IsNullOrEmpty (searchBar.Text))
						await corePageView.PushPage (searchBar.Text);
					else
						await Navigation.PushModalAsync (TestCases.GetTestCases ());
				})
			};

			var stackLayout = new StackLayout () { 
				Children = {
					testCasesButton,
					searchBar,
					new Button {
						Text = "Click to Force GC", 
						Command = new Command(() => {
							GC.Collect ();
							GC.WaitForPendingFinalizers ();
							GC.Collect ();
						})
					}

				}
			};

			Content = new AbsoluteLayout {
				Children = {
					{ new CoreRootView (), new Rectangle(0, 0.0, 1, 0.35), AbsoluteLayoutFlags.All },
					{ stackLayout, new Rectangle(0, 0.5, 1, 0.30), AbsoluteLayoutFlags.All },
					{ corePageView, new Rectangle(0, 1.0, 1.0, 0.35), AbsoluteLayoutFlags.All },
				}
			};
		}
	}

	public interface IStringProvider
	{
		string CoreGalleryTitle { get; }
	}

	public static class CoreGallery
	{
		public static Page GetMainPage ()
		{
			return new CoreNavigationPage ();
		}
	}
}
