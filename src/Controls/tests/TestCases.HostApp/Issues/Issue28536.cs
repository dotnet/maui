namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28536, "Navigation breaks VisualState styles for Picker", PlatformAffected.UWP)]
public class Issue28536NavigationPage : TestNavigationPage
{
	protected override void Init()
	{
		Issue28536 page = new Issue28536();
		Navigation.PushAsync(page);
	}

	class Issue28536 : TestContentPage
	{
		protected override void Init()
		{
			Style visualStatePickerStyle = new Style(typeof(Picker))
			{
				Setters =
			{
				new Setter { Property = Picker.TextColorProperty, Value = Colors.Green },
				new Setter
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
									Name = "Disabled",
									Setters =
									{
										new Setter { Property = Picker.TextColorProperty, Value = Colors.Red },
									}
								}
							}
						}
					}
				}
			}
			};

			Switch enabledSwitch = new Switch
			{
				VerticalOptions = LayoutOptions.Center,
				WidthRequest = 20,
				Margin = 0,
				HorizontalOptions = LayoutOptions.Start,
				AutomationId = "Switch"
			};

			Picker picker = new Picker
			{
				HorizontalOptions = LayoutOptions.Center,
				FontSize = 24,
				Style = visualStatePickerStyle,
				ItemsSource = new[]
				{
					"Baboon", "Capuchin Monkey", "Blue Monkey", "Squirrel Monkey",
					"Golden Lion Tamarind", "Howler Monkey", "Japanese Macaque"
				},
				SelectedIndex = 1,
				AutomationId = "VisualStatePicker"
			};

			picker.SetBinding(Picker.IsEnabledProperty, new Binding("IsToggled", source: enabledSwitch));

			Button navigateButton = new Button
			{
				Text = "Go to Next Page",
				HorizontalOptions = LayoutOptions.Center,
				AutomationId = "NavigateButton"
			};

			navigateButton.Clicked += (_, _) => Navigation.PushAsync(new Issue28536SecondPage());

			Content = new VerticalStackLayout
			{
				Padding = new Thickness(30, 0),
				Spacing = 25,
				Children = { enabledSwitch, picker, navigateButton }
			};
		}
	}

	class Issue28536SecondPage : TestContentPage
	{
		protected override void Init()
		{
			Content = new Label { Text = "Next Page" };
		}
	}
}


