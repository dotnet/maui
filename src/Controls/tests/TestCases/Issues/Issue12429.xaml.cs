using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 12429, "[iOS] CollectionView Items display issue when Header is resized", PlatformAffected.iOS)]

public partial class Issue12429 : ContentPage
{
	public ObservableCollection<string> ItemList { get; }
	public ObservableCollection<string> ItemListHeader { get; }

	public Command AddCommand => new(() => ItemListHeader.Add($"HeaderItem{ItemListHeader.Count + 1}"));

	public Issue12429()
	{
		InitializeComponent();
		ItemList = new() { "Item1", "Item2", "Itme3" };
		ItemListHeader = new() { "HeaderItem1" };
		BindingContext = this;
	}
}
