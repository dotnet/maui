using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1660, "[Enhancement] IsSpellCheckEnabled on Entry/Editor", PlatformAffected.All)]
	public class Issue1660
		: TestContentPage
	{
		protected override void Init()
		{
			var text = "The quck bron fx jumps ovr the lazyy doog";

			var entryDefaults = new Entry { Text = text };
			var editorDefaults = new Editor { Text = text };
			var entryNoSpellCheck = new Entry { Text = text, IsSpellCheckEnabled = false };
			var editorNoSpellCheck = new Editor { Text = text, IsSpellCheckEnabled = false };
			var entryToggleable = new Entry { Text = text };
			var editorToggleable = new Editor { Text = text };
			var toggle = new Switch { IsToggled = true };

			var stackLayout = new StackLayout();
			stackLayout.Children.Add(new Label { Text = "Defaults" });
			stackLayout.Children.Add(entryDefaults);
			stackLayout.Children.Add(editorDefaults);
			stackLayout.Children.Add(new Label { Text = "Spell checking disabled" });
			stackLayout.Children.Add(entryNoSpellCheck);
			stackLayout.Children.Add(editorNoSpellCheck);
			stackLayout.Children.Add(new Label { Text = "Toggleable spell checking" });
			stackLayout.Children.Add(entryToggleable);
			stackLayout.Children.Add(editorToggleable);
			stackLayout.Children.Add(toggle);

			toggle.Toggled += (_, b) =>
			{
				entryToggleable.IsSpellCheckEnabled = b.Value;
				editorToggleable.IsSpellCheckEnabled = b.Value;
			};

			stackLayout.Padding = new Thickness(0, 20, 0, 0);
			Content = stackLayout;
		}
	}
}
