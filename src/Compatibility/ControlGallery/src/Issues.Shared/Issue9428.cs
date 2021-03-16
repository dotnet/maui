using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9428, "[Android] Swapping out Image on ImageButton can cause measuring issues",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
	[NUnit.Framework.Category(UITestCategories.ManualReview)]
#endif
	public class Issue9428 : TestContentPage
	{
		protected override void Init()
		{
			ImageButton imageButton = new ImageButton()
			{
				HeightRequest = 200,
				WidthRequest = 200,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				BackgroundColor = Color.Transparent,
				AutomationId = "Coffee"
			};

			VisualStateGroup vsg = new VisualStateGroup()
			{
				Name = "button",
				States =
				{
					new VisualState()
					{
						Name = "Normal",
						Setters =
						{
							new Setter()
							{
								Property = ImageButton.SourceProperty,
								Value = "coffee.png"
							}
						}
					},
					new VisualState()
					{
						Name = "Pressed",
						Setters =
						{
							new Setter()
							{
								Property = ImageButton.SourceProperty,
								Value = "coffee2.jpg"
							}
						}
					}
				}
			};

			VisualStateManager.SetVisualStateGroups(imageButton, new VisualStateGroupList()
			{
				vsg
			});

			Content = new StackLayout()
			{
				Children =
				{
					new Label(){ Text = "Click on the image and it should stay the same size"},
					imageButton
				}
			};
		}
	}
}
