using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23608, "The checkbox's checked state color does not update when the IsEnabled property is changed dynamically", PlatformAffected.All)]
public partial class Issue23608 : ContentPage
{
	public Issue23608()
	{
		InitializeComponent();
		BindingContext = new Issue23608ViewModel();
	}
}

public class Issue23608ViewModel : INotifyPropertyChanged
{
	private bool _isEnabled = false;

	public bool IsEnabled
	{
		get => _isEnabled;
		set
		{
			if (_isEnabled != value)
			{
				_isEnabled = value;
				OnPropertyChanged();
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}