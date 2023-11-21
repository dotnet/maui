using System;
using Microsoft.Maui.Controls;

namespace Controls.Sample.UITests.Elements
{
    public partial class StackLayoutSpacing : ContentPage
    {
        public StackLayoutSpacing()
        {
            InitializeComponent();
        }

		void NoSpacingButtonClicked(object sender, EventArgs e)
		{
			TestStackLayout.Spacing = 0;
		}

		void SpacingButtonClicked(object sender, EventArgs e)
		{
			TestStackLayout.Spacing = 40;
		}
	}
}
