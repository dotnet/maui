using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37427, "CollectionView item content renders with zero width on iOS", PlatformAffected.iOS | PlatformAffected.macOS)]
public partial class Issue37427 : ContentPage
{
	static readonly Color[] s_palette =
	[
		Colors.Red,
		Colors.Orange,
		Colors.Gold,
		Colors.Green,
		Colors.Blue,
	];

	public Issue37427()
	{
		InitializeComponent();

		Items = new ObservableCollection<Issue37427Item>(
			Enumerable.Range(1, 10).Select(index =>
				new Issue37427Item($"Card {index}", s_palette, "dotnet_bot.png")));

		BindingContext = this;
	}

	public ObservableCollection<Issue37427Item> Items { get; }
}

public sealed record Issue37427Item(string Name, IEnumerable<Color> PreviewColors, string ImageSource)
{
	public string AutomationId => $"37427{Name.Replace(" ", string.Empty, StringComparison.Ordinal)}";
}
