using Controls.Sample.UITests;
using Maui.Controls.Sample.CollectionViewGalleries;
using Maui.Controls.Sample.Issues;

namespace Maui.Controls.Sample
{
	internal class CorePageView : CollectionView
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
			// Concepts & Abstracts
			new GalleryPageFactory(() => new BorderGallery(), "Border Gallery"),
			new GalleryPageFactory(() => new DragAndDropGallery(), "Drag and Drop Gallery"),
			new GalleryPageFactory(() => new FontsGalleryPage(), "Fonts Gallery"),
			new GalleryPageFactory(() => new GestureRecognizerGallery(), "Gesture Recognizer Gallery"),
			new GalleryPageFactory(() => new InputTransparencyGalleryPage(), "Input Transparency Gallery"),
			new GalleryPageFactory(() => new ImageLoadingGalleryPage(), "Image Loading Gallery"),
			new GalleryPageFactory(() => new AlertsGalleryPage(), "Alerts Gallery"),
			// Elements
			new GalleryPageFactory(() => new ActivityIndicatorCoreGalleryPage(), "ActivityIndicator Gallery"),
			new GalleryPageFactory(() => new BorderControlPage(), "Border Feature Matrix"),
			new GalleryPageFactory(() => new BoxViewCoreGalleryPage(), "Box Gallery"),
			new GalleryPageFactory(() => new ButtonControlPage(), "Button Feature Matrix"),
			new GalleryPageFactory(() => new ButtonCoreGalleryPage(), "Button Gallery"),
			new GalleryPageFactory(() => new CarouselViewCoreGalleryPage(), "CarouselView Gallery"),
			new GalleryPageFactory(() => new CheckBoxCoreGalleryPage(), "CheckBox Gallery"),
			new GalleryPageFactory(() => new CollectionViewCoreGalleryPage(), "CollectionView Gallery"),
			new GalleryPageFactory(() => new DatePickerControlPage(), "Date Picker Feature Matrix"),
			new GalleryPageFactory(() => new DatePickerCoreGalleryPage(), "Date Picker Gallery"),
			new GalleryPageFactory(() => new EditorCoreGalleryPage(), "Editor Gallery"),
			new GalleryPageFactory(() => new EntryCoreGalleryPage(), "Entry Gallery"),
			new GalleryPageFactory(() => new FrameCoreGalleryPage(), "Frame Gallery"),
			new GalleryPageFactory(() => new ImageButtonCoreGalleryPage(), "Image Button Gallery"),
			new GalleryPageFactory(() => new ImageCoreGalleryPage(), "Image Gallery"),
			new GalleryPageFactory(() => new KeyboardScrollingGridGallery(), "Keyboard Scrolling Gallery - Grid with Star Row"),
			new GalleryPageFactory(() => new KeyboardScrollingNonScrollingPageLargeTitlesGallery(), "Keyboard Scrolling Gallery - NonScrolling Page / Large Titles"),
			new GalleryPageFactory(() => new KeyboardScrollingNonScrollingPageSmallTitlesGallery(), "Keyboard Scrolling Gallery - NonScrolling Page / Small Titles"),
		  new GalleryPageFactory(() => new KeyboardScrollingScrollingPageLargeTitlesGallery(), "Keyboard Scrolling Gallery - Scrolling Page / Large Titles"),
			new GalleryPageFactory(() => new KeyboardScrollingScrollingPageSmallTitlesGallery(), "Keyboard Scrolling Gallery - Scrolling Page / Small Titles"),
			new GalleryPageFactory(() => new LabelCoreGalleryPage(), "Label Gallery"),
			new GalleryPageFactory(() => new ListViewCoreGalleryPage(), "ListView Gallery"),
			new GalleryPageFactory(() => new PickerControlPage(), "Picker Feature Matrix"),
			new GalleryPageFactory(() => new PickerCoreGalleryPage(), "Picker Gallery"),
			new GalleryPageFactory(() => new ProgressBarControlPage(), "ProgressBar Feature Matrix"),
			new GalleryPageFactory(() => new ProgressBarCoreGalleryPage(), "Progress Bar Gallery"),
			new GalleryPageFactory(() => new RadioButtonControlPage(), "RadioButton Feature Matrix"),
			new GalleryPageFactory(() => new RadioButtonCoreGalleryPage(), "RadioButton Gallery"),
			new GalleryPageFactory(() => new ScrollViewCoreGalleryPage(), "ScrollView Gallery"),
			new GalleryPageFactory(() => new ShadowFeaturePage(), "Shadow Feature Matrix"),
			new GalleryPageFactory(() => new SearchBarControlPage(), "Search Bar Feature Matrix"),
			new GalleryPageFactory(() => new SearchBarCoreGalleryPage(), "Search Bar Gallery"),
			new GalleryPageFactory(() => new SliderCoreGalleryPage(), "Slider Gallery"),
			new GalleryPageFactory(() => new StepperControlPage(), "Stepper Feature Matrix"),
			new GalleryPageFactory(() => new StepperCoreGalleryPage(), "Stepper Gallery"),
			new GalleryPageFactory(() => new SwitchControlPage(), "Switch Feature Matrix"),
			new GalleryPageFactory(() => new SwitchCoreGalleryPage(), "Switch Gallery"),
			new GalleryPageFactory(() => new SwipeViewCoreGalleryPage(), "SwipeView Gallery"),
			new GalleryPageFactory(() => new TimePickerControlPage(), "Time Picker Feature Matrix"),
			new GalleryPageFactory(() => new TimePickerCoreGalleryPage(), "Time Picker Gallery"),
			new GalleryPageFactory(() => new WebViewCoreGalleryPage(), "WebView Gallery"),
			new GalleryPageFactory(() => new RefreshViewControlPage(), "RefreshView Feature Matrix"),
			new GalleryPageFactory(() => new SliderControlPage(), "Slider Feature Matrix"),
			new GalleryPageFactory(() => new CheckBoxControlPage(), "CheckBox Feature Matrix"),
			new GalleryPageFactory(() => new CollectionViewFeaturePage(), "CollectionView Feature Matrix"),
			new GalleryPageFactory(() => new LabelControlPage(), "Label Feature Matrix"),
			new GalleryPageFactory(() => new CarouselViewFeaturePage(), "CarouselView Feature Matrix"),
			new GalleryPageFactory(() => new EntryControlPage(), "Entry Feature Matrix"),
			new GalleryPageFactory(() => new ImageControlPage(), "Image Feature Matrix"),
			new GalleryPageFactory(() => new ImageButtonControlPage(), "ImageButton Feature Matrix"),
			new GalleryPageFactory(() => new BoxViewControlPage(), "BoxView Feature Matrix"),
			new GalleryPageFactory(() => new HybridWebViewControlPage(), "HybridWebView Feature Matrix"),
			new GalleryPageFactory(() => new ScrollViewControlPage(), "ScrollView Feature Matrix"),
			new GalleryPageFactory(() => new GraphicsViewControlPage(), "GraphicsView Feature Matrix"),
			new GalleryPageFactory(() => new EditorControlPage(), "Editor Feature Matrix"),
			new GalleryPageFactory(() => new ShapesControlPage(), "Shapes Feature Matrix"),
			new GalleryPageFactory(() => new ContentPageControlPage(), "ContentPage Feature Matrix"),
			new GalleryPageFactory(() => new FlyoutControlPage(), "Flyout Feature Matrix"),
			new GalleryPageFactory(() => new SwipeViewControlPage(), "SwipeView Feature Matrix"),
			new GalleryPageFactory(() => new WebViewControlPage(), "WebView Feature Matrix"),
			new GalleryPageFactory(() => new TwoPaneViewControlPage(), "TwoPaneView Feature Matrix"),
			new GalleryPageFactory(() => new TitleBarControlPage(), "TitleBar Feature Matrix"),
			new GalleryPageFactory(() => new IndicatorViewControlPage(), "IndicatorView Feature Matrix"),
		};


		public CorePageView(Page rootPage)
		{
			_titleToPage = _pages.ToDictionary(o => o.Title.ToLowerInvariant());

			_pages.Sort((x, y) => string.Compare(x.Title, y.Title, StringComparison.OrdinalIgnoreCase));

			var template = new DataTemplate(() =>
			{
				var cell = new Grid();

				var label = new Label
				{
					FontSize = 14,
					VerticalOptions = LayoutOptions.Center,
					Margin = new Thickness(6)
				};

				label.SetBinding(Label.TextProperty, "Title");
				label.SetBinding(Label.AutomationIdProperty, "TitleAutomationId");

				cell.Add(label);

				return cell;
			});

			ItemTemplate = template;
			ItemsSource = _pages;
			SelectionMode = SelectionMode.Single;

			SelectionChanged += (sender, args) =>
			{
				if (SelectedItem == null)
				{
					return;
				}

				var selection = args.CurrentSelection;

				if (selection.Count == 0)
					return;

				var item = args.CurrentSelection[0];
				if (item is GalleryPageFactory page)
				{
					var realize = page.Realize();

					Dispatcher.Dispatch(() => Application.Current.MainPage = realize);
				}

				SelectedItem = null;
			};
		}

		async Task PushPage(Page contentPage)
		{
			await Navigation.PushAsync(contentPage);
		}

		readonly Dictionary<string, GalleryPageFactory> _titleToPage;
		public Task<bool> NavigateToGalleryPage(string pageTitle)
		{
			if (_titleToPage.TryGetValue(pageTitle.ToLowerInvariant(), out GalleryPageFactory pageFactory))
			{
				var page = pageFactory.Realize();
				this.Window.Page = page;
				return Task.FromResult(true);
			}

			return Task.FromResult(false);
		}

		public async Task<bool> NavigateToTest(string pageTitle)
		{
			var testCaseScreen = new TestCases.TestCaseScreen();
			if (testCaseScreen.TryToNavigateTo(pageTitle))
			{
				return true;
			}
			else if (await NavigateToGalleryPage(pageTitle))
			{
				return true;
			}

			return false;
		}


		public void FilterPages(string filter)
		{
			ItemsSource = string.IsNullOrWhiteSpace(filter)
				? _pages
				: _pages.Where(p => p.Title.Contains(filter, StringComparison.OrdinalIgnoreCase));
		}
	}
}