#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	public abstract class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public virtual void OnAppearing()
		{
		}

		protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool Set<T>(ref T destination, T value, Action? onChanged = null, [CallerMemberName] string? propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(destination, value))
				return false;

			destination = value;

			RaisePropertyChanged(propertyName);
			onChanged?.Invoke();

			return true;
		}
	}
}