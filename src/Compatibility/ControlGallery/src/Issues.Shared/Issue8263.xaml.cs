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
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8263, "[Enhancement] Add On/Off VisualStates for Switch")]
	public partial class Issue8263 : TestContentPage
	{
		public Issue8263()
		{
#if APP
			InitializeComponent();
#endif
		}
		protected override void Init()
		{

		}

#if UITEST
		[Test]
		[Category(UITestCategories.ManualReview)]
		public void SwitchOnOffVisualStatesTest()
		{
			RunningApp.WaitForElement("Switch");
			RunningApp.Screenshot("Switch Default");
			RunningApp.Tap("Switch");
			RunningApp.Screenshot("Switch Off with Red ThumbColor");
			RunningApp.Tap("Switch");
			RunningApp.Screenshot("Switch On with Green ThumbColor");
		}
#endif
	}
}