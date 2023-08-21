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

using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.VisualStateManagerGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class StateTriggerToggleGallery : ContentPage
	{
		public StateTriggerToggleGallery()
		{
			InitializeComponent();
			BindingContext = new StateTriggerToggleGalleryViewModel();
		}

		public class StateTriggerToggleGalleryViewModel : BindableObject
		{
			bool _toggleState;
			bool _toggleStateInverted;

			public StateTriggerToggleGalleryViewModel()
			{
				ToggleState = false;
			}

			public bool ToggleState
			{
				get => _toggleState;
				set
				{
					_toggleState = value;
					OnPropertyChanged(nameof(ToggleState));
					ToggleStateInverted = !value;
				}
			}

			public bool ToggleStateInverted
			{
				get => _toggleStateInverted;
				set
				{
					_toggleStateInverted = value;
					OnPropertyChanged(nameof(ToggleStateInverted));
				}
			}
		}
	}
}