namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7886, "PushModalAsync modal page with Entry crashes on close for MacOS (NRE)", PlatformAffected.macOS)]
public partial class Issue7886 : TestContentPage
{
	const string TriggerModalAutomationId = "TriggerModal";
	const string PopModalAutomationId = "Done";

	public string ButtonAutomationId { get => TriggerModalAutomationId; }

	protected override void Init()
	{
	}
	public Issue7886()
	{
		InitializeComponent();
		BindingContext = this;
	}

	void Handle_Clicked(object sender, EventArgs e)
	{
		Navigation.PushModalAsync(new NavigationPage(new ModalPage()));
	}

	class ModalPage : ContentPage
	{
		public ModalPage()
		{
			BackgroundColor = Colors.Orange;

			var tbi = new ToolbarItem("Done", null, () => Navigation.PopModalAsync())
			{
				AutomationId = PopModalAutomationId
			};

			ToolbarItems.Add(tbi);

			Content = new Entry
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
		}
	}
}