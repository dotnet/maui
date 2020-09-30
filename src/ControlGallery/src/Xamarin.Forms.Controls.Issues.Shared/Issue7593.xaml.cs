using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
using System.Linq;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CollectionView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7593, "[Bug][iOS] Pull-to-refresh crash in horizontal grid CollectionView/RefreshView", PlatformAffected.iOS)]
	public partial class Issue7593 : TestContentPage
	{
#if APP
		public Issue7593()
		{
			InitializeComponent();

			BindingContext = new ViewModel7593();
		}
#endif

		protected override void Init()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public class ViewModel7593 : INotifyPropertyChanged
	{
		int itemCount = 10;
		const int MaximumItemCount = 50;
		const int PageSize = 10;
		const int RefreshDuration = 2;
		bool isRefreshing;

		public ObservableCollection<Model7593> Animals { get; private set; } = new ObservableCollection<Model7593>();

		public bool IsRefreshing
		{
			get { return isRefreshing; }
			set
			{
				isRefreshing = value;
				OnPropertyChanged();
			}
		}

		public ICommand LoadMoreDataCommand => new Command(GetNextPageOfData);
		public ICommand RefreshCommand => new Command(async () => await RefreshDataAsync());

		public ViewModel7593()
		{
			AddBears();
		}

		void GetNextPageOfData()
		{
			switch (itemCount)
			{
				case 10:
					AddCats();
					break;
				case 20:
					AddDogs();
					break;
				case 30:
					AddElephants();
					break;
				case 40:
					AddMonkeys();
					break;
			}

			if (itemCount < MaximumItemCount)
			{
				itemCount += PageSize;
			}
		}

		async Task RefreshDataAsync()
		{
			IsRefreshing = true;
			await Task.Delay(TimeSpan.FromSeconds(RefreshDuration));
			GetNextPageOfData();
			IsRefreshing = false;
		}

		void AddBears()
		{
			Animals.Add(new Model7593
			{
				Name = "American Black Bear",
				Location = "North America",
				Details = "The American black bear is a medium-sized bear native to North America. It is the continent's smallest and most widely distributed bear species. American black bears are omnivores, with their diets varying greatly depending on season and location. They typically live in largely forested areas, but do leave forests in search of food. Sometimes they become attracted to human communities because of the immediate availability of food. The American black bear is the world's most common bear species.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/0/08/01_Schwarzbär.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Asian Black Bear",
				Location = "Asia",
				Details = "The Asian black bear, also known as the moon bear and the white-chested bear, is a medium-sized bear species native to Asia and largely adapted to arboreal life. It lives in the Himalayas, in the northern parts of the Indian subcontinent, Korea, northeastern China, the Russian Far East, the Honshū and Shikoku islands of Japan, and Taiwan. It is classified as vulnerable by the International Union for Conservation of Nature (IUCN), mostly because of deforestation and hunting for its body parts.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b7/Ursus_thibetanus_3_%28Wroclaw_zoo%29.JPG/180px-Ursus_thibetanus_3_%28Wroclaw_zoo%29.JPG"
			});
			Animals.Add(new Model7593
			{
				Name = "Brown Bear",
				Location = "Northern Eurasia & North America",
				Details = "The brown bear is a bear that is found across much of northern Eurasia and North America. In North America the population of brown bears are often called grizzly bears. It is one of the largest living terrestrial members of the order Carnivora, rivaled in size only by its closest relative, the polar bear, which is much less variable in size and slightly larger on average. The brown bear's principal range includes parts of Russia, Central Asia, China, Canada, the United States, Scandinavia and the Carpathian region, especially Romania, Anatolia and the Caucasus. The brown bear is recognized as a national and state animal in several European countries.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5d/Kamchatka_Brown_Bear_near_Dvuhyurtochnoe_on_2015-07-23.jpg/320px-Kamchatka_Brown_Bear_near_Dvuhyurtochnoe_on_2015-07-23.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Grizzly-Polar Bear Hybrid",
				Location = "Canadian Artic",
				Details = "A grizzly–polar bear hybrid is a rare ursid hybrid that has occurred both in captivity and in the wild. In 2006, the occurrence of this hybrid in nature was confirmed by testing the DNA of a unique-looking bear that had been shot near Sachs Harbour, Northwest Territories on Banks Island in the Canadian Arctic. The number of confirmed hybrids has since risen to eight, all of them descending from the same female polar bear.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/7e/Grolar.JPG/276px-Grolar.JPG"
			});
			Animals.Add(new Model7593
			{
				Name = "Sloth Bear",
				Location = "Indian Subcontinent",
				Details = "The sloth bear is an insectivorous bear species native to the Indian subcontinent. It is listed as Vulnerable on the IUCN Red List, mainly because of habitat loss and degradation. It has also been called labiated bear because of its long lower lip and palate used for sucking insects. Compared to brown and black bears, the sloth bear is lankier, has a long, shaggy fur and a mane around the face, and long, sickle-shaped claws. It evolved from the ancestral brown bear during the Pleistocene and through convergent evolution shares features found in insect-eating mammals.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/6/6c/Sloth_Bear_Washington_DC.JPG/320px-Sloth_Bear_Washington_DC.JPG"
			});
			Animals.Add(new Model7593
			{
				Name = "Sun Bear",
				Location = "Southeast Asia",
				Details = "The sun bear is a bear species occurring in tropical forest habitats of Southeast Asia. It is listed as Vulnerable on the IUCN Red List. The global population is thought to have declined by more than 30% over the past three bear generations. Suitable habitat has been dramatically reduced due to the large-scale deforestation that has occurred throughout Southeast Asia over the past three decades. The sun bear is also known as the honey bear, which refers to its voracious appetite for honeycombs and honey.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a6/Sitting_sun_bear.jpg/319px-Sitting_sun_bear.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Polar Bear",
				Location = "Artic Circle",
				Details = "The polar bear is a hypercarnivorous bear whose native range lies largely within the Arctic Circle, encompassing the Arctic Ocean, its surrounding seas and surrounding land masses. It is a large bear, approximately the same size as the omnivorous Kodiak bear. A boar (adult male) weighs around 350–700 kg (772–1,543 lb), while a sow (adult female) is about half that size. Although it is the sister species of the brown bear, it has evolved to occupy a narrower ecological niche, with many body characteristics adapted for cold temperatures, for moving across snow, ice and open water, and for hunting seals, which make up most of its diet. Although most polar bears are born on land, they spend most of their time on the sea ice. Their scientific name means maritime bear and derives from this fact. Polar bears hunt their preferred food of seals from the edge of sea ice, often living off fat reserves when no sea ice is present. Because of their dependence on the sea ice, polar bears are classified as marine mammals.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/6/66/Polar_Bear_-_Alaska_%28cropped%29.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Spectacled Bear",
				Location = "South America",
				Details = "The spectacled bear, also known as the Andean bear or Andean short-faced bear and locally as jukumari (Aymara), ukumari (Quechua) or ukuku, is the last remaining short-faced bear. Its closest relatives are the extinct Florida spectacled bear, and the giant short-faced bears of the Middle to Late Pleistocene age. Spectacled bears are the only surviving species of bear native to South America, and the only surviving member of the subfamily Tremarctinae. The species is classified as Vulnerable by the IUCN because of habitat loss.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/99/Spectacled_Bear_-_Houston_Zoo.jpg/264px-Spectacled_Bear_-_Houston_Zoo.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Short-faced Bear",
				Location = "Extinct",
				Details = "The short-faced bears is an extinct bear genus that inhabited North America during the Pleistocene epoch from about 1.8 Mya until 11,000 years ago. It was the most common early North American bear and was most abundant in California. There are two recognized species: Arctodus pristinus and Arctodus simus, with the latter considered to be one of the largest known terrestrial mammalian carnivores that has ever existed. It has been hypothesized that their extinction coincides with the Younger Dryas period of global cooling commencing around 10,900 BC.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b8/ArctodusSimusSkeleton.jpg/320px-ArctodusSimusSkeleton.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "California Grizzly Bear",
				Location = "Extinct",
				Details = "The California grizzly bear is an extinct subspecies of the grizzly bear, the very large North American brown bear. Grizzly could have meant grizzled (that is, with golden and grey tips of the hair) or fear-inspiring. Nonetheless, after careful study, naturalist George Ord formally classified it in 1815 – not for its hair, but for its character – as Ursus horribilis (terrifying bear). Genetically, North American grizzlies are closely related; in size and coloring, the California grizzly bear was much like the grizzly bear of the southern coast of Alaska. In California, it was particularly admired for its beauty, size and strength. The grizzly became a symbol of the Bear Flag Republic, a moniker that was attached to the short-lived attempt by a group of American settlers to break away from Mexico in 1846. Later, this rebel flag became the basis for the state flag of California, and then California was known as the Bear State.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/d/de/Monarch_the_bear.jpg"
			});
		}

		void AddCats()
		{
			Animals.Add(new Model7593
			{
				Name = "Abyssinian",
				Location = "Ethopia",
				Details = "The Abyssinian is a breed of domestic short-haired cat with a distinctive tickedtabby coat, in which individual hairs are banded with different colors. The breed is named for Abyssinia (now called Ethiopia), where it is believed to have originated.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9b/Gustav_chocolate.jpg/168px-Gustav_chocolate.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Arabian Mau",
				Location = "Arabian Peninsula",
				Details = "The Arabian Mau is a formal breed of domestic cat, originated from the desert cat, a short-haired landrace native to the desert of the Arabian Peninsula. It lives there in the streets and has adapted very well to the extreme climate. The Arabian Mau is recognized as a formal breed by few fancier and breeder organization and cat registry, World Cat Federation (WCF) and Emirates Feline Federation (EFF). Based on one landrace, the Arabian Mau is a natural breed.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/d/d3/Bex_Arabian_Mau.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Bengal",
				Location = "Asia",
				Details = "The Bengal cat is a domesticated cat breed created from hybrids of domestic cats and the Asian leopard cat – the breed name comes from the taxonomic name. Back-crossing to domestic cats is then done with the goal of creating a healthy, and docile cat with wild-looking, high-contrast coat. Bengals have a wild appearance and may show spots, rosettes, arrowhead markings, or marbling.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/ba/Paintedcats_Red_Star_standing.jpg/187px-Paintedcats_Red_Star_standing.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Burmese",
				Location = "Thailand",
				Details = "The Burmese cat is a breed of domestic cat, originating in Thailand, believed to have its roots near the present Thai-Burma border and developed in the United States and Britain.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/0/04/Blissandlucky11.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Cyprus",
				Location = "Cyprus",
				Details = "Cyprus cats, also known as Cypriot cats, Saint Helen cats, and Saint Nicholas cats, are a landrace of domestic cat found across the island of Cyprus. A standardized breed is being developed from them; among cat fancier and breeder organizations, it is presently fully recognized by the World Cat Federation (WCF), with breeding regulated by the World Cat Congress (WCC), under the name Aphrodite's Giant; and provisionally by The International Cat Association (TICA) as the Aphrodite. All three organizations permit shorthaired and semi-longhaired versions and no out-crossing to other breeds.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b9/CyprusShorthair.jpg/320px-CyprusShorthair.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "German Rex",
				Location = "Germany",
				Details = "The German Rex is a medium-sized breed with slender legs of a medium length. The head is round with well-developed cheeks and large, open ears. The eyes are of medium size in colours related to the coat colour. The coat is silky and short with a tendency to curl. The whiskers also curl, though less strongly than in the Cornish Rex. They may be nearly straight. All colours of coat, including white, are allowed. The body development is heavier than in the Cornish Rex - more like the European Shorthairs.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c7/German_rex_harry_%28cropped%29.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Highlander",
				Location = "United States",
				Details = "The Highlander (also known as the Highlander Shorthair, and originally as the Highland Lynx), is an experimental breed of cat. The unique appearance of the Highlander comes from the deliberate cross between the Desert Lynx and the Jungle Curl breeds, also recently developed. The latter of these has some non-domestic ancestry from two Asian small cat species, the leopard cat and jungle cat, making the Highlander nominally a feline hybrid, though its foundation stock is mostly domestic cat.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/15/Highlander-7.jpg/293px-Highlander-7.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Manx",
				Location = "Isle of Man",
				Details = "The Manx cat is a breed of domestic cat originating on the Isle of Man, with a naturally occurring mutation that shortens the tail. Many Manx have a small stub of a tail, but Manx cats are best known as being entirely tailless; this is the most distinguishing characteristic of the breed, along with elongated hind legs and a rounded head. Manx cats come in all coat colours and patterns, though all-white specimens are rare, and the coat range of the original stock was more limited. Long-haired variants are sometimes considered a separate breed, the Cymric. Manx are prized as skilled hunters, and thus have often been sought by farmers with rodent problems, and been a preferred ship's cat breed. They are said to be social, tame and active. An old local term for the cats on their home island is stubbin. Manx have been exhibited in cat shows since the 1800s, with the first known breed standard published in 1903.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/en/9/9b/Manx_cat_by_Karen_Weaver.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Peterbald",
				Location = "Russia",
				Details = "The Peterbald is a cat breed of Russian origin. It was created in St Petersburg in 1994 from an experimental breeding by Olga S. Mironova. They resemble something Shorthairs with a hair-losing gene. The breed was accepted for Championship class competition in 2009.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c7/Peterbald_male_Shango_by_Irina_Polunina.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Scottish Fold",
				Location = "Scotland",
				Details = "The Scottish Fold is a breed of domestic cat with a natural dominant-gene mutation that affects cartilage throughout the body, causing the ears to fold, bending forward and down towards the front of the head, which gives the cat what is often described as an owl-like appearance.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5d/Adult_Scottish_Fold.jpg/240px-Adult_Scottish_Fold.jpg"
			});
		}

		void AddDogs()
		{
			Animals.Add(new Model7593
			{
				Name = "Afghan Hound",
				Location = "Afghanistan",
				Details = "The Afghan Hound is a hound that is distinguished by its thick, fine, silky coat and its tail with a ring curl at the end. The breed is selectively bred for its unique features in the cold mountains of Afghanistan.  Other names for this breed are Kuchi Hound, Tāzī, Balkh Hound, Baluchi Hound, Barakzai Hound, Shalgar Hound, Kabul Hound, Galanday Hound or sometimes incorrectly African Hound.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/6/69/Afghane.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Alpine Dachsbracke",
				Location = "Austria",
				Details = "The Alpine Dachsbracke is a small breed of dog of the scent hound type originating in Austria. The Alpine Dachsbracke was bred to track wounded deer as well as boar, hare, and fox. It is highly efficient at following a trail even after it has gone cold. The Alpine Dachsbracke is very sturdy, and Austria is said to be the country of origin.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/2/23/Alpejski_gończy_krótkonożny_g99.jpg/320px-Alpejski_gończy_krótkonożny_g99.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "American Bulldog",
				Location = "United States",
				Details = "The American Bulldog is a breed of utility dog descended from the Old English Bulldog.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/5/5e/American_Bulldog_600.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Bearded Collie",
				Location = "Scotland",
				Details = "The Bearded Collie, or Beardie, is a herding breed of dog once used primarily by Scottish shepherds, but now mostly a popular family companion. Bearded Collies have an average weight of 18–27 kilograms (40–60 lb). Males are around 51–56 centimetres (20–22 in) tall at the withers while females are around 51–53 centimetres (20–21 in) tall.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/9/9c/Bearded_Collie_600.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Boston Terrier",
				Location = "United States",
				Details = "The Boston Terrier is a breed of dog originating in the United States of America. This American Gentleman was accepted in 1893 by the American Kennel Club as a non-sporting breed. Color and markings are important when distinguishing this breed to the AKC standard. They should be either black, brindle or seal with white markings. Bostons are small and compact with a short tail and erect ears. The AKC says they are highly intelligent and very easily trained. They are friendly and can be stubborn at times. The average life span of a Boston is around 11 to 13 years, though some can live well into their teens.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/d7/Boston-terrier-carlos-de.JPG/320px-Boston-terrier-carlos-de.JPG"
			});
			Animals.Add(new Model7593
			{
				Name = "Canadian Eskimo",
				Location = "Canada",
				Details = "The Canadian Eskimo Dog is an Arctic breed of working dog, which is often considered to be one of North America's oldest and rarest remaining purebred indigenous domestic canines. Other names include qimmiq or qimmit. They were brought from Siberia to North America by the Thule people 1,000 years ago, along with the Greenland Dog that is genetically identical.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/79/Spoonsced.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Eurohound",
				Location = "Scandinavia",
				Details = "A Eurohound (also known as a Eurodog or Scandinavian hound) is a type of dog bred for sled dog racing. The Eurohound is typically crossbred from the Alaskan husky group and any of a number of pointing breeds.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/9/98/Eurohound.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Irish Terrier",
				Location = "Ireland",
				Details = "The Irish Terrier is a dog breed from Ireland, one of many breeds of terrier. The Irish Terrier is considered one of the oldest terrier breeds. The Dublin dog show in 1873 was the first to provide a separate class for Irish Terriers. By the 1880s, Irish Terriers were the fourth most popular breed in Ireland and Britain.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/56/IrishTerrierSydenhamHillWoods.jpg/180px-IrishTerrierSydenhamHillWoods.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Kerry Beagle",
				Location = "Ireland",
				Details = "The Kerry Beagle is one of the oldest Irish hound breeds, believed to be descendant from the Old Southern Hound or the Celtic Hounds. It is the only extant scent hound breed native to Ireland.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/75/Kerry_Beagle_from_1915.JPG"
			});
			Animals.Add(new Model7593
			{
				Name = "Norwegian Buhund",
				Location = "Norway",
				Details = "The Norwegian Buhund is a breed of dog of the spitz type. It is closely related to the Icelandic Sheepdog and the Jämthund. The Buhund is used as an all purpose farm and herding dog, as well as watch dog and a nanny dog.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/3/3b/Norwegian_Buhund_600.jpg"
			});
		}

		void AddElephants()
		{
			Animals.Add(new Model7593
			{
				Name = "African Bush Elephant",
				Location = "Africa",
				Details = "The African bush elephant, also known as the African savanna elephant, is the larger of the two species of African elephants, and the largest living terrestrial animal. These elephants were previously regarded as the same species, but the African forest elephant has been reclassified as L. cyclotis.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/91/African_Elephant_%28Loxodonta_africana%29_bull_%2831100819046%29.jpg/320px-African_Elephant_%28Loxodonta_africana%29_bull_%2831100819046%29.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "African Forest Elephant",
				Location = "Africa",
				Details = "The African forest elephant is a forest-dwelling species of elephant found in the Congo Basin. It is the smallest of the three extant species of elephant, but still one of the largest living terrestrial animals. The African forest elephant and the African bush elephan  were considered to be one species until genetic studies indicated that they separated an estimated 2–7 million years ago. From an estimated population size of over 2 million prior to the colonization of Africa, the population in 2015 is estimated to be about 100,000 forest elephants, mostly living in the forests of Gabon. Due to a slower birth rate, the forest elephant takes longer to recover from poaching, which caused its population to fall by 65% from 2002 to 2014.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/6/6a/African_Forest_Elephant.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Desert Elephant",
				Location = "Africa",
				Details = "Desert elephants, or desert-adapted elephants are not a distinct species of elephant but are African bush elephants that have made their homes in the Namib and Sahara deserts in Africa. It was believed at one time that they were a subspecies of the African bush elephant but this is no longer thought to be the case. Desert-dwelling elephants were once more widespread in Africa than they are now and are currently found only in Namibia and Mali. They tend to migrate from one waterhole to another following traditional routes which depend on the seasonal availability of food and water. They face pressure from poaching and from changes in land use by humans.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/77/Desert_elephants_in_the_Huab_River.jpg/320px-Desert_elephants_in_the_Huab_River.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Borneo Elephant",
				Location = "Asia",
				Details = "The Borneo elephant, also called the Borneo pygmy elephant, is a subspecies of Asian elephant that inhabits northeastern Borneo, in Indonesia and Malaysia. Its origin remains the subject of debate. A definitive subspecific classification as Elephas maximus borneensis awaits a detailed range-wide morphometric and genetic study. Since 1986, Elephas maximus has been listed as Endangered on the IUCN Red List as the population has declined by at least 50% over the last three generations, estimated to be 60–75 years. The species is pre-eminently threatened by habitat loss, degradation and fragmentation.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e4/Elephant_%40_kabini.jpg/180px-Elephant_%40_kabini.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Indian Elephant",
				Location = "Asia",
				Details = "The Indian elephant is one of three extant recognized subspecies of the Asian elephant and native to mainland Asia. Since 1986, the Asian elephant has been listed as Endangered on the IUCN Red List as the wild population has declined by at least 50% since the 1930s to 1940s, i.e. three elephant generations. The Asian elephant is threatened by habitat loss, degradation and fragmentation.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/98/Elephas_maximus_%28Bandipur%29.jpg/320px-Elephas_maximus_%28Bandipur%29.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Sri Lankan Elephant",
				Location = "Asia",
				Details = "The Sri Lankan elephant is one of three recognized subspecies of the Asian elephant, and native to Sri Lanka. Since 1986, Elephas maximus has been listed as endangered by IUCN as the population has declined by at least 50% over the last three generations, estimated to be 60–75 years. The species is primarily threatened by habitat loss, degradation and fragmentation.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b1/Srilankan_tuskelephant.jpg/213px-Srilankan_tuskelephant.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Sumatran Elephant",
				Location = "Asia",
				Details = "The Sumatran elephant is one of three recognized subspecies of the Asian elephant, and native to the Indonesia island of Sumatra. In 2011, the Sumatran elephant has been classified as critically endangered by IUCN as the population has declined by at least 80% over the last three generations, estimated to be about 75 years. The subspecies is pre-eminently threatened by habitat loss, degradation and fragmentation, and poaching; over 69% of potential elephant habitat has been lost within the last 25 years. Much of the remaining forest cover is in blocks smaller than 250 km2 (97 sq mi), which are too small to contain viable elephant populations.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b6/Borobudur-Temple-Park_Elephant-cage-01.jpg/320px-Borobudur-Temple-Park_Elephant-cage-01.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Pygmy Elephant",
				Location = "Africa & Asia",
				Details = "Pygmy elephants live in both Africa and Asia.The African pygmy elephant is currently considered to be a tiny morph of the African forest elephant. The Borneo elephant, a well-documented variety of elephant, is also calledmpygmy elephant. This elephant, inhabiting tropical rainforest in north Borneo (east Sabah and extreme north Kalimantan), was long thought to be identical to the Asian elephant and descended from a captive population. In 2003, DNA comparison revealed them to be probably a new subspecies.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/9/93/Borneo-elephant-PLoS_Biology.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Mammoth",
				Location = "Extinct",
				Details = "A mammoth is any species of the extinct genus Mammuthus, one of the many genera that make up the order of trunked mammals called proboscideans. The various species of mammoth were commonly equipped with long, curved tusks and, in northern species, a covering of long hair. They lived from the Pliocene epoch (from around 5 million years ago) into the Holocene at about 4,000 years ago, and various species existed in Africa, Europe, Asia, and North America. They were members of the family Elephantidae, which also contains the two genera of modern elephants and their ancestors.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/6/62/Columbian_mammoth.JPG/320px-Columbian_mammoth.JPG"
			});
			Animals.Add(new Model7593
			{
				Name = "Mastodon",
				Location = "Extinct",
				Details = "Mastodons are any species of extinct proboscideans in the genus Mammut, distantly related to elephants, that inhabited North and Central America during the late Miocene or late Pliocene up to their extinction at the end of the Pleistocene 10,000 to 11,000 years ago. Mastodons lived in herds and were predominantly forest-dwelling animals that fed on a mixed diet obtained by browsing and grazing with a seasonal preference for browsing, similar to living elephants.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b0/Mammut_americanum.jpg/320px-Mammut_americanum.jpg"
			});
		}

		void AddMonkeys()
		{
			Animals.Add(new Model7593
			{
				Name = "Baboon",
				Location = "Africa & Asia",
				Details = "Baboons are African and Arabian Old World monkeys belonging to the genus Papio, part of the subfamily Cercopithecinae.",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/f/fc/Papio_anubis_%28Serengeti%2C_2009%29.jpg/200px-Papio_anubis_%28Serengeti%2C_2009%29.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Capuchin Monkey",
				Location = "Central & South America",
				Details = "The capuchin monkeys are New World monkeys of the subfamily Cebinae. Prior to 2011, the subfamily contained only a single genus, Cebus.",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/4/40/Capuchin_Costa_Rica.jpg/200px-Capuchin_Costa_Rica.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Blue Monkey",
				Location = "Central and East Africa",
				Details = "The blue monkey or diademed monkey is a species of Old World monkey native to Central and East Africa, ranging from the upper Congo River basin east to the East African Rift and south to northern Angola and Zambia",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/8/83/BlueMonkey.jpg/220px-BlueMonkey.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Squirrel Monkey",
				Location = "Central & South America",
				Details = "The squirrel monkeys are the New World monkeys of the genus Saimiri. They are the only genus in the subfamily Saimirinae. The name of the genus Saimiri is of Tupi origin, and was also used as an English name by early researchers.",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/2/20/Saimiri_sciureus-1_Luc_Viatour.jpg/220px-Saimiri_sciureus-1_Luc_Viatour.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Golden Lion Tamarin",
				Location = "Brazil",
				Details = "The golden lion tamarin also known as the golden marmoset, is a small New World monkey of the family Callitrichidae.",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/8/87/Golden_lion_tamarin_portrait3.jpg/220px-Golden_lion_tamarin_portrait3.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Japanese Macaque",
				Location = "Japan",
				Details = "The Japanese macaque, is a terrestrial Old World monkey species native to Japan. They are also sometimes known as the snow monkey because they live in areas where snow covers the ground for months each",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/c/c1/Macaca_fuscata_fuscata1.jpg/220px-Macaca_fuscata_fuscata1.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Mandrill",
				Location = "Southern Cameroon, Gabon, Equatorial Guinea, and Congo",
				Details = "The mandrill is a primate of the Old World monkey family, closely related to the baboons and even more closely to the drill. It is found in southern Cameroon, Gabon, Equatorial Guinea, and Congo.",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/7/75/Mandrill_at_san_francisco_zoo.jpg/220px-Mandrill_at_san_francisco_zoo.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Proboscis Monkey",
				Location = "Borneo",
				Details = "The proboscis monkey or long-nosed monkey, known as the bekantan in Malay, is a reddish-brown arboreal Old World monkey that is endemic to the south-east Asian island of Borneo.",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/e/e5/Proboscis_Monkey_in_Borneo.jpg/250px-Proboscis_Monkey_in_Borneo.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Red-shanked Douc",
				Location = "Vietnam, Laos",
				Details = "The red-shanked douc is a species of Old World monkey, among the most colourful of all primates. This monkey is sometimes called the \"costumed ape\" for its extravagant appearance. From its knees to its ankles it sports maroon-red \"stockings\", and it appears to wear white forearm length gloves. Its attire is finished with black hands and feet. The golden face is framed by a white ruff, which is considerably fluffier in males. The eyelids are a soft powder blue. The tail is white with a triangle of white hair at the base. Males of all ages have a white spot on both sides of the corners of the rump patch, and red and white genitals.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9f/Portrait_of_a_Douc.jpg/159px-Portrait_of_a_Douc.jpg"
			});
			Animals.Add(new Model7593
			{
				Name = "Gray-shanked Douc",
				Location = "Vietnam",
				Details = "The gray-shanked douc langur is a douc species native to the Vietnamese provinces of Quảng Nam, Quảng Ngãi, Bình Định, Kon Tum, and Gia Lai. The total population is estimated at 550 to 700 individuals. In 2016, Dr Benjamin Rawson, Country Director of Fauna & Flora International - Vietnam Programme, announced a discovery of an additional population of more than 500 individuals found in Central Vietnam, bringing the total population up to approximately 1000 individuals.",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0b/Cuc.Phuong.Primate.Rehab.center.jpg/320px-Cuc.Phuong.Primate.Rehab.center.jpg"
			});
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}

	[Preserve(AllMembers = true)]
	public class Model7593
	{
		public string Name { get; set; }
		public string Location { get; set; }
		public string Details { get; set; }
		public string ImageUrl { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}