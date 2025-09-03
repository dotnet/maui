using Microsoft.Maui.ManualTests.Models;

namespace Microsoft.Maui.ManualTests.ViewModels
{
	public class GroupedAnimalsViewModel
	{
		bool includeEmptyGroups;
		public List<AnimalGroup> Animals { get; private set; } = new List<AnimalGroup>();

		public GroupedAnimalsViewModel(bool emptyGroups = false)
		{
			includeEmptyGroups = emptyGroups;
			CreateAnimalsCollection();
		}

		void CreateAnimalsCollection()
		{
			if (includeEmptyGroups)
				Animals.Add(new AnimalGroup("Aardvarks", new List<Animal>()));

			Animals.Add(new AnimalGroup("Bears", new List<Animal>
			{
				new Animal
				{
					Name = "American Black Bear",
					Location = "North America",
					Details = "The American black bear is a medium-sized bear native to North America. It is the continent's smallest and most widely distributed bear species. American black bears are omnivores, with their diets varying greatly depending on season and location. They typically live in largely forested areas, but do leave forests in search of food. Sometimes they become attracted to human communities because of the immediate availability of food. The American black bear is the world's most common bear species.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/0/08/01_Schwarzbär.jpg"
				},
				new Animal
				{
					Name = "Asian Black Bear",
					Location = "Asia",
					Details = "The Asian black bear, also known as the moon bear and the white-chested bear, is a medium-sized bear species native to Asia and largely adapted to arboreal life. It lives in the Himalayas, in the northern parts of the Indian subcontinent, Korea, northeastern China, the Russian Far East, the Honshū and Shikoku islands of Japan, and Taiwan. It is classified as vulnerable by the International Union for Conservation of Nature (IUCN), mostly because of deforestation and hunting for its body parts.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b7/Ursus_thibetanus_3_%28Wroclaw_zoo%29.JPG/180px-Ursus_thibetanus_3_%28Wroclaw_zoo%29.JPG"
				},
				new Animal
				{
					Name = "Brown Bear",
					Location = "Northern Eurasia & North America",
					Details = "The brown bear is a bear that is found across much of northern Eurasia and North America. In North America the population of brown bears are often called grizzly bears. It is one of the largest living terrestrial members of the order Carnivora, rivaled in size only by its closest relative, the polar bear, which is much less variable in size and slightly larger on average. The brown bear's principal range includes parts of Russia, Central Asia, China, Canada, the United States, Scandinavia and the Carpathian region, especially Romania, Anatolia and the Caucasus. The brown bear is recognized as a national and state animal in several European countries.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5d/Kamchatka_Brown_Bear_near_Dvuhyurtochnoe_on_2015-07-23.jpg/320px-Kamchatka_Brown_Bear_near_Dvuhyurtochnoe_on_2015-07-23.jpg"
				},
				new Animal
				{
					Name = "Grizzly-Polar Bear Hybrid",
					Location = "Canadian Artic",
					Details = "A grizzly–polar bear hybrid is a rare ursid hybrid that has occurred both in captivity and in the wild. In 2006, the occurrence of this hybrid in nature was confirmed by testing the DNA of a unique-looking bear that had been shot near Sachs Harbour, Northwest Territories on Banks Island in the Canadian Arctic. The number of confirmed hybrids has since risen to eight, all of them descending from the same female polar bear.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/7e/Grolar.JPG/276px-Grolar.JPG"
				},
				new Animal
				{
					Name = "Sloth Bear",
					Location = "Indian Subcontinent",
					Details = "The sloth bear is an insectivorous bear species native to the Indian subcontinent. It is listed as Vulnerable on the IUCN Red List, mainly because of habitat loss and degradation. It has also been called labiated bear because of its long lower lip and palate used for sucking insects. Compared to brown and black bears, the sloth bear is lankier, has a long, shaggy fur and a mane around the face, and long, sickle-shaped claws. It evolved from the ancestral brown bear during the Pleistocene and through convergent evolution shares features found in insect-eating mammals.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/6/6c/Sloth_Bear_Washington_DC.JPG/320px-Sloth_Bear_Washington_DC.JPG"
				},
				new Animal
				{
					Name = "Sun Bear",
					Location = "Southeast Asia",
					Details = "The sun bear is a bear species occurring in tropical forest habitats of Southeast Asia. It is listed as Vulnerable on the IUCN Red List. The global population is thought to have declined by more than 30% over the past three bear generations. Suitable habitat has been dramatically reduced due to the large-scale deforestation that has occurred throughout Southeast Asia over the past three decades. The sun bear is also known as the honey bear, which refers to its voracious appetite for honeycombs and honey.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a6/Sitting_sun_bear.jpg/319px-Sitting_sun_bear.jpg"
				},
				new Animal
				{
					Name = "Polar Bear",
					Location = "Artic Circle",
					Details = "The polar bear is a hypercarnivorous bear whose native range lies largely within the Arctic Circle, encompassing the Arctic Ocean, its surrounding seas and surrounding land masses. It is a large bear, approximately the same size as the omnivorous Kodiak bear. A boar (adult male) weighs around 350–700 kg (772–1,543 lb), while a sow (adult female) is about half that size. Although it is the sister species of the brown bear, it has evolved to occupy a narrower ecological niche, with many body characteristics adapted for cold temperatures, for moving across snow, ice and open water, and for hunting seals, which make up most of its diet. Although most polar bears are born on land, they spend most of their time on the sea ice. Their scientific name means maritime bear and derives from this fact. Polar bears hunt their preferred food of seals from the edge of sea ice, often living off fat reserves when no sea ice is present. Because of their dependence on the sea ice, polar bears are classified as marine mammals.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/6/66/Polar_Bear_-_Alaska_%28cropped%29.jpg"
				},
				new Animal
				{
					Name = "Spectacled Bear",
					Location = "South America",
					Details = "The spectacled bear, also known as the Andean bear or Andean short-faced bear and locally as jukumari (Aymara), ukumari (Quechua) or ukuku, is the last remaining short-faced bear. Its closest relatives are the extinct Florida spectacled bear, and the giant short-faced bears of the Middle to Late Pleistocene age. Spectacled bears are the only surviving species of bear native to South America, and the only surviving member of the subfamily Tremarctinae. The species is classified as Vulnerable by the IUCN because of habitat loss.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/99/Spectacled_Bear_-_Houston_Zoo.jpg/264px-Spectacled_Bear_-_Houston_Zoo.jpg"
				},
				new Animal
				{
					Name = "Short-faced Bear",
					Location = "Extinct",
					Details = "The short-faced bears is an extinct bear genus that inhabited North America during the Pleistocene epoch from about 1.8 Mya until 11,000 years ago. It was the most common early North American bear and was most abundant in California. There are two recognized species: Arctodus pristinus and Arctodus simus, with the latter considered to be one of the largest known terrestrial mammalian carnivores that has ever existed. It has been hypothesized that their extinction coincides with the Younger Dryas period of global cooling commencing around 10,900 BC.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b8/ArctodusSimusSkeleton.jpg/320px-ArctodusSimusSkeleton.jpg"
				},
				new Animal
				{
					Name = "California Grizzly Bear",
					Location = "Extinct",
					Details = "The California grizzly bear is an extinct subspecies of the grizzly bear, the very large North American brown bear. Grizzly could have meant grizzled (that is, with golden and grey tips of the hair) or fear-inspiring. Nonetheless, after careful study, naturalist George Ord formally classified it in 1815 – not for its hair, but for its character – as Ursus horribilis (terrifying bear). Genetically, North American grizzlies are closely related; in size and coloring, the California grizzly bear was much like the grizzly bear of the southern coast of Alaska. In California, it was particularly admired for its beauty, size and strength. The grizzly became a symbol of the Bear Flag Republic, a moniker that was attached to the short-lived attempt by a group of American settlers to break away from Mexico in 1846. Later, this rebel flag became the basis for the state flag of California, and then California was known as the Bear State.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/d/de/Monarch_the_bear.jpg"
				}
			}));

			Animals.Add(new AnimalGroup("Cats", new List<Animal>
			{
				new Animal
				{
					Name = "Abyssinian",
					Location = "Ethopia",
					Details = "The Abyssinian is a breed of domestic short-haired cat with a distinctive tickedtabby coat, in which individual hairs are banded with different colors. The breed is named for Abyssinia (now called Ethiopia), where it is believed to have originated.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9b/Gustav_chocolate.jpg/168px-Gustav_chocolate.jpg"
				},
				new Animal
				{
					Name = "Arabian Mau",
					Location = "Arabian Peninsula",
					Details = "The Arabian Mau is a formal breed of domestic cat, originated from the desert cat, a short-haired landrace native to the desert of the Arabian Peninsula. It lives there in the streets and has adapted very well to the extreme climate. The Arabian Mau is recognized as a formal breed by few fancier and breeder organization and cat registry, World Cat Federation (WCF) and Emirates Feline Federation (EFF). Based on one landrace, the Arabian Mau is a natural breed.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/d/d3/Bex_Arabian_Mau.jpg"
				},
				new Animal
				{
					Name = "Bengal",
					Location = "Asia",
					Details = "The Bengal cat is a domesticated cat breed created from hybrids of domestic cats and the Asian leopard cat – the breed name comes from the taxonomic name. Back-crossing to domestic cats is then done with the goal of creating a healthy, and docile cat with wild-looking, high-contrast coat. Bengals have a wild appearance and may show spots, rosettes, arrowhead markings, or marbling.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/ba/Paintedcats_Red_Star_standing.jpg/187px-Paintedcats_Red_Star_standing.jpg"
				},
				new Animal
				{
					Name = "Burmese",
					Location = "Thailand",
					Details = "The Burmese cat is a breed of domestic cat, originating in Thailand, believed to have its roots near the present Thai-Burma border and developed in the United States and Britain.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/0/04/Blissandlucky11.jpg"
				},
				new Animal
				{
					Name = "Cyprus",
					Location = "Cyprus",
					Details = "Cyprus cats, also known as Cypriot cats, Saint Helen cats, and Saint Nicholas cats, are a landrace of domestic cat found across the island of Cyprus. A standardized breed is being developed from them; among cat fancier and breeder organizations, it is presently fully recognized by the World Cat Federation (WCF), with breeding regulated by the World Cat Congress (WCC), under the name Aphrodite's Giant; and provisionally by The International Cat Association (TICA) as the Aphrodite. All three organizations permit shorthaired and semi-longhaired versions and no out-crossing to other breeds.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b9/CyprusShorthair.jpg/320px-CyprusShorthair.jpg"
				},
				new Animal
				{
					Name = "German Rex",
					Location = "Germany",
					Details = "The German Rex is a medium-sized breed with slender legs of a medium length. The head is round with well-developed cheeks and large, open ears. The eyes are of medium size in colours related to the coat colour. The coat is silky and short with a tendency to curl. The whiskers also curl, though less strongly than in the Cornish Rex. They may be nearly straight. All colours of coat, including white, are allowed. The body development is heavier than in the Cornish Rex - more like the European Shorthairs.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c7/German_rex_harry_%28cropped%29.jpg"
				},
				new Animal
				{
					Name = "Highlander",
					Location = "United States",
					Details = "The Highlander (also known as the Highlander Shorthair, and originally as the Highland Lynx), is an experimental breed of cat. The unique appearance of the Highlander comes from the deliberate cross between the Desert Lynx and the Jungle Curl breeds, also recently developed. The latter of these has some non-domestic ancestry from two Asian small cat species, the leopard cat and jungle cat, making the Highlander nominally a feline hybrid, though its foundation stock is mostly domestic cat.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/15/Highlander-7.jpg/293px-Highlander-7.jpg"
				},
				new Animal
				{
					Name = "Manx",
					Location = "Isle of Man",
					Details = "The Manx cat is a breed of domestic cat originating on the Isle of Man, with a naturally occurring mutation that shortens the tail. Many Manx have a small stub of a tail, but Manx cats are best known as being entirely tailless; this is the most distinguishing characteristic of the breed, along with elongated hind legs and a rounded head. Manx cats come in all coat colours and patterns, though all-white specimens are rare, and the coat range of the original stock was more limited. Long-haired variants are sometimes considered a separate breed, the Cymric. Manx are prized as skilled hunters, and thus have often been sought by farmers with rodent problems, and been a preferred ship's cat breed. They are said to be social, tame and active. An old local term for the cats on their home island is stubbin. Manx have been exhibited in cat shows since the 1800s, with the first known breed standard published in 1903.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/en/9/9b/Manx_cat_by_Karen_Weaver.jpg"
				},
				new Animal
				{
					Name = "Peterbald",
					Location = "Russia",
					Details = "The Peterbald is a cat breed of Russian origin. It was created in St Petersburg in 1994 from an experimental breeding by Olga S. Mironova. They resemble Oriental Shorthairs with a hair-losing gene. The breed was accepted for Championship class competition in 2009.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c7/Peterbald_male_Shango_by_Irina_Polunina.jpg"
				},
				new Animal
				{
					Name = "Scottish Fold",
					Location = "Scotland",
					Details = "The Scottish Fold is a breed of domestic cat with a natural dominant-gene mutation that affects cartilage throughout the body, causing the ears to fold, bending forward and down towards the front of the head, which gives the cat what is often described as an owl-like appearance.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5d/Adult_Scottish_Fold.jpg/240px-Adult_Scottish_Fold.jpg"
				},
				new Animal
				{
					Name = "Sphynx",
					Location = "Europe",
					Details = "The Sphynx cat is a breed of cat known for its lack of coat (fur). It was developed through selective breeding, starting in the 1960s. The skin should have the texture of chamois, as it has fine hairs, or they may be completely hairless. Whiskers may be present, either whole or broken, or may be totally absent. They also have a narrow, long head, and webbed feet. Their skin is the color that their fur would be, and all the usual cat markings (solid, point, van, tabby, tortie, etc.) may be found on the Sphynx cat's skin. Because they have no coat, they lose more body heat than coated cats. This makes them warm to the touch as well as heat-seeking.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e8/Sphinx2_July_2006.jpg/180px-Sphinx2_July_2006.jpg"
				}
			}));

			Animals.Add(new AnimalGroup("Dogs", new List<Animal>
			{
				new Animal
				{
					Name = "Afghan Hound",
					Location = "Afghanistan",
					Details = "The Afghan Hound is a hound that is distinguished by its thick, fine, silky coat and its tail with a ring curl at the end. The breed is selectively bred for its unique features in the cold mountains of Afghanistan.  Other names for this breed are Kuchi Hound, Tāzī, Balkh Hound, Baluchi Hound, Barakzai Hound, Shalgar Hound, Kabul Hound, Galanday Hound or sometimes incorrectly African Hound.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/6/69/Afghane.jpg"
				},
				new Animal
				{
					Name = "Alpine Dachsbracke",
					Location = "Austria",
					Details = "The Alpine Dachsbracke is a small breed of dog of the scent hound type originating in Austria. The Alpine Dachsbracke was bred to track wounded deer as well as boar, hare, and fox. It is highly efficient at following a trail even after it has gone cold. The Alpine Dachsbracke is very sturdy, and Austria is said to be the country of origin.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/2/23/Alpejski_gończy_krótkonożny_g99.jpg/320px-Alpejski_gończy_krótkonożny_g99.jpg"
				},
				new Animal
				{
					Name = "American Bulldog",
					Location = "United States",
					Details = "The American Bulldog is a breed of utility dog descended from the Old English Bulldog.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/5/5e/American_Bulldog_600.jpg"
				},
				new Animal
				{
					Name = "Bearded Collie",
					Location = "Scotland",
					Details = "The Bearded Collie, or Beardie, is a herding breed of dog once used primarily by Scottish shepherds, but now mostly a popular family companion. Bearded Collies have an average weight of 18–27 kilograms (40–60 lb). Males are around 51–56 centimetres (20–22 in) tall at the withers while females are around 51–53 centimetres (20–21 in) tall.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/9/9c/Bearded_Collie_600.jpg"
				},
				new Animal
				{
					Name = "Boston Terrier",
					Location = "United States",
					Details = "The Boston Terrier is a breed of dog originating in the United States of America. This American Gentleman was accepted in 1893 by the American Kennel Club as a non-sporting breed. Color and markings are important when distinguishing this breed to the AKC standard. They should be either black, brindle or seal with white markings. Bostons are small and compact with a short tail and erect ears. The AKC says they are highly intelligent and very easily trained. They are friendly and can be stubborn at times. The average life span of a Boston is around 11 to 13 years, though some can live well into their teens.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/d7/Boston-terrier-carlos-de.JPG/320px-Boston-terrier-carlos-de.JPG"
				},
				new Animal
				{
					Name = "Canadian Eskimo",
					Location = "Canada",
					Details = "The Canadian Eskimo Dog is an Arctic breed of working dog, which is often considered to be one of North America's oldest and rarest remaining purebred indigenous domestic canines. Other names include qimmiq or qimmit. They were brought from Siberia to North America by the Thule people 1,000 years ago, along with the Greenland Dog that is genetically identical.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/79/Spoonsced.jpg"
				},
				new Animal
				{
					Name = "Eurohound",
					Location = "Scandinavia",
					Details = "A Eurohound (also known as a Eurodog or Scandinavian hound) is a type of dog bred for sled dog racing. The Eurohound is typically crossbred from the Alaskan husky group and any of a number of pointing breeds.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/9/98/Eurohound.jpg"
				},
				new Animal
				{
					Name = "Irish Terrier",
					Location = "Ireland",
					Details = "The Irish Terrier is a dog breed from Ireland, one of many breeds of terrier. The Irish Terrier is considered one of the oldest terrier breeds. The Dublin dog show in 1873 was the first to provide a separate class for Irish Terriers. By the 1880s, Irish Terriers were the fourth most popular breed in Ireland and Britain.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/56/IrishTerrierSydenhamHillWoods.jpg/180px-IrishTerrierSydenhamHillWoods.jpg"
				},
				new Animal
				{
					Name = "Kerry Beagle",
					Location = "Ireland",
					Details = "The Kerry Beagle is one of the oldest Irish hound breeds, believed to be descendant from the Old Southern Hound or the Celtic Hounds. It is the only extant scent hound breed native to Ireland.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/75/Kerry_Beagle_from_1915.JPG"
				},
				new Animal
				{
					Name = "Norwegian Buhund",
					Location = "Norway",
					Details = "The Norwegian Buhund is a breed of dog of the spitz type. It is closely related to the Icelandic Sheepdog and the Jämthund. The Buhund is used as an all purpose farm and herding dog, as well as watch dog and a nanny dog.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/3/3b/Norwegian_Buhund_600.jpg"
				},
				new Animal
				{
					Name = "Patterdale Terrier",
					Location = "England",
					Details = "The Patterdale Terrier is a breed of dog descended from the Northern terrier breeds of the early 20th century. The origins of the breed can be traced back to the Lake District, specifically to Ullswater Hunt master Joe Bowman, an early Border Terrier breeder.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/ec/05078045_Patterdale_Terrier.jpg/320px-05078045_Patterdale_Terrier.jpg"
				},
				new Animal
				{
					Name = "St. Bernard",
					Location = "Italy, Switzerland",
					Details = "The St. Bernard or St Bernard is a breed of very large working dog from the western Alps in Italy and Switzerland. They were originally bred for rescue by the hospice of the Great St Bernard Pass on the Italian-Swiss border. The hospice, built by and named after Italian monk Bernard of Menthon, acquired its first dogs between 1660 and 1670. The breed has become famous through tales of alpine rescues, as well as for its enormous size.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/6/64/Hummel_Vedor_vd_Robandahoeve.jpg/320px-Hummel_Vedor_vd_Robandahoeve.jpg"
				}
			}));

			Animals.Add(new AnimalGroup("Elephants", new List<Animal>
			{
				new Animal
				{
					Name = "African Bush Elephant",
					Location = "Africa",
					Details = "The African bush elephant, also known as the African savanna elephant, is the larger of the two species of African elephants, and the largest living terrestrial animal. These elephants were previously regarded as the same species, but the African forest elephant has been reclassified as L. cyclotis.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/91/African_Elephant_%28Loxodonta_africana%29_bull_%2831100819046%29.jpg/320px-African_Elephant_%28Loxodonta_africana%29_bull_%2831100819046%29.jpg"
				},
				new Animal
				{
					Name = "African Forest Elephant",
					Location = "Africa",
					Details = "The African forest elephant is a forest-dwelling species of elephant found in the Congo Basin. It is the smallest of the three extant species of elephant, but still one of the largest living terrestrial animals. The African forest elephant and the African bush elephan  were considered to be one species until genetic studies indicated that they separated an estimated 2–7 million years ago. From an estimated population size of over 2 million prior to the colonization of Africa, the population in 2015 is estimated to be about 100,000 forest elephants, mostly living in the forests of Gabon. Due to a slower birth rate, the forest elephant takes longer to recover from poaching, which caused its population to fall by 65% from 2002 to 2014.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/6/6a/African_Forest_Elephant.jpg"
				},
				new Animal
				{
					Name = "Desert Elephant",
					Location = "Africa",
					Details = "Desert elephants, or desert-adapted elephants are not a distinct species of elephant but are African bush elephants that have made their homes in the Namib and Sahara deserts in Africa. It was believed at one time that they were a subspecies of the African bush elephant but this is no longer thought to be the case. Desert-dwelling elephants were once more widespread in Africa than they are now and are currently found only in Namibia and Mali. They tend to migrate from one waterhole to another following traditional routes which depend on the seasonal availability of food and water. They face pressure from poaching and from changes in land use by humans.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/77/Desert_elephants_in_the_Huab_River.jpg/320px-Desert_elephants_in_the_Huab_River.jpg"
				},
				new Animal
				{
					Name = "Borneo Elephant",
					Location = "Asia",
					Details = "The Borneo elephant, also called the Borneo pygmy elephant, is a subspecies of Asian elephant that inhabits northeastern Borneo, in Indonesia and Malaysia. Its origin remains the subject of debate. A definitive subspecific classification as Elephas maximus borneensis awaits a detailed range-wide morphometric and genetic study. Since 1986, Elephas maximus has been listed as Endangered on the IUCN Red List as the population has declined by at least 50% over the last three generations, estimated to be 60–75 years. The species is pre-eminently threatened by habitat loss, degradation and fragmentation.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e4/Elephant_%40_kabini.jpg/180px-Elephant_%40_kabini.jpg"
				},
				new Animal
				{
					Name = "Indian Elephant",
					Location = "Asia",
					Details = "The Indian elephant is one of three extant recognized subspecies of the Asian elephant and native to mainland Asia. Since 1986, the Asian elephant has been listed as Endangered on the IUCN Red List as the wild population has declined by at least 50% since the 1930s to 1940s, i.e. three elephant generations. The Asian elephant is threatened by habitat loss, degradation and fragmentation.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/98/Elephas_maximus_%28Bandipur%29.jpg/320px-Elephas_maximus_%28Bandipur%29.jpg"
				},
				new Animal
				{
					Name = "Sri Lankan Elephant",
					Location = "Asia",
					Details = "The Sri Lankan elephant is one of three recognized subspecies of the Asian elephant, and native to Sri Lanka. Since 1986, Elephas maximus has been listed as endangered by IUCN as the population has declined by at least 50% over the last three generations, estimated to be 60–75 years. The species is primarily threatened by habitat loss, degradation and fragmentation.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b1/Srilankan_tuskelephant.jpg/213px-Srilankan_tuskelephant.jpg"
				},
				new Animal
				{
					Name = "Sumatran Elephant",
					Location = "Asia",
					Details = "The Sumatran elephant is one of three recognized subspecies of the Asian elephant, and native to the Indonesia island of Sumatra. In 2011, the Sumatran elephant has been classified as critically endangered by IUCN as the population has declined by at least 80% over the last three generations, estimated to be about 75 years. The subspecies is pre-eminently threatened by habitat loss, degradation and fragmentation, and poaching; over 69% of potential elephant habitat has been lost within the last 25 years. Much of the remaining forest cover is in blocks smaller than 250 km2 (97 sq mi), which are too small to contain viable elephant populations.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b6/Borobudur-Temple-Park_Elephant-cage-01.jpg/320px-Borobudur-Temple-Park_Elephant-cage-01.jpg"
				},
				new Animal
				{
					Name = "Pygmy Elephant",
					Location = "Africa & Asia",
					Details = "Pygmy elephants live in both Africa and Asia.The African pygmy elephant is currently considered to be a tiny morph of the African forest elephant. The Borneo elephant, a well-documented variety of elephant, is also calledmpygmy elephant. This elephant, inhabiting tropical rainforest in north Borneo (east Sabah and extreme north Kalimantan), was long thought to be identical to the Asian elephant and descended from a captive population. In 2003, DNA comparison revealed them to be probably a new subspecies.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/9/93/Borneo-elephant-PLoS_Biology.jpg"
				},
				new Animal
				{
					Name = "Mammoth",
					Location = "Extinct",
					Details = "A mammoth is any species of the extinct genus Mammuthus, one of the many genera that make up the order of trunked mammals called proboscideans. The various species of mammoth were commonly equipped with long, curved tusks and, in northern species, a covering of long hair. They lived from the Pliocene epoch (from around 5 million years ago) into the Holocene at about 4,000 years ago, and various species existed in Africa, Europe, Asia, and North America. They were members of the family Elephantidae, which also contains the two genera of modern elephants and their ancestors.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/6/62/Columbian_mammoth.JPG/320px-Columbian_mammoth.JPG"
				},
				new Animal
				{
					Name = "Mastodon",
					Location = "Extinct",
					Details = "Mastodons are any species of extinct proboscideans in the genus Mammut, distantly related to elephants, that inhabited North and Central America during the late Miocene or late Pliocene up to their extinction at the end of the Pleistocene 10,000 to 11,000 years ago. Mastodons lived in herds and were predominantly forest-dwelling animals that fed on a mixed diet obtained by browsing and grazing with a seasonal preference for browsing, similar to living elephants.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b0/Mammut_americanum.jpg/320px-Mammut_americanum.jpg"
				},
				new Animal
				{
					Name = "Dwarf Elephant",
					Location = "Extinct",
					Details = "Dwarf elephants are prehistoric members of the order Proboscidea which, through the process of allopatric speciation on islands, evolved much smaller body sizes (around 1.5-2.3 metres) in comparison with their immediate ancestors. Dwarf elephants are an example of insular dwarfism, the phenomenon whereby large terrestrial vertebrates (usually mammals) that colonize islands evolve dwarf forms, a phenomenon attributed to adaptation to resource-poor environments and selection for early maturation and reproduction. Some modern populations of Asian elephants have also undergone size reduction on islands to a lesser degree, resulting in populations of pygmy elephants.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/6/69/Elephas_skeleton.JPG/320px-Elephas_skeleton.JPG"
				},
				new Animal
				{
					Name = "Pygmy Mammoth",
					Location = "Extinct",
					Details = "The pygmy mammoth or Channel Islands mammoth is an extinct species of dwarf elephant descended from the Columbian mammoth of mainland North America. This species became extinct during the Quaternary extinction event in which many megafauna species became extinct due to changing conditions to which the species could not adapt. A case of island or insular dwarfism, from a recent analysis in 2010 it was determined that M. exilis was on average, 1.72 m (5.6 ft) tall at the shoulders and 760 kg (1,680 lb) in weight, in stark contrast to its 4.3 m (14 ft) tall, 9,070 kg (20,000 lb) ancestor. Another estimate gives a shoulder height of 2.02 m (6.6 ft) and a weight of 1,350 kg (2,980 lb).",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/f/f6/Mammuthus_exilis.jpg"
				}
			}));

			if (includeEmptyGroups)
				Animals.Add(new AnimalGroup("Horses", new List<Animal>()));

			Animals.Add(new AnimalGroup("Monkeys", new List<Animal>
			{
				new Animal
				{
					Name = "Baboon",
					Location = "Africa & Asia",
					Details = "Baboons are African and Arabian Old World monkeys belonging to the genus Papio, part of the subfamily Cercopithecinae.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/f/fc/Papio_anubis_%28Serengeti%2C_2009%29.jpg/200px-Papio_anubis_%28Serengeti%2C_2009%29.jpg"
				},
				new Animal
				{
					Name = "Capuchin Monkey",
					Location = "Central & South America",
					Details = "The capuchin monkeys are New World monkeys of the subfamily Cebinae. Prior to 2011, the subfamily contained only a single genus, Cebus.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/40/Capuchin_Costa_Rica.jpg/200px-Capuchin_Costa_Rica.jpg"
				},
				new Animal
				{
					Name = "Blue Monkey",
					Location = "Central and East Africa",
					Details = "The blue monkey or diademed monkey is a species of Old World monkey native to Central and East Africa, ranging from the upper Congo River basin east to the East African Rift and south to northern Angola and Zambia",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/83/BlueMonkey.jpg/220px-BlueMonkey.jpg"
				},
				new Animal
				{
					Name = "Squirrel Monkey",
					Location = "Central & South America",
					Details = "The squirrel monkeys are the New World monkeys of the genus Saimiri. They are the only genus in the subfamily Saimirinae. The name of the genus Saimiri is of Tupi origin, and was also used as an English name by early researchers.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/2/20/Saimiri_sciureus-1_Luc_Viatour.jpg/220px-Saimiri_sciureus-1_Luc_Viatour.jpg"
				},
				new Animal
				{
					Name = "Golden Lion Tamarin",
					Location = "Brazil",
					Details = "The golden lion tamarin also known as the golden marmoset, is a small New World monkey of the family Callitrichidae.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/87/Golden_lion_tamarin_portrait3.jpg/220px-Golden_lion_tamarin_portrait3.jpg"
				},
				new Animal
				{
					Name = "Howler Monkey",
					Location = "South America",
					Details = "Howler monkeys are among the largest of the New World monkeys. Fifteen species are currently recognised. Previously classified in the family Cebidae, they are now placed in the family Atelidae.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0d/Alouatta_guariba.jpg/200px-Alouatta_guariba.jpg"
				},
				new Animal
				{
					Name = "Japanese Macaque",
					Location = "Japan",
					Details = "The Japanese macaque, is a terrestrial Old World monkey species native to Japan. They are also sometimes known as the snow monkey because they live in areas where snow covers the ground for months each",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c1/Macaca_fuscata_fuscata1.jpg/220px-Macaca_fuscata_fuscata1.jpg"
				},
				new Animal
				{
					Name = "Mandrill",
					Location = "Southern Cameroon, Gabon, Equatorial Guinea, and Congo",
					Details = "The mandrill is a primate of the Old World monkey family, closely related to the baboons and even more closely to the drill. It is found in southern Cameroon, Gabon, Equatorial Guinea, and Congo.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/75/Mandrill_at_san_francisco_zoo.jpg/220px-Mandrill_at_san_francisco_zoo.jpg"
				},
				new Animal
				{
					Name = "Proboscis Monkey",
					Location = "Borneo",
					Details = "The proboscis monkey or long-nosed monkey, known as the bekantan in Malay, is a reddish-brown arboreal Old World monkey that is endemic to the south-east Asian island of Borneo.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e5/Proboscis_Monkey_in_Borneo.jpg/250px-Proboscis_Monkey_in_Borneo.jpg"
				},
				new Animal
				{
					Name = "Red-shanked Douc",
					Location = "Vietnam, Laos",
					Details = "The red-shanked douc is a species of Old World monkey, among the most colourful of all primates. This monkey is sometimes called the \"costumed ape\" for its extravagant appearance. From its knees to its ankles it sports maroon-red \"stockings\", and it appears to wear white forearm length gloves. Its attire is finished with black hands and feet. The golden face is framed by a white ruff, which is considerably fluffier in males. The eyelids are a soft powder blue. The tail is white with a triangle of white hair at the base. Males of all ages have a white spot on both sides of the corners of the rump patch, and red and white genitals.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9f/Portrait_of_a_Douc.jpg/159px-Portrait_of_a_Douc.jpg"
				},
				new Animal
				{
					Name = "Gray-shanked Douc",
					Location = "Vietnam",
					Details = "The gray-shanked douc langur is a douc species native to the Vietnamese provinces of Quảng Nam, Quảng Ngãi, Bình Định, Kon Tum, and Gia Lai. The total population is estimated at 550 to 700 individuals. In 2016, Dr Benjamin Rawson, Country Director of Fauna & Flora International - Vietnam Programme, announced a discovery of an additional population of more than 500 individuals found in Central Vietnam, bringing the total population up to approximately 1000 individuals.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0b/Cuc.Phuong.Primate.Rehab.center.jpg/320px-Cuc.Phuong.Primate.Rehab.center.jpg"
				},
				new Animal
				{
					Name = "Golden Snub-nosed Monkey",
					Location = "China",
					Details = "The golden snub-nosed monkey is an Old World monkey in the Colobinae subfamily. It is endemic to a small area in temperate, mountainous forests of central and Southwest China. They inhabit these mountainous forests of Southwestern China at elevations of 1,500-3,400 m above sea level. The Chinese name is Sichuan golden hair monkey. It is also widely referred to as the Sichuan snub-nosed monkey. Of the three species of snub-nosed monkeys in China, the golden snub-nosed monkey is the most widely distributed throughout China.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c8/Golden_Snub-nosed_Monkeys%2C_Qinling_Mountains_-_China.jpg/165px-Golden_Snub-nosed_Monkeys%2C_Qinling_Mountains_-_China.jpg"
				},
				new Animal
				{
					Name = "Black Snub-nosed Monkey",
					Location = "China",
					Details = "The black snub-nosed monkey, also known as the Yunnan snub-nosed monkey, is an endangered species of primate in the family Cercopithecidae. It is endemic to China, where it is known to the locals as the Yunnan golden hair monkey and the black golden hair monkey. It is threatened by habitat loss. It was named after Bishop Félix Biet.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/59/RhinopitecusBieti.jpg/320px-RhinopitecusBieti.jpg"
				},
				new Animal
				{
					Name = "Tonkin Snub-nosed Monkey",
					Location = "Vietnam",
					Details = "The Tonkin snub-nosed monkey or Dollman's snub-nosed monkey is a slender-bodied arboreal Old World monkey, endemic to northern Vietnam. It is a black and white monkey with a pink nose and lips and blue patches round the eyes. It is found at altitudes of 200 to 1,200 m (700 to 3,900 ft) on fragmentary patches of forest on craggy limestone areas. First described in 1912, the monkey was rediscovered in 1990 but is exceedingly rare. In 2008, fewer than 250 individuals were thought to exist, and the species was the subject of intense conservation effort. The main threats faced by these monkeys is habitat loss and hunting, and the International Union for Conservation of Nature has rated the species as \"critically endangered\".",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9c/Tonkin_snub-nosed_monkeys_%28Rhinopithecus_avunculus%29.jpg/320px-Tonkin_snub-nosed_monkeys_%28Rhinopithecus_avunculus%29.jpg"
				},
				new Animal
				{
					Name = "Thomas's Langur",
					Location = "Indonesia",
					Details = "Thomas's langur is a species of primate in the family Cercopithecidae. It is endemic to North Sumatra, Indonesia. Its natural habitat is subtropical or tropical dry forests. It is threatened by habitat loss. Its native names are reungkah in Acehnese and kedih in Alas.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/31/Thomas%27s_langur_Presbytis_thomasi.jpg/142px-Thomas%27s_langur_Presbytis_thomasi.jpg"
				},
				new Animal
				{
					Name = "Purple-faced Langur",
					Location = "Sri Lanka",
					Details = "The purple-faced langur, also known as the purple-faced leaf monkey, is a species of Old World monkey that is endemic to Sri Lanka. The animal is a long-tailed arboreal species, identified by a mostly brown appearance, dark face (with paler lower face) and a very shy nature. The species was once highly prevalent, found in suburban Colombo and the \"wet zone\" villages (areas with high temperatures and high humidity throughout the year, whilst rain deluges occur during the monsoon seasons), but rapid urbanization has led to a significant decrease in the population level of the monkeys.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/02/Semnopithèque_blanchâtre_mâle.JPG/192px-Semnopithèque_blanchâtre_mâle.JPG"
				},
				new Animal
				{
					Name = "Gelada",
					Location = "Ethiopia",
					Details = "The gelada, sometimes called the bleeding-heart monkey or the gelada baboon, is a species of Old World monkey found only in the Ethiopian Highlands, with large populations in the Semien Mountains. Theropithecus is derived from the Greek root words for \"beast-ape.\" Like its close relatives the baboons, it is largely terrestrial, spending much of its time foraging in grasslands.",
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/13/Gelada-Pavian.jpg/320px-Gelada-Pavian.jpg"
				}
			}));
		}
	}
}
