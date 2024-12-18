using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26669, "[Windows] CollectionView ScrollTo method crashes on with invalid values", PlatformAffected.UWP)]
	public partial class Issue26669 : ContentPage
	{
		public Issue26669()
		{
			InitializeComponent();
			List<CollectionViewGroup> teams = new List<CollectionViewGroup>
			{
				new CollectionViewGroup("Team A", new List<string> { "Member 1", "Member 2" }),
				new CollectionViewGroup("Team B", new List<string> { "Member 3", "Member 4" })
			};
			
			collectionView.ItemsSource = teams;
			ScrollTo.Clicked += ScrollToClicked;
		}

		void ScrollToClicked(object sender, EventArgs e)
		{
			collectionView.ScrollTo(0, 55);
		}
	}

	class CollectionViewGroup : List<string>
	{
		public string Name { get; }

		public CollectionViewGroup(string name, List<string> members) : base(members)
		{
			Name = name;
		}
	}
}