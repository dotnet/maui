using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.GroupingGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class GroupingPlusSelection : ContentPage
	{
		public GroupingPlusSelection()
		{
			InitializeComponent();
			CollectionView.ItemsSource = new SuperTeams();
		}
	}
}