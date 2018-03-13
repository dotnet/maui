using Caboodle.Samples.ViewModel;
using Xamarin.Forms;

namespace Caboodle.Samples.View
{
    public class BasePage : ContentPage
    {
        public BasePage()
        {
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is BaseViewModel vm)
            {
                vm.OnAppearing();
            }
        }

        protected override void OnDisappearing()
        {
            if (BindingContext is BaseViewModel vm)
            {
                vm.OnDisappearing();
            }

            base.OnDisappearing();
        }
    }
}
