using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.HeaderFooterGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HeaderFooterGridHorizontal : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource(10);
		object? header;
		object? footer;

		public HeaderFooterGridHorizontal()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();
			CollectionView.ItemsSource = _demoFilteredItemSource.Items;
		}



		void AddContentClicked(object sender, System.EventArgs e)
		{
			if (sender is VisualElement ve && ve.Parent is StackLayout sl)
				sl.Children.Add(new Label() { Text = "Grow" });
		}

		void ToggleHeader(object sender, System.EventArgs e)
		{
			header = CollectionView.Header ?? header;

			if (CollectionView.Header == null)
				CollectionView.Header = header;
			else
				CollectionView.Header = null;
		}

		void ToggleFooter(object sender, System.EventArgs e)
		{
			footer = CollectionView.Footer ?? footer;

			if (CollectionView.Footer == null)
				CollectionView.Footer = footer;
			else
				CollectionView.Footer = null;
		}
	}
}