using System;
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
			if (Device.Idiom == TargetIdiom.Watch)
				NavigationPage.SetHasNavigationBar(this, false);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			SetupBinding(BindingContext);
		}

		protected override void OnDisappearing()
		{
			TearDownBinding(BindingContext);

			base.OnDisappearing();
		}

		protected void SetupBinding(object bindingContext)
		{
			if (bindingContext is BaseViewModel vm)
			{
				vm.DoDisplayAlert += OnDisplayAlert;
				vm.DoNavigate += OnNavigate;
				vm.OnAppearing();
			}
		}

		protected void TearDownBinding(object bindingContext)
		{
			if (bindingContext is BaseViewModel vm)
			{
				vm.OnDisappearing();
				vm.DoDisplayAlert -= OnDisplayAlert;
				vm.DoNavigate -= OnNavigate;
			}
		}

		Task OnDisplayAlert(string message)
		{
			return DisplayAlert(Title, message, "OK");
		}

		Task OnNavigate(BaseViewModel vm, bool showModal)
		{
			var name = vm.GetType().Name;
			name = name.Replace("ViewModel", "Page");

			var ns = GetType().Namespace;
			var pageType = Type.GetType($"{ns}.{name}");

			var page = (BasePage)Activator.CreateInstance(pageType);
			page.BindingContext = vm;

			return showModal
				? Navigation.PushModalAsync(page)
				: Navigation.PushAsync(page);
		}
	}
}
