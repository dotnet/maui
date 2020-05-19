using System;
using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Maui.PlatformConfiguration;
using System.Maui.PlatformConfiguration.AndroidSpecific;

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7015, "Test the placeholder implementation for Image control ")]
	public class Issue7015 : TestContentPage
	{
		const string batata = "batata.png";
		const string bank = "bank.png";
		const string bell = "bell.png";
		const string fromSource = "ImageSource";
		const string fromPlaceholder = "Placeholder";
		const string correctUrl = "https://media.licdn.com/dms/image/C4E03AQEWNdL1Usduag/profile-displayphoto-shrink_200_200/0?e=1579132800&v=beta&t=4f9CONMr5lqdT7KFq_wp5SSq__mxxaQpW_HxLq3JKYM";
		const string wrongUrl = "http://avatars.githubusercontent.co/u/20712372?s=400&u=ecb5fe0584cba02ab4c7e159768e9366a95e3&v=4";

		protected override void Init()
		{
			var label = new Label
			{
				Text = "Press the image button and change the Source, from Image above. See the label at the end of the page to see if it's the PlaceholderImage or the ImageSource",
				VerticalOptions = LayoutOptions.Start
			};

			var image = new Image
			{
				ErrorPlaceholder = bank,
				Source = batata
			};

			var correctUriSource = new UriImageSource { CachingEnabled = false, Uri = new Uri(correctUrl) };
			var wrongUriSource = new UriImageSource { CachingEnabled = false, Uri = new Uri(wrongUrl) };

			var urlImage = new Image
			{
				ErrorPlaceholder = bank,
				Source = wrongUriSource,
				LoadingPlaceholder = bell
			};

			var sourceIs = new Label
			{
				Text = fromPlaceholder,
				VerticalOptions = LayoutOptions.EndAndExpand,
				HorizontalTextAlignment = TextAlignment.Center
			};

			var imageB = new ImageButton
			{
				Source = bank,
				Command = new Command(() =>
				{
					var source = urlImage.Source.ToString();
					urlImage.Source = (source.Contains(wrongUrl)) ? correctUriSource : wrongUriSource;

					source = image.Source.ToString();
					image.Source = (source.Contains(batata)) ? bell : batata;
					source = image.Source.ToString();
					sourceIs.Text = (source.Contains(batata)) ? fromPlaceholder : fromSource;
				})
			};

			var stack = new StackLayout
			{
				Padding = new Thickness(15)
			};

			stack.Children.Add(label);
			stack.Children.Add(image);
			stack.Children.Add(imageB);
			stack.Children.Add(urlImage);
			stack.Children.Add(sourceIs);

			Content = stack;
		}
	}
}
