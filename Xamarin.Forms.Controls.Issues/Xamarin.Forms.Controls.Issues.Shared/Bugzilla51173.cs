using System;
using System.Globalization;
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
	[Issue(IssueTracker.Bugzilla, 51173, "ImageRenderer, async void SetImage - Cannot catch exceptions", PlatformAffected.All)]
	public class Bugzilla51173 : TestContentPage
	{
#if UITEST
		[Test(Description = "Attempt to load an image from a URI which does not exist")]
		public void Bugzilla51173_NonexistentUri()
		{
			
			RunningApp.WaitForElement(q => q.Marked(UriDoesNotExist));

			RunningApp.Tap(UriDoesNotExist);
			RunningApp.WaitForElement(q => q.Marked(ErrorLogged));
			RunningApp.WaitForElement(q => q.Marked(NotLoading));
		}

		[Test(Description = "The IImageSourceHandler itself throws an exception")]
		public void Bugzilla51173_HandlerThrowsException()
		{
			RunningApp.WaitForElement(q => q.Marked(HandlerThrows));
			
			RunningApp.Tap(HandlerThrows);
			RunningApp.WaitForElement(q => q.Marked(ErrorLogged));
			RunningApp.WaitForElement(q => q.Marked(NotLoading));
		}

		[Test(Description = "The URI is valid, but the image data is not")]
		public void Bugzilla51173_RealUriWithInvalidImageData()
		{
			RunningApp.WaitForElement(q => q.Marked(RealUriInvalidImage));

			RunningApp.Tap(RealUriInvalidImage);
			RunningApp.WaitForElement(q => q.Marked(ErrorLogged));
			RunningApp.WaitForElement(q => q.Marked(NotLoading));
		}

		[Test(Description = "Attempt to load a local image which does not exist")]
		public void Bugzilla51173_NonexistentImage()
		{
			RunningApp.WaitForElement(q => q.Marked(ImageDoesNotExist));

			RunningApp.Tap(ImageDoesNotExist);
			RunningApp.WaitForElement(q => q.Marked(ErrorLogged));
			RunningApp.WaitForElement(q => q.Marked(NotLoading));
		}

		[Test(Description = "Attempt to load a local image which exists, but has corrupt data")]
		public void Bugzilla51173_InvalidImage()
		{
			RunningApp.WaitForElement(q => q.Marked(ImageIsInvalid));

			RunningApp.Tap(ImageIsInvalid);
			RunningApp.WaitForElement(q => q.Marked(ErrorLogged));
			RunningApp.WaitForElement(q => q.Marked(NotLoading));
		}

		[Test(Description = "Load a valid image")]
		public void Bugzilla51173_ValidImage()
		{
			RunningApp.WaitForElement(q => q.Marked(ValidImage));
			RunningApp.Tap(ValidImage);
			RunningApp.WaitForElement(q => q.Marked(NotLoading));
		}
#endif

		const string ValidImage = "Valid Image";
		const string ImageDoesNotExist = "Non-existent Image File";
		const string ImageIsInvalid = "Invalid Image File (bad data)";
		const string UriDoesNotExist = "Non-existent URI";
		const string HandlerThrows = "Source Throws Exception";
		const string RealUriInvalidImage = "Valid URI with invalid image file";
		const string ErrorLogged = "Error logged";

		const string Loading = "Loading";
		const string NotLoading = "Not Loading";

		Label _results;
		Image _image;

		class LoadingConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if ((bool)value)
				{
					return Loading;
				}

				return NotLoading;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				throw new NotImplementedException();
			}
		}

		protected override void Init()
		{
			_results = new Label { Margin = 10, FontAttributes = FontAttributes.Bold, BackgroundColor = Color.Silver, HorizontalTextAlignment = TextAlignment.Center};

			var errorMessage = new Label();

			Log.Listeners.Add(
				new DelegateLogListener((c, m) => Device.BeginInvokeOnMainThread(() =>
				{
					_results.Text = ErrorLogged;
					errorMessage.Text = m;
				})));
			
			var instructions = new Label
			{
				Text =
					"Pressing the 'Valid Image' button should display an image of a coffee cup. Every other button should cause the messager 'Error logged' to appear at the top of the page."
			};

			_image = new Image { BackgroundColor = Color.Bisque, HeightRequest = 20 };

			var loadingState = new Label();
			loadingState.SetBinding(Label.TextProperty, new Binding(Image.IsLoadingProperty.PropertyName, BindingMode.Default, new LoadingConverter()));
			loadingState.BindingContext = _image;

			var legit = CreateTest(() => _image.Source = ImageSource.FromFile("coffee.png"), ValidImage);

			var invalidImageFileName = CreateTest(() => _image.Source = ImageSource.FromFile("fake.png"), ImageDoesNotExist); 

			var invalidImageFile = CreateTest(() => _image.Source = ImageSource.FromFile("invalidimage.jpg"), ImageIsInvalid);

			var fakeUri = CreateTest(() => _image.Source = ImageSource.FromUri(new Uri("http://not.real")), UriDoesNotExist);

			var crashImage = CreateTest(() => _image.Source = new FailImageSource(), HandlerThrows);

			var uriInvalidImageData =
				CreateTest(() => _image.Source = ImageSource.FromUri(new Uri("https://gist.githubusercontent.com/hartez/a2dda6b5c78852bcf4832af18f21a023/raw/39f4cd2e9fe8514694ac7fa0943017eb9308853d/corrupt.jpg")),
					RealUriInvalidImage);

			Content = new StackLayout
			{
				Margin = new Thickness(5, 20, 5, 0),
				Children =
				{
					_results,
					loadingState,
					_image,
					instructions,
					legit,
					invalidImageFileName,
					invalidImageFile,
					fakeUri,
					crashImage,
					uriInvalidImageData,
					errorMessage
				}
			};
		}

		Button CreateTest(Action imageLoadAction, string title)
		{
			var button = new Button { Text = title };

			button.Clicked += (sender, args) =>
			{
				_results.Text = "";
				imageLoadAction();
			};

			return button;
		}
	}
}