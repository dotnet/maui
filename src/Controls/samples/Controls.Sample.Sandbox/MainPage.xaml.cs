using System.Collections.ObjectModel;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}
}
public class CollectionViewViewModel
{
	public ObservableCollection<string> ItemList { get; set; }

	public CollectionViewViewModel()
	{
		ItemList = new ObservableCollection<string>();
	}
}