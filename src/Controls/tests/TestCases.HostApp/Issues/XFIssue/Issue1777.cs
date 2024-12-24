namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 1777, "Adding picker items when picker is in a ViewCell breaks", PlatformAffected.WinPhone)]
public class Issue1777 : TestContentPage
{
	Picker _pickerTable = null;
	Picker _pickerNormal = null;
	string _pickerTableId = "pickerTableId";
	string _btnText = "do magic";

	protected override void Init()
	{
		StackLayout stackLayout = new StackLayout();
		Content = stackLayout;

		var instructions = new Label
		{
			Text = $@"Tap the ""{_btnText}"" button. Then click on the picker inside the Table. The picker should display ""test 0"". If not, the test failed."
		};

		stackLayout.Children.Add(instructions);

		TableView tableView = new TableView();
		stackLayout.Children.Add(tableView);

		TableRoot tableRoot = new TableRoot();
		tableView.Root = tableRoot;

		TableSection tableSection = new TableSection("Table");
		tableRoot.Add(tableSection);

		ViewCell viewCell = new ViewCell();
		tableSection.Add(viewCell);

		ContentView contentView = new ContentView();
#pragma warning disable CS0618 // Type or member is obsolete
		contentView.HorizontalOptions = LayoutOptions.FillAndExpand;
#pragma warning restore CS0618 // Type or member is obsolete
		viewCell.View = contentView;

		_pickerTable = new Picker();
		_pickerTable.AutomationId = _pickerTableId;
#pragma warning disable CS0618 // Type or member is obsolete
		_pickerTable.HorizontalOptions = LayoutOptions.FillAndExpand;
#pragma warning restore CS0618 // Type or member is obsolete
		contentView.Content = _pickerTable;

		Label label = new Label();
		label.Text = "Normal";
		stackLayout.Children.Add(label);

		_pickerNormal = new Picker();
		stackLayout.Children.Add(_pickerNormal);

		Button button = new Button();
		button.Clicked += button_Clicked;
		button.Text = _btnText;
		button.AutomationId = _btnText;
		stackLayout.Children.Add(button);

		//button_Clicked(button, EventArgs.Empty);
		_pickerTable.SelectedIndex = 0;
		_pickerNormal.SelectedIndex = 0;
	}

	void button_Clicked(object sender, EventArgs e)
	{
		_pickerTable.Items.Add("test " + _pickerTable.Items.Count);
		_pickerNormal.Items.Add("test " + _pickerNormal.Items.Count);
	}
}
