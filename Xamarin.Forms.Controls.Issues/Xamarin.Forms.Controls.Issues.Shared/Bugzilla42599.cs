using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;
using System;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 42599, "LineBreakMode does not work on UWP", PlatformAffected.WinRT)]
	public class Bugzilla42599 : TestContentPage
	{
		protected override void Init()
		{
			var scrollView = new ScrollView();
			var layout = new StackLayout();

			foreach (var lineBreakMode in Enum.GetValues(typeof(LineBreakMode)).Cast<LineBreakMode>())
			{
				layout.Children.Add(GetLayout(lineBreakMode));
			}
			scrollView.Content = layout;
			Content = scrollView;
		}

		static StackLayout GetLayout(LineBreakMode lineBreakMode)
		{
			var text = "";

			switch (lineBreakMode)
			{
				default:
				case LineBreakMode.NoWrap:
					text = "This is a long sentence that should NOT wrap. If this sentence has wrapped, then this test has failed.";
					break;
				case LineBreakMode.WordWrap:
					text = "This is a long sentence that should word wrap. If this sentence has NOT wrapped, then this test has failed.";
					break;
				case LineBreakMode.CharacterWrap:
					text = "This is a long sentence that should character wrap. If this sentence has NOT wrapped, then this test has failed.";
					break;
				case LineBreakMode.HeadTruncation:
					text = "This is a long sentence that should truncate at the beginning. If this sentence has NOT truncated, then this test has failed.";
					break;
				case LineBreakMode.TailTruncation:
					text = "This is a long sentence that should truncate at the end. If this sentence has NOT truncated, then this test has failed.";
					break;
				case LineBreakMode.MiddleTruncation:
					text = "This is a long sentence that should truncate at the middle. If this sentence has NOT truncated, then this test has failed.";
					break;
			}

			var label = new Label
			{
				LineBreakMode = lineBreakMode,
				Text = text,
			};

			var layout = new StackLayout
			{
				Children = { label },
				Orientation = StackOrientation.Horizontal
			};

			return layout;
		}
	}
}