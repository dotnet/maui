using System;
using System.Linq;

namespace Xamarin.Forms.Controls
{

	public class FlowDirectionGalleryLandingPage : ContentPage
	{
		FlowDirection DeviceDirection => Device.FlowDirection;

		public FlowDirectionGalleryLandingPage()
		{

			var np = new Button { Text = "Try NavigationPage", Command = new Command(() => { PushNavigationPage(DeviceDirection); }) };
			var mdp = new Button { Text = "Try MasterDetailPage", Command = new Command(() => { PushMasterDetailPage(DeviceDirection); }) };
			var crp = new Button { Text = "Try CarouselPage", Command = new Command(() => { PushCarouselPage(DeviceDirection); }) };
			var tp = new Button { Text = "Try TabbedPage", Command = new Command(() => { PushTabbedPage(DeviceDirection); }) };
			var cp = new Button { Text = "Try ContentPage", Command = new Command(() => { PushContentPage(DeviceDirection); }) };

			Content = new StackLayout
			{
				Children = {
					new Label { Text = "Click a button below to swap the MainPage of the app." },
					np,
					mdp,
					crp,
					tp,
					cp
				}
			};
		}

		public static void PushNavigationPage(FlowDirection direction)
		{
			((App)Application.Current).SetMainPage(new FlowDirectionGalleryNP(direction));
		}

		public static void PushMasterDetailPage(FlowDirection direction)
		{
			((App)Application.Current).SetMainPage(new FlowDirectionGalleryMDP(direction));
		}

		public static void PushCarouselPage(FlowDirection direction)
		{
			((App)Application.Current).SetMainPage(new FlowDirectionGalleryCarP(direction));
		}

		public static void PushTabbedPage(FlowDirection direction)
		{
			((App)Application.Current).SetMainPage(new FlowDirectionGalleryTP(direction));
		}

		public static void PushContentPage(FlowDirection direction)
		{
			((App)Application.Current).SetMainPage(new FlowDirectionGalleryCP(direction)
			{
				FlowDirection = direction
			});
		}
	}

	public class FlowDirectionGalleryNP : NavigationPage
	{
		public FlowDirectionGalleryNP(FlowDirection direction)
		{
			FlowDirection = direction;
			Navigation.PushAsync(new FlowDirectionGalleryCP(direction));
			Navigation.PushAsync(new FlowDirectionGalleryCP(direction));
		}
	}

	public class FlowDirectionGalleryMDP : MasterDetailPage
	{
		public FlowDirectionGalleryMDP(FlowDirection direction)
		{
			FlowDirection = direction;
			Master = new FlowDirectionGalleryCP(direction) { Title = "Master", BackgroundColor = Color.Red };
			Detail = new NavigationPage(new FlowDirectionGalleryCP(direction) { Title = "Detail" });
			IsPresented = true;
		}
	}

	public class FlowDirectionGalleryCarP : CarouselPage
	{
		public FlowDirectionGalleryCarP(FlowDirection direction)
		{
			FlowDirection = direction;
			Children.Add(new FlowDirectionGalleryCP(direction) { Title = "1" });
			Children.Add(new FlowDirectionGalleryCP(direction) { Title = "2" });
			Children.Add(new FlowDirectionGalleryCP(direction) { Title = "3" });
		}
	}

	public class FlowDirectionGalleryTP : TabbedPage
	{
		public FlowDirectionGalleryTP(FlowDirection direction)
		{
			FlowDirection = direction;
			Children.Add(new FlowDirectionGalleryCP(direction) { Title = "1", BackgroundColor = Color.Red });
			Children.Add(new FlowDirectionGalleryCP(direction) { Title = "2", BackgroundColor = Color.Orange });
			Children.Add(new FlowDirectionGalleryCP(direction) { Title = "3", BackgroundColor = Color.Yellow });
		}
	}

	public class FlowDirectionGalleryCP : ContentPage
	{
		FlowDirection DeviceDirection => Device.FlowDirection;

		Page ParentPage => (Parent as Page) ?? this;

		public FlowDirectionGalleryCP(FlowDirection direction)
		{
			var item = new ToolbarItem
			{
				Icon = "coffee.png",
				Text = "Item 1",
			};

			var item2 = new ToolbarItem
			{
				Icon = "bank.png",
				Text = "Item 2",
			};

			ToolbarItems.Add(item);
			ToolbarItems.Add(item2);

			Title = "Flow Direction Gallery";
			NavigationPage.SetHasBackButton(this, true);
			NavigationPage.SetBackButtonTitle(this, "Back");
			SetContent(direction);
		}

		void SetContent(FlowDirection direction)
		{
			var hOptions = LayoutOptions.Start;

			var imageCell = new DataTemplate(typeof(ImageCell));
			imageCell.SetBinding(ImageCell.ImageSourceProperty, ".");
			imageCell.SetBinding(ImageCell.TextProperty, ".");

			var textCell = new DataTemplate(typeof(TextCell));
			textCell.SetBinding(TextCell.DetailProperty, ".");

			var entryCell = new DataTemplate(typeof(EntryCell));
			entryCell.SetBinding(EntryCell.TextProperty, ".");

			var switchCell = new DataTemplate(typeof(SwitchCell));
			switchCell.SetBinding(SwitchCell.OnProperty, ".");
			switchCell.SetValue(SwitchCell.TextProperty, "Switch Cell!");

			var vc = new ViewCell
			{
				View = new StackLayout
				{
					Children = { new Label { HorizontalOptions = hOptions, Text = "View Cell! I have context actions." } }
				}
			};

			var a1 = new MenuItem { Text = "First" };
			vc.ContextActions.Add(a1);
			var a2 = new MenuItem { Text = "Second" };
			vc.ContextActions.Add(a2);

			var viewCell = new DataTemplate(() => vc);

			var relayout = new Switch
			{
				IsToggled = true,
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.Center
			};

			var flipButton = new Button
			{
				Text = direction == FlowDirection.RightToLeft ? "Switch to Left To Right" : "Switch to Right To Left",
				HorizontalOptions = LayoutOptions.StartAndExpand,
				VerticalOptions = LayoutOptions.Center
			};

			flipButton.Clicked += (s, e) =>
			{
				FlowDirection newDirection;
				if (direction == FlowDirection.LeftToRight || direction == FlowDirection.MatchParent)
					newDirection = FlowDirection.RightToLeft;
				else
					newDirection = FlowDirection.LeftToRight;

				if (relayout.IsToggled)
				{
					ParentPage.FlowDirection = newDirection;

					direction = newDirection;

					flipButton.Text = direction == FlowDirection.RightToLeft ? "Switch to Left To Right" : "Switch to Right To Left";

					return;
				}

				if (ParentPage == this)
				{
					FlowDirectionGalleryLandingPage.PushContentPage(newDirection);
					return;
				}
				string parentType = ParentPage.GetType().ToString();
				switch (parentType)
				{
					case "Xamarin.Forms.Controls.FlowDirectionGalleryMDP":
						FlowDirectionGalleryLandingPage.PushMasterDetailPage(newDirection);
						break;
					case "Xamarin.Forms.Controls.FlowDirectionGalleryCarP":
						FlowDirectionGalleryLandingPage.PushCarouselPage(newDirection);
						break;
					case "Xamarin.Forms.Controls.FlowDirectionGalleryNP":
						FlowDirectionGalleryLandingPage.PushNavigationPage(newDirection);
						break;
					case "Xamarin.Forms.Controls.FlowDirectionGalleryTP":
						FlowDirectionGalleryLandingPage.PushTabbedPage(newDirection);
						break;
				}
			};

			var horStack = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Children = { flipButton, new Label { Text = "Relayout", HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center }, relayout }
			};

			var grid = new Grid
			{
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
				},
				RowDefinitions = {
					new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
				}
			};

			int col = 0;
			int row = 0;

			var ai = AddView<ActivityIndicator>(grid, ref col, ref row);
			ai.IsRunning = true;

			var box = AddView<BoxView>(grid, ref col, ref row);
			box.WidthRequest = box.HeightRequest = 20;
			box.BackgroundColor = Color.Purple;

			var btn = AddView<Button>(grid, ref col, ref row);
			btn.Text = "Some text";

			var date = AddView<DatePicker>(grid, ref col, ref row, 2);

			var edit = AddView<Editor>(grid, ref col, ref row);
			edit.WidthRequest = 100;
			edit.HeightRequest = 100;
			edit.Text = "Some longer text for wrapping";

			var entry = AddView<Entry>(grid, ref col, ref row);
			entry.WidthRequest = 100;
			entry.Text = "Some text";

			var image = AddView<Image>(grid, ref col, ref row);
			image.Source = "oasis.jpg";

			var lbl1 = AddView<Label>(grid, ref col, ref row);
			lbl1.WidthRequest = 100;
			lbl1.HorizontalTextAlignment = TextAlignment.Start;
			lbl1.Text = "Start text";

			var lblLong = AddView<Label>(grid, ref col, ref row);
			lblLong.WidthRequest = 100;
			lblLong.HorizontalTextAlignment = TextAlignment.Start;
			lblLong.Text = "Start text that should wrap and wrap and wrap";

			var lbl2 = AddView<Label>(grid, ref col, ref row);
			lbl2.WidthRequest = 100;
			lbl2.HorizontalTextAlignment = TextAlignment.End;
			lbl2.Text = "End text";

			var lbl3 = AddView<Label>(grid, ref col, ref row);
			lbl3.WidthRequest = 100;
			lbl3.HorizontalTextAlignment = TextAlignment.Center;
			lbl3.Text = "Center text";

			//var ogv = AddView<OpenGLView>(grid, ref col, ref row, hOptions, vOptions, margin);

			var pkr = AddView<Picker>(grid, ref col, ref row);
			pkr.ItemsSource = Enumerable.Range(0, 10).ToList();

			var sld = AddView<Slider>(grid, ref col, ref row);
			sld.WidthRequest = 100;
			sld.Maximum = 10;
			Device.StartTimer(TimeSpan.FromSeconds(1), () =>
			{
				sld.Value += 1;
				if (sld.Value == 10d)
					sld.Value = 0;
				return true;
			});

			var stp = AddView<Stepper>(grid, ref col, ref row);

			var swt = AddView<Switch>(grid, ref col, ref row);

			var time = AddView<TimePicker>(grid, ref col, ref row, 2);

			var prog = AddView<ProgressBar>(grid, ref col, ref row, 2);
			prog.WidthRequest = 200;
			prog.BackgroundColor = Color.DarkGray;
			Device.StartTimer(TimeSpan.FromSeconds(1), () =>
			{
				prog.Progress += .1;
				if (prog.Progress == 1d)
					prog.Progress = 0;
				return true;
			});

			var srch = AddView<SearchBar>(grid, ref col, ref row, 2);
			srch.WidthRequest = 200;
			srch.Text = "Some text";

			TableView tbl = new TableView
			{
				Intent = TableIntent.Menu,
				Root = new TableRoot
					{
						new TableSection("TableView")
						{
							new TextCell
							{
								Text = "A",
							},

							new TextCell
							{
								Text = "B",
							},

							new TextCell
							{
								Text = "C",
							},

							new TextCell
							{
								Text = "D",
							},
						}
					}
			};

			var stack = new StackLayout
			{
				Children = {        new Button { Text = "Go back to Gallery home", Command = new Command(()=> { ((App)Application.Current).SetMainPage(((App)Application.Current).CreateDefaultMainPage()); }) },
									new Label { Text = $"Device Direction: {DeviceDirection}" },
									horStack,
									grid,
									new Label { Text = "TableView", FontSize = 10, TextColor = Color.DarkGray },
									tbl,
									new Label { Text = "ListView w/ TextCell", FontSize = 10, TextColor = Color.DarkGray },
									new ListView { HorizontalOptions = hOptions, ItemsSource = Enumerable.Range(0, 3).Select(c => "Text Cell!"), ItemTemplate = textCell },
									new Label { Text = "ListView w/ SwitchCell", FontSize = 10, TextColor = Color.DarkGray },
									new ListView { HorizontalOptions = hOptions, ItemsSource = Enumerable.Range(0, 3).Select(c => true), ItemTemplate = switchCell },
									new Label { Text = "ListView w/ EntryCell", FontSize = 10, TextColor = Color.DarkGray },
									new ListView { HorizontalOptions = hOptions, ItemsSource = Enumerable.Range(0, 3).Select(c => "Entry Cell!"), ItemTemplate = entryCell },
									new Label { Text = "ListView w/ ImageCell", FontSize = 10, TextColor = Color.DarkGray },
									new ListView { HorizontalOptions = hOptions, ItemsSource = Enumerable.Range(0, 3).Select(c => "coffee.png"), ItemTemplate = imageCell },
									new Label { Text = "ListView w/ ViewCell", FontSize = 10, TextColor = Color.DarkGray },
									new ListView { HorizontalOptions = hOptions, ItemsSource = Enumerable.Range(0, 3), ItemTemplate = viewCell },
								 },

				HorizontalOptions = hOptions
			};

			Content = new ScrollView
			{
				Content = stack
			};
		}

		T AddView<T>(Grid grid, ref int col, ref int row, int colSpan = 1) where T : View
		{
			var hOptions = LayoutOptions.Start;
			var vOptions = LayoutOptions.End;
			var margin = new Thickness(0, 10);
			var bgColor = Color.LightGray;

			T view = (T)Activator.CreateInstance(typeof(T));

			view.VerticalOptions = vOptions;
			view.HorizontalOptions = hOptions;
			view.Margin = margin;
			view.BackgroundColor = bgColor;

			var label = new Label { Text = $"({col},{row}) {typeof(T).ToString()}", FontSize = 10, TextColor = Color.DarkGray };

			if (colSpan > 1 && col > 0)
				NextCell(ref col, ref row, colSpan);

			grid.Children.Add(label, col, col + colSpan, row, row + 1);
			grid.Children.Add(view, col, col + colSpan, row, row + 1);

			NextCell(ref col, ref row, colSpan);

			return (T)view;
		}

		void NextCell(ref int col, ref int row, int colspan)
		{
			if (col == 0 && colspan == 1)
			{
				col = 1;
			}
			else
			{
				col = 0;
				row++;
			}
		}
	}
}