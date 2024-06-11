using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif


namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Device.IsInvokeRequired race condition causes crash")]
	public class IsInvokeRequiredRaceCondition : TestContentPage
	{
		protected override void Init()
		{
			var button = new Button
			{
				AutomationId = "crashButton",
				Text = "Start Test"
			};

			var success = new Label { Text = "Success", IsVisible = false, AutomationId = "successLabel" };

			var instructions = new Label { Text = "Click the Start Test button. " };

			Content = new StackLayout
			{
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				Children = { instructions, success, button }
			};

			button.Clicked += async (sender, args) =>
			{
				await Task.WhenAll(GenerateTasks());
				success.IsVisible = true;
			};
		}

		List<Task> GenerateTasks()
		{
			var result = new List<Task>();

			for (int n = 0; n < 1000; n++)
			{
				result.Add(Task.Run(() => { var t = Device.IsInvokeRequired; }));
			}

			return result;
		}

#if UITEST
		[Test]
		[Microsoft.Maui.Controls.Compatibility.UITests.MovedToAppium]
		public void ShouldNotCrash()
		{
			RunningApp.Tap(q => q.Marked("crashButton"));
			RunningApp.WaitForElement(q => q.Marked("successLabel"));
		}
#endif

	}
}