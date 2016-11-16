using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.UITest.iOS;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 32801, "Memory Leak in TabbedPage + NavigationPage")]
	public class Bugzilla32801 : TestTabbedPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init ()
		{
			Children.Add (new NavigationPage (new TestDemoPage (1)) { Title = "Tab", Icon = "bank.png" });
			Children.Add (new NavigationPage (new TestDemoPage (1)) { Title = "Tab 1", Icon = "bank.png" });

		}

		public class TestDemoPage : ContentPage
		{
			int _level = 0;

			public TestDemoPage (int level)
			{	
				_level = level;

				System.Diagnostics.Debug.WriteLine ("Page Level {0} : Created", _level);

				Title = string.Format ("Level {0}", level);

				var lblStack = new Label ();

				var buttonAdd = new Button {
					Text = "Add Level",
					AutomationId = "btnAdd",
					BackgroundColor = Color.Aqua
				};

				buttonAdd.Clicked += (sender, e) => Navigation.PushAsync (new TestDemoPage (_level + 1));

				var buttonStack = new Button {
					Text = "Show Navigation Stack",
					AutomationId = "btnStack",
					BackgroundColor = Color.Aqua
				};

				buttonStack.Clicked += (object sender, EventArgs e) => {
					lblStack.Text = "Stack " + Navigation.NavigationStack.Count.ToString ();
					System.Diagnostics.Debug.WriteLine ("------------------------------------------------------------");
					foreach (TestDemoPage page in Navigation.NavigationStack)
						System.Diagnostics.Debug.WriteLine ("Items On Navigation Stack =====> Level {0}", page._level);
					System.Diagnostics.Debug.WriteLine ("------------------------------------------------------------");
				};

				Content = new StackLayout {
					Padding = new Thickness (20.0),
					Spacing = 20.0,
					Children = {
						buttonAdd, buttonStack, lblStack
					}
				};
			}

			~TestDemoPage ()
			{
				System.Diagnostics.Debug.WriteLine ("Page Level {0} : Destroyed", _level);
			}
		}

		#if UITEST && __IOS__
		[Test]
		public void Bugzilla32801Test ()
		{
			RunningApp.Tap (c => c.Marked ("btnAdd"));
			RunningApp.Tap (c => c.Marked ("btnAdd"));
			RunningApp.Tap (c => c.Marked ("btnStack"));
			RunningApp.WaitForElement (c => c.Marked ("Stack 3"));
			RunningApp.Tap (c => c.Marked ("Tab"));
			RunningApp.Tap (c => c.Marked ("btnStack"));
			RunningApp.WaitForElement (c => c.Marked ("Stack 1"));
		}

		[TearDown]
		public void TearDown() 
		{
			RunningApp.SetOrientationPortrait ();
		}
#endif
	}
}
