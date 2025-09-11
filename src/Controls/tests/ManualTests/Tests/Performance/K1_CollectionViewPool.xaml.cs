using Microsoft.Maui.ManualTests.Categories;
using Microsoft.Maui.ManualTests.Performance.CollectionViewPool;

namespace Microsoft.Maui.ManualTests.Tests.Performance;

[Test(
	id: "K1",
	title: "CollectionViewPool",
	category: Category.Performance)]
public partial class K1_CollectionViewPool : ContentPage
{
	public K1_CollectionViewPool()
	{
		InitializeComponent();
	}

	private async void Button_Clicked(object sender, EventArgs e)
	{
		var page = new PoolPage();
		await page.LoadLogs();

		try
		{
			await this.Navigation.PushAsync(page, true);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
		}
	}
}