using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45926, "MessagingCenter prevents subscriber from being collected", PlatformAffected.All)]
	public class Bugzilla45926 : TestNavigationPage
	{
		protected override void Init()
		{
			Button createPage, sendMessage, doGC;

			Label instanceCount = new Label();
			Label messageCount = new Label();

			instanceCount.Text = $"Instances: {_45926SecondPage.InstanceCounter.ToString()}";
			messageCount.Text = $"Messages: {_45926SecondPage.MessageCounter.ToString()}";

			var content = new ContentPage {
				Title = "Test",
				Content = new StackLayout {
					VerticalOptions = LayoutOptions.Center,
					Children = {
						(createPage = new Button { Text = "New Page" }),
						(sendMessage = new Button { Text = "Send Message" }),
						(doGC = new Button { Text = "Do GC" }),
						instanceCount, messageCount
					}
				}
			};

			createPage.Clicked += (s, e) =>
			{
				PushAsync(new _45926IntermediatePage());
				PushAsync(new _45926SecondPage());
			};
			
			sendMessage.Clicked += (s, e) =>
			{
				MessagingCenter.Send (this, "Test");
			};

			doGC.Clicked += (sender, e) => {
				GC.Collect ();
				GC.WaitForPendingFinalizers();
				instanceCount.Text = $"Instances: {_45926SecondPage.InstanceCounter.ToString()}";
				messageCount.Text = $"Messages: {_45926SecondPage.MessageCounter.ToString()}";
			};

			PushAsync(content);
		}

#if UITEST
		[Test]
		public void Issue45926Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("New Page"));

			RunningApp.Tap (q => q.Marked ("New Page"));
			RunningApp.WaitForElement (q => q.Marked ("Second Page #1"));
			RunningApp.Back();
			RunningApp.WaitForElement (q => q.Marked ("Intermediate Page"));
			RunningApp.Back();
			RunningApp.Tap(q => q.Marked("Do GC"));
			RunningApp.Tap(q => q.Marked("Do GC"));
			RunningApp.Tap(q => q.Marked("Send Message"));
			RunningApp.Tap(q => q.Marked("Do GC"));

			RunningApp.WaitForElement (q => q.Marked ("Instances: 0"));
			RunningApp.WaitForElement (q => q.Marked ("Messages: 0"));
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class _45926IntermediatePage : ContentPage
	{
		public _45926IntermediatePage()
		{
			Content = new Label { Text = "Intermediate Page" };
		}
	}

	[Preserve(AllMembers = true)]
	public class _45926SecondPage : ContentPage
	{
		public static int InstanceCounter = 0;
		public static int MessageCounter = 0;

		public _45926SecondPage ()
		{
			Interlocked.Increment(ref InstanceCounter);

			Content = new Label {
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Second Page #" + (InstanceCounter)
			};

			MessagingCenter.Subscribe<Bugzilla45926> (this, "Test", OnMessage);
		}

		protected override void OnDisappearing ()
		{
			base.OnDisappearing ();
		}

		void OnMessage (Bugzilla45926 app)
		{
			System.Diagnostics.Debug.WriteLine ("Got Test message!");
			Interlocked.Increment(ref MessageCounter);
		}

		~_45926SecondPage ()
		{
			Interlocked.Decrement(ref InstanceCounter);
			System.Diagnostics.Debug.WriteLine ("~SecondPage: {0}", GetHashCode ());
		}
	}
}