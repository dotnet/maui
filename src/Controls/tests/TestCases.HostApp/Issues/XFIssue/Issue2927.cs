using System.ComponentModel;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2927, "ListView item tapped not firing multiple times")]
public class Issue2927 : TestContentPage // or TestFlyoutPage, etc .
{

	public class Issue2927Cell : TextCell, INotifyPropertyChanged
	{
		int _numberOfTimesTapped;
		string _cellId;

		public Issue2927Cell(string id)
		{
			_cellId = id;
			NumberOfTimesTapped = 0;
		}

		public int NumberOfTimesTapped
		{
			get { return _numberOfTimesTapped; }
			set
			{
				_numberOfTimesTapped = value;
				Text = _cellId + " " + _numberOfTimesTapped.ToString();
			}
		}
	}

	protected override void Init()
	{
		var cells = new[] {
			new Issue2927Cell ("Cell1"),
			new Issue2927Cell ("Cell2"),
			new Issue2927Cell ("Cell3"),
		};

		BindingContext = cells;
		var template = new DataTemplate(typeof(TextCell));
		template.SetBinding(TextCell.TextProperty, "Text");

		var listView = new ListView
		{
			ItemTemplate = template,
			ItemsSource = cells
		};

		listView.ItemTapped += (s, e) =>
		{
			var obj = (Issue2927Cell)e.Item;
			obj.NumberOfTimesTapped += 1;
		};
		SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		Content = listView;
	}
}
