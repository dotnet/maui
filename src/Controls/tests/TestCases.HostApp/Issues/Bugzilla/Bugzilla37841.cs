using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 37841, "TableView EntryCells and TextCells cease to update after focus change", PlatformAffected.Android)]
public class Bugzilla37841 : TestContentPage
{
	_37841ViewModel _viewModel;

	protected override void Init()
	{
		_viewModel = new _37841ViewModel();

		var instructions = new Label { FontSize = 16, Text = @"Click on the Generate button. 
The EntryCell should display '12345' and the TextCell should display '6789'. 
Click on the Generate button a second time. 
The EntryCell should display '112358' and the TextCell should display '48151623'." };

		var button = new Button { Text = "Generate", AutomationId = "Generate" };
		button.SetBinding(Button.CommandProperty, nameof(_37841ViewModel.GetNextNumbersCommand));

		var random1 = new EntryCell { IsEnabled = false, Label = "Entry Cell", AutomationId = "entrycell" };
		random1.SetBinding(EntryCell.TextProperty, nameof(_37841ViewModel.Value1));

		var textCell = new TextCell { IsEnabled = false, Detail = "TextCell", AutomationId = "textcell" };
		textCell.SetBinding(TextCell.TextProperty, nameof(_37841ViewModel.Value2));

		var buttonViewCell = new ViewCell { View = button };

		var section = new TableSection("") {
			random1,
			textCell,
			buttonViewCell
		};

		var root = new TableRoot { section };
		var tv = new TableView { Root = root };

		Content = new StackLayout
		{
			Children = { instructions, tv }
		};

		BindingContext = _viewModel;
	}


	public class _37841ViewModel : INotifyPropertyChanged
	{
		public int Value1
		{
			get { return _value1; }
			set
			{
				if (value != _value1)
				{
					_value1 = value;
					RaisePropertyChanged();
				}
			}
		}

		public int Value2
		{
			get { return _value2; }
			set
			{
				if (value != _value2)
				{
					_value2 = value;
					RaisePropertyChanged();
				}
			}
		}

		public Command GetNextNumbersCommand
			=> _getNextNumbersCommand ?? (_getNextNumbersCommand = new Command(ExecuteGenerateRandomCommand));

		class SomeNumbers : IEnumerable<int>
		{
			public IEnumerator<int> GetEnumerator()
			{
				while (true)
				{
					yield return 12345;
					yield return 6789;
					yield return 112358;
					yield return 48151623;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		readonly IEnumerator<int> _numberList = new SomeNumbers().GetEnumerator();

		void ExecuteGenerateRandomCommand()
		{
			_numberList.MoveNext();
			Value1 = _numberList.Current;
			_numberList.MoveNext();
			Value2 = _numberList.Current;
		}

		void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;

			handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#region INotifyPropertyChanged implementation

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		Command _getNextNumbersCommand;
		int _value1;
		int _value2;
	}
}
