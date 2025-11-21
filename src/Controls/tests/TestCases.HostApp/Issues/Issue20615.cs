using System.Collections;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20615, "CollectionView selecteditem background lost if collectionview (or parent) IsEnabled changed.", PlatformAffected.Android)]
public class Issue20615 : ContentPage
{
	ICommand parentGridIsEnabledTriggerCmd { get; set; }
	ArrayList values { get; set; }
	Grid parentGrid;
	public Issue20615()
	{
		var gridStyle = new Style(typeof(Grid));
		gridStyle.Setters.Add(new Setter
		{
			Property = VisualStateManager.VisualStateGroupsProperty,
			Value = new VisualStateGroupList
			{
				new VisualStateGroup
				{
					Name = "CommonStates",
					States =
					{
						new VisualState { Name = "Normal" },
						new VisualState
						{
							Name = "Selected",
							Setters =
							{
								new Setter
								{
									Property = Grid.BackgroundColorProperty,
									Value = Colors.LightGreen
								}
							}
						}
					}
				}
			}
		});

		Resources = new ResourceDictionary();
		Resources.Add(gridStyle);
		parentGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
			}
		};

		var buttonStack = new VerticalStackLayout();

		var button1 = new Button
		{
			Text = "Parent Grid Disable and Enable"
		};
		button1.Clicked += Button_Clicked;
		button1.AutomationId = "ParentGridDisableEnableButton";

		buttonStack.Children.Add(button1);

		Grid.SetRow(buttonStack, 0);
		parentGrid.Children.Add(buttonStack);

		var collectionView = new CollectionView
		{
			SelectionMode = SelectionMode.Single,
			VerticalOptions = LayoutOptions.Start
		};
		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var grid = new Grid();
			var label = new Label
			{
				FontSize = 20
			};
			label.SetBinding(Label.TextProperty, ".");
			grid.Children.Add(label);
			return grid;
		});
		collectionView.AutomationId = "CollectionView";

		Grid.SetRow(collectionView, 1);
		parentGrid.Children.Add(collectionView);
		Content = parentGrid;
		values = ["First", "Second", "Third"];
		collectionView.ItemsSource = values;
		collectionView.SelectedItem = values[0];
		BindingContext = this;
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		parentGrid.IsEnabled = false;
		parentGrid.IsEnabled = true;
	}
}