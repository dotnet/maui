using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6705, "InvokeOnMainThreadAsync throws NullReferenceException", PlatformAffected.All)]
	public class Issue6705 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			StackLayout stack = null;
			stack = new StackLayout
			{
				Children =
				{
					new Button
					{
						AutomationId = "Button1",
						Text = "BeginInvokeOnMainThread",
						Command = new Command(() => Device.BeginInvokeOnMainThread(() => stack.Children.Add(new Label { Text = "1" })))
					},
					new Button
					{
						AutomationId = "Button2",
						Text = "InvokeOnMainThreadAsync Action",
						Command = new Command(async () => await Device.InvokeOnMainThreadAsync(() => stack.Children.Add(new Label { Text = "2" })))
					},
					new Button
					{
						AutomationId = "Button3",
						Text = "InvokeOnMainThreadAsync Func<Task>",
						Command = new Command(async () => await Device.InvokeOnMainThreadAsync(() => { stack.Children.Add(new Label { Text = "3" }); return Task.CompletedTask; }))
					},
					new Button
					{
						AutomationId = "Button4",
						Text = "InvokeOnMainThreadAsync Func<int>",
						Command = new Command(async () => await Device.InvokeOnMainThreadAsync(() => { stack.Children.Add(new Label { Text = "4" }); return 4; }))
					},
					new Button
					{
						AutomationId = "Button5",
						Text = "InvokeOnMainThreadAsync Func<Task<int>>",
						Command = new Command(async () => await Device.InvokeOnMainThreadAsync(() => { stack.Children.Add(new Label { Text = "5" }); return Task.FromResult(5); }))
					},
				}
			};

			Content = stack;
		}

#if UITEST
		[Test]
		public void Issue6705Test()
		{
			for (var i = 1; i < 6; i++)
			{
				RunningApp.WaitForElement($"Button{i}");
				RunningApp.Tap($"Button{i}");
				RunningApp.WaitForElement($"{i}");
			}
		}
#endif
	}
}