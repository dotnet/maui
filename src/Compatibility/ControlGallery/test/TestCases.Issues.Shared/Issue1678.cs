using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1678, "[Enhancement] Entry: Read-only entry", PlatformAffected.All)]
	public class Issue1678
		: TestContentPage
	{
		protected override void Init()
		{
			var entryText = "Entry Lorem Ipsum";
			var editorText = "Editor Lorem Ipsum";

			var entryDefaults = new Entry { Text = entryText };
			var editorDefaults = new Editor { Text = editorText };
			var entryReadOnly = new Entry { Text = entryText, IsReadOnly = true };
			var editorReadOnly = new Editor { Text = editorText, IsReadOnly = true };
			var entryToggleable = new Entry { Text = entryText };
			var editorToggleable = new Editor { Text = editorText };

			var toggle = new Switch { IsToggled = false };

			var stackLayout = new StackLayout();
			stackLayout.Children.Add(new Label { Text = "Defaults" });
			stackLayout.Children.Add(entryDefaults);
			stackLayout.Children.Add(editorDefaults);
			stackLayout.Children.Add(new Label { Text = "Read Only" });
			stackLayout.Children.Add(entryReadOnly);
			stackLayout.Children.Add(editorReadOnly);
			stackLayout.Children.Add(new Label { Text = "Toggleable is read only" });
			stackLayout.Children.Add(entryToggleable);
			stackLayout.Children.Add(editorToggleable);
			stackLayout.Children.Add(toggle);

			toggle.Toggled += (_, b) =>
			{
				entryToggleable.IsReadOnly = b.Value;
				editorToggleable.IsReadOnly = b.Value;
			};

			stackLayout.Padding = new Thickness(0, 20, 0, 0);
			Content = stackLayout;
		}
	}
}
