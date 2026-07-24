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

		internal event Func<string, string, string, string, Task<bool>> DoDisplayConfirm;

		internal event Func<string, string, string, Task<string>> DoDisplayPrompt;

		internal event Func<BaseViewModel, bool, Task> DoNavigate;

		public Task DisplayAlertAsync(string message)
		{
			return DoDisplayAlert?.Invoke(message) ?? Task.CompletedTask;
		}

		public Task<bool> DisplayConfirmAsync(string title, string message, string accept, string cancel)
		{
			return DoDisplayConfirm?.Invoke(title, message, accept, cancel) ?? Task.FromResult(false);
		}

		public Task<string> DisplayPromptAsync(string title, string message, string initialValue)
		{
			return DoDisplayPrompt?.Invoke(title, message, initialValue) ?? Task.FromResult<string>(null);
		}

		public Task NavigateAsync(BaseViewModel vm, bool showModal = false)
		{
			return DoNavigate?.Invoke(vm, showModal) ?? Task.CompletedTask;
		}
	}
}
