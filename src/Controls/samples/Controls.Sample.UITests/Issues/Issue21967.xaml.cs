using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
 
namespace Maui.Controls.Sample.Issues;
 
[Issue(IssueTracker.Github, 21967, "CollectionView causes invalid measurements on resize", PlatformAffected.Android)]
public partial class Issue21967 : ContentPage
{
    public Issue21967()
    {
        InitializeComponent();
        cv.ItemsSource = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" };
        button.Clicked += (_, _) =>
        {
            if (cv.WidthRequest == 200)
            {
                cv.WidthRequest = 100;
            }
            else
            {
                cv.WidthRequest = 200;
            }
        };
    }
}