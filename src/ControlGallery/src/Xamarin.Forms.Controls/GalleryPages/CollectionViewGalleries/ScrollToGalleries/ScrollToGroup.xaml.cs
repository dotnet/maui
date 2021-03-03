using System;
using System.Linq;
using Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.GroupingGalleries;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.ScrollToGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ScrollToGroup : ContentPage
	{
		SuperTeams _source = new SuperTeams();

		public ScrollToGroup()
		{
			InitializeComponent();
			CollectionView.ItemsSource = _source;

			ScrollTo.Clicked += ScrollToClicked;
			ScrollToItem.Clicked += ScrollToItemClicked;
		}

		void ScrollToItemClicked(object sender, EventArgs e)
		{
			var groupName = GroupName.Text;
			var itemName = ItemName.Text;

			var team = _source.FirstOrDefault(t => t.Name == groupName);

			if (team == null)
			{
				return;
			}

			var member = team.FirstOrDefault(t => t.Name == itemName);

			CollectionView.ScrollTo(member, team);
		}

		void ScrollToClicked(object sender, EventArgs e)
		{
			var groupIndex = int.Parse(GroupIndex.Text);
			var itemIndex = int.Parse(ItemIndex.Text);

			CollectionView.ScrollTo(itemIndex, groupIndex);
		}
	}
}