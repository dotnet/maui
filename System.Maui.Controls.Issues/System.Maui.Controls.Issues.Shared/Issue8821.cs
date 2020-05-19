using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Image)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8821, "Animations of downloaded gifs are not playing on Android", PlatformAffected.Android)]
	public class Issue8821 : TestContentPage
	{
		Image _image;

		public Issue8821()
		{
			var instructions = new Label
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Press the DownloadFile button and then the Animate button. Verify that the gif is downloaded and animate without problems."
			};

			var downloadButton = new Button { Text = "DownloadFile" };
			downloadButton.Clicked += async (sender, args) =>
			{
				string nextURL = "https://upload.wikimedia.org/wikipedia/commons/c/c0/An_example_animation_made_with_Pivot.gif";

				await CreateImage(nextURL);
			};

			_image = new Image { Source = string.Empty };

			var animateButton = new Button { Text = "Animate" };
			animateButton.Clicked += (sender, args) =>
			{
				_image.IsAnimationPlaying = true;
			};

			Content = new StackLayout
			{
				Padding = new Thickness(20, 35, 20, 20),
				Children =
				{
					instructions,
					downloadButton,
					_image,
					animateButton
				}
			};
		}

		public string SecondImageSource { get; set; }

		protected override void Init()
		{
			Title = "Issue 8821";
		}

		async Task CreateImage(string imageUrl)
		{
			var bytes = await DownloadImageAsync(imageUrl);

			string path;

#if WINDOWS_UWP
			path = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
			path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#endif
			SecondImageSource = Path.Combine(path, "Issue8821.gif");
			File.WriteAllBytes(SecondImageSource, bytes);

			_image.Source = SecondImageSource;
			OnPropertyChanged(nameof(SecondImageSource));
		}

		async Task<byte[]> DownloadImageAsync(string imageUrl)
		{
			try
			{
				using (var httpClient = new HttpClient())
				using (var httpResponse = await httpClient.GetAsync(imageUrl))
				{
					if (httpResponse.StatusCode == HttpStatusCode.OK)
						return await httpResponse.Content.ReadAsByteArrayAsync();
					else
						return null;
				}
			}
			catch
			{
				return null;
			}
		}
	}
}