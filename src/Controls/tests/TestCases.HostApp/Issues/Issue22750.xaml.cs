using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 22750, "Using radiobuttons in a group, pressing one button works fine, but pressing the second does not reset the first hence", PlatformAffected.Android)]
	public partial class Issue22750 : ContentPage
	{
		readonly Issue22750ViewModel _vm;

		public Issue22750()
		{
			InitializeComponent();

			BindingContext = _vm = new Issue22750ViewModel();
		}
		
		void OnRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			RadioButton button = sender as RadioButton;

			if (button.Content.ToString() == "Yes")
				_vm.Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
			else
				_vm.Text = "Lorem ipsum dolor sit amet 2.";
		}
	}

	public class Issue22750ViewModel : BindableObject
	{
		string _stringInQuestion;

		public Issue22750ViewModel()
		{
			Text = "Lorem ipsum dolor sit amet 1.";
		}

		public string Text
		{
			get { return _stringInQuestion; }
			set
			{
				_stringInQuestion = value;
				OnPropertyChanged();
			}
		}
	}
}