using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.ManualTest, "G5", "Button Command CanExecute can disable the control", PlatformAffected.All)]
	public partial class Issue18617 : ContentPage
	{
		string _status;

		public Issue18617()
		{
			InitializeComponent();

			OnCommand = new Command(execute: HandleCommand, canExecute: () => !Toggle);
			OffCommand = new Command(execute: HandleCommand, canExecute: () => Toggle);

			BindingContext = this;
		}

		void HandleCommand()
		{
			Toggle = !Toggle;
			Status = !Toggle ? "OnButton enabled OffButton disabled" : "OnButton disabled OffButton enabled";

			((Command)OnCommand).ChangeCanExecute();
			((Command)OffCommand).ChangeCanExecute();
		}

		public bool Toggle { get; set; } = false;

		public ICommand OnCommand { get; set; }

		public ICommand OffCommand { get; set; }

		public string Status
		{
			get { return _status; }
			set
			{
				_status = value;
				OnPropertyChanged();
			}
		}
	}
}