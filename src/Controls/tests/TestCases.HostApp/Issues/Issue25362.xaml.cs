using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 25362, "[iOS] CollectionView Items display issue when Header is resized", PlatformAffected.iOS)]

public partial class Issue25362 : ContentPage
{
	public ObservableCollection<string> ItemList { get; }
	public ObservableCollection<string> ItemListHeader { get; }

	public Command AddCommand => new(() => ItemListHeader.Add($"HeaderItem{ItemListHeader.Count + 1}"));

	public Issue25362()
	{
		InitializeComponent();
		ItemList = new() { "Item1", "Item2", "Item3" };
		ItemListHeader = new() { "HeaderItem1" };
		BindingContext = this;
	}
}
