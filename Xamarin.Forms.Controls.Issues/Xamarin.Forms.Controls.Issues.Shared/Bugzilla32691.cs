using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 32691, "Clearing an image by setting Image.Source to null, while Image.IsLoading is true, doesn't work.")]
	public class Bugzilla32691 : TestContentPage
	{
		const string KSetImageSource = "SET IMAGE SOURCE";
		const string KClearImageSource = "CLEAR IMAGE SOURCE";

		protected override void Init ()
		{
#pragma warning disable 618
			var label = new Label () { XAlign = TextAlignment.Center };
#pragma warning restore 618
			var image = new Image ();

			image.PropertyChanged += (sender, e) => {
				if (e.PropertyName == "IsLoading")
					label.Text = image.IsLoading ? "Loading" : "Done";
			};

			var btnSetOrClear = new Button () { Text = KSetImageSource, AutomationId = "btnLoad" };

			btnSetOrClear.Clicked += delegate {
				if (btnSetOrClear.Text == KSetImageSource) {
					ClearImageCache ();
					image.Source =
						"https://raw.githubusercontent.com/xamarin/Xamarin.Forms/master/banner.png";
					btnSetOrClear.Text = KClearImageSource;
				} else {
					image.Source = null;
					btnSetOrClear.Text = KSetImageSource;
				}
			};

			Content = new StackLayout {
				Orientation = StackOrientation.Vertical, 
				Padding = new Thickness (10),
				Children = { btnSetOrClear, image, label }
			};
		}

		void ClearImageCache ()
		{
			var cacheService = DependencyService.Get<ICacheService> ();
			cacheService?.ClearImageCache ();
		}
	}
}
