﻿using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1878, "[UWP] Setting SearchBar.CancelButtonColor affects all SearchBars on page", PlatformAffected.UWP)]
	public class GitHub1878 : TestContentPage
	{
		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label
			{
				Text = "The SearchBars below should have different cancel button colors. "
						+ "If they each have the same cancel button color, this test has failed."
			};

			var sb1 = new SearchBar { Text = "This should have a red cancel button." };
			var sb2 = new SearchBar { Text = "This should have a blue cancel button." };

			sb1.CancelButtonColor = Colors.Red;
			sb2.CancelButtonColor = Colors.Blue;

			layout.Children.Add(instructions);
			layout.Children.Add(sb1);
			layout.Children.Add(sb2);

			Content = layout;
		}
	}
}