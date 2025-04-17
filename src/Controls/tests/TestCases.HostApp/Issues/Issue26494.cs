using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26494, "ListView headers gets invisible when collapsing/expanding sections", PlatformAffected.iOS)]
public class Issue26494 : TestContentPage
{
	protected override void Init()
	{
		var listview = new ListView
		{
			IsGroupingEnabled = true,
			HasUnevenRows = true,
			GroupHeaderTemplate = new DataTemplate(typeof(Issue26494GroupHeaderViewCell)),
			ItemsSource = Enumerable.Range(0, 3).Select(i => new Issue26494GroupViewModel(Enumerable.Range(0, 10).Select(j => $"{i}{i}{i}").ToList()) { Title = $"Group{i}" }).ToList()
		};

		Content = new StackLayout
		{
			Children = { listview }
		};
	}


	class Issue26494GroupHeaderViewCell : ViewCell
	{
		public Issue26494GroupHeaderViewCell()
		{

			var tapGesture = new TapGestureRecognizer();
			tapGesture.Tapped += HeaderCell_OnTapped;
			var lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Fill, TextColor = Colors.Black, FontSize = 16 };
			lbl.SetBinding(Label.TextProperty, new Binding("Title"));
			lbl.SetBinding(Label.AutomationIdProperty, new Binding("Title"));
			lbl.GestureRecognizers.Add(tapGesture);
			View = lbl;
		}

		void HeaderCell_OnTapped(object sender, EventArgs e)
		{
			var cell = (Label)sender;
			var vm = cell.BindingContext as Issue26494GroupViewModel;
			vm?.Toggle();
		}
	}

	class Issue26494GroupViewModel : ObservableCollection<string>
	{
		public bool IsCollapsed { get; private set; }

		public string Title { get; set; }

		private readonly List<string> _strings;

		public Issue26494GroupViewModel(List<string> strings)
		{
			_strings = strings;
			UpdateCollection();
		}

		public void Toggle()
		{
			IsCollapsed = !IsCollapsed;
			UpdateCollection();
		}

		private void UpdateCollection()
		{
			if (!IsCollapsed)
			{
				foreach (var item in _strings)
					Add(item);
			}
			else
				Clear();
		}
	}
}