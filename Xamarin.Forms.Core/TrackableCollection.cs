using System;
using System.Collections.ObjectModel;

namespace Xamarin.Forms
{
	internal class TrackableCollection<T> : ObservableCollection<T>
	{
		public event EventHandler Clearing;

		protected override void ClearItems()
		{
			Clearing?.Invoke(this, EventArgs.Empty);
			base.ClearItems();
		}
	}
}