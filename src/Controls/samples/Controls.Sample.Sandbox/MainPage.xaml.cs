using System.Collections.ObjectModel;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public ObservableCollection<string> Items { get; } = new(Enumerable.Range(1, 20).Select(i => $"Item {i}"));

	public MainPage()
	{
		InitializeComponent();
		collectionView.ItemsSource = Items;
	}

	void OnReorderCompleted(object? sender, EventArgs e)
	{
		reorderStatusLabel.Text = $"ReorderCompleted: {DateTime.Now:T}";

		System.Diagnostics.Debug.WriteLine("Reorder completed. New order:");
		for (int i = 0; i < Items.Count; i++)
		{
			System.Diagnostics.Debug.WriteLine($"  [{i}] {Items[i]}");
		}
	}

	async void OnGoToGroupedReorderClicked(object? sender, EventArgs e)
	{
		await Navigation.PushAsync(new GroupedReorderPage());
	}
}

public class Member
{
	public string Name { get; set; }
	public Member(string name) => Name = name;
	public override string ToString() => Name;
}

public class ObservableTeam : ObservableCollection<Member>
{
	public string Name { get; set; }

	public ObservableTeam(string name, List<Member> members) : base(members)
	{
		Name = name;
	}

	public override string ToString() => Name;
}

public class ObservableSuperTeams : ObservableCollection<ObservableTeam>
{
	public ObservableSuperTeams()
	{
		Add(new ObservableTeam("Avengers", new List<Member>
		{
			new("Thor"),
			new("Captain America"),
			new("Iron Man"),
			new("The Hulk"),
		}));

		Add(new ObservableTeam("Fantastic Four", new List<Member>
		{
			new("The Thing"),
			new("Human Torch"),
			new("Invisible Woman"),
			new("Mr. Fantastic"),
		}));

		// Empty group — tests dragging items into an empty group
		Add(new ObservableTeam("New Recruits", new List<Member>()));

		Add(new ObservableTeam("Defenders", new List<Member>
		{
			new("Doctor Strange"),
			new("Namor"),
			new("Silver Surfer"),
		}));
	}
}