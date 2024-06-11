using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.CollectionViewGalleries.GroupingGalleries
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