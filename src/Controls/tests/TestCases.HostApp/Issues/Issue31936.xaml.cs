using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31936, "Back button glyph (FontImageSource) is not vertically centered on iOS 26", PlatformAffected.iOS)]
public partial class Issue31936 : ContentPage
{
	public Issue31936()
	{
		InitializeComponent();

		BackCommand = new Command(HandleBackCommand);

		BindingContext = this;
	}

	public ICommand BackCommand { get; set; }

	private async void HandleBackCommand()
	{
		await Shell.Current.GoToAsync("..");
	}
}
