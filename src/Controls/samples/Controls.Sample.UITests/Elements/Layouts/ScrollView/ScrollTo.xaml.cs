using System;
using Microsoft.Maui.Controls;

namespace Controls.Sample.UITests.Elements
{
    public partial class ScrollTo : ContentPage
    {
        public ScrollTo()
        {
            InitializeComponent();
        }

        async void OnButtonClicked(object sender, EventArgs e)
        {
            await TestScrollView.ScrollToAsync(FinalLabel, ScrollToPosition.End, true);
        }

        void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
        {
            Console.WriteLine($"ScrollX: {e.ScrollX}, ScrollY: {e.ScrollY}");
        }
    }
}