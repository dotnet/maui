using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1593, "Android reports zero minimum size for almost all controls", PlatformAffected.Android)]
	public class Issue1593 : ContentPage
	{
		public Issue1593 ()
		{
			var title = new Label {
				Text = "Select League",
#pragma warning disable 618
				Font = Font.SystemFontOfSize (NamedSize.Large),
#pragma warning restore 618
				TextColor = Color.White
			};

			# region Season Filter

			var seasonLabel = new Label {
				Text = "Season",
#pragma warning disable 618
				Font = Font.SystemFontOfSize (NamedSize.Small),
#pragma warning restore 618
				TextColor = Color.White
			};

			var seasonPicker = new Picker {
				WidthRequest = 140,
				Title = "Season",
				Items = { "Test 1", "Test 2"}
			};

			var seasonPanel = new StackLayout {
				Children = {
					seasonLabel,
					seasonPicker
				}
			};

			#endregion

			# region Sport Filter

			var sportLabel = new Label {
				Text = "Sport",
#pragma warning disable 618
				Font = Font.SystemFontOfSize (NamedSize.Small),
#pragma warning restore 618
				TextColor = Color.White
			};

			var sportPicker = new Picker {
				WidthRequest = 140,
				Title = "Sport",
				Items = { "Test 1", "Test 2"}
			};

			var sportPanel = new StackLayout {
				Children = {
					sportLabel,
					sportPicker
				}
			};

			#endregion

			var filtersPanel = new StackLayout {
				Padding = new Thickness (0, 10, 0, 0),
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Fill,
				Children = {
					seasonPanel,
					sportPanel
				}
			};

			var leagues = new ListView {
				MinimumHeightRequest = 100,
				ItemsSource = new [] {
					"Test 1",
					"Test 2",
					"Test 3",
					"Test 4",
					"Test 5",
					"Test 6",
					"Test 7",
					"Test 8",
					"Test 9",
					"Test 10",
					"Test 11",
					"Test 12",
					"Test 13",
					"Test 14",
					"Test 15",
				},
				BackgroundColor = Color.Gray,
				ItemTemplate = new DataTemplate (() => {
					var leagueName = new Label {
#pragma warning disable 618
						Font = Font.SystemFontOfSize (NamedSize.Large),
#pragma warning restore 618
						BackgroundColor = Color.Transparent,
						TextColor = Color.White,
						VerticalOptions = LayoutOptions.CenterAndExpand,
						LineBreakMode = LineBreakMode.WordWrap
					};
					leagueName.SetBinding (Label.TextProperty, ".");

					var row = new StackLayout {
						Padding = new Thickness (5, 0, 5, 0),
						BackgroundColor = Color.Transparent,
						Orientation = StackOrientation.Horizontal,
						VerticalOptions = LayoutOptions.CenterAndExpand,
						Children = {
							leagueName
						}
					};

					return new ViewCell {
						View = row
					};
				})
			};

			var activityIndicator = new ActivityIndicator {
				VerticalOptions =
					LayoutOptions.CenterAndExpand,
					IsVisible = false
			};

			var titlePanel = new StackLayout {
				Orientation = StackOrientation.Horizontal,
				Spacing = 50,
				Children = {
					title,
					activityIndicator
				}
			};

			var standingsButton = new Button {
				Text = "Standings",
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};

			var scheduleButton = new Button {
				Text = "Schedule",
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};

			var buttonPanel = new StackLayout {
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				Children = {
					standingsButton,
					scheduleButton
				}
			};

			Content = new StackLayout {
				Padding = 20,
				Children = {
					titlePanel,
					filtersPanel,
					leagues,
					buttonPanel
				}
			};
		}
	}
}
