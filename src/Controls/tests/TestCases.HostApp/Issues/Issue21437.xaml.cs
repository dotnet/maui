using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21437, "Removing TapGestureRecognizer with at least 2 taps causes Exception", PlatformAffected.Android)]

public partial class Issue21437 : ContentPage
{
	public ObservableCollection<string> Items { get; } = new() { "Item1", "Item2", "Item3" };

	public Command TapCommand => new Command<string>(obj =>
	{
		Items.Remove(obj);
	});

	public Issue21437()
	{
		InitializeComponent();
		BindingContext = this;
	}
}
