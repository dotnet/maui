using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
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
	[Issue(IssueTracker.Github, 7817, "[Android/iOS] Changing ItemsUpdatingScrollMode has no effect on CarouselView")]
	public partial class Issue7817 : TestContentPage
	{
		public Issue7817()
		{
#if APP
			Title = "Issue 7817";
			InitializeComponent();
#endif
		}

		protected override async void Init()
		{
			BindingContext = new Issue7817ViewModel();
			await ((Issue7817ViewModel)BindingContext).CreateCollectionAsync();
		}

		void OnItemsUpdatingScrollModeChanged(object sender, EventArgs e)
		{
#if APP
			carouselView.ItemsUpdatingScrollMode = (ItemsUpdatingScrollMode)(sender as EnumPicker).SelectedItem;
#endif
		}
	}

	[Preserve(AllMembers = true)]
	public class EnumPicker : Picker
	{
		public static readonly BindableProperty EnumTypeProperty = BindableProperty.Create(nameof(EnumType), typeof(Type), typeof(EnumPicker),
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				var picker = (EnumPicker)bindable;

				if (oldValue != null)
				{
					picker.ItemsSource = null;
				}
				if (newValue != null)
				{
					if (!((Type)newValue).GetTypeInfo().IsEnum)
						throw new ArgumentException("EnumPicker: EnumType property must be enumeration type");

					picker.ItemsSource = Enum.GetValues((Type)newValue);
				}
			});

		public Type EnumType
		{
			set => SetValue(EnumTypeProperty, value);
			get => (Type)GetValue(EnumTypeProperty);
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue7817Model
	{
		public int Index { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public string Details { get; set; }
		public string ImageUrl { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue7817ViewModel : BindableObject
	{
		const int AddItemDelay = 2000;

		public ObservableCollection<Issue7817Model> Monkeys { get; private set; } = new ObservableCollection<Issue7817Model>();

		public async Task CreateCollectionAsync()
		{
			Monkeys.Add(new Issue7817Model
			{
				Index = 0,
				Name = "Baboon",
				Location = "Africa & Asia",
				Details = "Baboons are African and Arabian Old World monkeys belonging to the genus Papio, part of the subfamily Cercopithecinae.",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/f/fc/Papio_anubis_%28Serengeti%2C_2009%29.jpg/200px-Papio_anubis_%28Serengeti%2C_2009%29.jpg"
			});

			await Task.Delay(AddItemDelay);

			Monkeys.Add(new Issue7817Model
			{
				Index = 1,
				Name = "Capuchin Monkey",
				Location = "Central & South America",
				Details = "The capuchin monkeys are New World monkeys of the subfamily Cebinae. Prior to 2011, the subfamily contained only a single genus, Cebus.",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/4/40/Capuchin_Costa_Rica.jpg/200px-Capuchin_Costa_Rica.jpg"
			});

			await Task.Delay(AddItemDelay);

			Monkeys.Add(new Issue7817Model
			{
				Index = 2,
				Name = "Blue Monkey",
				Location = "Central and East Africa",
				Details = "The blue monkey or diademed monkey is a species of Old World monkey native to Central and East Africa, ranging from the upper Congo River basin east to the East African Rift and south to northern Angola and Zambia",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/8/83/BlueMonkey.jpg/220px-BlueMonkey.jpg"
			});

			Monkeys.Add(new Issue7817Model
			{
				Index = 3,
				Name = "Thomas's Langur",
				Location = "Indonesia",
				Details = "Thomas's langur is a species of primate in the family Cercopithecidae. It is endemic to North Sumatra, Indonesia. Its natural habitat is subtropical or tropical dry forests. It is threatened by habitat loss. Its native names are reungkah in Acehnese and kedih in Alas.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/31/Thomas%27s_langur_Presbytis_thomasi.jpg/142px-Thomas%27s_langur_Presbytis_thomasi.jpg"
			});

			await Task.Delay(AddItemDelay);

			Monkeys.Add(new Issue7817Model
			{
				Index = 4,
				Name = "Purple-faced Langur",
				Location = "Sri Lanka",
				Details = "The purple-faced langur, also known as the purple-faced leaf monkey, is a species of Old World monkey that is endemic to Sri Lanka. The animal is a long-tailed arboreal species, identified by a mostly brown appearance, dark face (with paler lower face) and a very shy nature. The species was once highly prevalent, found in suburban Colombo and the \"wet zone\" villages (areas with high temperatures and high humidity throughout the year, whilst rain deluges occur during the monsoon seasons), but rapid urbanization has led to a significant decrease in the population level of the monkeys.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/02/Semnopithèque_blanchâtre_mâle.JPG/192px-Semnopithèque_blanchâtre_mâle.JPG"
			});

			await Task.Delay(AddItemDelay);

			Monkeys.Add(new Issue7817Model
			{
				Index = 5,
				Name = "Gelada",
				Location = "Ethiopia",
				Details = "The gelada, sometimes called the bleeding-heart monkey or the gelada baboon, is a species of Old World monkey found only in the Ethiopian Highlands, with large populations in the Semien Mountains. Theropithecus is derived from the Greek root words for \"beast-ape.\" Like its close relatives the baboons, it is largely terrestrial, spending much of its time foraging in grasslands.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/13/Gelada-Pavian.jpg/320px-Gelada-Pavian.jpg"
			});
		}
	}
}