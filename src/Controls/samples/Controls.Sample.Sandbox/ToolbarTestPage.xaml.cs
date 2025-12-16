using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
    public partial class ToolbarTestPage : ContentPage
    {
        private int _toolbarItemCounter = 1;

        public ToolbarTestPage()
        {
            InitializeComponent();
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            StatusLabel.Text = "Status: Add clicked";
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            StatusLabel.Text = "Status: Edit clicked";
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            StatusLabel.Text = "Status: Delete clicked";
        }

        private void OnShareClicked(object sender, EventArgs e)
        {
            StatusLabel.Text = "Status: Share clicked";
        }

        private void OnMoreClicked(object sender, EventArgs e)
        {
            StatusLabel.Text = "Status: More clicked";
        }

        private void OnAddToolbarItemClicked(object sender, EventArgs e)
        {
            var newItem = new ToolbarItem
            {
                Text = $"Dynamic {_toolbarItemCounter++}",
                Order = ToolbarItemOrder.Secondary
            };
            newItem.Clicked += (s, args) => StatusLabel.Text = $"Status: {newItem.Text} clicked";
            ToolbarItems.Add(newItem);
            StatusLabel.Text = $"Status: Added {newItem.Text} toolbar item";
        }

        private void OnRemoveToolbarItemClicked(object sender, EventArgs e)
        {
            if (ToolbarItems.Count > 0)
            {
                var lastItem = ToolbarItems[ToolbarItems.Count - 1];
                ToolbarItems.Remove(lastItem);
                StatusLabel.Text = $"Status: Removed toolbar item";
            }
            else
            {
                StatusLabel.Text = "Status: No toolbar items to remove";
            }
        }

        private void OnToggleEnabledClicked(object sender, EventArgs e)
        {
            if (ToolbarItems.Count > 0)
            {
                var firstItem = ToolbarItems[0];
                firstItem.IsEnabled = !firstItem.IsEnabled;
                StatusLabel.Text = $"Status: First toolbar item enabled = {firstItem.IsEnabled}";
            }
        }

        private void OnChangeTitleClicked(object sender, EventArgs e)
        {
            Title = $"Changed {DateTime.Now:HH:mm:ss}";
            StatusLabel.Text = $"Status: Title changed to '{Title}'";
        }

        private void OnLongTitleClicked(object sender, EventArgs e)
        {
            Title = "This is a Very Long Title That Should Test Text Wrapping or Truncation";
            StatusLabel.Text = "Status: Long title set";
        }

        private void OnResetTitleClicked(object sender, EventArgs e)
        {
            Title = "Toolbar Test";
            StatusLabel.Text = "Status: Title reset";
        }
    }
}
