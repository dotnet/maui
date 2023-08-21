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
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2594, "StackLayout produces overlapping layouts on some phones with specific screen sizes", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public class Issue2594 : ContentPage
	{
		public Issue2594()
		{
			var layout = new StackLayout
			{
				Children = {
					new StackLayout {
						BackgroundColor = Colors.Red,
						Orientation = StackOrientation.Horizontal,
						Children = {
							new StackLayout {
								BackgroundColor = Colors.Gray,
								Children = {
									new Label {Text = "LONG TEXT. LONG TEXT. LONG TEXT. LONG TEXT. LONG TEXT.", TextColor = Colors.Olive},
								}
							},
							new Label {Text = "Some other text"}
						}
					},
					new Label {Text = "Overlapped text.", TextColor = Colors.Red}
				}
			};

			Padding = new Thickness(0, 20, 0, 0);
			Content = layout;
		}
	}
}
