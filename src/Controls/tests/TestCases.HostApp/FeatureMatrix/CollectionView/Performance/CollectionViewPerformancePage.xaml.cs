using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class CollectionViewPerformancePage : ContentPage
{
	public CollectionViewPerformancePage()
	{
		InitializeComponent();
	}

	private async void OnSimpleTemplateButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new SimpleTemplatePerformancePage());
	}

	private async void OnComplexTemplateButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new ComplexTemplatePerformancePage());
	}

	private async void OnScrollingPerformanceButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new ScrollingPerformancePage());
	}

	private async void OnGridSpanButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new GridSpanPerformancePage());
	}

	private async void OnGroupingOrientationButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new GroupingOrientationPerformancePage());
	}
}
