using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 38770, "RaiseChild and LowerChild do not work on Windows", PlatformAffected.WinRT)]
	public class Bugzilla38770 : TestContentPage
	{
		StackLayout _boxStack;
		Label _colorsPositionLabel;

		protected override void Init()
		{
			var red = new BoxView
			{
				BackgroundColor = Color.Red,
				WidthRequest = 50,
				HeightRequest = 50,
				TranslationX = 25
			};
			var green = new BoxView
			{
				BackgroundColor = Color.Green,
				WidthRequest = 50,
				HeightRequest = 50
			};
			var blue = new BoxView
			{
				BackgroundColor = Color.Blue,
				WidthRequest = 50,
				HeightRequest = 50,
				TranslationX = -25
			};
			_boxStack = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Spacing = 0,
				Margin = new Thickness(0, 50, 0, 0),
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					red,
					green,
					blue
				}
			};
			_boxStack.ChildrenReordered += BoxStackOnChildrenReordered;

			var raiseButtons = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Button
					{
						Text = "Raise Red",
						WidthRequest = 110,
						Command = new Command(() => _boxStack.RaiseChild(red))
					},
					new Button
					{
						Text = "Raise Green",
						WidthRequest = 110,
						Command = new Command(() => _boxStack.RaiseChild(green))
					},
					new Button
					{
						Text = "Raise Blue",
						WidthRequest = 110,
						Command = new Command(() => _boxStack.RaiseChild(blue))
					}
				}
			};
			var lowerButtons = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Button
					{
						Text = "Lower Red",
						WidthRequest = 110,
						Command = new Command(() => _boxStack.LowerChild(red))
					},
					new Button
					{
						Text = "Lower Green",
						WidthRequest = 110,
						Command = new Command(() => _boxStack.LowerChild(green))
					},
					new Button
					{
						Text = "Lower Blue",
						WidthRequest = 110,
						Command = new Command(() => _boxStack.LowerChild(blue))
					}
				}
			};

			_colorsPositionLabel = new Label
			{
				FormattedText = new FormattedString()
			};
			FormatColorsChildrenPositionText();

			var colorsPositionStack = new StackLayout()
			{
				Margin = new Thickness(0, 50, 0, 0),
				Children =
				{
					new Label()
					{
						Text = "Colors collection order (i.e. z-index)"
					},
					_colorsPositionLabel
				}
			};

			var instructions = new StackLayout()
			{
				Margin = new Thickness(0, 50, 0, 0),
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label()
					{
						Text = "When LOWERING an item it's being moved to the START of the collection of children, therefore decreasing it's z-index",
						FontSize = 15
					},
					new Label()
					{
						Text = "When RAISING an item it's being moved to the END of the list of children, therefore increasing its z-index",
						FontSize = 15
					},
					new Label()
					{
						Text = "For instance, if you decide to press LOWER GREEN button, then the GREEN color should no longer be visible - it will become the first item in the list (lowest z-index) and therefore it will get covered by RED and BLUE.",
						Margin = new Thickness(0, 10, 0, 0)
					}
				}
			};

			Content = new StackLayout
			{
				Children =
				{
					raiseButtons,
					lowerButtons,
					_boxStack,
					colorsPositionStack,
					instructions
				}
			};
		}

		void FormatColorsChildrenPositionText()
		{
			_colorsPositionLabel.FormattedText.Spans.Clear();
			for (var i = 0; i < _boxStack.Children.Count; i++)
			{
				if (_boxStack.Children[i].BackgroundColor.R > 0)
				{
					_colorsPositionLabel.FormattedText.Spans.Add(new Span()
					{
						Text = $"{i} Red\n"
					});
					continue;
				}

				if (_boxStack.Children[i].BackgroundColor.G > 0)
				{
					_colorsPositionLabel.FormattedText.Spans.Add(new Span()
					{
						Text = $"{i} Green\n"
					});
					continue;
				}

				_colorsPositionLabel.FormattedText.Spans.Add(new Span()
				{
					Text = $"{i} Blue\n"
				});
			}
		}

		void BoxStackOnChildrenReordered(object sender, EventArgs e)
		{
			FormatColorsChildrenPositionText();
		}
	}
}