// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AdaptiveCollectionView : ContentPage
	{
		public AdaptiveCollectionView()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			SizeChanged += OnAdaptiveCollectionViewSizeChanged;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			SizeChanged -= OnAdaptiveCollectionViewSizeChanged;
		}

		void OnAdaptiveCollectionViewSizeChanged(object sender, System.EventArgs e)
		{
			CollectionView.ItemsLayout = Width > 600
				? new GridItemsLayout(3, ItemsLayoutOrientation.Vertical)
				: new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
		}
	}
}