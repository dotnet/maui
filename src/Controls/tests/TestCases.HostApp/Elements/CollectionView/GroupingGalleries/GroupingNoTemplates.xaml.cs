using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.CollectionViewGalleries.GroupingGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class GroupingNoTemplates : ContentPage
	{
		public GroupingNoTemplates()
		{
			InitializeComponent();
			CollectionView.ItemsSource = new SuperTeams();
		}
	}
}