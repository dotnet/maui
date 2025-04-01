using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 2653, "[UWP] Grid insert z-order on UWP broken in Forms 3",
		PlatformAffected.UWP)]
	public class Issue2653 : TestContentPage
	{
		BoxView bv = null;
		Microsoft.Maui.Controls.Compatibility.Grid layout = null;
		const string ButtonText = "Insert Box View";
		const string MoveUp = "Move Box View Up";
		const string MoveDown = "Move Box View Down";
		const string BoxViewIsOverlappingButton = "Box View Is Overlapping";
		const string Success = "BoxView Not Overlapping";
		string instructions = $"Click {ButtonText}. If Box View shows up over me test has failed.";
		const string TestForButtonClicked = "Test For Clicked";
		const string FailureText = "If this is visible test fails";
		const string ClickShouldAddText = "Clicking me should add a top layer of text";

		protected override void Init()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			layout = new Microsoft.Maui.Controls.Compatibility.Grid { BackgroundColor = Colors.Red, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };
#pragma warning restore CS0618 // Type or member is obsolete

			layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
			layout.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });
			layout.Children.Add(new Grid()
			{
				Children =
				{
					new Label()
					{
						Margin = 10,
						Text = FailureText,
						BackgroundColor = Colors.White
					}
				}
			});

			layout.Children.Add(new Button()
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
			});

			this.On<iOS>().SetUseSafeArea(true);

			var labelInstructions = new Label { Text = instructions };

			Content = new StackLayout()
			{
				Children =
				{
					labelInstructions,
					new Button(){ Text = MoveUp, AutomationId = MoveUp, Command = new Command(() =>
					{
						AddBoxView();
						layout.RaiseChild(bv);
					}),  HeightRequest = 45},
					new Button(){ Text = MoveDown, AutomationId = MoveDown, Command = new Command(() =>
					{
						AddBoxView();
						layout.LowerChild(bv);
					}),  HeightRequest = 45},
					layout,
					new Button()
					{
						AutomationId = TestForButtonClicked,
						Text = TestForButtonClicked, Command = new Command(() =>
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
								labelInstructions.Text = BoxViewIsOverlappingButton;
							}
						}),
						HeightRequest = 45
					},
					new Button()
					{
						Text = ClickShouldAddText, Command = new Command(() =>
						{
							layout.Children.Insert(0, new Label());
							layout.Children.Add(new Grid()
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
							});
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

			layout.Children.Insert(0, bv);
			return true;
		}
	}
}
