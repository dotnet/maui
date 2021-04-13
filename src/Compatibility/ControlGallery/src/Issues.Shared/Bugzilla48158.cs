using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 48158, "Hidden controls become transparent, needs manual verification", PlatformAffected.iOS)]
	public class Bugzilla48158 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			var grdInner = new Grid { BackgroundColor = Colors.Red, IsVisible = false, Padding = new Thickness(10) };
			var btn = new Button { Text = "Click and verify background is red" };
			btn.Clicked += (s, e) =>
			{
				grdInner.IsVisible = !grdInner.IsVisible;
			};
			var grd = new Grid();
			grd.Children.Add(grdInner);
			grd.Children.Add(btn);
			Content = grd;
		}
	}
}