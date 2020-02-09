using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.DualScreen;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.TwoPaneViewGalleries
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TwoPage : ContentPage
    {
		IItemsLayout horizontalLayout = null;
        IItemsLayout verticalItemsLayout = null;
        bool disableUpdates = false;
		private double contentWidth;
		private double contentHeight;

		public DualScreenInfo DualScreenLayoutInfo { get; }
        bool IsSpanned => DualScreenLayoutInfo.SpanningBounds.Length > 0;

        public TwoPage()
        {
            InitializeComponent();
            DualScreenLayoutInfo = new DualScreenInfo(layout);

            cv.ItemsSource =
                Enumerable.Range(0, 1000)
                    .Select(i => $"Page {i}")
                    .ToList();
        }

        protected override void OnAppearing()
        {
            DualScreenLayoutInfo.PropertyChanged += OnFormsWindowPropertyChanged;
            DualScreenInfo.Current.PropertyChanged += OnFormsWindowPropertyChanged;
            SetupColletionViewLayout();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DualScreenLayoutInfo.PropertyChanged -= OnFormsWindowPropertyChanged;
            DualScreenInfo.Current.PropertyChanged -= OnFormsWindowPropertyChanged;
        }

        void OnFormsWindowPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Content == null || disableUpdates)
                return;

			SetupColletionViewLayout();
			if (e.PropertyName == nameof(DualScreenInfo.Current.HingeBounds))
			{
				OnPropertyChanged(nameof(HingeWidth));
			}
        }

		public double ContentHeight
		{
			get => contentHeight;
			set
			{
				if (contentHeight == value)
					return;

				contentHeight = value;
				OnPropertyChanged(nameof(ContentHeight));
			}
		}

		public double ContentWidth
		{
			get => contentWidth;
			set
			{
				if (contentWidth == value)
					return;

				contentWidth = value;
				OnPropertyChanged(nameof(ContentWidth));
			}
		}

		public double Pane1Height => IsSpanned ? (DualScreenLayoutInfo.SpanningBounds[0].Height) : layout.Height;

        public double Pane2Height => IsSpanned ? (DualScreenLayoutInfo.SpanningBounds[1].Height) : 0d;

        public double HingeWidth => DualScreenLayoutInfo?.HingeBounds.Width ?? DualScreenInfo.Current?.HingeBounds.Width ?? 0d;


        void SetupColletionViewLayout()
		{
			ContentWidth = IsSpanned ? (DualScreenLayoutInfo.SpanningBounds[0].Width) : layout.Width;
			ContentHeight = (!DualScreenLayoutInfo.IsLandscape) ? Pane1Height : Pane1Height + Pane2Height;
			disableUpdates = true;
			if (verticalItemsLayout == null)
			{
				horizontalLayout = cv.ItemsLayout;
				verticalItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
				{
					SnapPointsAlignment = SnapPointsAlignment.Start,
					SnapPointsType = SnapPointsType.None
				};
			}
			
			if (!DualScreenLayoutInfo.IsLandscape)
			{
				if (cv.ItemsLayout != horizontalLayout)
				{
					cv.ItemsLayout = horizontalLayout;
				}
			}
			else
			{
					
				if (cv.ItemsLayout != verticalItemsLayout)
				{
					cv.ItemsLayout = verticalItemsLayout;
				}
			}

			disableUpdates = false;
        }
    }
}