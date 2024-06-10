using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

public sealed class ListItem
{
    public string Identifier { get; } = Guid.NewGuid().ToString("D");
}

[Issue(IssueTracker.Github, 8761, "CollectionView Header Template and Footer Template don't work", PlatformAffected.Android)]
public partial class Issue8761 : ContentPage
{
    public ObservableCollection<ListItem> Items { get; } = new();

    public Issue8761()
    {
        BindingContext = this;
        InitializeComponent();
    }

    private void AddItem(object sender, EventArgs e)
    {
        Items.Add(new ListItem());
    }
}
