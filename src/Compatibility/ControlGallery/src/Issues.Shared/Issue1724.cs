﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1724, "[Enhancement] ImageButton", PlatformAffected.All)]
	public class Issue1724 : TestContentPage
	{
		bool animation = false;

		protected override void Init()
		{
			int sizes = 200;
			var radius = new ImageButton()
			{
				BorderColor = Colors.Brown,
				BorderWidth = 5,
				BackgroundColor = Colors.Yellow,
				Aspect = Aspect.Fill,
				CornerRadius = 10,
				Source = "coffee.png",
				HorizontalOptions = LayoutOptions.Center,
				HeightRequest = sizes,
				WidthRequest = sizes,

			};

			radius.On<Android>().SetShadowColor(Colors.Green);
			radius.On<Android>().SetIsShadowEnabled(true);
			radius.On<Android>().SetShadowOffset(new Size(25, 25));

			var radiusBackground = new ImageButton()
			{
				BorderColor = Colors.Brown,
				BorderWidth = 5,
				Source = "coffee.png",
				CornerRadius = 10,
				HorizontalOptions = LayoutOptions.Center,
				BackgroundColor = Colors.Pink,
				HeightRequest = sizes,
				WidthRequest = sizes
			};

			radiusBackground.On<Android>().SetShadowColor(Colors.Green);
			radiusBackground.On<Android>().SetIsShadowEnabled(true);
			radiusBackground.On<Android>().SetShadowOffset(new Size(5, 5));


			StackLayout layout = new StackLayout()
			{
				Children =
				{
					new Label(){ Text = "No padding?" },
					new ImageButton()
					{
						Source = "coffee.png",
						BackgroundColor = Colors.GreenYellow
					},
					new Label(){ Text = "Do I have left padding? I should have left padding." },
					new ImageButton()
					{
						Source = "coffee.png",
						BackgroundColor = Colors.Green,
						Padding = new Thickness(100, 0, 0, 0)
					},
					new Label(){ Text = "Do I have top padding? I should have top padding." },
					new ImageButton()
					{
						Source = "coffee.png",
						BackgroundColor = Colors.LawnGreen,
						Padding = new Thickness(0, 30, 0, 0)
					},
					new Label(){ Text = "Do I have right padding? I should have right padding."},
					new ImageButton()
					{
						Source = "coffee.png",
						BackgroundColor = Colors.LightGreen,
						Padding = new Thickness(0, 0, 100, 0)
					},
					new Label(){ Text = "Do I have bottom padding? I should have bottom padding." },
					new ImageButton()
					{
						Source = "coffee.png",
						BackgroundColor = Colors.ForestGreen,
						Padding = new Thickness(0, 0, 0, 30)
					},
					new Label(){ Text = "Do you see image from a Uri?" },
					new ImageButton()
					{
						Source = "https://raw.githubusercontent.com/xamarin/Xamarin.Forms/main/Microsoft.Maui.Controls.ControlGallery/coffee.png",
						BackgroundColor = Colors.ForestGreen
					},
					new Label(){ Text = "Invalid Image Uri just to test it doesn't crash" },
					new ImageButton()
					{
						Source = "http://xamarin.com/imginvalidf@#$R(P&fb.png",
						BackgroundColor = Colors.ForestGreen
					},
					new Label(){ Text = "Aspect: Aspect.Fill with shadows" },
					radius,
					new Label(){ Text = "Aspect: Aspect.AspectFit with shadows" },
					radiusBackground,
					new Label(){ Text = "BorderColor:Color.Green, BorderWidth:10" },
					new ImageButton()
					{
						Source = "coffee.png",
						HorizontalOptions = LayoutOptions.Center,
						HeightRequest = sizes,
						WidthRequest = sizes,
						BorderColor = Colors.Green,
						BorderWidth = 10
					},
					new Label(){ Text = "BorderColor:Color.Green, BorderWidth:10, Aspect:Aspect.Fill" },
					new ImageButton()
					{
						Source = "coffee.png",
						HorizontalOptions = LayoutOptions.Center,
						HeightRequest = sizes,
						WidthRequest = sizes,
						BorderColor = Colors.Green,
						BorderWidth = 10,
						Aspect = Aspect.Fill
					},
					new Label(){ Text = "BackgroundColor:Green" },
					new ImageButton()
					{
						Source = "coffee.png",
						HorizontalOptions = LayoutOptions.Center,
						HeightRequest = sizes,
						WidthRequest = sizes,
						BackgroundColor = Colors.Green
					},
					new Label(){ Text = "BorderWidth: 5, CornerRadius:10, BorderColor:Brown" },
					new ImageButton()
					{
						BorderColor = Colors.Brown,
						BorderWidth = 5,
						Source = "coffee.png",
						CornerRadius = 10,
						HorizontalOptions = LayoutOptions.Center,
						HeightRequest = sizes,
						WidthRequest = sizes
					},
					new Label(){ Text = $"CornerRadius:{sizes / 2}, BorderWidth: 5, BorderColor:Red" },
					new ImageButton()
					{
						Source = "coffee.png",
						HorizontalOptions = LayoutOptions.Center,
						BorderColor = Colors.Red,
						BorderWidth = 5,
						CornerRadius = sizes / 2,
						HeightRequest = sizes,
						WidthRequest = sizes
					},
				}
			};

			var buttons = layout.Children.OfType<ImageButton>();
			layout.Children.Insert(0, ActionGrid(buttons.ToList()));
			PaddingAnimation(buttons);

			Content = new ScrollView() { Content = layout };
		}

		Grid ActionGrid(List<ImageButton> buttons)
		{
			ImageButton firstButton = buttons.FirstOrDefault();
			Grid actionGrid = new Grid();
			actionGrid.AddChild(new Button()
			{
				Text = "Add Right",
				Command = new Command(() =>
				{
					var button = firstButton;
					button.Padding = new Thickness(button.Padding.Left, 0, button.Padding.Right + 10, 0);
				})
			}, 0, 0);
			actionGrid.AddChild(new Button()
			{
				Text = "Add Left",
				Command = new Command(() =>
				{
					var button = firstButton;
					button.Padding = new Thickness(button.Padding.Left + 10, 0, button.Padding.Right, 0);
				})
			}, 0, 1);

			actionGrid.AddChild(new Button()
			{
				Text = "Animation",
				Command = new Command(() => animation = !animation)
			}, 1, 1);
			actionGrid.AddChild(new Button()
			{
				Text = "Add Top",
				Command = new Command(() =>
				{
					var button = firstButton;
					button.Padding = new Thickness(0, button.Padding.Top + 10, 0, button.Padding.Bottom);
				})
			}, 2, 0);
			actionGrid.AddChild(new Button()
			{
				Text = "Add Bottom",
				Command = new Command(() =>
				{
					var button = firstButton;
					button.Padding = new Thickness(0, button.Padding.Top, 0, button.Padding.Bottom + 10);
				})
			}, 2, 1);
			return actionGrid;
		}

		void PaddingAnimation(IEnumerable<ImageButton> buttons)
		{
			Task.Run(async () =>
			{
				int increment = 1;
				int current = 0;
				int max = 15;
				int FPS = 30;
				int sleep = 1000 / FPS;

				while (true)
				{
					await Task.Delay(sleep);

					if (!animation)
						continue;

					current += increment;
					if (current > max || current < 0)
					{
						increment *= -1;
						current += increment * 2;
					}

					Device.BeginInvokeOnMainThread(() =>
					{
						foreach (var button in buttons)
						{
							var padding = button.Padding;
							button.Padding = padding = new Thickness(
								padding.Left + increment,
								padding.Top + increment,
								padding.Right + increment,
								padding.Bottom + increment);
						}
					});
				}
			});
		}
	}
}