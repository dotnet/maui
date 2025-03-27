namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 15806, "RadioButton Border color not working for focused visual state", PlatformAffected.All)]
class Issue15806 : ContentPage
{
	public Issue15806()
	{
		VerticalStackLayout verticalStackLayout = new VerticalStackLayout();

		Style radioButtonStyle = new Style(typeof(RadioButton))
		{
			Setters =
			{
				new Setter
				{
					Property = VisualStateManager.VisualStateGroupsProperty,
					Value = new VisualStateGroupList
					{
						new VisualStateGroup
						{
							Name = "CheckedStates",
							States =
							{
								new VisualState
								{
									Name = "Normal",
									Setters =
									{
										new Setter { Property = RadioButton.BorderColorProperty, Value = Colors.Red },
										new Setter { Property = RadioButton.BorderWidthProperty, Value = 2 } 
									}
								},

								new VisualState
								{
									Name = "Focused",
									Setters = 
									{
										new Setter { Property = RadioButton.BorderColorProperty, Value = Colors.DarkCyan },
										new Setter { Property = RadioButton.BorderWidthProperty, Value = 2 }
									}
								}
							}
						}
					}
				}
			}
		};

		RadioButton focusedRadioButton = new RadioButton
		{
			AutomationId = "FocusedRadioButton",
			Content = "RadioButton",
			Style = radioButtonStyle,
			IsChecked = true,
		};

		RadioButton normalRadioButton = new RadioButton
		{
			AutomationId = "NormalRadioButton",
			Content = "RadioButton",
			Style = radioButtonStyle,
		};

		verticalStackLayout.Children.Add(focusedRadioButton);
		verticalStackLayout.Children.Add(normalRadioButton);

		Content = verticalStackLayout;
	}
}
