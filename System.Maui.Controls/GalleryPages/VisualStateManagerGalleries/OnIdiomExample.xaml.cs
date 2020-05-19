using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.VisualStateManagerGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class OnIdiomExample : ContentPage
	{
		const string DefaultState = "Normal";
		string _currentState = DefaultState;

		public OnIdiomExample ()
		{
			InitializeComponent ();
		}

		void Button_OnClicked(object sender, EventArgs e)
		{
			if (_currentState == DefaultState)
			{
				_currentState = "CustomState";
				VisualStateManager.GoToState(DemoLabel, _currentState);
				ToggleButton.Text = "Change Label to Normal state";

			}
			else
			{
				_currentState = DefaultState;
				VisualStateManager.GoToState(DemoLabel, _currentState);
				ToggleButton.Text = "Change Label to Custom state";
			}
		}
	}
}