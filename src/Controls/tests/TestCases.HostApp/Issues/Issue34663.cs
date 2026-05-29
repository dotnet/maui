namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34663, "CV2 MakeVisible grouped ScrollTo produces inconsistent positions", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34663 : ContentPage
{
	readonly CollectionView2 _collectionView;
	readonly Picker _positionPicker;
	readonly Switch _animateSwitch;
	int _scrollCount;

	public Issue34663()
	{
		var instructions = new ScrollView
		{
			HeightRequest = 60,
			Content = new VerticalStackLayout
			{
				Children =
				{
					new Label { Text = "1. Click 'Scroll to third cat' with MakeVisible position." },
					new Label { Text = "2. Manually scroll back to top." },
					new Label { Text = "3. Repeat — scroll position should be consistent." },
				}
			}
		};

		_positionPicker = new Picker
		{
			AutomationId = "PositionPicker",
			ItemsSource = new[] { "MakeVisible", "Start", "Center", "End" },
			SelectedIndex = 0,
		};
		var pickerRow = new HorizontalStackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Label { Text = "ScrollToPosition: ", VerticalTextAlignment = TextAlignment.Center },
				_positionPicker,
			}
		};

		_animateSwitch = new Switch { AutomationId = "AnimateSwitch", IsToggled = true };
		var switchRow = new HorizontalStackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Label { Text = "Animate scroll: ", VerticalTextAlignment = TextAlignment.Center },
				_animateSwitch,
			}
		};

		var scrollToButton = new Button
		{
			AutomationId = "ScrollToThirdCatButton",
			Text = "Scroll to third cat",
		};
		scrollToButton.Clicked += OnScrollToClicked;

		_collectionView = new CollectionView2
		{
			AutomationId = "GroupedCollectionView",
			IsGrouped = true,
			ItemsSource = CreateGroupedData(),
			ItemTemplate = new DataTemplate(() =>
			{
				var image = new Image
				{
					Aspect = Aspect.AspectFill,
					HeightRequest = 60,
					WidthRequest = 60,
				};
				image.SetBinding(Image.SourceProperty, "ImageUrl");

				var nameLabel = new Label { FontAttributes = FontAttributes.Bold };
				nameLabel.SetBinding(Label.TextProperty, "Name");
				nameLabel.SetBinding(Label.AutomationIdProperty, "Name");

				var locationLabel = new Label
				{
					FontAttributes = FontAttributes.Italic,
					VerticalOptions = LayoutOptions.End,
				};
				locationLabel.SetBinding(Label.TextProperty, "Location");

				var grid = new Grid
				{
					Padding = new Thickness(10),
					RowDefinitions =
					{
						new RowDefinition(GridLength.Auto),
						new RowDefinition(GridLength.Auto),
					},
					ColumnDefinitions =
					{
						new ColumnDefinition(GridLength.Auto),
						new ColumnDefinition(GridLength.Star),
					}
				};

				Grid.SetRowSpan(image, 2);
				grid.Children.Add(image);

				Grid.SetColumn(nameLabel, 1);
				grid.Children.Add(nameLabel);

				Grid.SetRow(locationLabel, 1);
				Grid.SetColumn(locationLabel, 1);
				grid.Children.Add(locationLabel);

				return grid;
			}),
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					BackgroundColor = Colors.LightGray,
					FontSize = 20,
					FontAttributes = FontAttributes.Bold,
				};
				label.SetBinding(Label.TextProperty, "Name");
				label.SetBinding(Label.AutomationIdProperty, new Binding("Name", stringFormat: "GroupHeader_{0}"));
				return label;
			}),
			GroupFooterTemplate = new DataTemplate(() =>
			{
				var label = new Label { Margin = new Thickness(0, 0, 0, 10) };
				label.SetBinding(Label.TextProperty, new Binding("Count", stringFormat: "Total animals: {0:D}"));
				return label;
			})
		};

		// Grid layout: Auto rows for controls, Star for CollectionView
		var grid = new Grid
		{
			Margin = new Thickness(20),
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star),
			}
		};

		Grid.SetRow(instructions, 0);
		Grid.SetRow(pickerRow, 1);
		Grid.SetRow(switchRow, 2);
		Grid.SetRow(scrollToButton, 3);
		Grid.SetRow(_collectionView, 4);

		grid.Children.Add(instructions);
		grid.Children.Add(pickerRow);
		grid.Children.Add(switchRow);
		grid.Children.Add(scrollToButton);
		grid.Children.Add(_collectionView);

		Content = grid;
	}

	void OnScrollToClicked(object sender, EventArgs e)
	{
		_scrollCount++;
		var position = (ScrollToPosition)_positionPicker.SelectedIndex;
		_collectionView.ScrollTo(2, 1, position, _animateSwitch.IsToggled);
	}

	static List<AnimalGroup34663> CreateGroupedData()
	{
		return new List<AnimalGroup34663>
		{
			new("Bears", new List<Animal34663>
			{
				new("American Black Bear", "North America", "https://upload.wikimedia.org/wikipedia/commons/0/08/01_Schwarzb%C3%A4r.jpg"),
				new("Asian Black Bear", "Asia", "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b7/Ursus_thibetanus_3_%28Wroclaw_zoo%29.JPG/180px-Ursus_thibetanus_3_%28Wroclaw_zoo%29.JPG"),
				new("Brown Bear", "Northern Eurasia", "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5d/Kamchatka_Brown_Bear_near_Dvuhyurtochnoe_on_2015-07-23.jpg/320px-Kamchatka_Brown_Bear_near_Dvuhyurtochnoe_on_2015-07-23.jpg"),
				new("Grizzly-Polar Hybrid", "Canadian Artic", "https://upload.wikimedia.org/wikipedia/commons/thumb/7/7e/Grolar.JPG/276px-Grolar.JPG"),
				new("Sloth Bear", "Indian Subcontinent", "https://upload.wikimedia.org/wikipedia/commons/thumb/6/6c/Sloth_Bear_Washington_DC.JPG/320px-Sloth_Bear_Washington_DC.JPG"),
				new("Sun Bear", "Southeast Asia", "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a6/Sitting_sun_bear.jpg/319px-Sitting_sun_bear.jpg"),
				new("Polar Bear", "Artic Circle", "https://upload.wikimedia.org/wikipedia/commons/6/66/Polar_Bear_-_Alaska_%28cropped%29.jpg"),
				new("Spectacled Bear", "South America", "https://upload.wikimedia.org/wikipedia/commons/thumb/9/99/Spectacled_Bear_-_Houston_Zoo.jpg/264px-Spectacled_Bear_-_Houston_Zoo.jpg"),
				new("Short-faced Bear", "Extinct", "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b8/ArctodusSimusSkeleton.jpg/320px-ArctodusSimusSkeleton.jpg"),
				new("California Grizzly", "Extinct", "https://upload.wikimedia.org/wikipedia/commons/d/de/Monarch_the_bear.jpg"),
			}),
			new("Cats", new List<Animal34663>
			{
				new("Abyssinian", "Ethiopia", "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9b/Gustav_chocolate.jpg/168px-Gustav_chocolate.jpg"),
				new("Arabian Mau", "Arabian Peninsula", "https://upload.wikimedia.org/wikipedia/commons/d/d3/Bex_Arabian_Mau.jpg"),
				new("Bengal", "Asia", "https://upload.wikimedia.org/wikipedia/commons/thumb/b/ba/Paintedcats_Red_Star_standing.jpg/187px-Paintedcats_Red_Star_standing.jpg"),
				new("Burmese", "Thailand", "https://upload.wikimedia.org/wikipedia/commons/0/04/Blissandlucky11.jpg"),
				new("Cyprus", "Cyprus", "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b9/CyprusShorthair.jpg/320px-CyprusShorthair.jpg"),
				new("German Rex", "Germany", "https://upload.wikimedia.org/wikipedia/commons/c/c7/German_rex_harry_%28cropped%29.jpg"),
				new("Highlander", "United States", "https://upload.wikimedia.org/wikipedia/commons/thumb/1/15/Highlander-7.jpg/293px-Highlander-7.jpg"),
				new("Manx", "Isle of Man", "https://upload.wikimedia.org/wikipedia/en/9/9b/Manx_cat_by_Karen_Weaver.jpg"),
				new("Peterbald", "Russia", "https://upload.wikimedia.org/wikipedia/commons/c/c7/Peterbald_male_Shango_by_Irina_Polunina.jpg"),
				new("Scottish Fold", "Scotland", "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5d/Adult_Scottish_Fold.jpg/240px-Adult_Scottish_Fold.jpg"),
				new("Sphynx", "Europe", "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e8/Sphinx2_July_2006.jpg/180px-Sphinx2_July_2006.jpg"),
			}),
			new("Dogs", new List<Animal34663>
			{
				new("Afghan Hound", "Afghanistan", "https://upload.wikimedia.org/wikipedia/commons/6/69/Afghane.jpg"),
				new("Alpine Dachsbracke", "Austria", "https://upload.wikimedia.org/wikipedia/commons/thumb/2/23/Alpejski_go%C5%84czy_kr%C3%B3tkonogy_g99.jpg/320px-Alpejski_go%C5%84czy_kr%C3%B3tkonogy_g99.jpg"),
				new("American Bulldog", "United States", "https://upload.wikimedia.org/wikipedia/commons/5/5e/American_Bulldog_600.jpg"),
				new("Bearded Collie", "Scotland", "https://upload.wikimedia.org/wikipedia/commons/9/9c/Bearded_Collie_600.jpg"),
				new("Boston Terrier", "United States", "https://upload.wikimedia.org/wikipedia/commons/thumb/d/d7/Boston-terrier-carlos-de.JPG/320px-Boston-terrier-carlos-de.JPG"),
				new("Canadian Eskimo", "Canada", "https://upload.wikimedia.org/wikipedia/commons/7/79/Spoonsced.jpg"),
				new("Eurohound", "Scandinavia", "https://upload.wikimedia.org/wikipedia/commons/9/98/Eurohound.jpg"),
				new("Irish Terrier", "Ireland", "https://upload.wikimedia.org/wikipedia/commons/thumb/5/56/IrishTerrierSydenhamHillWoods.jpg/180px-IrishTerrierSydenhamHillWoods.jpg"),
				new("Kerry Beagle", "Ireland", "https://upload.wikimedia.org/wikipedia/commons/7/75/Kerry_Beagle_from_1915.JPG"),
				new("Norwegian Buhund", "Norway", "https://upload.wikimedia.org/wikipedia/commons/3/3b/Norwegian_Buhund_600.jpg"),
			}),
			new("Elephants", new List<Animal34663>
			{
				new("African Bush Elephant", "Africa", "https://upload.wikimedia.org/wikipedia/commons/thumb/9/91/African_Elephant_%28Loxodonta_africana%29_bull_%2831100819046%29.jpg/320px-African_Elephant_%28Loxodonta_africana%29_bull_%2831100819046%29.jpg"),
				new("African Forest Elephant", "Africa", "https://upload.wikimedia.org/wikipedia/commons/thumb/6/6a/African_Forest_Elephant.jpg/180px-African_Forest_Elephant.jpg"),
				new("Asian Elephant", "Asia", "https://upload.wikimedia.org/wikipedia/commons/thumb/9/98/Elephas_maximus_%28Bandipur%29.jpg/320px-Elephas_maximus_%28Bandipur%29.jpg"),
			}),
			new("Monkeys", new List<Animal34663>
			{
				new("Baboon", "Africa & Asia", "https://upload.wikimedia.org/wikipedia/commons/thumb/9/96/Portrait_Of_A_Baboon.jpg/314px-Portrait_Of_A_Baboon.jpg"),
				new("Capuchin Monkey", "Central & South America", "https://upload.wikimedia.org/wikipedia/commons/thumb/4/40/Capuchin_Costa_Rica.jpg/200px-Capuchin_Costa_Rica.jpg"),
				new("Blue Monkey", "Central & East Africa", "https://upload.wikimedia.org/wikipedia/commons/thumb/8/83/BlueMonkey.jpg/220px-BlueMonkey.jpg"),
				new("Squirrel Monkey", "Central & South America", "https://upload.wikimedia.org/wikipedia/commons/thumb/2/20/Saimiri_sciureus-1_Luc_Viatour.jpg/220px-Saimiri_sciureus-1_Luc_Viatour.jpg"),
				new("Golden Lion Tamarin", "Brazil", "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8b/GoldenLionTamarin-001.jpg/222px-GoldenLionTamarin-001.jpg"),
				new("Howler Monkey", "South America", "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0d/Alouatta_guariba.jpg/200px-Alouatta_guariba.jpg"),
				new("Japanese Macaque", "Japan", "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c1/Macaca_fuscata_fuscata1.jpg/220px-Macaca_fuscata_fuscata1.jpg"),
				new("Mandrill", "Southern Cameroon", "https://upload.wikimedia.org/wikipedia/commons/thumb/7/75/Mandrill_at_san_francisco_zoo.jpg/220px-Mandrill_at_san_francisco_zoo.jpg"),
				new("Proboscis Monkey", "Borneo", "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e5/Proboscis_Monkey_in_Borneo.jpg/250px-Proboscis_Monkey_in_Borneo.jpg"),
				new("Spider Monkey", "Central & South America", "https://upload.wikimedia.org/wikipedia/commons/thumb/4/40/Ateles_geoffroyi_Costa_Rica.jpg/200px-Ateles_geoffroyi_Costa_Rica.jpg"),
			}),
		};
	}
}

public class Animal34663
{
	public string Name { get; set; }
	public string Location { get; set; }
	public string ImageUrl { get; set; }

	public Animal34663(string name, string location, string imageUrl)
	{
		Name = name;
		Location = location;
		ImageUrl = imageUrl;
	}
}

public class AnimalGroup34663 : List<Animal34663>
{
	public string Name { get; private set; }

	public AnimalGroup34663(string name, List<Animal34663> animals) : base(animals)
	{
		Name = name;
	}
}
