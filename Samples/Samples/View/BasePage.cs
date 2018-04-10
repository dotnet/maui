using System.Threading.Tasks;
using Samples.ViewModel;
using Xamarin.Forms;

namespace Samples.View
{
    public class BasePage : ContentPage
    {
        public BasePage()
        {
            NavigationPage.SetBackButtonTitle(this, "Back");
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
