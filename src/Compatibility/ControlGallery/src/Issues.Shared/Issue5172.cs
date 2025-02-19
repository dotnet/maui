using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TableView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5172, "ImageCell does not load image from URI - Android", PlatformAffected.Android)]
	public class Issue5172 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			Label header = new Label
			{
				Text = "Please make sure a image is shown in the ImageCell bellow",
				FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
				HorizontalOptions = LayoutOptions.Center
			};

			TableView tableView = new TableView
			{
				Intent = TableIntent.Form,
				Root = new TableRoot
				{
					new TableSection
					{
						new ImageCell
						{
                            // Some differences with loading images in initial release.
                            ImageSource = ImageSource.FromUri(new Uri("https://raw.githubusercontent.com/xamarin/Xamarin.Forms/543da8aec914efc6ce98794302792ef948cc28c8/Microsoft.Maui.Controls.ControlGallery.Android/Resources/drawable/coffee.png")),
							Text = "This is an ImageCell",
							Detail = "This is some detail text",
						}
					}
				}
			};

			Padding = new Thickness(10, 20, 10, 5);

			Content = new StackLayout
			{
				Children =
				{
					header,
					tableView
				}
			};
		}

	}
}