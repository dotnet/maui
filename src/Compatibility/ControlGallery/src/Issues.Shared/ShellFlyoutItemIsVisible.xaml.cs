//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell.FlyoutItemIsVisible")]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public partial class ShellFlyoutItemIsVisible : TestShell
	{
		public bool ItemIsVisible { get; } = false;
		public bool FlyoutItemNotVisible { get; } = false;
		public bool FlyoutItemVisible { get; } = true;

		public ShellFlyoutItemIsVisible()
		{
#if APP
			InitializeComponent();
#endif
		}

		async void GoToUnreachable(object sender, System.EventArgs e)
		{
			try
			{
				await GoToAsync("//Unreachable");
			}
			catch
			{
#if APP
				lblResult.Text = "Expected Result. Navigation has failed";
#endif
			}
		}

		async void GoToNoFlyoutItem(object sender, System.EventArgs e)
		{
			await GoToAsync("//NoFlyoutItem");
		}

		async void GoBackHome(object sender, System.EventArgs e)
		{
			await GoToAsync("//Home");
		}

		protected override void Init()
		{
			BindingContext = this;
		}


#if UITEST
		[Test, NUnit.Framework.Category(UITestCategories.Shell)]
		public void CanStillNavigateToContentNotPresentInFlyout()
		{
			RunningApp.Tap("GoToNoFlyoutItem");
			RunningApp.WaitForElement("Success");
		}

		[Test, NUnit.Framework.Category(UITestCategories.Shell)]
		public void NavigationFailsTryingToNavigateToContentSetToNotVisible()
		{
			RunningApp.Tap("GoToUnreachable");
			RunningApp.WaitForNoElement("Failure");
		}


		[Test, NUnit.Framework.Category(UITestCategories.Shell)]
		public void BasicFlyoutItemIsVisibleValidate()
		{			
			ShowFlyout();
			RunningApp.WaitForNoElement("Failure");
			RunningApp.WaitForElement("FlyoutItemShowing");
		}
#endif
	}
}