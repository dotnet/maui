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
	[Issue (IssueTracker.Github, 1677, "[Enhancement] Entry: Control over text-prediction", PlatformAffected.All)]
	public class Issue1677
		: TestContentPage
	{
		protected override void Init()
		{
			var entryDefaults = new Entry();
			var entryNoTextPrediction = new Entry { IsTextPredictionEnabled = false };
			// IsTextPredictionEnabled should be ignored for email Entry
			var entryEmail = new Entry { Text = "foo@example.com", Keyboard = Keyboard.Email, IsTextPredictionEnabled = true };
			// IsTextPredictionEnabled should be ignored for numeric Entry
			var entryNumeric = new Entry { Text = "01234", Keyboard = Keyboard.Numeric, IsTextPredictionEnabled = true };
			// On Android disabling either spell checking or text prediction both turn off text suggestions so this Entry
			// should behave the same as entryNoTextPrediction above
			var entryNoSpellChecking = new Entry { IsSpellCheckEnabled = false };

			var stackLayout = new StackLayout();
			stackLayout.Children.Add(new Label { Text = "Defaults" });
			stackLayout.Children.Add(entryDefaults);
			stackLayout.Children.Add(new Label { Text = "Text prediction disabled" });
			stackLayout.Children.Add(entryNoTextPrediction);
			stackLayout.Children.Add(new Label { Text = "Spell checking disabled" });
			stackLayout.Children.Add(entryNoSpellChecking);
			stackLayout.Children.Add(new Label { Text = "Email" });
			stackLayout.Children.Add(entryEmail);
			stackLayout.Children.Add(new Label { Text = "Numeric" });
			stackLayout.Children.Add(entryNumeric);

			stackLayout.Padding = new Thickness(0, 20, 0, 0);
			Content = stackLayout;
		}
	}
}
