using System;
using System.Linq;
using Maui.Controls.Sample.Pages.CollectionViewGalleries.GroupingGalleries;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.ScrollToGalleries
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

		void ScrollToItemClicked(object? sender, EventArgs e)
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

		void ScrollToClicked(object? sender, EventArgs e)
		{
			var groupIndex = int.Parse(GroupIndex.Text);
			var itemIndex = int.Parse(ItemIndex.Text);

			CollectionView.ScrollTo(itemIndex, groupIndex);
		}
	}
}