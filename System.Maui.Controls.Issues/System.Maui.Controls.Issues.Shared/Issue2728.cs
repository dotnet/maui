using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2728, "[macOS] Label FontAttributes Italic is not working")]
	public class Issue2728 : TestContentPage
	{
		const string _lblHome = "Hello Label";

		protected override void Init()
		{
			var label = new Label { Text = _lblHome, FontAttributes = FontAttributes.Italic };

			Content = new StackLayout
			{
				Children = {
					label
				}
			};
		}


#if UITEST
		[Test]
		public void Issue2728TestsItalicLabel ()
		{
			RunningApp.WaitForElement(q => q.Text(_lblHome));
			RunningApp.Screenshot ("Label rendererd with italic font");
		}
#endif

	}
}
