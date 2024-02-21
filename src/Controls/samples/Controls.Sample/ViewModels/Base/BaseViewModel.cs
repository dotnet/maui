using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.ViewModels.Base
{
	public abstract class BaseViewModel : INotifyPropertyChanged
	{
		protected bool SetProperty<T>(ref T backingStore, T value,
		[CallerMemberName] string propertyName = "",
		Action? onChanged = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			backingStore = value;
			onChanged?.Invoke();
			OnPropertyChanged(propertyName);
			return true;
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}