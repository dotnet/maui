﻿using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29411, "[Android] CarouselView.Loop = false causes crash on Android when changed at runtime", PlatformAffected.Android)]
public partial class Issue29411 : ContentPage
{
    Issue29411_ViewModel _viewModel;
    public Issue29411()
    {
        InitializeComponent();
		_viewModel = new Issue29411_ViewModel();
        BindingContext = _viewModel;
	}

    void OnButtonClicked(object sender, EventArgs e)
    {
        if (carouselView.Loop)
        {
            carouselView.Loop = false;
             
        }
    }
}


public class Issue29411_ViewModel : BindableObject
{
    public ObservableCollection<string> ItemsSource { get; set; }
 
	public DataTemplate ItemTemplate { get; set; }
   
    public Issue29411_ViewModel()
    {
        ItemsSource = new ObservableCollection<string>
        {
            "Item 1",
            "Item 2",
            "Item 3",
            "Item 4",
            "Item 5",
        };
 
       ItemTemplate = new DataTemplate(() =>
        {
            var label = new Label
            {
                Margin = new Thickness(10),
                FontSize = 18,
                AutomationId = "Label", 
                BackgroundColor = Colors.LightGray
                 
            };
            label.SetBinding(Label.TextProperty, ".");
            return label;
        });
    }
}