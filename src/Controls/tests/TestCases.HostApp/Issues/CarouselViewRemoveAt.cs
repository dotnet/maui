using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	// Issue10300 (src\ControlGallery\src\Issues.Shared\Issue10300.cs
	[Issue(IssueTracker.None, 10300, "ObservableCollection.RemoveAt(index) with a valid index raises ArgumentOutOfRangeException", PlatformAffected.iOS)]
	public class CarouselViewRemoveAt : ContentPage
	{
		readonly CarouselView _carousel;

		public class ModalPage : ContentPage
		{
			public ModalPage()
			{
				var btn = new Button
				{
					AutomationId = "CloseMe",
					Text = "CloseMe",
					TextColor = Colors.White,
					BackgroundColor = Colors.Red,
					VerticalOptions = LayoutOptions.End
				};
				btn.Clicked += OnCloseClicked;
				Content = btn;
			}

			void OnCloseClicked(object sender, EventArgs e)
			{
				Navigation.PopModalAsync();
#pragma warning disable CS0618 // Type or member is obsolete
				MessagingCenter.Instance.Send<Page>(this, "DeleteMe");
#pragma warning restore CS0618 // Type or member is obsolete
			}
		}

		public CarouselViewRemoveAt()
		{
			Items = new ObservableCollection<ModelCarouselViewRemoveAt>(new[]
			{
				new ModelCarouselViewRemoveAt("1", Colors.Aqua),
				new ModelCarouselViewRemoveAt("2", Colors.BlueViolet),
				new ModelCarouselViewRemoveAt("3", Colors.Coral),
				new ModelCarouselViewRemoveAt("4", Colors.DarkGoldenrod),
				new ModelCarouselViewRemoveAt("5", Colors.Fuchsia),
				new ModelCarouselViewRemoveAt("6", Colors.Gold),
				new ModelCarouselViewRemoveAt("7", Colors.HotPink),
				new ModelCarouselViewRemoveAt("8", Colors.IndianRed),
				new ModelCarouselViewRemoveAt("9", Colors.Khaki),
			});
			_carousel = new CarouselView
			{
				ItemTemplate = new DataTemplate(() =>
			{
				var l = new Grid();
				l.SetBinding(Grid.BackgroundColorProperty, new Binding("Color"));
				var label = new Label
				{
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center
				};
				label.SetBinding(Label.TextProperty, new Binding("Text"));
				l.Children.Add(label);
				return l;
			})
			};
			_carousel.CurrentItemChanged += Carousel_CurrentItemChanged;
			_carousel.PositionChanged += Carousel_PositionChanged;
			_carousel.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			Grid.SetColumnSpan(_carousel, 2);

			_carousel.SetBinding(CarouselView.ItemsSourceProperty, new Binding("Items"));
			_carousel.BindingContext = this;

			var grd = new Grid
			{
				Margin = new Thickness(5)
			};
			grd.ColumnDefinitions.Add(new ColumnDefinition());
			grd.ColumnDefinitions.Add(new ColumnDefinition());

			grd.RowDefinitions.Add(new RowDefinition());
			grd.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

			var btn = new Button
			{
				AutomationId = "DeleteMe",
				Text = "DeleteMe",
				BackgroundColor = Colors.Red,
				TextColor = Colors.White
			};
			var btnAdd = new Button
			{
				AutomationId = "AddMe",
				Text = "AddMe",
				BackgroundColor = Colors.Red,
				TextColor = Colors.White
			};
			btn.Clicked += OnDeleteClicked;
			Grid.SetRow(btn, 1);
			Grid.SetColumn(btn, 0);

			btnAdd.Clicked += OnAddClicked;
			Grid.SetRow(btnAdd, 1);
			Grid.SetColumn(btnAdd, 1);

			grd.Children.Add(_carousel);
			grd.Children.Add(btn);
			grd.Children.Add(btnAdd);
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			Content = grd;
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Instance.Subscribe<Page>(this, "DeleteMe", Callback);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		void Carousel_PositionChanged(object sender, PositionChangedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"Position old {e.PreviousPosition} Position new {e.CurrentPosition}");
		}

		void Carousel_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"Current old {e.PreviousItem} Current new {e.CurrentItem}");
		}

		void Callback(Page page)
		{
			var index = Items.IndexOf(_carousel.CurrentItem as ModelCarouselViewRemoveAt);
			System.Diagnostics.Debug.WriteLine($"DeleteMe {index}");
			Items.RemoveAt(index);
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Instance.Unsubscribe<Page>(this, "DeleteMe");
#pragma warning restore CS0618 // Type or member is obsolete
		}

		public ObservableCollection<ModelCarouselViewRemoveAt> Items { get; set; }


		async void OnDeleteClicked(object sender, EventArgs e)
		{
			await Navigation.PushModalAsync(new ModalPage());
		}

		void OnAddClicked(object sender, EventArgs e)
		{
			Items.Insert(0, new ModelCarouselViewRemoveAt("0", Colors.PaleGreen));
		}
	}

	public class ModelCarouselViewRemoveAt
	{
		public string Text { get; set; }

		public Color Color { get; set; }

		public ModelCarouselViewRemoveAt(string text, Color color)
		{
			Text = text;
			Color = color;
		}

		public override string ToString()
		{
			return Text;
		}
	}
}