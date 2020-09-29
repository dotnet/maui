using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CarouselView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7865, "[iOS] CarouselView setting Position has dif behavior than ScrollTo", PlatformAffected.iOS)]
	public partial class Issue7865 : TestContentPage
	{
		public Issue7865()
		{
#if APP
			Title = "Issue 7865";
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			BindingContext = new Issue7865ViewModel();
		}

		public void OnPositionChanged(object sender, PositionChangedEventArgs args)
		{
#if APP
			IndicatorView.SelectedItem = (BindingContext as Issue7865ViewModel).Monkeys[args.CurrentPosition];
#endif
		}

		public void IndicatorSelectionChanged(object sender, SelectionChangedEventArgs args)
		{
#if APP
			ItemsCarousel.Position = (BindingContext as Issue7865ViewModel).Monkeys.IndexOf(args.CurrentSelection[0] as Issue7865Model);
#endif
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue7865Model
	{
		public int Index { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public string Details { get; set; }
		public string ImageUrl { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue7865ViewModel : BindableObject
	{
		public Issue7865ViewModel()
		{
			CreateItems();
		}

		public ObservableCollection<Issue7865Model> Monkeys { get; private set; } = new ObservableCollection<Issue7865Model>();

		public void CreateItems()
		{
			Monkeys.Add(new Issue7865Model
			{
				Index = 0,
				Name = "Baboon",
				Location = "Africa & Asia",
				Details = "Baboons are African and Arabian Old World monkeys belonging to the genus Papio, part of the subfamily Cercopithecinae.",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/f/fc/Papio_anubis_%28Serengeti%2C_2009%29.jpg/200px-Papio_anubis_%28Serengeti%2C_2009%29.jpg"
			});

			Monkeys.Add(new Issue7865Model
			{
				Index = 1,
				Name = "Capuchin Monkey",
				Location = "Central & South America",
				Details = "The capuchin monkeys are New World monkeys of the subfamily Cebinae. Prior to 2011, the subfamily contained only a single genus, Cebus.",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/4/40/Capuchin_Costa_Rica.jpg/200px-Capuchin_Costa_Rica.jpg"
			});

			Monkeys.Add(new Issue7865Model
			{
				Index = 2,
				Name = "Blue Monkey",
				Location = "Central and East Africa",
				Details = "The blue monkey or diademed monkey is a species of Old World monkey native to Central and East Africa, ranging from the upper Congo River basin east to the East African Rift and south to northern Angola and Zambia",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/8/83/BlueMonkey.jpg/220px-BlueMonkey.jpg"
			});

			Monkeys.Add(new Issue7865Model
			{
				Index = 3,
				Name = "Thomas's Langur",
				Location = "Indonesia",
				Details = "Thomas's langur is a species of primate in the family Cercopithecidae. It is endemic to North Sumatra, Indonesia. Its natural habitat is subtropical or tropical dry forests. It is threatened by habitat loss. Its native names are reungkah in Acehnese and kedih in Alas.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/31/Thomas%27s_langur_Presbytis_thomasi.jpg/142px-Thomas%27s_langur_Presbytis_thomasi.jpg"
			});

			Monkeys.Add(new Issue7865Model
			{
				Index = 4,
				Name = "Purple-faced Langur",
				Location = "Sri Lanka",
				Details = "The purple-faced langur, also known as the purple-faced leaf monkey, is a species of Old World monkey that is endemic to Sri Lanka. The animal is a long-tailed arboreal species, identified by a mostly brown appearance, dark face (with paler lower face) and a very shy nature. The species was once highly prevalent, found in suburban Colombo and the \"wet zone\" villages (areas with high temperatures and high humidity throughout the year, whilst rain deluges occur during the monsoon seasons), but rapid urbanization has led to a significant decrease in the population level of the monkeys.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/02/Semnopithèque_blanchâtre_mâle.JPG/192px-Semnopithèque_blanchâtre_mâle.JPG"
			});
		}
	}
}