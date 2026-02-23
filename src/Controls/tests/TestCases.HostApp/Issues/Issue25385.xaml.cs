namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25385, "DataTrigger doesn't change shape background color", PlatformAffected.All)]
	public partial class Issue25385 : ContentPage
	{
		public Issue25385()
		{
			InitializeComponent();
			BindingContext = new Issue25385ViewModel();
		}

		public class Issue25385ViewModel : ViewModel
		{
			private bool _isActive;
			public bool IsActive
			{
				get => _isActive;
				set
				{
					if (_isActive != value)
					{
						_isActive = value;
						OnPropertyChanged();
					}
				}
			}

			public Command ChangeColorCommand => new(() => IsActive = !IsActive);
		}
	}
}