using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4597, "[Android] ImageCell not loading images and setting ImageSource to null has no effect",
		PlatformAffected.Android)]
#if UITEST
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

		string _disappearText = "You should see 4 images. Clicking this should cause the images to all disappear";
		string _appearText = "Clicking this should cause the images to all appear";
		string _theListView = "theListViewAutomationId";
		string _fileName = "coffee.png";
		string _uriImage = "https://raw.githubusercontent.com/xamarin/Xamarin.Forms/master/Xamarin.Forms.Controls/coffee.png";
		bool _isUri = false;

		protected override void Init()
		{
			_image = new Image() { Source = _fileName, AutomationId = _fileName, ClassId = "Something" };
			_button = new Button() { ImageSource = _fileName, AutomationId = _fileName };
			_imageButton = new ImageButton() { Source = _fileName, AutomationId = _fileName };
			_listView = new ListView()
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var cell = new ImageCell();
					cell.SetBinding(ImageCell.ImageSourceProperty, ".");
					return cell;
				}),
				AutomationId = _theListView,
				ItemsSource = new[] { _fileName },
				HasUnevenRows = true,
				BackgroundColor = Color.Purple
			};

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
				AutomationId = "SwitchUri",
				IsToggled = false
			};
			switchToUri.Toggled += (_, e) => _isUri =	e.Value;
			var switchWithCaption = new Grid() { HeightRequest = 60 };
			switchWithCaption.AddChild(new Label { Text = "Image From Url" }, 0, 0);
			switchWithCaption.AddChild(switchToUri, 1, 0);

			var layout = new StackLayout()
			{
				Children =
				{
					button,
					switchWithCaption,
					_image,
					_button,
					_imageButton,
					_listView,
				}
			};

			Content = layout;
		}
#if UITEST
		[Test]
		public void TestImagesDisappearCorrectly()
		{
			RunningApp.WaitForElement(_fileName);
			var elementsBefore = RunningApp.WaitForElement(_fileName);
#if !__WINDOWS__
			var imageCell = RunningApp.Query(app => app.Marked(_theListView).Descendant()).Where(x => x.Class.Contains("Image")).FirstOrDefault();
#endif

#if __IOS__
			Assert.AreEqual(4, elementsBefore.Where(x => x.Class.Contains("Image")).Count());
#elif __ANDROID__
			Assert.AreEqual(3, elementsBefore.Length);
#else
			Assert.AreEqual(4, elementsBefore.Count());
#endif


#if !__WINDOWS__
			Assert.IsNotNull(imageCell);
#endif

			RunningApp.Tap("ClickMe");
			RunningApp.WaitForElement(_appearText);
			var elementsAfter = RunningApp.WaitForElement(_fileName);

#if !__WINDOWS__
			var imageCellAfter = RunningApp.Query(app => app.Marked(_theListView).Descendant()).Where(x => x.Class.Contains("Image")).FirstOrDefault();
			Assert.IsNull(imageCellAfter);
#endif

#if __IOS__
			Assert.AreEqual(0, elementsAfter.Where(x => x.Class.Contains("Image")).Count());
#elif __ANDROID__
			foreach (var newElement in elementsAfter)
			{
				foreach(var oldElement in elementsBefore)
				{
					if(newElement.Class == oldElement.Class)
					{
						Assert.IsTrue(newElement.Rect.Height < oldElement.Rect.Height);
						continue;
					}
				}
			}
#endif
			RunningApp.Tap("SwitchUri");
			RunningApp.Tap("ClickMe");
			RunningApp.WaitForElement(_disappearText);
#if !__WINDOWS__
			imageCell = RunningApp.Query(app => app.Marked(_theListView).Descendant()).Where(x => x.Class.Contains("Image")).FirstOrDefault();
#endif

#if __IOS__
			Assert.AreEqual(4, elementsBefore.Where(x => x.Class.Contains("Image")).Count());
#elif __ANDROID__
			Assert.AreEqual(3, elementsBefore.Length);
#else
			Assert.AreEqual(4, elementsBefore.Count());
#endif


#if !__WINDOWS__
			Assert.IsNotNull(imageCell);
#endif
			RunningApp.Tap("ClickMe");
			RunningApp.WaitForElement(_appearText);
			elementsAfter = RunningApp.WaitForElement(_fileName);
#if !__WINDOWS__
			imageCellAfter = RunningApp.Query(app => app.Marked(_theListView).Descendant()).Where(x => x.Class.Contains("Image")).FirstOrDefault();
			Assert.IsNull(imageCellAfter);
#endif

#if __IOS__
			Assert.AreEqual(0, elementsAfter.Where(x => x.Class.Contains("Image")).Count());
#elif __ANDROID__
			foreach (var newElement in elementsAfter)
			{
				foreach (var oldElement in elementsBefore)
				{
					if (newElement.Class == oldElement.Class)
					{
						Assert.IsTrue(newElement.Rect.Height < oldElement.Rect.Height);
						continue;
					}
				}
			}
#else
			//can't validate if images have vanished until this is resolved
			Assert.Inconclusive(@"https://github.com/xamarin/Xamarin.Forms/issues/4731");
#endif
		}
#endif
	}
}
