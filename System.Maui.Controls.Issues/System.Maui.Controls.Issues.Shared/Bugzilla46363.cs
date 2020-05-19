using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Gestures)]
	[Category(UITestCategories.ListView)]
	[Category(UITestCategories.Cells)]
	[Category(UITestCategories.ContextActions)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 46363, "TapGestureRecognizer blocks List View Context Actions", PlatformAffected.Android)]
	public class Bugzilla46363 : TestContentPage
	{
		const string Target = "Two";
		const string ContextAction = "Context Action";
		const string TapSuccess = "Tap Success";
		const string TapFailure = "Tap command executed more than once";
		const string ContextSuccess = "Context Menu Success";
		const string Testing = "Testing";

		static Command s_tapCommand;
		static Command s_contextCommand;

		protected override void Init()
		{
			var list = new List<string> { "One", Target, "Three", "Four" };

			var lv = new ListView
			{
				ItemsSource = list,
				ItemTemplate = new DataTemplate(typeof(_46363Template))
			};

			var instructions = new Label();
			var result = new Label { Text = Testing };

			s_tapCommand = new Command(() =>
			{
				if (result.Text == TapSuccess || result.Text == TapFailure)
				{
					// We want this test to fail if the tap command is executed more than once
					result.Text = TapFailure;
				}
				else
				{
					result.Text = TapSuccess;
				}
			});

			s_contextCommand = new Command(() =>
			{
				result.Text = ContextSuccess;
			});

			var layout = new StackLayout { VerticalOptions = LayoutOptions.Fill, HorizontalOptions = LayoutOptions.Fill };

			layout.Children.Add(instructions);
			layout.Children.Add(result);
			layout.Children.Add(lv);

			Content = layout;
		}

		[Preserve(AllMembers = true)]
		class _46363Template : ViewCell
		{
			public _46363Template()
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				View = new StackLayout { Children = { label } };

				ContextActions.Add(new MenuItem
				{
					Text = ContextAction,
					Command = s_contextCommand
				});

				View.GestureRecognizers.Add(new TapGestureRecognizer
				{
					Command = s_tapCommand
				});
			}
		}

#if UITEST
		[Test]
		public void _46363_Tap_Succeeds()
		{
			RunningApp.WaitForElement(Testing);
			RunningApp.Tap(Target);
			RunningApp.WaitForElement(TapSuccess);

			// First run at fixing this caused the context menu to open on a regular tap
			// So this check is to ensure that doesn't happen again
			RunningApp.WaitForNoElement(ContextAction);
		}

		[Test]
		public void _46363_ContextAction_Succeeds()
		{
			RunningApp.WaitForElement(Testing);
			RunningApp.ActivateContextMenu(Target);
			RunningApp.WaitForElement(ContextAction);
			RunningApp.Tap(ContextAction);
			RunningApp.WaitForElement(ContextSuccess);
		}
#endif
	}
}