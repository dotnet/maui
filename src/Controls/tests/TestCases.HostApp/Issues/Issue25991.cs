using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25991, "CarouselView reverts to displaying first item in collection when collection modified", PlatformAffected.UWP)]
	public class Issue25991 : ContentPage
	{
		public class Issue25991Model
		{
			public string Text { get; set; }

			public string AutomationId { get; set; }
		}

		ObservableCollection<Issue25991Model> _collection = new ObservableCollection<Issue25991Model>()
		{
			new Issue25991Model() { Text = "Item 1" , AutomationId = "Issue25991Item1" },
			new Issue25991Model() { Text = "Item 2", AutomationId = "Issue25991Item2"  }
		};

		Label InfoLabel { get; set; }
		CarouselView TestCarouselView { get; set; }

		public Issue25991()
		{
			StackLayout mainStackLayout = new StackLayout();
			mainStackLayout.Margin = new Thickness(5);

			Label labelInstructions = new Label()
			{
				AutomationId = "WaitForStubControl",
				Text = "Instructions:\nThe CarouselView contains Item 1 and Item 2. It will initially show the first page, Item 1.\n" +
				"Click the 'Scroll to Item 2' button, and the CarouselView will correctly show Item 2.\n" +
				"Click 'Add Item', which will add a new Item record to the collection.\n" +
				"The CarouselView must stay in the Item 2."
			};
			mainStackLayout.Add(labelInstructions);

			InfoLabel = new Label()
			{
				AutomationId = "InfoLabel",
			};

			mainStackLayout.Add(InfoLabel);

			TestCarouselView = new CarouselView()
			{
				ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset,
				ItemsSource = _collection,
				Loop = false,
				HeightRequest = 400
			};

			TestCarouselView.ItemTemplate = new DataTemplate(() =>
			{
				StackLayout stackLayout = new StackLayout();
				Label nameLabel = new Label();
				nameLabel.SetBinding(Label.TextProperty, "Text");
				nameLabel.SetBinding(Label.AutomationIdProperty, "AutomationId");
				nameLabel.FontSize = 25;
				nameLabel.TextColor = Colors.Red;
				stackLayout.Children.Add(nameLabel);
				return stackLayout;
			});

			TestCarouselView.CurrentItemChanged += OnTestCarouselViewCurrentItemChanged;
			mainStackLayout.Add(TestCarouselView);

			Button scrollToPerson2Button = new Button()
			{
				AutomationId = "ScrollToPerson2Button",
				Text = "Scroll to Item 2"
			};
			scrollToPerson2Button.Clicked += OnScrollToPerson2ButtonClicked;
			mainStackLayout.Add(scrollToPerson2Button);

			Button addItemButton = new Button()
			{
				AutomationId = "AddItemButton",
				Text = "Add Item",
			};
			addItemButton.Clicked += OnAddButtonClicked;
			mainStackLayout.Add(addItemButton);


			HorizontalStackLayout buttonsStackLayout = new HorizontalStackLayout();
			Button keepItemsInViewButton = new Button()
			{
				AutomationId = "KeepItemsInViewButton",
				Text = "keepItemsInView",
			};
			keepItemsInViewButton.Clicked += OnKeepItemsInViewButtonClicked;
			buttonsStackLayout.Add(keepItemsInViewButton);
			Button keepScrollOffsetButton = new Button()
			{
				AutomationId = "KeepScrollOffsetButton",
				Text = "KeepScrollOffset",
			};
			keepScrollOffsetButton.Clicked += OnKeepScrollOffsetButtonClicked;
			buttonsStackLayout.Add(keepScrollOffsetButton);
			Button keepLastItemInViewButton = new Button()
			{
				AutomationId = "KeepLastItemInViewButton",
				Text = "KeepLastItemInView",
			};
			keepLastItemInViewButton.Clicked += OnKeepLastItemInViewButtonClicked;
			buttonsStackLayout.Add(keepLastItemInViewButton);
			mainStackLayout.Add(buttonsStackLayout);

			Content = mainStackLayout;
		}

		void OnTestCarouselViewCurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
		{
			InfoLabel.Text = TestCarouselView.Position.ToString();
		}

		void OnScrollToPerson2ButtonClicked(object sender, EventArgs e)
		{
			TestCarouselView.ScrollTo(1);
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			_collection.Add(new Issue25991Model()
			{
				Text = "Item " + (_collection.Count + 1),
				AutomationId = "Issue25991Item" + (_collection.Count + 1)
			});
		}

		void OnKeepItemsInViewButtonClicked(object sender, EventArgs e)
		{
			TestCarouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepItemsInView;
		}

		void OnKeepScrollOffsetButtonClicked(object sender, EventArgs e)
		{
			TestCarouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;
		}

		void OnKeepLastItemInViewButtonClicked(object sender, EventArgs e)
		{
			TestCarouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
		}
	}
}