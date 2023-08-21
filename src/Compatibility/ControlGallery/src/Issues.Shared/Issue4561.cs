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

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4561, "Keyboard navigation does not work", PlatformAffected.Android)]
	public class Issue4561 : TestContentPage
	{
		[Preserve(AllMembers = true)]
		public class CustomView : View
		{
		}

		protected override void Init()
		{
			Content = new StackLayout
			{
				Children = {
					new Label { Text = "Select any editbox below and try using the TAB key navigation. Focus should change from one editbox to another" },
					new CustomView()
				}
			};
		}
	}
}
