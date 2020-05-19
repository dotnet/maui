using System;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5172, "ImageCell does not load image from URI - Android", PlatformAffected.Android)]
	public class Issue5172 : TestContentPage // or TestMasterDetailPage, etc ...
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
                            ImageSource = ImageSource.FromUri(new Uri("https://raw.githubusercontent.com/xamarin/System.Maui/543da8aec914efc6ce98794302792ef948cc28c8/System.Maui.ControlGallery.Android/Resources/drawable/coffee.png")),
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