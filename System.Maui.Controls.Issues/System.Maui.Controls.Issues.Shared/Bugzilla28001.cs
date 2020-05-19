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
	[Issue (IssueTracker.Bugzilla, 28001, "[Android] TabbedPage: invisible tabs are not Disposed", PlatformAffected.Android)]
	public class Bugzilla28001 : TestContentPage
	{
		static int s_disposeCount;
		static Label s_lbl;

		void HandleDispose (object sender, EventArgs e)
		{
			s_disposeCount++;
			s_lbl.Text = string.Format ("Dispose {0} pages", s_disposeCount);
		}

		protected override void Init ()
		{
			s_disposeCount = 0;
			s_lbl = new Label { AutomationId = "lblDisposedCound" };
			var tab1 = new DisposePage { Title = "Tab1" }; 
			var tab2 = new DisposePage { Title = "Tab2" };
			tab1.RendererDisposed += HandleDispose;				
			tab2.RendererDisposed += HandleDispose;

			tab2.PopAction = tab1.PopAction = async () => await Navigation.PopAsync ();

			var tabbedPage = new TabbedPage { Children = { tab1, tab2 } };
			var btm = new Button { Text = "Push" };

			btm.Clicked += async  (object sender, EventArgs e) => {
				await Navigation.PushAsync (tabbedPage);
			};

			Content = new StackLayout { Children = { btm, s_lbl } };
		}

	
		#if UITEST
		[Test]
		public void Bugzilla28001Test ()
		{
			RunningApp.Screenshot ("I am at Bugzilla 28001");
			RunningApp.Tap (q => q.Marked ("Push"));
			RunningApp.Tap (q => q.Marked ("Tab2"));
			RunningApp.Tap (q => q.Marked ("Tab1"));
			RunningApp.Tap (q => q.Marked ("Pop"));
			RunningApp.WaitForElement (q => q.Marked (string.Format ("Dispose {0} pages", 2)));
		}
#endif
	}
}
