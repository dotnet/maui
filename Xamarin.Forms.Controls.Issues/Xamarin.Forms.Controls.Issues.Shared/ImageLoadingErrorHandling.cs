using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Image Loading Error Handling", PlatformAffected.WinRT)]
	public class ImageLoadingErrorHandling : TestContentPage
	{
		protected override void Init()
		{
			Log.Listeners.Add(
				new DelegateLogListener((c, m) => Device.BeginInvokeOnMainThread(() => DisplayAlert(c, m, "Cool, Thanks"))));

			var image = new Image() {BackgroundColor = Color.White};

			Grid legit = CreateTest(() => image.Source = ImageSource.FromFile("coffee.png"),
				"Valid Image",
				"Clicking this button should load an image at the top of the page.",
				Color.Silver);

			Grid invalidImageFileName = CreateTest(() => image.Source = ImageSource.FromFile("fake.png"),
				"Non-existent Image File",
				"Clicking this button should display an alert dialog with an error that the image failed to load.");

			Grid invalidImageFile = CreateTest(() => image.Source = ImageSource.FromFile("invalidimage.jpg"),
				"Invalid Image File (bad data)",
				"Clicking this button should display an alert dialog with an error that the image failed to load.",
				Color.Silver);

			Grid fakeUri = CreateTest(() => image.Source = ImageSource.FromUri(new Uri("http://not.real")),
				"Non-existent URI",
				Device.OS == TargetPlatform.Windows && Device.Idiom == TargetIdiom.Phone 
				? "Clicking this button should display an alert dialog. The error message should include the text 'NotFound'."
				: "Clicking this button should display an alert dialog. The error message should include the text 'the server name or address could not be resolved'.");

			// This used to crash the app with an uncatchable error; need to make sure it's not still doing that
			Grid crashImage = CreateTest(() => image.Source = new FailImageSource(),
				"Source Throws Exception",
				"Clicking this button should display an alert dialog. The error messages hould include the test 'error updating image source'.",
				Color.Silver);

			Grid uriInvalidImageData =
				CreateTest(() => image.Source = ImageSource.FromUri(new Uri("https://gist.githubusercontent.com/hartez/a2dda6b5c78852bcf4832af18f21a023/raw/39f4cd2e9fe8514694ac7fa0943017eb9308853d/corrupt.jpg")),
					"Valid URI with invalid image file",
					"Clicking this button should display an alert dialog. The error message should include the text 'UriImageSourceHandler could not load https://gist.githubusercontent.com/hartez/a2dda6b5c78852bcf4832af18f21a023/raw/39f4cd2e9fe8514694ac7fa0943017eb9308853d/corrupt.jpg'");

			Content = new StackLayout
			{
				Children =
				{
					image,
					legit,
					invalidImageFileName,
					invalidImageFile,
					fakeUri,
					crashImage,
					uriInvalidImageData
				}
			};
		}

		static Grid CreateTest(Action imageLoadAction, string title, string instructions, Color? backgroundColor = null)
		{
			var button = new Button { Text = "Test" };

			button.Clicked += (sender, args) => imageLoadAction();

			var titleLabel = new Label
			{
				Text = title,
				FontAttributes = FontAttributes.Bold
			};

			var label = new Label
			{
				Text = instructions
			};

			var grid = new Grid
			{
				ColumnDefinitions =
					new ColumnDefinitionCollection { new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition() },
				RowDefinitions = new RowDefinitionCollection { new RowDefinition { Height = 80 } }
			};

			if (backgroundColor.HasValue)
			{
				grid.BackgroundColor = backgroundColor.Value;
			}

			grid.AddChild(titleLabel, 0, 0);
			grid.AddChild(label, 1, 0);
			grid.AddChild(button, 2, 0);

			return grid;
		}
	}
}