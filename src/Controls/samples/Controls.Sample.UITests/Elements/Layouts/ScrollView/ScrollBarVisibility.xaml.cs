using System;
using Microsoft.Maui.Controls;

namespace Controls.Sample.UITests.Elements
{
	public partial class ScrollBarVisibility : ContentPage
    {
        public ScrollBarVisibility()
        {
            InitializeComponent();
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			TestScrollView.VerticalScrollBarVisibility = Microsoft.Maui.ScrollBarVisibility.Always;
		}
	}
}
