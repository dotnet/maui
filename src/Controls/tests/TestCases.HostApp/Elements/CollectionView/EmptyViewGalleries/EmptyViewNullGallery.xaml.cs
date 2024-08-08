using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.CollectionViewGalleries.EmptyViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EmptyViewNullGallery : ContentPage
	{
		public EmptyViewNullGallery(bool useOnlyText = true)
		{
			InitializeComponent();
			string emptyViewText = "Nothing to display.";
			CollectionView.EmptyView = useOnlyText ? emptyViewText :
													 new Grid
													 {
														 Children = { new Label
													 {
														 Text = emptyViewText,
														 HorizontalOptions = LayoutOptions.Center,
														 VerticalOptions = LayoutOptions.Center,
														 FontAttributes = FontAttributes.Bold
													 } }
													 };
		}
	}
}