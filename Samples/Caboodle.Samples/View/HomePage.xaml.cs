using System;
using Caboodle.Samples.Model;
using Xamarin.Forms;

namespace Caboodle.Samples.View
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var item = e.Item as SampleItem;
            if (item == null)
                return;

            await Navigation.PushAsync((Page)Activator.CreateInstance(item.PageType));

            // deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}
