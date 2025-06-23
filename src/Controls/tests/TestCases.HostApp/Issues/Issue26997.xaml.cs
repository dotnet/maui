using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26997, "CollectionView should not crash on iOS 15 and 16", PlatformAffected.iOS)]
public partial class Issue26997 : ContentPage
{
	public ObservableCollection<string> ImagesToDisplay { get; } = new();

	public Issue26997()
	{
		InitializeComponent();
		BindingContext = this;
		AddData();
	}

	void AddData()
	{
		ImagesToDisplay.Add("dotnet_bot.png");
		ImagesToDisplay.Add("dotnet_bot.png");
		ImagesToDisplay.Add("dotnet_bot.png");
	}
}