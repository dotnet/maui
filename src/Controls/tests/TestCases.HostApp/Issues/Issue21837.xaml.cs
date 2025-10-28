using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21837, "Span's TapGestureRecognizer not working if text is truncated", PlatformAffected.All)]

public partial class Issue21837 : ContentPage
{
	private int _tapCount;
	public int TapCount
	{
		get => _tapCount;
		set
		{
			_tapCount = value;
			OnPropertyChanged();
		}
	}

	public Command TapCommand { get; set; }

	public Issue21837()
	{
		InitializeComponent();
		TapCommand = new Command(() => ++TapCount);
		BindingContext = this;
	}
}
