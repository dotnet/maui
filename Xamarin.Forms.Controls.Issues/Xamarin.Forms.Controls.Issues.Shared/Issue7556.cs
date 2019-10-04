using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
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
			Master = new ContentPage() { Title = "Master", BackgroundColor = Color.Blue };
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
				lblThings = new Label();
				Content = new StackLayout()
				{
					Children =
					{
						lblThings,
						new Button()
						{
							Text = "Click to rotate through MasterBehavior settings and test each one",
							Command = new Command(OnChangeMasterBehavior)
						}
					}
				};
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
				OnChangeMasterBehavior();
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
	}
}
