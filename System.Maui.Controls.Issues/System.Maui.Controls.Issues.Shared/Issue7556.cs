using System;
using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Linq;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using System.Maui.Core.UITests;
using Xamarin.UITest.iOS;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7556, "[iOS] Masterbehavior.popover not being observed on iOS 13",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.MasterDetailPage)]
	[NUnit.Framework.Category(UITestCategories.ManualReview)]
#endif
	public class Issue7556 : TestMasterDetailPage
	{
		protected override void Init()
		{
			Master = new ContentPage()
			{
				Content = new StackLayout()
				{
					Children = 
					{
						new Label() { Margin = 20, Text = "Master Visible", TextColor = Color.White }
					}
				},
				Title = "Master",
				BackgroundColor = Color.Blue
			};

			Detail = new NavigationPage(new DetailsPage(this) { Title = "Details" });
		}

		[Preserve(AllMembers = true)]
		public class DetailsPage : ContentPage
		{
			MasterDetailPage MDP { get; }
			Label lblThings;

			public DetailsPage(MasterDetailPage masterDetailPage)
			{
				MDP = masterDetailPage;
				lblThings = new Label() { HorizontalTextAlignment = TextAlignment.Center, AutomationId = "CurrentMasterBehavior" };

				Content = new StackLayout()
				{
					Children =
					{
						lblThings,
						new Button()
						{
							Text = "Click to rotate through MasterBehavior settings and test each one",
							Command = new Command(OnChangeMasterBehavior),
							AutomationId = "ChangeMasterBehavior"
						},
						new Button()
						{
							Text = "Push Modal Page When on Split MasterBehavior",
							AutomationId = "PushModalPage",
							Command = new Command(() =>
							{
								Navigation.PushModalAsync(new ContentPage(){
									Content = new Button()
									{
										Text = "After popping this Page MasterBehavior should still be split",
										AutomationId = "PopModalPage",
										Command = new Command(() => Navigation.PopModalAsync())
									}
								});
							})
						},
						new Label(){ HorizontalTextAlignment = TextAlignment.Center, Text = "Close Master" }
					}
				};


				MDP.MasterBehavior = MasterBehavior.Split;
				lblThings.Text = MDP.MasterBehavior.ToString();
			}

			void OnChangeMasterBehavior()
			{
				var behavior = MDP.MasterBehavior;
				var results = Enum.GetValues(typeof(MasterBehavior)).Cast<MasterBehavior>().ToList();

				int nextIndex = results.IndexOf(behavior) + 1;
				if (nextIndex >= results.Count)
					nextIndex = 0;

				MDP.MasterBehavior = results[nextIndex];
				lblThings.Text = MDP.MasterBehavior.ToString();
			}
		}

#if UITEST && __IOS__
		[Test]
		public void MasterStillVisibleAfterPushingAndPoppingModalPage()
		{
			if (!RunningApp.IsTablet())
				return;

			RunningApp.SetOrientationLandscape();
			RunningApp.WaitForElement("Split");
			RunningApp.WaitForElement("Master Visible");
			RunningApp.Tap("PushModalPage");
			RunningApp.Tap("PopModalPage");
			RunningApp.WaitForElement("Master Visible");
		}

		[Test]
		public void SplitOnLandscapeFailsToDetectClose()
		{
			if (!RunningApp.IsTablet())
				return;

			while(RunningApp.WaitForElement("CurrentMasterBehavior")[0].ReadText() != MasterBehavior.SplitOnLandscape.ToString())
			{
				RunningApp.Tap("ChangeMasterBehavior");

				if(RunningApp.Query("Master Visible").Length > 0)
					RunningApp.Tap("Close Master");
			}

			RunningApp.Tap("Master");
			RunningApp.WaitForElement("Master Visible");
			RunningApp.Tap("Close Master");

			RunningApp.SetOrientationLandscape();
			RunningApp.SetOrientationPortrait();
			RunningApp.SetOrientationLandscape();
			RunningApp.SetOrientationPortrait();

			if (RunningApp.Query("Master Visible").Length > 0)
				RunningApp.Tap("Close Master");

			RunningApp.Tap("Master");
			RunningApp.WaitForElement("Master Visible");
			RunningApp.Tap("Close Master");
			RunningApp.Tap("Master");
			RunningApp.WaitForElement("Master Visible");
		}

		[TearDown]
		public override void TearDown() 
		{
			RunningApp.SetOrientationPortrait ();
			base.TearDown();
		}
#endif
	}
}
