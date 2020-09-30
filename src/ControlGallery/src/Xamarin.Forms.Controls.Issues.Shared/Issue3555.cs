using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3555, "[Enhancement] Editor: Control over text-prediction", PlatformAffected.All)]
	public class Issue3555
		: TestContentPage
	{
		protected override void Init()
		{
			var editorDefaults = new Editor();
			var editorFull = new Editor();
			editorFull.Keyboard = Keyboard.Create(KeyboardFlags.All);
			var editorNoTextPrediction = new Editor { IsTextPredictionEnabled = false };
			// IsTextPredictionEnabled should be ignored for email in Editor
			var editorEmail = new Editor { Text = "moses@example.com", Keyboard = Keyboard.Email, IsTextPredictionEnabled = true };
			// IsTextPredictionEnabled should be ignored for numeric Editor
			var editorNumeric = new Editor { Text = "01234", Keyboard = Keyboard.Numeric, IsTextPredictionEnabled = true };
			// On Android disabling either spell checking or text prediction both turn off text suggestions so this Editor
			// should behave the same as editorNoTextPrediction above
			var editorNoSpellChecking = new Editor { IsSpellCheckEnabled = false };
			var stackLayout = new StackLayout();
			stackLayout.Children.Add(new Label { Text = "Defaults" });
			stackLayout.Children.Add(editorDefaults);
			stackLayout.Children.Add(new Label { Text = "Text prediction disabled" });
			stackLayout.Children.Add(editorNoTextPrediction);
			stackLayout.Children.Add(new Label { Text = "Spell checking disabled" });
			stackLayout.Children.Add(editorNoSpellChecking);
			stackLayout.Children.Add(new Label { Text = "Email" });
			stackLayout.Children.Add(editorEmail);
			stackLayout.Children.Add(new Label { Text = "Numeric" });
			stackLayout.Children.Add(editorNumeric);
			stackLayout.Children.Add(new Label { Text = "Full" });
			stackLayout.Children.Add(editorFull);
			stackLayout.Padding = new Thickness(0, 20, 0, 0);
			Content = stackLayout;
		}
	}
}
