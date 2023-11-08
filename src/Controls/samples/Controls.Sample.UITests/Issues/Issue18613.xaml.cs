using System;
using System.Diagnostics.Metrics;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "G1", "Disabled Button cannot trigger Command", PlatformAffected.All)]
	public partial class Issue18613 : ContentPage
	{
		public Issue18613()
		{
			InitializeComponent();
		}
		
		void OnButtonClicked(object sender, EventArgs e)
		{
			Console.WriteLine("Button_Clicked");
		}
	}

	public class Issue18613ViewModel : BindableObject
	{
		int _count;
		string _counter;

		public Issue18613ViewModel()
		{
			Command = new Command(HandleCommand);
		}

		public bool IsButtonEnabled { get; set; } = false;

		public ICommand Command { get; set; }

		public string Counter
		{
			get { return _counter; }
			set
			{
				_counter = value;
				OnPropertyChanged();
			}
		}

		void HandleCommand()
		{
			_count++;
			Counter = $"Tapped {_count} times";
			Console.WriteLine("HandleCommand");
		}
	}
}