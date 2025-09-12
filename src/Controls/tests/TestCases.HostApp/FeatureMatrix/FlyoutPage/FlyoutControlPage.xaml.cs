using System;
using Microsoft.Maui.Controls;
namespace Maui.Controls.Sample;

public class FlyoutControlPage : NavigationPage
{
	private FlyoutPageViewModel _viewModel;
	public FlyoutControlPage()
	{
		_viewModel = new FlyoutPageViewModel();
		PushAsync(new FlyoutControlMainPage(_viewModel));
	}
}
public partial class FlyoutControlMainPage : FlyoutPage
{
	private FlyoutPageViewModel _viewModel;
	private Page _originalFlyoutPage;
	public FlyoutControlMainPage(FlyoutPageViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
		NavigationPage.SetHasNavigationBar(this, false);
		_originalFlyoutPage = this.Flyout;
	}
	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new FlyoutPageViewModel();
		NavigatedToLabel.Text = "NavigatedTo: ";
		NavigatingFromLabel.Text = "NavigatingFrom: ";
		NavigatedFromLabel.Text = "NavigatedFrom: ";
		await Navigation.PushAsync(new FlyoutOptionsPage(_viewModel));
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

#if WINDOWS
    if (FlyoutLayoutBehavior == FlyoutLayoutBehavior.Split ||
        FlyoutLayoutBehavior == FlyoutLayoutBehavior.Default)
    {
        SetBinding(FlyoutPage.IsPresentedProperty,
            new Binding(nameof(FlyoutPageViewModel.IsPresented),
                        mode: BindingMode.OneWayToSource));
    }
    else
    {
        SetBinding(FlyoutPage.IsPresentedProperty,
            new Binding(nameof(FlyoutPageViewModel.IsPresented),
                        mode: BindingMode.TwoWay));
    }
#else
		if (FlyoutLayoutBehavior == FlyoutLayoutBehavior.Split)
		{
			SetBinding(FlyoutPage.IsPresentedProperty,
				new Binding(nameof(FlyoutPageViewModel.IsPresented),
							mode: BindingMode.OneWayToSource));
		}
		else
		{
			SetBinding(FlyoutPage.IsPresentedProperty,
				new Binding(nameof(FlyoutPageViewModel.IsPresented),
							mode: BindingMode.TwoWay));
		}
#endif
	}


	private void OnPageNavigatedTo(object sender, NavigatedToEventArgs e)
	{
		if (sender is Page page)
		{
			NavigatedToLabel.Text = $"NavigatedTo: {page.Title}";
		}
	}

	private void OnPageNavigatingFrom(object sender, NavigatingFromEventArgs e)
	{
		if (sender is Page page)
		{
			NavigatingFromLabel.Text = $"NavigatingFrom: {page.Title}";
		}
	}

	private void OnPageNavigatedFrom(object sender, NavigatedFromEventArgs e)
	{
		if (sender is Page page)
		{
			NavigatedFromLabel.Text = $"NavigatedFrom: {page.Title}";
		}
	}

	private async void OnGoToNewPageClicked(object sender, EventArgs e)
	{
		var newFlyoutPage = new FlyoutPage
		{
			FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover,
			Flyout = new ContentPage
			{
				Title = "New Flyout",
				IconImageSource = "coffee.png",
				Content = new VerticalStackLayout
				{
					Children =
				{
					new Label { Text = "Flyout for New Page" }
				}
				}
			},
			Detail = new NavigationPage(new ContentPage
			{
				Title = "New Flyout Detail",
				Content = new VerticalStackLayout
				{
					Children =
				{
					new Label { Text = "This is the detail of the new FlyoutPage" },
					new Button
					{
						Text = "Close New FlyoutPage",
						AutomationId = "CloseNewFlyoutPageButton",
						Command = new Command(async () =>
						{
							await Application.Current.MainPage.Navigation.PopModalAsync();
						})
					}
				}
				}
			})
		};
		await Application.Current.MainPage.Navigation.PushModalAsync(newFlyoutPage);
	}


	private void OnSetFlyout1Clicked(object sender, EventArgs e)
	{
		this.Flyout = new ContentPage
		{
			Title = "Flyout 1",
			Content = new VerticalStackLayout
			{
				Children =
			{
				new Label { Text = "Flyout 1 - Item 1" },
				new Label { Text = "Flyout 1 - Item 2" },
				new Button {
					Text = "Back to Original Flyout",
					AutomationId = "BackToOriginalFlyoutButton1",
					Command = new Command(() =>
					{
						RestoreOriginalFlyoutPage();
						this.IsPresented = false;
					})
				}
			}
			}
		};
	}

	private void OnSetFlyout2Clicked(object sender, EventArgs e)
	{
		this.Flyout = new ContentPage
		{
			Title = "Flyout 2",
			Content = new VerticalStackLayout
			{
				Children =
			{
				new Label { Text = "Flyout 2 - Item 1" },
				new Label { Text = "Flyout 2 - Item 2" },
				new Button {
					Text = "Back to Original Flyout",
					AutomationId = "BackToOriginalFlyoutButton2",
					Command = new Command(() =>
					{
						RestoreOriginalFlyoutPage();
						this.IsPresented = false;
					})
				}
			}
			}
		};
	}

	private void RestoreOriginalFlyoutPage()
	{
		if (_originalFlyoutPage != null)
			this.Flyout = _originalFlyoutPage;
	}
	private async void OnSetDetail1Clicked(object sender, EventArgs e)
	{
		if (this.Detail is NavigationPage navPage)
		{
			var detailPage = new ContentPage
			{
				Title = "Detail 1",
				Content = new VerticalStackLayout
				{
					Children =
						{
							new Label { Text = "Detail 1 - Content" },
							new Button {
								Text = "Back to Original Page",
								AutomationId = "BackToOriginalDetailButton1",
								Command = new Command(async () => await navPage.PopAsync())
							}
						}
				}
			};
			detailPage.NavigatedTo += OnPageNavigatedTo;
			detailPage.NavigatingFrom += OnPageNavigatingFrom;
			detailPage.NavigatedFrom += OnPageNavigatedFrom;
			await navPage.PushAsync(detailPage);
		}
	}

	private async void OnSetDetail2Clicked(object sender, EventArgs e)
	{
		if (this.Detail is NavigationPage navPage)
		{
			var detailPage = new ContentPage
			{
				Title = "Detail 2",
				Content = new VerticalStackLayout
				{
					Children =
						{
							new Label { Text = "Detail 2 - Content" },
							new Button {
								Text = "Back to Original Page",
								AutomationId = "BackToOriginalDetailButton2",
								Command = new Command(async () => await navPage.PopAsync())
							}
						}
				}
			};
			detailPage.NavigatedTo += OnPageNavigatedTo;
			detailPage.NavigatingFrom += OnPageNavigatingFrom;
			detailPage.NavigatedFrom += OnPageNavigatedFrom;

			await navPage.PushAsync(detailPage);
		}
	}

	void OnCloseFlyoutClicked(object sender, EventArgs e)
	{
		this.IsPresented = false;
	}

	void OnOpenFlyoutClicked(object sender, EventArgs e)
	{
		this.IsPresented = true;
	}
}
