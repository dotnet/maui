using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2831, "[IOS] label not rendering in BOLD when using a STYLE",
		PlatformAffected.iOS)]
	public class Issue2831 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Children = {
					new Label
					{
						VerticalOptions = LayoutOptions.CenterAndExpand,
						HorizontalOptions = LayoutOptions.CenterAndExpand,
						HorizontalTextAlignment = TextAlignment.Center,
						Text = "MUST BE BOLD",
						TextColor = Colors.Black,
						FontSize = 50,
						FontAttributes = FontAttributes.Bold,
						Style = Device.Styles.ListItemDetailTextStyle,
					}
				}
			};
		}
	}

#if UITEST

#endif
}
