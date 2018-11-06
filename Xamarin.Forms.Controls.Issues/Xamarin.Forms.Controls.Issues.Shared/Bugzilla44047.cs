using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44047, "Memory leak when using SetBackButtonTitle on iOS", PlatformAffected.iOS)]
	public class Bugzilla44047 : TestMasterDetailPage
	{
		protected override void Init()
		{
			Master = new ContentPage
			{
				Title = "Menu"
			};

			Detail = new NavigationPage(new Page1());
		}
	}

	public class Page1 : ContentPage
	{
		public Page1()
		{
			Title = "Page1";
			Content = new Button
			{
				Text = "Open Page2",
				Command = new Command(async o =>
				{
					await (Parent as NavigationPage).PushAsync(new Page2());
				})
			};
		}
	}

	public class Page2 : ContentPage
	{
		public Page2()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Title = "Page2";
			System.Diagnostics.Debug.WriteLine("Constructor");
			NavigationPage.SetBackButtonTitle(this, "Custom");
		}

		~Page2()
		{
			System.Diagnostics.Debug.WriteLine("Finalizer");
		}
	}
}
