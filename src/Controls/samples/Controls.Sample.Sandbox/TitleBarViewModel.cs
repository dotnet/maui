using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample
{
	public class TitleBarViewModel : BaseViewModel
	{
		private string _title = "";
		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				OnPropertyChanged();
			}
		}

		private string _subtitle = "";
		public string Subtitle
		{
			get { return _subtitle; }
			set
			{
				_subtitle = value;
				OnPropertyChanged();
			}
		}

		private bool _showTitleBar = true;
		public bool ShowTitleBar
		{
			get { return _showTitleBar; }
			set
			{
				_showTitleBar = value;
				OnPropertyChanged();
			}
		}
	}

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
