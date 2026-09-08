namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 58833, "ListView SelectedItem Binding does not fire", PlatformAffected.Android)]
public class Bugzilla58833 : TestContentPage
{
	const string ItemSelectedSuccess = "ItemSelected Success";
	const string TapGestureSucess = "TapGesture Fired";
	Label _resultLabel;
	static Label s_tapGestureFired;


	class TestCell : ViewCell
	{
		readonly Label _content;

		internal static int s_index;

		public TestCell()
		{
			_content = new Label();

			if (s_index % 2 == 0)
			{
				_content.GestureRecognizers.Add(new TapGestureRecognizer
				{
					Command = new Command(() =>
					{
						s_tapGestureFired.Text = TapGestureSucess;
					})
				});
			}

			View = _content;
			ContextActions.Add(new MenuItem { Text = s_index++ + " Action" });
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			_content.Text = (string)BindingContext;
		}
	}

	protected override void Init()
	{
		TestCell.s_index = 0;

		_resultLabel = new Label { Text = "Testing..." };
		s_tapGestureFired = new Label { Text = "Testing..." };

		var items = new List<string>();
		for (int i = 0; i < 5; i++)
			items.Add($"Item #{i}");

		var list = new ListView
		{
			ItemTemplate = new DataTemplate(typeof(TestCell)),
			ItemsSource = items
		};
		list.ItemSelected += List_ItemSelected;

		Content = new StackLayout
		{
			Children = {
				_resultLabel,
				s_tapGestureFired,
				list
			}
		};
	}

	void List_ItemSelected(object sender, SelectedItemChangedEventArgs e)
	{
		_resultLabel.Text = ItemSelectedSuccess;
	}
}