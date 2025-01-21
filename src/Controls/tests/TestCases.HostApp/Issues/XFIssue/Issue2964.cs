namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2964, "TabbedPage toolbar item crash")]
public class Issue2964 : TestFlyoutPage
{
	public class ModalPage : ContentPage
	{
		public ModalPage()
		{
			Content = new Button
			{
				AutomationId = "ModalPagePopButton",
				Text = "Pop Me",
				Command = new Command(async () =>
				{
#pragma warning disable CS0618 // Type or member is obsolete
					MessagingCenter.Send(this, "update");
#pragma warning restore CS0618 // Type or member is obsolete
					await Navigation.PopModalAsync();
				})
			};
		}
	}

	public class Page1 : ContentPage
	{
		public Page1()
		{
			Title = "Testpage 1";

#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<ModalPage>(this, "update", sender =>
			{
				BlowUp();
			});
#pragma warning restore CS0618 // Type or member is obsolete

			Content = new Button
			{
				AutomationId = "Page1PushModalButton",
				Text = "press me",
				Command = new Command(async () => await Navigation.PushModalAsync(new ModalPage()))
			};
		}

		void BlowUp()
		{
			Content = new Label
			{
				AutomationId = "Page1Label",
				Text = "Page1"
			};
		}
	}

	protected override void Init()
	{
		Title = "Test";
		Flyout = new ContentPage
		{
			Title = "Flyout",
			Content = new Button
			{
				AutomationId = "FlyoutButton",
				Text = "Make a new page",
				Command = new Command(() =>
				{
					Detail = new Page1();
					FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
					IsPresented = false;
				})
			}
		};

		Detail = new Page1();

		IsPresented = true;
	}
}
