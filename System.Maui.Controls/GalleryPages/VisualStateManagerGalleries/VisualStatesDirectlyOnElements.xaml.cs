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
	public partial class VisualStatesDirectlyOnElements : ContentPage
	{
		string _currentColorState = "Normal";
		string _currentAlignmentState = "LeftAligned";

		public VisualStatesDirectlyOnElements ()
		{
			InitializeComponent ();
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

			CurrentState.Text = $"{_currentColorState}, {_currentAlignmentState}";
			VisualStateManager.GoToState(ALabel, _currentColorState);
			VisualStateManager.GoToState(AButton, _currentColorState);
		}

		void ToggleAlignment_OnClicked(object sender, EventArgs e)
		{
			if (_currentAlignmentState == "LeftAligned")
			{
				_currentAlignmentState = "Centered";
			}
			else
			{
				_currentAlignmentState = "LeftAligned";
			}

			CurrentState.Text = $"{_currentColorState}, {_currentAlignmentState}";
			VisualStateManager.GoToState(ALabel, _currentAlignmentState);
			VisualStateManager.GoToState(AButton, _currentAlignmentState);
		}
	}
}