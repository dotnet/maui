using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms;

namespace Xamarin.Forms.Controls.Issues
{
	// This test covers the issue reported in https://github.com/xamarin/Xamarin.Forms/issues/2763

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2763,
		"[Core] StakLayout Padding update issue", NavigationBehavior.PushAsync)]
	public class Issue2763 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Padding update issue";
			StackLayout parentLayout1 = null;
			StackLayout parentLayout2 = null;
			StackLayout parentLayout3 = null;
			StackLayout parentLayout4 = null;

			StackLayout stackLayout = new StackLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				BackgroundColor = Color.Blue,
				Children =
				{
					new BoxView
					{
						Color = Color.Red,
						HeightRequest = 100,
						WidthRequest = 100,
					}
				}
			};
			StackLayout stackLayout2 = new StackLayout
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Color.Blue,
				Children =
				{
					new BoxView
					{
						HorizontalOptions = LayoutOptions.Start,
						Color = Color.Red,
						HeightRequest = 100,
						WidthRequest = 100,
					}
				}
			};

			ContentView contentView = new ContentView
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				BackgroundColor = Color.Blue,
				Content =
				new BoxView
				{
					Color = Color.Red,
					HeightRequest = 100,
					WidthRequest = 100,
				}
			};

			FlexLayout flex = new FlexLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				BackgroundColor = Color.Blue,
				Children =
				{
					new BoxView
					{
						Color = Color.Red,
						HeightRequest = 100,
						WidthRequest = 100,
					}
				}
			};

			Slider paddingSlider = new Slider
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Minimum = 0.0,
				Maximum = 100.0,
			};

			stackLayout.SetBinding(Forms.Layout.PaddingProperty, new Binding() { Path = "Value", Source = paddingSlider });
			contentView.SetBinding(Forms.Layout.PaddingProperty, new Binding() { Path = "Value", Source = paddingSlider });
			flex.SetBinding(Forms.Layout.PaddingProperty, new Binding() { Path = "Value", Source = paddingSlider });
			stackLayout2.SetBinding(Forms.Layout.PaddingProperty, new Binding() { Path = "Value", Source = paddingSlider });

			// Build the page.
			this.Padding = new Thickness(20);
			this.Content = new StackLayout
			{
				Spacing = 20,
				Children =
				{
					new StackLayout
					{
						Orientation = StackOrientation.Horizontal,
						Children =
						{
							new Label
							{
								Text = "Padding"
							},
							paddingSlider,
						}
					},
					new Button()
					{
						Text = "Force update",
						Command = new Command(() =>
						{
							var boxview = new BoxView();
							parentLayout1.Children.Add(boxview);
							parentLayout1.Children.Remove(boxview);
							parentLayout2.Children.Add(boxview);
							parentLayout2.Children.Remove(boxview);
							parentLayout3.Children.Add(boxview);
							parentLayout3.Children.Remove(boxview);
							parentLayout4.Children.Add(boxview);
							parentLayout4.Children.Remove(boxview);
						})
					},
					new ScrollView
					{
						HorizontalOptions = LayoutOptions.FillAndExpand,
						VerticalOptions = LayoutOptions.FillAndExpand,
						Content = new StackLayout
						{
							Spacing = 20,
							Children =
							{
								(parentLayout4 = new StackLayout
								{
									HeightRequest = 200,
									Children = { new Label { Text = "StackLayout2" }, stackLayout2 },
								}),
								(parentLayout1 = new StackLayout
								{
									Children = { new Label { Text = "StackLayout" }, stackLayout },
								}),
								(parentLayout2 = new StackLayout
								{
									Children = { new Label { Text = "ContentView" }, contentView }
								}),
								(parentLayout3 = new StackLayout
								{
									Children = { new Label { Text = "FlexLayout" }, flex }
								})

							}
						}
					}
				}
			};
		}
	}
}
