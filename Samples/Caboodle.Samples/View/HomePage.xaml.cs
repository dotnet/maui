using Caboodle.Samples.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Caboodle.Samples.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
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

			await Navigation.PushAsync((Page)Activator.CreateInstance(item.Page));

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}
