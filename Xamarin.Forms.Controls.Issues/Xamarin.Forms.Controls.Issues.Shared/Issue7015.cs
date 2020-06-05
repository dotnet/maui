using System;
using System.Collections;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7015, "Test the placeholder implementation for Image control ")]
	public class Issue7015 : TestContentPage
	{
		const string InvalidImageFile = "batata.png";
		const string ErrorPlaceholderFile = "bank.png";
		const string LoadingImageFile = "bell.png";
		const string LegitImageFile = "coffee.png";

		const string CorrectUrl = "https://raw.githubusercontent.com/xamarin/Xamarin.Forms/master/Xamarin.Forms.ControlGallery.WindowsUniversal/FlowerBuds.jpg";
		const string WrongUrl = "http://avatars.githubusercontent.co/u/20712372?s=400&u=ecb5fe0584cba02ab4c7e159768e9366a95e3&v=4";
		readonly UriImageSource _correctUriSource = new UriImageSource { CachingEnabled = false, Uri = new Uri(CorrectUrl) };
		readonly UriImageSource _wrongUriSource = new UriImageSource { CachingEnabled = false, Uri = new Uri(WrongUrl) };

		Image _localImage;
		Image _remoteImage;

		protected override void Init()
		{
			var layout = new StackLayout
			{
				Padding = new Thickness(15),
				Spacing = 5
			};

			layout.Children.Add(Examples());
			layout.Children.Add(Toggle());

			layout.Children.Add(Local());
			layout.Children.Add(Remote());

			Content = layout;
		}

		View Row(string label, Image image)
		{
			var row = new StackLayout { Margin = new Thickness(0, 0, 0, 20), Orientation = StackOrientation.Horizontal };
			row.Children.Add(new Label { Text = label, VerticalTextAlignment = TextAlignment.Center });
			row.Children.Add(image);
			return row;
		}

		View Examples()
		{
			var exampleStack = new StackLayout { Margin = new Thickness(0, 0, 0, 20) };

			var exampleLegitLocalImage = new Image { Source = LegitImageFile };
			var exampleLegitRemoteImage = new Image { Source = CorrectUrl };
			var exampleErrorImage = new Image { Source = ErrorPlaceholderFile };
			var exampleLoadingImage = new Image { Source = LoadingImageFile };

			exampleStack.Children.Add(Row("Valid local image:", exampleLegitLocalImage));
			exampleStack.Children.Add(Row("Valid remote image:", exampleLegitRemoteImage));
			exampleStack.Children.Add(Row("Loading placeholder:", exampleLoadingImage));
			exampleStack.Children.Add(Row("Error placeholder:", exampleErrorImage));

			return exampleStack;
		}

		View Toggle() 
		{
			var toggleStack = new StackLayout { Orientation = StackOrientation.Horizontal };

			var currentSource = new Label
			{
				Text = $"Should be displaying the error placeholder",
				VerticalTextAlignment = TextAlignment.Center
			};

			var toggleButton = new Button { Text = "Toggle" };
			toggleButton.Command = new Command(() =>
			{
				var source = _localImage.Source.ToString();
				var invalid = source.Contains(InvalidImageFile);

				_remoteImage.Source = invalid ? _correctUriSource : _wrongUriSource;
				_localImage.Source = invalid ? LegitImageFile : InvalidImageFile;

				currentSource.Text = invalid 
					? "Should be displaying the valid images" 
					: "Should be displaying the error placeholders";
			});

			toggleStack.Children.Add(toggleButton);
			toggleStack.Children.Add(currentSource);

			return toggleStack;
		}

		View Local() 
		{
			var localImageStack = new StackLayout { Orientation = StackOrientation.Horizontal, BackgroundColor = Color.LightGreen };

			_localImage = new Image
			{
				ErrorPlaceholder = ErrorPlaceholderFile,
				LoadingPlaceholder = LoadingImageFile,
				Source = InvalidImageFile
			};

			var localImageLabel = new Label { Text = "Local image:" };

			localImageStack.Children.Add(localImageLabel);
			localImageStack.Children.Add(_localImage);

			return localImageStack;
		}

		View Remote()
		{
			var remoteStack = new StackLayout { Orientation = StackOrientation.Horizontal, BackgroundColor = Color.LightBlue };

			var urlImageLabel = new Label { Text = "Image from URL:" };

			_remoteImage = new Image
			{
				ErrorPlaceholder = ErrorPlaceholderFile,
				LoadingPlaceholder = LoadingImageFile,
				Source = _wrongUriSource
			};

			remoteStack.Children.Add(urlImageLabel);
			remoteStack.Children.Add(_remoteImage);

			return remoteStack;
		}
	}
}
