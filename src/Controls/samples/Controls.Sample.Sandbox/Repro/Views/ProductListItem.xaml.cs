using System.Diagnostics;
using FFImageLoading.Maui;

namespace AllTheLists.Views;

public partial class ProductListItem : ContentView
{
	public ProductListItem()
	{
		InitializeComponent();
	}

	void CachedImage_Error(object sender, CachedImageEvents.ErrorEventArgs e)
	{
		// Handle error
		Debug.WriteLine(e.Exception.Message);
		Debug.WriteLine(((CachedImage)sender).Source.ToString());
	}
}