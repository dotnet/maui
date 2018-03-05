using Caboodle.Samples.ViewModel;
using Microsoft.Caboodle;
using Xamarin.Forms;

namespace Caboodle.Samples.View
{
    public partial class BatteryPage : ContentPage
    {
        public BatteryPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Battery.BatteryChanged += OnBatteryChanged;
        }

        protected override void OnDisappearing()
        {
            Battery.BatteryChanged -= OnBatteryChanged;

            base.OnDisappearing();
        }

        void OnBatteryChanged(BatteryChangedEventArgs e)
        {
            if (BindingContext is BatteryViewModel vm)
            {
                vm.Update(e);
            }
        }
    }
}
