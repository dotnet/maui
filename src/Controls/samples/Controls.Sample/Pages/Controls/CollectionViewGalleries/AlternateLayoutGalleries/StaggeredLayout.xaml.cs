// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.AlternateLayoutGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class StaggeredLayout : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();

		public StaggeredLayout()
		{
			InitializeComponent();

			//CV.ItemTemplate = ExampleTemplates.RandomSizeTemplate();
			//CV.ItemsSource = _demoFilteredItemSource.Items;
		}
	}

	public class StaggeredCollectionView : CollectionView { }

	//public class StaggeredGridItemsLayout : GridItemsLayout
	//{
	//	public StaggeredGridItemsLayout([Parameter("Orientation")] ItemsLayoutOrientation orientation) : base(orientation)
	//	{
	//	}

	//	public StaggeredGridItemsLayout(int span, [Parameter("Orientation")] ItemsLayoutOrientation orientation) : base(span, orientation)
	//	{
	//	}
	//}
}