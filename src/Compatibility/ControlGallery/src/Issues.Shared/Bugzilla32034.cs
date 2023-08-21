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

using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 32034, "MissingMethodException while pushing and popping pages", PlatformAffected.Android, NavigationBehavior.PushModalAsync)]
	public class Bugzilla32034 : NavigationPage
	{
		public class ButtonPage : ContentPage
		{
			public ButtonPage()
			{
				Content = new StackLayout
				{
					Children = {
						new Button {
							Text = "Push", Command = new Command (o => ((NavigationPage) Parent).PushAsync (new ButtonPage ()))
						},
						new Button {
							Text = "Pop", Command = new Command (o => ((NavigationPage) Parent).Navigation.PopAsync ())
						},
					},
				};
			}
		}

		public Bugzilla32034() : base(new ButtonPage())
		{
		}
	}
}
