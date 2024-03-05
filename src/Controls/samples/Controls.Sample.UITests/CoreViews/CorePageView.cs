using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Controls.Sample.UITests;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample
{
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
			// Concepts & Abstracts
			new GalleryPageFactory(() => new BorderGallery(), "Border Gallery"),
			new GalleryPageFactory(() => new DragAndDropGallery(), "Drag and Drop Gallery"),
			new GalleryPageFactory(() => new GestureRecognizerGallery(), "Gesture Recognizer Gallery"),
			new GalleryPageFactory(() => new InputTransparencyGalleryPage(), "Input Transparency Gallery"),
			new GalleryPageFactory(() => new ImageLoadingGalleryPage(), "Image Loading Gallery"),
			// Elements
			new GalleryPageFactory(() => new ActivityIndicatorCoreGalleryPage(), "ActivityIndicator Gallery"),
			new GalleryPageFactory(() => new BoxViewCoreGalleryPage(), "Box Gallery"),
			new GalleryPageFactory(() => new ButtonCoreGalleryPage(), "Button Gallery"),
			new GalleryPageFactory(() => new CarouselViewCoreGalleryPage(), "CarouselView Gallery"),
			new GalleryPageFactory(() => new CheckBoxCoreGalleryPage(), "CheckBox Gallery"),
			new GalleryPageFactory(() => new CollectionViewCoreGalleryPage(), "CollectionView Gallery"),
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
			new GalleryPageFactory(() => new PickerCoreGalleryPage(), "Picker Gallery"),
			new GalleryPageFactory(() => new ProgressBarCoreGalleryPage(), "Progress Bar Gallery"),
			new GalleryPageFactory(() => new RadioButtonCoreGalleryPage(), "RadioButton Gallery"),
			new GalleryPageFactory(() => new ScrollViewCoreGalleryPage(), "ScrollView Gallery"),
			new GalleryPageFactory(() => new SearchBarCoreGalleryPage(), "Search Bar Gallery"),
			new GalleryPageFactory(() => new SliderCoreGalleryPage(), "Slider Gallery"),
			new GalleryPageFactory(() => new StepperCoreGalleryPage(), "Stepper Gallery"),
			new GalleryPageFactory(() => new SwitchCoreGalleryPage(), "Switch Gallery"),
			new GalleryPageFactory(() => new SwipeViewCoreGalleryPage(), "SwipeView Gallery"),
			new GalleryPageFactory(() => new TimePickerCoreGalleryPage(), "Time Picker Gallery"),
			new GalleryPageFactory(() => new WebViewCoreGalleryPage(), "WebView Gallery"),
		};

		public CorePageView(Page rootPage)
		{
			_titleToPage = _pages.ToDictionary(o => o.Title.ToLowerInvariant());

			_pages.Sort((x, y) => string.Compare(x.Title, y.Title, StringComparison.OrdinalIgnoreCase));

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
							{
								page.Visual = visual;
							}
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
				{
					return;
				}

				var item = args.SelectedItem;
				if (item is GalleryPageFactory page)
				{
					var realize = page.Realize();
					if (realize is Shell)
					{
						Application.Current.MainPage = realize;
					}
					else
					{
						await PushPage(realize);
					}
				}

				SelectedItem = null;
			};
		}

		async Task PushPage(Page contentPage)
		{
			await Navigation.PushAsync(contentPage);
		}

		readonly Dictionary<string, GalleryPageFactory> _titleToPage;
		public async Task<bool> PushPage(string pageTitle)
		{
			if (_titleToPage.TryGetValue(pageTitle.ToLowerInvariant(), out GalleryPageFactory pageFactory))
			{
				var page = pageFactory.Realize();

				await PushPage(page);
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