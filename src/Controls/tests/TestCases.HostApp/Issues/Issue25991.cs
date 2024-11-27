using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25991, "CarouselView reverts to displaying 1st item in collection when collection modified", PlatformAffected.UWP)]
	public class Issue25991 : ContentPage
	{
		public class Issue25991Model
		{
			public string Text { get; set; }
		}

		ObservableCollection<Issue25991Model> _collection = new ObservableCollection<Issue25991Model>() 
		{ 
			new Issue25991Model() { Text = "Item 1" }, 
			new Issue25991Model() { Text = "Item 2" } 
		};

		private CarouselView MyCarouselView { get; set; }

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

			MyCarouselView = new CarouselView()
			{
				ItemsSource = _collection,
				Loop = false
			};

			MyCarouselView.ItemTemplate = new DataTemplate(() =>
			{
				StackLayout stackLayout = new StackLayout();
				Label nameLabel = new Label();
				nameLabel.SetBinding(Label.TextProperty, "Text");
				nameLabel.FontSize = 25;
				nameLabel.TextColor = Colors.Red;
				stackLayout.Children.Add(nameLabel);
				return stackLayout;
			});

			mainStackLayout.Add(MyCarouselView);

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

			Content = mainStackLayout;
		}

		void OnScrollToPerson2ButtonClicked(object sender, EventArgs e)
		{
			MyCarouselView.ScrollTo(1);
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			_collection.Add(new Issue25991Model()
			{
				Text = "Item " + (_collection.Count + 1),
			});
		}
	}
}