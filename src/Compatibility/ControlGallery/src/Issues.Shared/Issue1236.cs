using System.Threading.Tasks;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1236, "Label binding", PlatformAffected.iOS)]
	public class Issue1236 : TestContentPage
	{
		protected override void Init()
		{
			Content = new Label { HeightRequest = 30, WidthRequest = 200, BackgroundColor = Color.Purple.WithLuminosity(.7) };
			Content.SetBinding(Label.TextProperty, ".");

			DelayUpdatingBindingContext();
		}

		async void DelayUpdatingBindingContext()
		{
			await Task.Delay(2000);
			BindingContext = "Lorem Ipsum Dolor Sit Amet";
		}

#if UITEST
		[Test]
		public void DelayedLabelBindingShowsUp()
		{
			Task.Delay(2000).Wait();
			RunningApp.WaitForElement("Lorem Ipsum Dolor Sit Amet");
		}
#endif
	}
}