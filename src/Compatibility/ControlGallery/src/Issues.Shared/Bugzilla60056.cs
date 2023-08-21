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
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 60056, "[UWP] ViewCell ignores margins of it's child", PlatformAffected.UWP)]
	public class Bugzilla60056 : TestContentPage
	{
		protected override void Init()
		{
			Content = new ListView
			{
				ItemsSource = new string[] { "A", "B", "C" },
				ItemTemplate = new DataTemplate(() =>
				{
					return new ViewCell
					{
						View = new StackLayout
						{
							Margin = 20,
							Children =
							{
								new Label {  Text = "I should be indented" },
								new Button { Margin = 5, Text = "I should be further indented" }
							}
						}
					};
				})
			};
		}
	}
}