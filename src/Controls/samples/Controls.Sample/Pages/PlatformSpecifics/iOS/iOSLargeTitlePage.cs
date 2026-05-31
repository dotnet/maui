using System;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages;

public partial class iOSLargeTitlePage : ContentPage
{
	public iOSLargeTitlePage()
	{
		Title = "Large Titles";

		// Set the parent NavigationPage's BarTextColor to black when this page appears
		Appearing += (s, e) =>
		{
			if (Parent is Microsoft.Maui.Controls.NavigationPage navPage)
			{
				navPage.BarTextColor = Colors.Black;
			}
		};

		var scrollView = new Microsoft.Maui.Controls.ScrollView
		{
			Content = new StackLayout
			{
				Padding = new Thickness(10),
				Spacing = 15,
				Children =
				{
					// Display Mode Section
					new Label
					{
						Text = "Display Mode",
						FontSize = 20,
						FontAttributes = FontAttributes.Bold,
						Margin = new Thickness(0, 10, 0, 0)
					},
					new Button
					{
						Text = "Never - Hide large title",
						Command = new Command(() => On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Never))
					},
					new Button
					{
						Text = "Always - Show large title",
						Command = new Command(() => On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Always))
					},
					new Button
					{
						Text = "Automatic - Inherit from previous page",
						Command = new Command(async () =>
						{
							var page = new ContentPage { Title = "Automatic Mode" };
							page.On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Automatic);
							await Navigation.PushAsync(page);
						})
					},

					// Navigation Configuration Section
					new BoxView { HeightRequest = 1, Color = Colors.Gray, Margin = new Thickness(0, 10) },
					new Label
					{
						Text = "Navigation Configuration",
						FontSize = 20,
						FontAttributes = FontAttributes.Bold
					},
					new Button
					{
						Text = "Toggle PrefersLargeTitles on NavigationPage",
						Command = new Command(async () =>
						{
							var navPage = (Microsoft.Maui.Controls.NavigationPage)Parent;
							navPage.On<iOS>().SetPrefersLargeTitles(!navPage.On<iOS>().PrefersLargeTitles());
							
							// Refresh the page to show the change
							Navigation.InsertPageBefore(new iOSLargeTitlePage(), this);
							await Navigation.PopAsync(false);
						})
					},

					// Scrolling Behavior Section
					new BoxView { HeightRequest = 1, Color = Colors.Gray, Margin = new Thickness(0, 10) },
					new Label
					{
						Text = "Scrolling Behavior",
						FontSize = 20,
						FontAttributes = FontAttributes.Bold
					},
					new Button
					{
						Text = "ScrollView - Watch title collapse on scroll",
						Command = new Command(async () =>
						{
							var navPage = (Microsoft.Maui.Controls.NavigationPage)Parent;
							navPage.On<iOS>().SetPrefersLargeTitles(true);

							var page = new ContentPage { Title = "Scroll Me" };
							page.On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Always);

							var scrollContent = new StackLayout { Padding = 20, Spacing = 10 };
							for (int i = 1; i <= 50; i++)
							{
								scrollContent.Children.Add(new Label
								{
									Text = $"Item {i} - Scroll to see the large title collapse",
									FontSize = 16,
									Padding = new Thickness(0, 10)
								});
							}

							page.Content = new Microsoft.Maui.Controls.ScrollView { Content = scrollContent };
							await Navigation.PushAsync(page);
						})
					},
					new Button
					{
						Text = "CollectionView - Large title with SafeArea",
						Command = new Command(async () =>
						{
							var navPage = (Microsoft.Maui.Controls.NavigationPage)Parent;
							navPage.On<iOS>().SetPrefersLargeTitles(true);

							var page = new ContentPage { Title = "CollectionView Demo", BackgroundColor = Colors.LightBlue };
							page.On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Always);
							page.SafeAreaEdges = Microsoft.Maui.SafeAreaEdges.All;

							var collectionView = new Microsoft.Maui.Controls.CollectionView
							{
								VerticalOptions = LayoutOptions.Fill
							};

							collectionView.ItemTemplate = new DataTemplate(() =>
							{
								var label = new Label
								{
									Text = "Scroll to see title collapse",
									FontSize = 18,
									Padding = 15
								};
								return label;
							});

							collectionView.ItemsSource = Enumerable.Range(1, 40);
							collectionView.Header = new Label { BackgroundColor = Colors.Pink, Text = "Header", Padding = 10, FontSize = 16 };
							collectionView.Footer = new Label { BackgroundColor = Colors.Yellow, Text = "Footer", Padding = 10, FontSize = 16 };

							page.Content = collectionView;
							await navPage.PushAsync(page);
						})
					},
					new Button
					{
						Text = "Transparent NavBar - Content shows through",
						Command = new Command(async () =>
						{
							var navPage = (Microsoft.Maui.Controls.NavigationPage)Parent;
							navPage.On<iOS>().SetPrefersLargeTitles(true);
							var previousBarColor = navPage.BarBackgroundColor;
							navPage.BarBackgroundColor = Colors.Transparent;

							var page = new ContentPage
							{
								Title = "Transparent Bar",
								// Create a gradient background that shows through the nav bar
								Background = new LinearGradientBrush
								{
									StartPoint = new Point(0, 0),
									EndPoint = new Point(0, 1),
									GradientStops = new GradientStopCollection
									{
										new GradientStop { Color = Color.FromArgb("#667eea"), Offset = 0.0f },
										new GradientStop { Color = Color.FromArgb("#764ba2"), Offset = 1.0f }
									}
								}
							};
							page.On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Always);

							var scrollContent = new StackLayout { Padding = 20, Spacing = 15 };
							scrollContent.Children.Add(new Label
							{
								Text = "Notice how the gradient background shows through the navigation bar area. This follows Apple's HIG recommendation.",
								FontSize = 16,
								TextColor = Colors.White,
								Margin = new Thickness(0, 20)
							});

							for (int i = 1; i <= 30; i++)
							{
								scrollContent.Children.Add(new Frame
								{
									BackgroundColor = Colors.White.WithAlpha(0.9f),
									CornerRadius = 10,
									Padding = 15,
									Content = new Label
									{
										Text = $"Card {i} - Scroll to see the effect",
										FontSize = 16
									}
								});
							}

							page.Content = new Microsoft.Maui.Controls.ScrollView { Content = scrollContent };
							await Navigation.PushAsync(page);
							
							// Restore opaque background when returning
							page.Disappearing += (s, e) => navPage.BarBackgroundColor = previousBarColor;
						})
					},

					// Back Button
					new BoxView { HeightRequest = 1, Color = Colors.Gray, Margin = new Thickness(0, 10) },
					new Button
					{
						Text = "← Back To Gallery",
						Margin = new Thickness(0, 10, 0, 20),
						Command = new Command(async () => await Navigation.PopAsync())
					}
				}
			}
		};

		Content = scrollView;
	}
}