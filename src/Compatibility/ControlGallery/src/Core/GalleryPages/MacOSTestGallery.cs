using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public class MacOSTestGallery : ContentPage
	{
		public MacOSTestGallery()
		{
			mainDemoStack.Children.Add(MakeNewStackLayout());
			var items = new List<MyItem1>();
			for (int i = 0; i < 5000; i++)
			{
				items.Add(new MyItem1 { Reference = "Hello this is a big text " + i.ToString(), ShowButton = i % 2 == 0, Image = "bank.png" });
			}

			var header = new Label { Text = "HELLO HEADER ", FontSize = 40, BackgroundColor = Colors.Pink };
			var lst4 = new ListView { Header = header, ItemTemplate = new DataTemplate(typeof(DemoViewCell)), BackgroundColor = Colors.Yellow, HeightRequest = 300, RowHeight = 50, ItemsSource = items, };

			var lst = new ListView { ItemTemplate = new DataTemplate(typeof(DemoEntryCell)), BackgroundColor = Colors.Yellow, HeightRequest = 300, RowHeight = 50, ItemsSource = items, };
			var lst1 = new ListView { ItemTemplate = new DataTemplate(typeof(DemoTextCell)), BackgroundColor = Colors.Yellow, HeightRequest = 300, RowHeight = 50, ItemsSource = items, };
			var lst2 = new ListView { ItemTemplate = new DataTemplate(typeof(DemoSwitchCell)), BackgroundColor = Colors.Yellow, HeightRequest = 300, RowHeight = 50, ItemsSource = items, };
			var lst3 = new ListView { ItemTemplate = new DataTemplate(typeof(DemoImageCell)), BackgroundColor = Colors.Yellow, HeightRequest = 300, RowHeight = 50, ItemsSource = items, };

			var bigbUtton = new Button { WidthRequest = 200, HeightRequest = 300, ImageSource = "bank.png" };

			var picker = new DatePicker();

			var timePicker = new TimePicker { Format = "T", Time = TimeSpan.FromHours(2) };

			var editor = new Editor { Text = "Edit this text on editor", HeightRequest = 100, TextColor = Colors.Yellow, BackgroundColor = Colors.Gray };

			var entry = new Entry { Placeholder = "Edit this text on entry", PlaceholderColor = Colors.Pink, TextColor = Colors.Yellow, BackgroundColor = Colors.Green };

			var frame = new Frame { HasShadow = true, BackgroundColor = Colors.Maroon, BorderColor = Colors.Lime, MinimumHeightRequest = 100 };


			var image = new Image { HeightRequest = 100, Source = "crimson.jpg" };

			var picker1 = new Picker { Title = "Select a team player", TextColor = Colors.Pink, BackgroundColor = Colors.Silver };
			picker1.Items.Add("Rui");
			picker1.Items.Add("Jason");
			picker1.Items.Add("Ez");
			picker1.Items.Add("Stephane");
			picker1.Items.Add("Samantha");
			picker1.Items.Add("Paul");

			picker1.SelectedIndex = 1;

			var progress = new ProgressBar { BackgroundColor = Colors.Purple, Progress = 0.5, HeightRequest = 50 };

			picker1.SelectedIndexChanged += (sender, e) =>
			{
				entry.Text = $"Selected {picker1.Items[picker1.SelectedIndex]}";

				progress.Progress += 0.1;
			};

			var searchBar = new SearchBar { BackgroundColor = Colors.Olive, TextColor = Colors.Maroon, CancelButtonColor = Colors.Pink };
			searchBar.Placeholder = "Please search";
			searchBar.PlaceholderColor = Colors.Orange;
			searchBar.SearchButtonPressed += (sender, e) =>
			{
				searchBar.Text = "Search was pressed";
			};

			var slider = new Slider { BackgroundColor = Colors.Lime, Value = 0.5 };

			slider.ValueChanged += (sender, e) =>
			{
				editor.Text = $"Slider value changed {slider.Value}";
			};

			var stepper = new Stepper { BackgroundColor = Colors.Yellow, Maximum = 100, Minimum = 0, Value = 10, Increment = 0.5 };

			stepper.ValueChanged += (sender, e) =>
			{
				editor.Text = $"Stepper value changed {stepper.Value}";
			};

			var labal = new Label { Text = "This is a Switch" };
			var switchR = new Switch { BackgroundColor = Colors.Fuchsia, IsToggled = true };
			switchR.Toggled += (sender, e) =>
			{
				entry.Text = $"switchR is toggled {switchR.IsToggled}";
			};
			var layoutSwitch = new StackLayout { Orientation = StackOrientation.Horizontal, BackgroundColor = Colors.Green };
			layoutSwitch.Children.Add(labal);
			layoutSwitch.Children.Add(switchR);

			var webView = new WebView { HeightRequest = 200, Source = "http://google.pt" };

			var mainStck = new StackLayout
			{
				Spacing = 10,
				BackgroundColor = Colors.Blue,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Children =
							{
								lst4,
								lst,
								lst1,
								lst2,
								lst3,
								webView,
								layoutSwitch,
								stepper,
								slider,
								searchBar,
								progress,
								picker1,
								image,
								frame,
								entry,
								editor,
								picker,
								timePicker,
								bigbUtton,
								new Button { Text = "Click Me", BackgroundColor = Colors.Gray },
								new Button { ImageSource = "bank.png", BackgroundColor = Colors.Gray },
								CreateButton(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 10)),
								CreateButton(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 10)),
								CreateButton(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Bottom, 10)),
								CreateButton(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 10)),
								mainDemoStack
							}
			};
			var lbl = new Label { Text = "Second label", TextColor = Colors.White, VerticalTextAlignment = TextAlignment.Start, HorizontalTextAlignment = TextAlignment.Center };
			mainStck.Children.Add(new Label { Text = "HELLO XAMARIN FORMS MAC", TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center });
			mainStck.Children.Add(lbl);
			mainStck.Children.Add(new BoxView { Color = Colors.Pink, HeightRequest = 200 });

			var scroller = new ScrollView { BackgroundColor = Colors.Yellow, HorizontalOptions = LayoutOptions.Center };

			scroller.Scrolled += (sender, e) =>
			{
				lbl.Text = $"Current Position {scroller.ScrollY}";
			};

			scroller.Content = mainStck;

			var actv = new ActivityIndicator { BackgroundColor = Colors.White, Color = Colors.Fuchsia, IsRunning = true };
			mainStck.Children.Add(actv);

			bigbUtton.Clicked += async (sender, e) =>
			{
				await scroller.ScrollToAsync(actv, ScrollToPosition.Center, true);
				actv.Color = null;
			};

			Content = scroller;
		}

		public static ContentPage MacDemoContentPage()
		{
			return new MacOSTestGallery();
		}

		public static NavigationPage MacDemoNavigationPage()
		{
			var np = new NavigationPage(GetNewPage()) { BarTextColor = Colors.Red, BarBackgroundColor = Colors.Yellow };

			np.Pushed += (sender, e) => System.Diagnostics.Debug.WriteLine("Pushed + " + np.CurrentPage.Title);

			np.Popped += (sender, e) => System.Diagnostics.Debug.WriteLine("Popped");

			np.PoppedToRoot += (sender, e) => System.Diagnostics.Debug.WriteLine("Popped to root");

			return np;
		}

		public static TabbedPage MacDemoTabbedPage()
		{

			var btnGo = new Button { Text = "Change Title" };
			var btnGo1 = new Button { Text = "Change Icon" };

			var lyout = new StackLayout();
			lyout.Children.Add(btnGo);
			//lyout.Children.Add(btnGo1);

			var tp = new TabbedPage { BarTextColor = Colors.Red, BarBackgroundColor = Colors.Yellow };

			var master = new ContentPage { IconImageSource = "bank.png", BackgroundColor = Colors.Red, Title = "Flyout", Content = lyout };

			var detail = new ContentPage { IconImageSource = "bank.png", BackgroundColor = Colors.Blue, Title = "Detail", Content = new Label { Text = "This is Detail Page" } };

			tp.Children.Add(master);
			tp.Children.Add(detail);

			tp.CurrentPage = detail;

			tp.CurrentPageChanged += (sender, e) =>
			{
				System.Diagnostics.Debug.WriteLine(tp.CurrentPage.Title);
			};

			btnGo.Clicked += (sender, e) =>
			{
				tp.CurrentPage.Title = "Tile changed";
				tp.CurrentPage.IconImageSource = null;
			};

			btnGo1.Clicked += (sender, e) =>
			{

			};
			return tp;
		}

		public static FlyoutPage MacDemoFlyoutPage()
		{
			var mdp = new FlyoutPage();

			var master = new ContentPage { BackgroundColor = Colors.Red, Title = "Flyout", Content = new Label { Text = "This is Flyout Page" } };

			var detail = new ContentPage { BackgroundColor = Colors.Blue, Title = "Detail", Content = new Label { Text = "This is Detail Page" } };

			mdp.Flyout = master;
			mdp.Detail = detail;

			return mdp;
		}

		internal static CarouselPage MacDemoCarouselPage()
		{

			var carouselPage = new CarouselPage { BackgroundColor = Colors.Yellow };

			var btnGo = new Button { Text = "Goto To Page 1 " };
			var btnGo1 = new Button { Text = "Goto To Page 3 " };
			var stck = new StackLayout();
			stck.Children.Add(btnGo);
			stck.Children.Add(btnGo1);
			var page = new ContentPage { Title = "Page1", BackgroundColor = Colors.Red, Content = new Label { Text = "Page 1 label", TextColor = Colors.White, VerticalTextAlignment = TextAlignment.Start, HorizontalTextAlignment = TextAlignment.Center } };
			var page2 = new ContentPage { Title = "Page2", BackgroundColor = Colors.Blue, Content = stck };
			var page3 = new ContentPage { Title = "Page3", BackgroundColor = Colors.Green, Content = new Label { Text = "Page 3 label" } };

			carouselPage.Children.Add(page);
			carouselPage.Children.Add(page2);
			carouselPage.Children.Add(page3);

			carouselPage.CurrentPage = page2;

			btnGo.Clicked += (sender, e) =>
			{
				carouselPage.CurrentPage = page;
			};

			btnGo1.Clicked += (sender, e) =>
			{
				carouselPage.CurrentPage = page3;
			};

			carouselPage.CurrentPageChanged += (sender, e) =>
			{
				System.Diagnostics.Debug.WriteLine(carouselPage.CurrentPage.Title);
			};
			return carouselPage;
		}

		static int _pageID;

		static StackLayout mainDemoStack = new StackLayout { BackgroundColor = Colors.Blue };

		static ContentPage GetNewPage()
		{
			var label = new Label { Text = $"Page {_pageID}" };
			var btnGo = new Button { Text = "Push Page" };
			var btnGo1 = new Button { Text = "Pop Page" };
			var lyout = new StackLayout();
			lyout.Children.Add(label);
			lyout.Children.Add(btnGo);
			lyout.Children.Add(btnGo1);

			btnGo.Clicked += async (sender, e) =>
			{
				_pageID++;
				await (lyout.Parent as Page).Navigation?.PushAsync(GetNewPage());

			};

			btnGo1.Clicked += async (sender, e) =>
			{
				_pageID--;
				await (lyout.Parent as Page).Navigation?.PopAsync();

			};

			return new ContentPage { IconImageSource = "bank.png", BackgroundColor = _pageID % 2 == 0 ? Colors.Blue : Colors.Green, Title = label.Text, Content = lyout };
		}

		static StackLayout MakeNewStackLayout()
		{
			var count = 0;
			var stacklayout = new StackLayout { BackgroundColor = Colors.Red };

			stacklayout.Children.Add(new Label { Text = $"HEllO {count}" });
			stacklayout.Children.Add(new Button
			{
				Text = "Change layout",
				Command = new Command(() =>
				{
					count += 2;
					stacklayout.Children.RemoveAt(2);

					var ll = new StackLayout();
					ll.Children.Add(new Label { Text = $"HEllO {count}" });
					ll.Children.Add(new Label { Text = $"HEllO {count + 1}" });
					stacklayout.Children.Add(ll);
				})
			});
			stacklayout.Children.Add(new Label { Text = $"HEllO {count + 1}" });
			count += 2;
			return stacklayout;
		}



		static Button CreateButton(Button.ButtonContentLayout layout)
		{
			return new Button
			{
				Text = "Click Me On Mac",
				ImageSource = "bank.png",
				FontFamily = "Helvetica",
				FontSize = 14,
				ContentLayout = layout,
				BackgroundColor = Colors.Black,
				TextColor = Colors.White
			};
		}

		class DemoSwitchCell : SwitchCell
		{
			public DemoSwitchCell()
			{
				SetBinding(TextProperty, new Binding("Reference"));
				SetBinding(OnProperty, new Binding("ShowButton"));
			}
		}

		class DemoImageCell : ImageCell
		{
			public DemoImageCell()
			{
				SetBinding(TextProperty, new Binding("Reference"));
				SetBinding(DetailProperty, new Binding("ShowButton"));
				SetBinding(ImageSourceProperty, new Binding("Image"));
			}
		}

		class DemoTextCell : TextCell
		{
			public DemoTextCell()
			{
				SetBinding(TextProperty, new Binding("Reference"));
				SetBinding(DetailProperty, new Binding("ShowButton"));
			}
		}

		class DemoEntryCell : EntryCell
		{
			public DemoEntryCell()
			{
				SetBinding(LabelProperty, new Binding("Reference"));
				SetBinding(TextProperty, new Binding("ShowButton"));
				LabelColor = Colors.Red;
				Placeholder = "This is a entry cell";
			}
		}

		class DemoViewCell : ViewCell
		{
			public DemoViewCell()
			{
				var box = new Image { BackgroundColor = Colors.Pink, WidthRequest = 100, HeightRequest = 40, Source = "bank.png" };
				var label = new Label { TextColor = Colors.White };
				var labelDetail = new Label { TextColor = Colors.White };

				label.SetBinding(Label.TextProperty, new Binding("Reference"));
				labelDetail.SetBinding(Label.TextProperty, new Binding("ShowButton"));

				var grid = new Grid { BackgroundColor = Colors.Black };

				grid.Children.Add(box, 0, 1, 0, 1);
				grid.Children.Add(label, 1, 0);
				grid.Children.Add(labelDetail, 1, 1);

				View = grid;
			}
		}

		public class MyItem1
		{
			public string Reference { get; set; }
			public string Image { get; set; }
			public bool ShowButton { get; set; }
		}
	}
}
