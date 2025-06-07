namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, "25256", "ListView ScrolledEventArgs.ScrollY does not reset when ItemsSource changes", PlatformAffected.Android)]
	public partial class Issue25256 : ContentPage
	{

		public Issue25256()
		{
			InitializeComponent();
			groupedList.ItemsSource = new List<GroupedList>()
			{
				new GroupedList("Group 1", Enumerable.Range(0, 100).Select(i => i.ToString()).ToList()),
			};

			list.ItemsSource = Enumerable.Range(0, 100).Select(i => i.ToString()).ToList();
		}

		private void ListView_Scrolled(object sender, ScrolledEventArgs e)
		{
			scrollYLabel.Text = e.ScrollY.ToString();
		}

		private void GroupedListView_Scrolled(object sender, ScrolledEventArgs e)
		{
			scrollYLabelGroupedList.Text = e.ScrollY.ToString();
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			groupedList.ItemsSource = new List<GroupedList>()
			{
				new GroupedList("Group 2", Enumerable.Range(100, 100).Select(i => i.ToString()).ToList()),
			};

			list.ItemsSource = Enumerable.Range(100, 100).Select(i => i.ToString()).ToList();
		}

		public class GroupedList : List<string>
		{
			public string Title { get; private set; }

			public GroupedList(string title, List<string> items) : base(items)
			{
				Title = title;
			}
		}
	}
}