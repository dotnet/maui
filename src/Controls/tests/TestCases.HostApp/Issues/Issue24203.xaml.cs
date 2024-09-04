using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24203, "FlyoutItemIsVisible is not working with bindings", PlatformAffected.All)]
public partial class Issue24203
{
	public Issue24203()
	{
		InitializeComponent();

		BindingContext = this;
	}

	private bool showFlyoutItem = true;
	public bool ShowFlyoutItem
	{
		get => showFlyoutItem;
		set
		{
			showFlyoutItem = value;
			OnPropertyChanged();
		}
	}

	public ICommand ToggleFlyoutItemCommand => new Command(() => ShowFlyoutItem = !ShowFlyoutItem);
	public ICommand ToggleFlyoutItemWithAttachedPropertyCommand => new Command(() => Shell.SetFlyoutItemIsVisible(Page1, !Shell.GetFlyoutItemIsVisible(Page1)));
}