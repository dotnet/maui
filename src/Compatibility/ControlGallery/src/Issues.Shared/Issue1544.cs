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
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1544, "StackLayout with zero spacing is not always zero spacing ", PlatformAffected.Android | PlatformAffected.WPF)]
	public class Issue1544 : TestContentPage
	{
		protected override void Init()
		{
			var colors = new[] {
				Color.FromArgb("#433DBA"),
				Color.FromArgb("#6461B7")
			};
			var layout = new StackLayout()
			{
				Spacing = 0,
				Children =
				{
					new Label()
					{
						BackgroundColor = colors[1],
						HeightRequest = 55.7
					}
				}
			};
			for (int i = 0; i < 40; i++)
			{
				layout.Children.Add(new BoxView()
				{
					BackgroundColor = colors[i % 2],
					HeightRequest = 10
				});
			}
			Content = layout;
		}
	}
}