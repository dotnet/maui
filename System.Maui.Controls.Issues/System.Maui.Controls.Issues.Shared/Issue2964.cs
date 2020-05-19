using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2964, "TabbedPage toolbar item crash")]
	public class Issue2964 : TestMasterDetailPage
	{
		public class ModalPage : ContentPage
		{
			public ModalPage ()
			{
				Content = new Button {
					AutomationId = "ModalPagePopButton",
					Text ="Pop Me",
					Command = new Command (async () => {
						MessagingCenter.Send (this, "update");
						await Navigation.PopModalAsync ();
					})
				};
			}
		}

		public class Page1 : ContentPage
		{
			public Page1 ()
			{
				Title = "Testpage 1";

				MessagingCenter.Subscribe<ModalPage> (this, "update", sender => {
					BlowUp ();
				});

				Content = new Button {
					AutomationId = "Page1PushModalButton",
					Text = "press me",
					Command = new Command (async () => await Navigation.PushModalAsync (new ModalPage ()))
				};
			}

			void BlowUp ()
			{
				Content = new Label { 
					AutomationId = "Page1Label",
					Text = "Page1" 
				};
			}
		}

		protected override void Init ()
		{
			Title = "Test";

			Master = new ContentPage {
				Title = "Master",
				Content = new Button { 
					AutomationId = "MasterButton",
					Text = "Make a new page",
					Command= new Command(() => {
						Detail = new Page1 ();
						IsPresented = false;
					})
				}
			};

			Detail = new Page1 ();

			IsPresented = true;
		}

#if UITEST
		[Test]
		public void Issue2964Test ()
		{
			RunningApp.Screenshot ("I am at Issue 2964");

			RunningApp.Tap (q => q.Marked ("MasterButton"));
			RunningApp.Screenshot ("Create new Detail instance");

			RunningApp.Tap (q => q.Marked ("Page1PushModalButton"));
			RunningApp.Screenshot ("Modal is pushed");

			RunningApp.Tap (q => q.Marked ("ModalPagePopButton"));
			RunningApp.Screenshot ("Modal is popped");

			RunningApp.WaitForElement (q => q.Marked ("Page1Label"));
			RunningApp.Screenshot ("Didn't blow up! :)");
		}
#endif
	}
}
