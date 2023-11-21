using System;
using Microsoft.Maui.Controls;

namespace Controls.Sample.UITests.Elements
{
    public partial class ChessboardPage : ContentPage
    {
        public ChessboardPage()
        {
            InitializeComponent();
        }

        void OnContentViewSizeChanged(object sender, EventArgs e)
        {
            ContentView contentView = sender as ContentView;
            double boardSize = Math.Min(contentView.Width, contentView.Height);
			TestAbsoluteLayout.WidthRequest = boardSize;
			TestAbsoluteLayout.HeightRequest = boardSize;
        }
    }
}
