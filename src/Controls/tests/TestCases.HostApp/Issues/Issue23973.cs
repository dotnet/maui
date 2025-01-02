namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, "23973", "Default Modal Page Is Not Transparent", PlatformAffected.All)]
public partial class Issue23973 : ContentPage
{
	public Issue23973()
	{
		Content = new VerticalStackLayout()
		{
			new Label(){
				Text = "Label 1"
			},
			new Button()
			{
				Text = "Click to show opaque default modal page",
				Command = new Command(async () => await Navigation.PushModalAsync(new ModalPage())),
				LineBreakMode = LineBreakMode.WordWrap,
				AutomationId = "PushModal"
			},
			new Button()
			{
				Text = "Click to show transparent modal page",
				Command = new Command(async () => await Navigation.PushModalAsync(new TransparentModalPage())),
				LineBreakMode = LineBreakMode.WordWrap,
				AutomationId = "PushTransparentModal"
			},
			new Label(){
				Text = "Label 2"
			},
			new Label(){
				Text = "Label 3"
			},
		};
	}

	public class TransparentModalPage : ContentPage
	{
		public TransparentModalPage()
		{
			Title = "Transparent Modal Page";
			BackgroundColor = Colors.Transparent;

			Content = new VerticalStackLayout()
			{
				new Button()
				{
					AutomationId = "PopModal",
					Text = "If you do not see through to the underlying page, this test has failed",
					LineBreakMode = LineBreakMode.WordWrap,
					Command = new Command(async () => await Navigation.PopModalAsync())
				}
			};
		}
	}

	public class ModalPage : ContentPage
	{
		public ModalPage()
		{
			Title = "Opaque Modal Page";

			Content = new VerticalStackLayout()
			{
				new Button()
				{
					AutomationId = "PopModal",
					Text = "If you see through to the underlying page, this test has failed",
					LineBreakMode = LineBreakMode.WordWrap,
					Command = new Command(async () => await Navigation.PopModalAsync())
				}
			};
		}
	}
}