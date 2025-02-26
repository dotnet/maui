using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Samples.ViewModel;

namespace Samples.View
{
	public class BasePage : ContentPage
	{
		public BasePage()
		{
			NavigationPage.SetBackButtonTitle(this, "Back");
			if (DeviceInfo.Idiom == DeviceIdiom.Watch)
				NavigationPage.SetHasNavigationBar(this, false);

			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
		}

		void OnLoaded(object sender, EventArgs e)
		{
			SetupBinding(BindingContext);
		}

		void OnUnloaded(object sender, EventArgs e)
		{
			TearDownBinding(BindingContext);
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
			return DisplayAlertAsync(Title, message, "OK");
		}

		Task OnNavigate(BaseViewModel vm, bool showModal)
		{
			var name = vm.GetType().Name;
			name = name.Replace("ViewModel", "Page", StringComparison.Ordinal);

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
