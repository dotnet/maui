using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class CollectionViewFeaturePage : NavigationPage
{
	public CollectionViewFeaturePage()
	{
		PushAsync(new CollectionViewFeatureMainPage());
	}
}

public partial class CollectionViewFeatureMainPage : ContentPage
{
	public CollectionViewFeatureMainPage()
	{
		InitializeComponent();
	}

	private async void OnEmptyViewButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new CollectionViewEmptyViewPage());
	}

	private async void OnHeaderFooterViewButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new CollectionViewHeaderPage());
	}

	private async void OnGroupingButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new CollectionViewGroupingPage());
	}

	private async void OnScrollingButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new CollectionViewScrollPage());
	}
	private async void OnDynamicChangesButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new CollectionViewDynamicPage());
	}

	private async void OnSelectionButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new CollectionViewSelectionPage());
	}


	private async void OnItemsSourceButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new CollectionViewItemsSourcePage());
	}
}