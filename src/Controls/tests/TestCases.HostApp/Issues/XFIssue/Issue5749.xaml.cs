using System.Collections;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 5749, "Disable horizontal scroll in the custom listview in android")]
public partial class Issue5749 : TestContentPage
{
	public Issue5749()
	{
		InitializeComponent();
		listViewHorizontal.ItemsSource = new string[] { "item1... ", "item2... ", "item3... ", "item4... ", "item5... ", "item6... ", "item7... ", "item8... ", "item9... ", "item10... " };
	}

	protected override void Init()
	{

	}

	void ToggleScrollViewIsEnabled(object sender, EventArgs args)
	{
		listViewHorizontal.IsEnabled = !listViewHorizontal.IsEnabled;
	}
}
public class CustomHorizontalListview : ScrollView
{
	public static readonly BindableProperty ItemsSourceProperty =
		BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(CustomHorizontalListview), default(IEnumerable));

	public IEnumerable ItemsSource
	{
		get { return (IEnumerable)GetValue(ItemsSourceProperty); }
		set { SetValue(ItemsSourceProperty, value); }
	}

	public static readonly BindableProperty ItemTemplateProperty =
		BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(CustomHorizontalListview), default(DataTemplate));

	public DataTemplate ItemTemplate
	{
		get { return (DataTemplate)GetValue(ItemTemplateProperty); }
		set { SetValue(ItemTemplateProperty, value); }
	}

	public static readonly BindableProperty SelectedCommandParameterProperty =
		BindableProperty.Create("SelectedCommandParameter", typeof(object), typeof(CustomHorizontalListview), null);

	public object SelectedCommandParameter
	{
		get { return GetValue(SelectedCommandParameterProperty); }
		set { SetValue(SelectedCommandParameterProperty, value); }
	}
	public void Render()
	{
		if (ItemTemplate == null || ItemsSource == null)
			return;

		var layout = new StackLayout();
		layout.Padding = 20;
		layout.Orientation = Orientation == ScrollOrientation.Vertical ? StackOrientation.Vertical : StackOrientation.Horizontal;

		foreach (var item in ItemsSource)
		{
			var viewCell = ItemTemplate.CreateContent() as ViewCell;
			viewCell.View.BindingContext = item;
			layout.AddLogicalChild(viewCell.View);
		}

		Content = layout;
	}
}