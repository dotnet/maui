using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 16053, "ListView SelectedItem retains its value after ListView is cleared", PlatformAffected.All)]
    public class Issue16053 : ContentPage
    {
        private ListView ListView1;
        private Label lbl;
        private ObservableCollection<string> Items;

        public Issue16053()
        {
            Items = new ObservableCollection<string>
            {
                "Coffee",
                "Tea",
                "Orange Juice",
                "Milk",
                "Iced Tea",
                "Mango Shake"
            };

            ListView1 = new ListView
            {
                Margin = new Thickness(10),
                BackgroundColor = Colors.LightGray,
                ItemsSource = Items,
                SelectedItem = Items[1]
            };

            lbl = new Label
            {
                Margin = new Thickness(10, 10, 10, 0),
                AutomationId = "SelectedItemLabel"
            };

            var clearButton = new Button
            {
                Text = "Clear",
                AutomationId = "ClearButton"
            };
            clearButton.Clicked += Button_Click_1;

            var changeItemsButton = new Button
            {
                Text = "Change Items",
                AutomationId = "ChangeItemsButton"
            };
            changeItemsButton.Clicked += Button_ChangeItems;

            var removeSelectedItemButton = new Button
            {
                Text = "Remove Selected Item",
                AutomationId = "RemoveSelectedItemButton"
            };
            removeSelectedItemButton.Clicked += Button_RemoveSelectedItem;

            var showSelectedItemButton = new Button
            {
                Text = "Click to show the SelectedItem",
                AutomationId = "ShowSelectedItem"
            };
            showSelectedItemButton.Clicked += Button_Clicked;

            Content = new VerticalStackLayout
            {
                Children =
                {
                    ListView1,
                    clearButton,
                    changeItemsButton,
                    removeSelectedItemButton,
                    showSelectedItemButton,
                    lbl
                }
            };
        }

        private void Button_Click_1(object sender, EventArgs e)
        {
            Items.Clear();
        }

        private void Button_ChangeItems(object sender, EventArgs e)
        {
            Items = new ObservableCollection<string>
            {
                "Water",
                "Soda",
                "Lemon Juice",
                "Milk",
                "Hot Chocolate"
            };
            ListView1.ItemsSource = Items;
            ListView1.SelectedItem = Items[1];
        }

        private void Button_RemoveSelectedItem(object sender, EventArgs e)
        {
            if (ListView1.SelectedItem is string selectedItem)
            {
                Items.Remove(selectedItem);
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            lbl.Text = ListView1.SelectedItem?.ToString() ?? "null";
        }
    }
}
