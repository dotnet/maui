using System;
using System.Threading.Tasks;

namespace Samples.ViewModel
{
	public class BaseViewModel : ObservableObject
	{
		bool isBusy;

		public bool IsBusy
		{
			get => isBusy;
			set => SetProperty(ref isBusy, value, onChanged: () => OnPropertyChanged(nameof(IsNotBusy)));
		}

		public bool IsNotBusy => !IsBusy;

		public virtual void OnAppearing()
		{
		}

		public virtual void OnDisappearing()
		{
		}

		internal event Func<string, Task> DoDisplayAlert;

		internal event Func<BaseViewModel, bool, Task> DoNavigate;

		public Task DisplayAlertAsync(string message)
		{
			return DoDisplayAlert?.Invoke(message) ?? Task.CompletedTask;
		}

		public Task NavigateAsync(BaseViewModel vm, bool showModal = false)
		{
			return DoNavigate?.Invoke(vm, showModal) ?? Task.CompletedTask;
		}
	}
}
