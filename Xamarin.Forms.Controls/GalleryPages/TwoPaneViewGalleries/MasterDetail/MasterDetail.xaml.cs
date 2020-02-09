using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.DualScreen;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.TwoPaneViewGalleries
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterDetail
    {
        bool IsSpanned => DualScreenInfo.Current.SpanMode != TwoPaneViewMode.SinglePane;
        DetailsPage detailsPagePushed;

        public MasterDetail()
        {
            InitializeComponent();
            masterPage.SelectionChanged += OnTitleSelected;
            detailsPagePushed = new DetailsPage();
        }

        private void OnTitleSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection == null || e.CurrentSelection.Count == 0)
                return;

            SetBindingContext();
            SetupViews();            
        }


        void SetBindingContext()
        {
            var bindingContext = masterPage.SelectedItem ?? (masterPage.ItemsSource as IList<MasterDetailsItem>)[0];
            detailsPagePushed.BindingContext = bindingContext;
            detailsPage.BindingContext = bindingContext;
        }

        async void SetupViews()
        {
            if (IsSpanned && !DualScreenInfo.Current.IsLandscape)
                SetBindingContext();

            if (detailsPage.BindingContext == null)
                return;

            if (!IsSpanned)
            {
                if (!Navigation.NavigationStack.Contains(detailsPagePushed))
                {
                    await Navigation.PushAsync(detailsPagePushed);
                }
            }

        }

        protected override void OnAppearing()
        {
            if (!IsSpanned)
                masterPage.SelectedItem = null;
            DualScreenInfo.Current.PropertyChanged += OnFormsWindowPropertyChanged;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DualScreenInfo.Current.PropertyChanged -= OnFormsWindowPropertyChanged;
        }

        void OnFormsWindowPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DualScreenInfo.Current.SpanMode) || e.PropertyName == nameof(DualScreenInfo.Current.IsLandscape))
            {
                SetupViews();
            }
        }
    }
}