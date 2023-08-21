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

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 9305, "Swipe View BackgroundColor Issues", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue9305 : TestContentPage
	{
		public Issue9305()
		{
#if APP
			Title = "Issue 9305";
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}
}