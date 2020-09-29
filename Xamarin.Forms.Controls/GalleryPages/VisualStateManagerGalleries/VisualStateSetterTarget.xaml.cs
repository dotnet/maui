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
	public partial class VisualStateSetterTarget : ContentPage
	{
		string _currentColorState = "Normal";

		public VisualStateSetterTarget()
		{
			InitializeComponent();
		}

		void ToggleValid_OnClicked(object sender, EventArgs e)
		{
			if (_currentColorState == "Normal")
			{
				_currentColorState = "Invalid";
			}
			else
			{
				_currentColorState = "Normal";
			}

			CurrentState.Text = $"Current state: {_currentColorState}";
			VisualStateManager.GoToState(TheStack, _currentColorState);
		}
	}
}