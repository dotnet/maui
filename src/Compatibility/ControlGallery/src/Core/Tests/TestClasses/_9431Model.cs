//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Tests
{
	public class _9431Model : System.ComponentModel.INotifyPropertyChanged
	{
		Color _bGColor;

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
		}

		public Color BGColor
		{
			get => _bGColor;
			set
			{
				_bGColor = value;
				OnPropertyChanged(nameof(BGColor));
			}
		}
	}
}
