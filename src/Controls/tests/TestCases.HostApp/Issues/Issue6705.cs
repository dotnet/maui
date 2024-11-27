namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 6705, "InvokeOnMainThreadAsync throws NullReferenceException", PlatformAffected.All)]
	public class Issue6705 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			StackLayout stack = null;
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete

			Content = stack;
		}
	}
}