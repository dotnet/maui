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

using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Brush)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11643, "[Bug] SwipeBehaviorOnInvoked=RemainOpen triggers event twice",
		PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue11643 : TestContentPage
	{
		public Issue11643()
		{
#if APP
			Title = "Issue 11643";
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}

#if APP
		void OnSwipeItemInvoked(object sender, EventArgs e)
		{
			DisplayAlert("SwipeView", "SwipeItem Invoked", "Ok");
		}
#endif
	}
}