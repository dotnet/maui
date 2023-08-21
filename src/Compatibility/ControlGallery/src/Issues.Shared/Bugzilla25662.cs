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
#if UITEST
	[Category(UITestCategories.Cells)]
	[Category(UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 25662, "Setting IsEnabled does not disable SwitchCell")]
	public class Bugzilla25662 : TestContentPage
	{
		[Preserve(AllMembers = true)]
		class MySwitch : SwitchCell
		{
			public MySwitch()
			{
				IsEnabled = false;
				SetBinding(SwitchCell.TextProperty, new Binding("."));
				OnChanged += (sender, e) => Text = "FAIL";
			}
		}

		protected override void Init()
		{
			var list = new ListView
			{
				ItemsSource = new[] {
					"One", "Two", "Three"
				},
				ItemTemplate = new DataTemplate(typeof(MySwitch))
			};

			Content = list;
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Bugzilla25662Test()
		{
			RunningApp.WaitForElement(q => q.Marked("One"));
			RunningApp.Tap(q => q.Marked("One"));
			RunningApp.WaitForNoElement(q => q.Marked("FAIL"));
		}
#endif
	}
}
