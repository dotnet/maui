//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.GroupingGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SomeEmptyGroups : ContentPage
	{
		public SomeEmptyGroups()
		{
			InitializeComponent();

			var teams = new List<Team>
			{
				new Team("Avengers", new List<Member>
				{
					new Member("Thor"),
					new Member("Captain America")
				}),

				new Team("Thundercats", new List<Member>()),

				new Team("Avengers", new List<Member>
				{
					new Member("Thor"),
					new Member("Captain America")
				}),

				new Team("Bionic Six", new List<Member>()),

				new Team("Fantastic Four", new List<Member>
				{
					new Member("The Thing"),
					new Member("The Human Torch"),
					new Member("The Invisible Woman"),
					new Member("Mr. Fantastic"),
				})
			};

			CollectionView.ItemsSource = teams;
		}
	}
}