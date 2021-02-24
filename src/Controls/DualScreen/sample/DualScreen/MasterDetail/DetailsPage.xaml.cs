using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.DualScreen;
using Microsoft.Maui.Controls.Xaml;

namespace DualScreen
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetailsPage
    {
        bool IsSpanned => DualScreenInfo.Current.SpanMode != TwoPaneViewMode.SinglePane;
        public DetailsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            DualScreenInfo.Current.PropertyChanged += OnFormsWindowPropertyChanged;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DualScreenInfo.Current.PropertyChanged -= OnFormsWindowPropertyChanged;
        }

        async void OnFormsWindowPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(DualScreenInfo.Current.SpanMode) || e.PropertyName == nameof(DualScreenInfo.Current.IsLandscape))
            {
                if (IsSpanned)
                    await Navigation.PopAsync();
            }
        }
    }
}