using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Collections.Generic;

#if UITEST
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
#endif

    [Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 42364, "ListView item's contextual action menu not being closed upon swiping a TabbedPage in AppCompat")]
    public class Bugzilla42354 : TestTabbedPage
    {
        protected override void Init()
        {
            var list = new ListView
            {
                ItemsSource = new List<string>
                {
                    "Cat",
                    "Dog",
                    "Rat"
                },
                ItemTemplate = new DataTemplate(() =>
                {
                    var cell = new TextCell();
                    cell.SetBinding(TextCell.TextProperty, ".");
                    cell.ContextActions.Add(new MenuItem
                    {
                        Text = "Action",
                        Icon = "icon",
                        IsDestructive = true,
                        Command = new Command(() => DisplayAlert("TITLE", "Context action invoked", "Ok")),
                    });
                    return cell;
                }),
            };

            Children.Add(new ContentPage
            {
                Title = "Page One",
                Content = new StackLayout
                {
                    Children =
                    {
                        new Button
                        {
                            Text = "Go to next page",
                            Command = new Command(() => Navigation.PushAsync(new ContentPage { Title = "Next Page", Content = new Label { Text = "Here" } }))
                        },
                        list
                    }
                }
            });

            Children.Add(new ContentPage { Title = "Page Two" });
        }
    }
}