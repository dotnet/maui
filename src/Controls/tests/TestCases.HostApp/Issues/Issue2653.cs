using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 2653, "[UWP] Grid insert z-order on UWP broken in Forms 3",
		PlatformAffected.UWP)]
	public class Issue2653 : TestContentPage
	{
		BoxView bv = null;
		Grid layout = null;
		const string ButtonText = "Insert Box View";
		const string MoveUp = "Increase ZIndex";
		const string MoveDown = "Decrease ZIndex";
		const string BoxViewIsOverlappingButton = "Box View Is Overlapping";
		const string Success = "BoxView Not Overlapping";
		string instructions = $"Click {ButtonText}. If Box View shows up over me test has failed.";
		const string TestForButtonClicked = "Test For Clicked";
		const string FailureText = "If this is visible test fails";
		const string ClickShouldAddText = "Clicking me should add a top layer of text";

		// Track the current ZIndex of the BoxView
		int currentZIndex = 0;

		protected override void Init()
		{
			layout = new Grid { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };

			layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
			layout.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });

			// Add the background label with default ZIndex (0)
			var backgroundLabel = new Label()
			{
				Margin = 10,
				Text = FailureText,
				BackgroundColor = Colors.White
			};

			var backgroundGrid = new Grid()
			{
				Children = { backgroundLabel }
			};
			layout.Children.Add(backgroundGrid);

			// Add the button with ZIndex of 1 to ensure it's above the background
			var actionButton = new Button()
			{
				AutomationId = ButtonText,
				Text = ButtonText,
				BackgroundColor = Colors.Green,
				Margin = 10,
				TextColor = Colors.White,
				Command = new Command(() =>
				{
					if (!AddBoxView())
					{
						layout.Children.Remove(bv);
					}
				})
			};
			layout.Children.Add(actionButton);
			Grid.SetZIndex(actionButton, 1);

			this.On<iOS>().SetUseSafeArea(true);

			var labelInstructions = new Label { Text = instructions };

			Content = new StackLayout()
			{
				Children =
				{
					labelInstructions,
					new Button() {
						Text = MoveUp,
						AutomationId = MoveUp,
						Command = new Command(() =>
						{
							AddBoxView();
							currentZIndex++;
							Grid.SetZIndex(bv, currentZIndex);
							labelInstructions.Text = $"BoxView ZIndex = {currentZIndex}";
						}),
						HeightRequest = 45
					},
					new Button() {
						Text = MoveDown,
						AutomationId = MoveDown,
						Command = new Command(() =>
						{
							AddBoxView();
							currentZIndex--;
							Grid.SetZIndex(bv, currentZIndex);
							labelInstructions.Text = $"BoxView ZIndex = {currentZIndex}";
						}),
						HeightRequest = 45
					},
					layout,
					new Button()
					{
						AutomationId = TestForButtonClicked,
						Text = TestForButtonClicked,
						Command = new Command(() =>
						{
							if(bv == null)
							{
								labelInstructions.Text = String.Empty;
							}
							else if(!layout.Children.Contains(bv))
							{
								labelInstructions.Text = Success;
							}
							else
							{
                                // Test if BoxView is overlapping based on ZIndex
                                int buttonZIndex = Grid.GetZIndex(actionButton);
								int boxViewZIndex = Grid.GetZIndex(bv);

								if (boxViewZIndex > buttonZIndex)
								{
									labelInstructions.Text = BoxViewIsOverlappingButton;
								}
								else
								{
									labelInstructions.Text = Success;
								}
							}
						}),
						HeightRequest = 45
					},
					new Button()
					{
						Text = ClickShouldAddText,
						Command = new Command(() =>
						{
                            // Add a label at the bottom of z-order
                            var newLabel = new Label();
							layout.Children.Add(newLabel);
							Grid.SetZIndex(newLabel, -1);
                            
                            // Add a grid with text at the top of z-order
                            var newGrid = new Grid()
							{
								Children =
								{
									new Label()
									{
										Margin = 10,
										Text = "If you can't see me test has failed",
										BackgroundColor = Colors.White
									}
								}
							};
							layout.Children.Add(newGrid);
							Grid.SetZIndex(newGrid, 10); // Set a high ZIndex to ensure it's on top
                        }),
						HeightRequest = 45
					}
				}
			};
		}

		bool AddBoxView()
		{
			if (bv != null && layout.Children.Contains(bv))
				return false;

			bv = new BoxView
			{
				Color = Colors.Purple,
				WidthRequest = 3000,
				HeightRequest = 3000,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			layout.Children.Add(bv);

			// Set the initial ZIndex for the BoxView
			currentZIndex = -1; // Start below other elements
			Grid.SetZIndex(bv, currentZIndex);

			return true;
		}
	}
}
