using System;
using System.Collections.ObjectModel;
using System.Threading;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	public class Expense
	{
		public string Name { get; private set; }
		public decimal Amount { get; private set; }

		public Expense (string name, decimal amount)
		{
			Name = name;
			Amount = amount;
		}
	}

	[Preserve (AllMembers = true)]
	public class ExpenseListViewCell : ViewCell
	{
		public ExpenseListViewCell ()
		{
			var expenseNameLabel = new Label ();
			expenseNameLabel.SetBinding (Label.TextProperty, "Name");

			var expenseAmountLabel = new Label ();
			var expenseAmountToStringConverter = new GenericValueConverter (obj => string.Format ("{0:C}", ((decimal)obj)));
			expenseAmountLabel.SetBinding (Label.TextProperty, new Binding ("Amount", converter: expenseAmountToStringConverter));

			var layout = new StackLayout ();

			layout.Children.Add (expenseNameLabel);
			layout.Children.Add (expenseAmountLabel);

			View = layout;
		}

		protected override void OnBindingContextChanged ()
		{
			// Fixme : this is happening because the View.Parent is getting 
			// set after the Cell gets the binding context set on it. Then it is inheriting
			// the parents binding context.
			View.BindingContext = BindingContext;
			base.OnBindingContextChanged ();
		}
	}

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.None, 0, "ListView ViewCell binding", PlatformAffected.All)]
	public class ListViewViewCellBinding : TestContentPage
	{

		// Binding issue with view cells
		public ObservableCollection<Expense> Expenses;

		protected override void Init ()
		{
			//BindingContext = this;

			Expenses = new ObservableCollection<Expense> {
				new Expense ("1", 100.0m),
				new Expense ("2", 200.0m),
				new Expense ("3", 300.0m)
			};

			var listView = new ListView ();

			listView.ItemsSource = Expenses;
			listView.ItemTemplate = new DataTemplate (typeof (ExpenseListViewCell));

			var layout = new StackLayout ();
			int numberAdded = 3;

			var label = new Label {
				Text = numberAdded.ToString()
			};

			var removeBtn = new Button { Text = "Remove" };
			removeBtn.Clicked += (s, e) => {
				if (numberAdded > 0) {
					numberAdded--;
					Expenses.RemoveAt (0);
					label.Text = numberAdded.ToString ();
				}
			};
			var addBtn = new Button { Text = "Add" };
			addBtn.Clicked += (s, e) => {
				Expenses.Add (new Expense ("4", 400.0m));
				numberAdded++;
				label.Text = numberAdded.ToString ();
			};

			
			layout.Children.Add (label);
			layout.Children.Add (removeBtn);
			layout.Children.Add (addBtn);
			layout.Children.Add (listView);

			Content = layout;
		}

#if UITEST
		[Test]
		public void ListViewViewCellBindingTestsAllElementsPresent ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Remove"));
			RunningApp.WaitForElement (q => q.Marked ("Add"));
			RunningApp.WaitForElement (q => q.Marked ("1"));
			RunningApp.WaitForElement (q => q.Marked ("$100.00"));
			RunningApp.WaitForElement (q => q.Marked ("2"));
			RunningApp.WaitForElement (q => q.Marked ("$200.00"));
			RunningApp.WaitForElement (q => q.Marked ("3"));
			RunningApp.WaitForElement (q => q.Marked ("$300.00"));

			RunningApp.Screenshot ("All elements exist");
		}

		[Test]
		public void ListViewViewCellBindingTestsAddListItem () 
		{
			RunningApp.Tap (q => q.Button ("Add"));
			RunningApp.WaitForElement (q => q.Marked ("4"));
			RunningApp.WaitForElement (q => q.Marked ("$400.00"));
			RunningApp.Screenshot ("List item added");
		}

		[Test]
		public void ListViewViewCellBindingTestsRemoveListItem () 
		{
			RunningApp.Tap (q => q.Button ("Remove"));
			RunningApp.WaitForNoElement (q => q.Marked ("1"));
			RunningApp.WaitForNoElement (q => q.Marked ("$100.00"));
			RunningApp.Screenshot ("List item removed");
		}
#endif

	}
}
