using System;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 36788, "Truncation Issues with Relative Layouts")]
	public class Bugzilla36788 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		Label _resultLabel;
		Label _testLabel;
		View _container;

		protected override void Init ()
		{
			// Initialize ui here instead of ctor
			var stackLayout = new StackLayout {
				Spacing = 8
			};

			var longString = "Very long text in single line to be truncated at tail. Adding extra text to make sure it gets truncated. And even more extra text because otherwise the test might fail if we're in, say, landscape orientation rather than portrait.";

			var contentView = new ContentView {
				Padding = 16,
				BackgroundColor = Color.Gray,
				Content = new Label {
					BackgroundColor = Color.Aqua,
					Text = longString,
					LineBreakMode = LineBreakMode.TailTruncation
				}
			};

			stackLayout.Children.Add (contentView);

			contentView = new ContentView {
				Padding = 16,
				BackgroundColor = Color.Gray,
				Content = new RelativeLayout {
					BackgroundColor = Color.Navy,
					Children = {
						{new Label {
							BackgroundColor = Color.Blue,
							Text = longString,
							LineBreakMode = LineBreakMode.TailTruncation
						}, Forms.Constraint.Constant (0)},
						{new Label {
							BackgroundColor = Color.Fuchsia,
							Text = longString,
							LineBreakMode = LineBreakMode.TailTruncation
						}, Forms.Constraint.Constant (0), Forms.Constraint.Constant (40)},
						{new Label {
							BackgroundColor = Color.Fuchsia,
							Text = longString,
							LineBreakMode = LineBreakMode.TailTruncation
						}, Forms.Constraint.Constant (10), Forms.Constraint.Constant (80)},
					}
				}
			};

			stackLayout.Children.Add (contentView);

			contentView = new ContentView {
				Padding = 16,
				BackgroundColor = Color.Gray,
				IsClippedToBounds = true,
				Content = _container = new RelativeLayout {
					IsClippedToBounds = true,
					BackgroundColor = Color.Navy,
					Children = {
						{_testLabel = new Label {
							BackgroundColor = Color.Blue,
							Text = longString,
							LineBreakMode = LineBreakMode.TailTruncation
						}, Forms.Constraint.Constant (0)},
						{new Label {
							BackgroundColor = Color.Fuchsia,
							Text = longString,
							LineBreakMode = LineBreakMode.TailTruncation
						}, Forms.Constraint.Constant (0), Forms.Constraint.Constant (40)},
						{new Label {
							BackgroundColor = Color.Fuchsia,
							Text = longString,
							LineBreakMode = LineBreakMode.TailTruncation
						}, Forms.Constraint.Constant (10), Forms.Constraint.Constant (80)},
					}
				}
			};

			stackLayout.Children.Add (contentView);

			_resultLabel = new Label ();
			stackLayout.Children.Add (_resultLabel);

			Content = stackLayout;
		}

		protected override async void OnAppearing ()
		{
			base.OnAppearing ();
			await Task.Delay (200);

			double fuzzFactor = 15; // labels sometimes overflow slightly, thanks hinting

			if (Math.Abs (_testLabel.Width - _container.Width) < fuzzFactor)
				_resultLabel.Text = "Passed";
		}

#if UITEST
		[Test]
		public void Bugzilla36788Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Passed"));
		}
#endif
	}
}
