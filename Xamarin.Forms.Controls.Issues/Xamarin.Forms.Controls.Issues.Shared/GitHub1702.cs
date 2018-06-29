using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1702, "[Enhancement] Padding on Buttons", PlatformAffected.All)]
	public class GitHub1702 : TestContentPage
	{
		bool animation = false;

		protected override void Init()
		{
			StackLayout layout = new StackLayout()
			{
				Children =
				{
					new Button()
					{
						Image = "coffee.png",
						BackgroundColor = Color.GreenYellow,
						Text = "No padding? Height 100",
						HeightRequest = 100,
					},
					new Button()
					{
						Image = "coffee.png",
						BackgroundColor = Color.Green,
						Padding = new Thickness(100, 0, 0, 0),
						Text = "Do I have left padding? I should have left padding.",
					},
					new Button()
					{
						Image = "coffee.png",
						BackgroundColor = Color.LawnGreen,
						Padding = new Thickness(0, 30, 0, 0),
						Text = "Do I have top padding? I should have top padding."
					},
					new Button()
					{
						Image = "coffee.png",
						BackgroundColor = Color.LightGreen,
						Padding = new Thickness(0, 0, 100, 0),
						Text = "Do I have right padding? I should have right padding."
					},
					new Button()
					{
						Image = "coffee.png",
						BackgroundColor = Color.ForestGreen,
						Padding = new Thickness(0, 0, 0, 30),
						Text = "Do I have bottom padding? I should have bottom padding."
					}
				}
			};

			var buttons = layout.Children.OfType<Button>();
			layout.Children.Insert(0, ActionGrid(buttons.ToList()));
			PaddingAnimation(buttons).Start();

			Content = layout;
		}

		Grid ActionGrid(List<Button> buttons)
		{
			Button firstButton = buttons.FirstOrDefault();
			Grid actionGrid = new Grid();
			actionGrid.AddChild(new Button()
			{
				Text = "Reset text",
				Command = new Command(() =>
				{
					buttons.ForEach(b => b.Text = string.Empty);
				})
			}, 0, 0);
			actionGrid.AddChild(new Button()
			{
				Text = "Reset padding",
				Command = new Command(() =>
				{
					buttons.ForEach(b => b.ClearValue(Button.PaddingProperty));
				})
			}, 0, 1);
			actionGrid.AddChild(new Button()
			{
				Text = "Set text",
				Command = new Command(() =>
				{
					buttons.ForEach(b => b.Text = "Some text");
				})
			}, 1, 0);
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
					if (!String.IsNullOrWhiteSpace(button.Text))
						button.Text = $"Top: {button.Padding.Top} Bottom: {button.Padding.Bottom}";
				})
			}, 2, 0);
			actionGrid.AddChild(new Button()
			{
				Text = "Add Bottom",
				Command = new Command(() =>
				{
					var button = firstButton;
					button.Padding = new Thickness(0, button.Padding.Top, 0, button.Padding.Bottom + 10);
					if (!String.IsNullOrWhiteSpace(button.Text))
						button.Text = $"Top: {button.Padding.Top} Bottom: {button.Padding.Bottom}";
				})
			}, 2, 1);
			return actionGrid;
		}

		Thread PaddingAnimation(IEnumerable<Button> buttons)
		{
			return new Thread(() =>
			{
				int increment = 1;
				int current = 0;
				int max = 15;
				int FPS = 30;
				int sleep = 1000 / FPS;

				while (true)
				{
					Thread.Sleep(sleep);

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