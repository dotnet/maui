using System;
using System.Threading.Tasks;
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
                vm.DoDisplayAlert += OnDisplayAlert;
                vm.OnAppearing();
            }
        }

        protected override void OnDisappearing()
        {
            if (BindingContext is BaseViewModel vm)
            {
                vm.OnDisappearing();
                vm.DoDisplayAlert -= OnDisplayAlert;
            }

            base.OnDisappearing();
        }

        Task OnDisplayAlert(string message)
        {
            return DisplayAlert(Title, message, "OK");
        }
    }
}
