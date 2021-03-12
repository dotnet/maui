using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
	[Issue(IssueTracker.Github, 6187, "Visual Material Entry Underline color should reference PlaceholderColor",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Entry)]
#endif
	public class Issue6187 : TestContentPage
	{
		protected override void Init()
		{
			Visual = VisualMarker.Material;

			Entry entry = new Entry
			{
				Placeholder = "Placeholder text...",
				PlaceholderColor = Color.Green,
				Text = "Typed Text ",
				TextColor = Color.Red
			};

			Label label = new Label
			{
				Text = "The underline of the Entry above should be Green to match the " +
						"color of the Placeholder text.",
				FontSize = 18.0
			};

			StackLayout layout = new StackLayout
			{
				Padding = new Thickness(0, 50, 0, 0)
			};

			layout.Children.Add(entry);
			layout.Children.Add(label);

			Content = layout;
		}
	}
}
