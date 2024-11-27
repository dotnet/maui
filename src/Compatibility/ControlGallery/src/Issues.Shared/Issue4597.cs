﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4597, "[Android] ImageCell not loading images and setting ImageSource to null has no effect",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(UITestCategories.Image)]
	[NUnit.Framework.Category(UITestCategories.ListView)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	public class Issue4597 : TestContentPage
	{
		ImageButton _imageButton;
		Button _button;
		Image _image;
		ListView _listView;

		string _disappearText = "You should see an Image. Clicking this should cause the image to disappear";
		string _appearText = "Clicking this should cause the image to reappear";
		string _theListView = "theListViewAutomationId";
		string _fileName = "xamarinlogo.png";
		string _fileNameAutomationId = "CoffeeAutomationId";
		string _uriImage = "https://github.com/xamarin/Xamarin.Forms/blob/3216ce4ccd096f8b9f909bbeea572dcf2a8c4466/Microsoft.Maui.Controls.ControlGallery.iOS/Resources/xamarinlogo.png?raw=true";
		bool _isUri = false;
		string _nextTestId = "NextTest";
		string _activeTestId = "activeTestId";
		string _switchUriId = "SwitchUri";
		string _imageFromUri = "Image From Uri";
		string _imageFromFile = "Image From File";

		protected override void Init()
		{
			Label labelActiveTest = new Label()
			{
				AutomationId = _activeTestId
			};

			_image = new Image() { Source = _fileName, AutomationId = _fileNameAutomationId };
			_button = new Button() { ImageSource = _fileName, AutomationId = _fileNameAutomationId };
			_imageButton = new ImageButton() { Source = _fileName, AutomationId = _fileNameAutomationId };
			_listView = new ListView()
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var cell = new ImageCell();
					cell.SetBinding(ImageCell.ImageSourceProperty, ".");
					cell.AutomationId = _fileNameAutomationId;
					return cell;
				}),
				AutomationId = _theListView,
				ItemsSource = new[] { _fileName },
				HasUnevenRows = true,
				BackgroundColor = Colors.Purple
			};

			View[] imageControls = new View[] { _image, _button, _imageButton, _listView };

			Button button = null;
			button = new Button()
			{
				AutomationId = "ClickMe",
				Text = _disappearText,
				Command = new Command(() =>
				{
					if (button.Text == _disappearText)
					{
						_image.Source = null;
						_button.ImageSource = null;
						_imageButton.Source = null;
						_listView.ItemsSource = new string[] { null };
						Device.BeginInvokeOnMainThread(() => button.Text = _appearText);
					}
					else
					{
						_image.Source = _isUri ? _uriImage : _fileName;
						_button.ImageSource = _isUri ? _uriImage : _fileName;
						_imageButton.Source = _isUri ? _uriImage : _fileName;
						_listView.ItemsSource = new string[] { _isUri ? _uriImage : _fileName };
						Device.BeginInvokeOnMainThread(() => button.Text = _disappearText);
					}
				})
			};

			var switchToUri = new Switch
			{
				AutomationId = _switchUriId,
				IsToggled = false,
				HeightRequest = 60
			};
			var sourceLabel = new Label { Text = _imageFromFile, AutomationId = "SourceLabel" };

			switchToUri.Toggled += (_, e) =>
			{
				_isUri = e.Value;

				// reset the images to visible
				button.Text = _appearText;
				button.SendClicked();

				if (_isUri)
					sourceLabel.Text = _imageFromUri;
				else
					sourceLabel.Text = _imageFromFile;
			};


			foreach (var view in imageControls)
			{
				view.BackgroundColor = Colors.Red;
			}

			StackLayout layout = null;
			layout = new StackLayout()
			{
				AutomationId = "layoutContainer",
				Children =
				{
					new StackLayout()
					{
						Orientation = StackOrientation.Horizontal,
						Children =
						{
							labelActiveTest,
							switchToUri,
							sourceLabel
						}
					},
					button,
					new Button()
					{
						Text = "Load Next Image Control to Test",
						Command = new Command(() =>
						{
							var activeImage = (View)layout.Children.Last();
							int nextIndex = imageControls.ToList().IndexOf(activeImage) + 1;

							if(nextIndex >= imageControls.Length)
								nextIndex = 0;

							layout.Children.Remove(activeImage);
							layout.Children.Add(imageControls[nextIndex]);
							labelActiveTest.Text = imageControls[nextIndex].GetType().Name;

							// reset the images to visible
							button.Text = _appearText;
							button.SendClicked();
						}),
						AutomationId = _nextTestId
					},
					imageControls[0]
				}
			};

			Content = layout;
			labelActiveTest.Text = imageControls[0].GetType().Name;
		}
#if UITEST

#if !WINDOWS
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void ImageFromFileSourceAppearsAndDisappearsCorrectly()
		{
			RunTest(nameof(Image), true);
		}

		[Test]
		[NUnit.Framework.Category(UITestCategories.RequiresInternetConnection)]
		[Compatibility.UITests.FailsOnMauiIOS]
		[Compatibility.UITests.FailsOnMauiAndroid]
		public void ImageFromUriSourceAppearsAndDisappearsCorrectly()
		{
			RunTest(nameof(Image), false);
		}


		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void ButtonFromFileSourceAppearsAndDisappearsCorrectly()
		{
			RunTest(nameof(Button), true);
		}

		[Test]
		[NUnit.Framework.Category(UITestCategories.RequiresInternetConnection)]
		[Compatibility.UITests.FailsOnMauiIOS]
		[Compatibility.UITests.FailsOnMauiAndroid]
		public void ButtonFromUriSourceAppearsAndDisappearsCorrectly()
		{
			RunTest(nameof(Button), false);
		}


		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void ImageButtonFromFileSourceAppearsAndDisappearsCorrectly()
		{
			RunTest(nameof(ImageButton), true);
		}

		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		[Compatibility.UITests.FailsOnMauiAndroid]
		[NUnit.Framework.Category(UITestCategories.RequiresInternetConnection)]
		public void ImageButtonFromUriSourceAppearsAndDisappearsCorrectly()
		{
			RunTest(nameof(ImageButton), false);
		}

		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void ImageCellFromFileSourceAppearsAndDisappearsCorrectly()
		{
			ImageCellTest(true);
		}

		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		[Compatibility.UITests.FailsOnMauiAndroid]
		public void ImageCellFromUriSourceAppearsAndDisappearsCorrectly()
		{
			ImageCellTest(false);
		}

		void ImageCellTest(bool fileSource)
		{
			string className = "ImageView";
			SetupTest(nameof(ListView), fileSource);

			var imageVisible =
				RunningApp.QueryUntilPresent(GetImage, 10, 2000);

			Assert.AreEqual(1, imageVisible.Length);
			SetImageSourceToNull();

			imageVisible = GetImage();
			Assert.AreEqual(0, imageVisible.Length);

			Xamarin.UITest.Queries.AppResult[] GetImage()
			{
				return RunningApp
					.Query(app => app.Marked(_theListView).Descendant())
					.Where(x => x.Class != null && x.Class.Contains(className)).ToArray();
			}
		}
#endif

		void RunTest(string testName, bool fileSource)
		{
			SetupTest(testName, fileSource);
			var foundImage = TestForImageVisible();
			SetImageSourceToNull();
			TestForImageNotVisible(foundImage);
		}


		void SetImageSourceToNull()
		{
			RunningApp.Tap("ClickMe");
			RunningApp.WaitForElement(_appearText);
		}

		Xamarin.UITest.Queries.AppResult TestForImageVisible()
		{
			var images = RunningApp.QueryUntilPresent(() =>
			{
				var result = RunningApp.WaitForElement(_fileNameAutomationId);

				if (result[0].Rect.Height > 1)
					return result;

				return Array.Empty<Xamarin.UITest.Queries.AppResult>();
			}, 10, 4000);

			Assert.AreEqual(1, images.Length);
			var imageVisible = images[0];

			Assert.Greater(imageVisible.Rect.Height, 1);
			Assert.Greater(imageVisible.Rect.Width, 1);
			return imageVisible;
		}

		void TestForImageNotVisible(Xamarin.UITest.Queries.AppResult previousFinding)
		{
			var imageVisible = RunningApp.Query(_fileNameAutomationId);

			if (imageVisible.Length > 0)
			{
				Assert.Less(imageVisible[0].Rect.Height, previousFinding.Rect.Height);
			}
		}

		void SetupTest(string controlType, bool fileSource)
		{
			RunningApp.WaitForElement(_nextTestId);
			string activeTest = null;
			while (RunningApp.Query(controlType).Length == 0)
			{
				activeTest = RunningApp.WaitForElement(_activeTestId)[0].ReadText();
				RunningApp.Tap(_nextTestId);
				RunningApp.WaitForNoElement(activeTest);
			}

			string sourceLabel = RunningApp.WaitForFirstElement("SourceLabel").ReadText();
			if (fileSource && sourceLabel != _imageFromFile)
				RunningApp.Tap(_switchUriId);
			else if (!fileSource && sourceLabel != _imageFromUri)
				RunningApp.Tap(_switchUriId);
		}
#endif
	}
}