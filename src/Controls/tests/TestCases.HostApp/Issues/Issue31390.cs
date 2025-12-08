using System.ComponentModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31390, "System.ArgumentException thrown when setting FlyoutLayoutBehavior dynamically", PlatformAffected.UWP)]
public class Issue31390 : NavigationPage
{
	private Issue31390ViewModel _viewModel;
	public Issue31390()
	{
		_viewModel = new Issue31390ViewModel();
		PushAsync(new Issue31390FlyoutPage(_viewModel));
	}
}

public partial class Issue31390FlyoutPage : FlyoutPage
{
	Issue31390ViewModel _viewModel;
	public Issue31390FlyoutPage(Issue31390ViewModel _viewModel)
	{
		_viewModel = new Issue31390ViewModel();
		BindingContext = _viewModel;

		var flyoutPage = new ContentPage
		{
			Title = "Flyout",
			Content = new StackLayout
			{
				Children =
				{
						new Label
						{
							Text = "This is Flyout",
							AutomationId = "Issue31390_FlyoutLabel"
						}
				}
			}
		};

		var detailPage = new ContentPage
		{
			Content = new StackLayout
			{
				Padding = 16,
				Spacing = 16,
				Children =
				{
					new Label { Text = "This is Detail Page" }
				}
			}
		};

		var navigationPage = new NavigationPage(detailPage)
		{
			Title = "Detail"
		};

		Flyout = flyoutPage;
		Detail = navigationPage;

		var ChangeToPopover = new ToolbarItem
		{
			Text = "GoToNextPage",
			AutomationId = "GoToNextPage"
		};
		ChangeToPopover.Clicked += NavigateToPopover_Clicked;

		ToolbarItems.Add(ChangeToPopover);

		SetBinding(FlyoutLayoutBehaviorProperty, new Binding(nameof(Issue31390ViewModel.FlyoutLayoutBehavior)));
	}

	private async void NavigateToPopover_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new Issue31390ViewModel();
		await Navigation.PushAsync(new Issue31390SecondPage(_viewModel));
	}
}

public class Issue31390SecondPage : ContentPage
{
	private readonly Issue31390ViewModel _viewModel;

	public Issue31390SecondPage(Issue31390ViewModel viewModel)
	{
		_viewModel = viewModel;
		BindingContext = _viewModel;
		ToolbarItems.Add(new ToolbarItem
		{
			Text = "Apply",
			AutomationId = "Apply",
			Command = new Command(() =>
			{
				Navigation.PopAsync();
			})
		});
		var ChangeToPopoverButton = new Button
		{
			Text = "Change to Popover",
			AutomationId = "ChangeToPopover"
		};
		ChangeToPopoverButton.Clicked += ChangeToPopoverButton_Clicked;

		Content = new StackLayout
		{
			Padding = 16,
			Children =
				{
					ChangeToPopoverButton
				}
		};
	}

	private void ChangeToPopoverButton_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button && BindingContext is Issue31390ViewModel vm)
		{

			vm.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

		}
	}
}

public class Issue31390ViewModel : INotifyPropertyChanged
{
	private FlyoutLayoutBehavior _flyoutLayoutBehavior = FlyoutLayoutBehavior.Default;
	public FlyoutLayoutBehavior FlyoutLayoutBehavior
	{
		get => _flyoutLayoutBehavior;
		set
		{
			if (_flyoutLayoutBehavior != value)
			{
				_flyoutLayoutBehavior = value;
				OnPropertyChanged(nameof(FlyoutLayoutBehavior));
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged(string propertyName) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}