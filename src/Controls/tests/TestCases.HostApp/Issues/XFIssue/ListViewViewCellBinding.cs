using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

public class Expense
{
	public string Name { get; private set; }
	public decimal Amount { get; private set; }

	public Expense(string name, decimal amount)
	{
		Name = name;
		Amount = amount;
	}
}

public class ExpenseListViewCell : ViewCell
{
	public ExpenseListViewCell()
	{
		var expenseNameLabel = new Label();
		expenseNameLabel.SetBinding(Label.TextProperty, "Name");

		var expenseAmountLabel = new Label();
		expenseAmountLabel.SetBinding(Label.AutomationIdProperty, new Binding("Amount"));
		var expenseAmountToStringConverter = new GenericValueConverter(obj => string.Format("{0:C}", ((decimal)obj)));
		expenseAmountLabel.SetBinding(Label.TextProperty, new Binding("Amount", converter: expenseAmountToStringConverter));

		var layout = new StackLayout(){
			expenseNameLabel,
			expenseAmountLabel
		};

		View = layout;
	}

	protected override void OnBindingContextChanged()
	{
		// Fixme : this is happening because the View.Parent is getting 
		// set after the Cell gets the binding context set on it. Then it is inheriting
		// the parents binding context.
		View.BindingContext = BindingContext;
		base.OnBindingContextChanged();
	}
}

[Issue(IssueTracker.None, 0, "ListView ViewCell binding", PlatformAffected.All)]
public class ListViewViewCellBinding : TestContentPage
{

	// Binding issue with view cells
	public ObservableCollection<Expense> Expenses;

	protected override void Init()
	{
		//BindingContext = this;

		Expenses = new ObservableCollection<Expense> {
			new Expense ("1", 100.0m),
			new Expense ("2", 200.0m),
			new Expense ("3", 300.0m)
		};

		var listView = new ListView();

		listView.ItemsSource = Expenses;
		listView.ItemTemplate = new DataTemplate(typeof(ExpenseListViewCell));

		int numberAdded = 3;

		var label = new Label
		{
			Text = numberAdded.ToString()
		};

		var removeBtn = new Button { Text = "Remove" };
		removeBtn.Clicked += (s, e) =>
		{
			if (numberAdded > 0)
			{
				numberAdded--;
				Expenses.RemoveAt(0);
				label.Text = numberAdded.ToString();
			}
		};
		var addBtn = new Button { Text = "Add" };
		addBtn.Clicked += (s, e) =>
		{
			Expenses.Add(new Expense("4", 400.0m));
			numberAdded++;
			label.Text = numberAdded.ToString();
		};

		var layout = new StackLayout()
		{
			label,
			removeBtn,
			addBtn,
			listView,
		};

		Content = layout;
	}
}
