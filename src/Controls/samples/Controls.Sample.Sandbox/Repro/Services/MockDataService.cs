using System.Diagnostics;
using System.Net.NetworkInformation;
using AllTheLists.Models;
using AllTheLists.Models.Learning;
using Fonts;
using Contact = AllTheLists.Models.Contact;
using System.Linq;

namespace AllTheLists.Services;

public static class MockDataService
{
    private static List<Product> _products;

    private static string[] shoeImages = new string[] { "shoe_01.png", "shoe_02.png", "shoe_03.png", "shoe_04.png", "shoe_05.png", "shoe_06.png", "shoe_07.png", "shoe_08.png" };
    
    public static List<Product> GenerateProducts()
    {

		if(_products != null)
			return _products;

        int count = 10000;
		_products = new List<Product>();
        for (int i = 0; i < count; i++)
        {
            var ran = Random.Shared.Next(1, 1000);
            _products.Add(
                new Product
                {
                    Id = i,
                    Name = $"Product {i}",
                    Price = ran,
                    Description = "This is a sample product description.",
                    ImageUrl = $"https://picsum.photos/80/80?random={i}",
                    SocialImageUrl = ran % 2 == 0 ? "" : $"https://picsum.photos/400/300?random={i}",
                    Image = shoeImages[Random.Shared.Next(0, shoeImages.Length)],
                    Company = $"Company {i}",
                    Type = $"Type {i}",
                    SalesCategory = Random.Shared.Next(0, 3) switch
                    {
                        0 => "NEW",
                        1 => "BEST SELLER",
                        _ => ""
                    },
                    Category = Random.Shared.Next(0, 4) switch
                    {
                        0 => "Men's Sportswear",
                        1 => "Men's Originals",
                        2 => "Originals",
                        _ => "Sportswear"
                    },
                    ColorWays = Random.Shared.Next(1, 16)
                });
        }
        return _products;
    }

   

    private static List<ProductDisplay> _productDisplays;

    public static List<ProductDisplay> GenerateProductDisplays()
    {
        if (_productDisplays != null)
            return _productDisplays;

        int count = 1000;
        _productDisplays = new List<ProductDisplay>();
        for (int i = 0; i < count; i++)
        {
            if (i < 4)
            {
                _productDisplays.Add(new ProductDisplay
                {
                    Products = GenerateProducts().GetRange(i * 2, 2)
                });
            }
            else if (i % 3 == 1)
            {
                _productDisplays.Add(new ProductDisplay
                {
                    Products = GenerateProducts().GetRange(i * 2 - 1, 1)
                });
            }
            else
            {
                _productDisplays.Add(new ProductDisplay
                {
                    Products = GenerateProducts().GetRange(i * 2 - 2, 2)
                });
            }

            Debug.WriteLine($"Product Display {i} has {GenerateProducts().GetRange(i * 2, 2).Count} products");
        }
        return _productDisplays;
    }  

    private static string _lipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur faucibus tortor nisi, at luctus massa molestie sit amet. Curabitur quam ligula, auctor sit amet viverra in, mattis eget libero. Nam pulvinar ante non mauris volutpat faucibus vitae ac diam. Fusce nec fringilla dolor. Nulla vitae justo quis mauris rutrum rutrum quis sed nisl. Nam vehicula erat et enim hendrerit, sollicitudin molestie nibh venenatis. Donec sapien sem, imperdiet et dui gravida, imperdiet viverra velit. Nulla sed quam porttitor, rhoncus felis quis, fermentum est. Quisque eu nulla urna. Ut vitae mauris vitae turpis malesuada lobortis nec sit amet massa. Vivamus vehicula arcu ac tempus aliquet. In mollis hendrerit consequat.";

    public static List<Review> GenerateReviews()
    {
        int count = 100;
        var reviews = new List<Review>();
        for (int i = 0; i < count; i++)
        {
            reviews.Add(new Review
            {
                Author = $"Author {i}",
                Comment = _lipsum.Substring(0, Random.Shared.Next(50, (_lipsum.Length - 1))),
                Car = $"Car {i}",
                ChargerType = $"Charger Type {i}",
                CreatedAt = DateTime.Now,
                Status = i % 2 == 0
            });
        }
        return reviews;
    }

    private static List<CheckIn> _checkIns;

    public static List<CheckIn> GenerateCheckIns()
    {
        if (_checkIns != null)
            return _checkIns;

        int count = 500;
        string[] avatars = guyAvatars.Concat(galAvatars).ToArray();
        _checkIns = new List<CheckIn>();
        for (int i = 0; i < count; i++)
        {
            _checkIns.Add(new CheckIn
            {
                Author = $"Author {i}",
                AuthorIcon = avatars[Random.Shared.Next(0, avatars.Length)],// $"https://picsum.photos/id/{Random.Shared.Next(1, 1000)}/40",
                Venue = new Venue
                {
                    Name = $"Venue {i}",
                    Address = $"Address {i}"
                },
                Product = GenerateProducts()[Random.Shared.Next(0, GenerateProducts().Count - 1)],
                Comment = _lipsum.Substring(0, Random.Shared.Next(0, 30)),
                Rating = Random.Shared.Next(1, 5),
                ServingStyle = $"Serving Style {i}",
                CreatedAt = DateTime.Now,
                SocialImage = $"https://picsum.photos/id/{Random.Shared.Next(1, 1000)}/300"
            });
        }
        return _checkIns;
    }

    private static List<Unit> _units;  

    public static string[] unitTitles = new string[]{
        "Basic Greetings",
        "Talking about Your Day",
        "Making Appointments with Friends",
        "Ordering in the Restaurant",
        "Shopping in Korea",
        "Asking for Directions",
        "Asking About the Past",
        "Talking about Weekend Plans",
        "Shopping in Korea",
        "Making Appointments with Friends",
        "Going to the Hospital"
    };

    public static string[] chapterTitles = new string[]{
        "Greeting for the First Time",
        "Starting an Introduction",
        "Talking about the Weather",
        "Talking about the Your Nationality",
        "Talking about Your Age",
        "Test"
    };

    public static string[] lessonTitles = new string[]{
        "Hello & Thank you",
        "What is your name?",
        "Where are you from?",
        "How old are you?",
        "What do you do?",
        "What is your phone number?",
        "What is your email address?",
        "What is your address?",
        "What are you doing?"
    };  

    public static string[] icons = new string[]{
        FontAwesome.Book,
        FontAwesome.Video,
        FontAwesome.Music,
        FontAwesome.Tv,
        FontAwesome.Film,
        FontAwesome.Gamepad,
        FontAwesome.Headphones,
        FontAwesome.Microphone,
    };

    public static List<Unit> GenerateUnits()
    {   

        if (_units != null)
            return _units;

        int count = 4;
        _units = new List<Unit>();
        for (int i = 0; i < count; i++)
        {
            _units.Add(new Unit
            {
                UnitNumber = i,
                Title = unitTitles[i],
                SubTitle = $"Sub Title {i}",
                Icon = $"{icons[Random.Shared.Next(0, icons.Length - 1)]}",
                Chapters = GenerateChapters(Random.Shared.Next(1, chapterTitles.Length))
            });
        }
        return _units;
    }

    private static List<Chapter> GenerateChapters(int v)
    {
        var chapters = new List<Chapter>();
        for (int i = 0; i < v; i++)
        {
            chapters.Add(new Chapter
            {
                Title = chapterTitles[i],
                Lessons = GenerateLessons(Random.Shared.Next(1, lessonTitles.Length))
            });
        }
        return chapters;
    }

    private static List<Lesson> GenerateLessons(int count)
    {
        var lessons = new List<Lesson>();
        for (int i = 0; i < count; i++)
        {
            lessons.Add(new Lesson
            {
                Title = lessonTitles[i]                
            });
        }
        return lessons;
    }

    private static List<Contact> _contacts;

    public static string[] guyFirstNames = new string[]{
        "John",
        "Michael",
        "William",
        "James",
        "David",
        "Joseph",
        "Charles",
        "Thomas",
        "Daniel",
        "Matthew",
        "Anthony",
        "Donald",
        "Mark",
        "Paul",
        "Steven",
        "Andrew",
        "Kenneth",
        "George",
        "Joshua",
        "Kevin",
        "Brian",
        "Edward",
        "Ronald",
        "Timothy",
        "Jason",
        "Jeffrey",
        "Ryan",
        "Jacob",
        "Gary",
        "Nicholas",
        "Eric",
        "Stephen",
        "Jonathan",
        "Larry",
        "Justin",
        "Scott",
        "Brandon",
        "Benjamin",
        "Samuel",
        "Gregory",
        "Frank",
        "Alexander",
        "Raymond",
        "Patrick",
        "Jack",
        "Dennis",
        "Jerry",
        "Tyler",
        "Aaron",
        "Jose",
        "Henry",
        "Douglas",
        "Adam",
        "Peter",
        "Nathan",
        "Zachary",
        "Walter",
        "Kyle",
        "Harold",
        "Carl",
        "Jeremy",
        "Keith",
        "Roger",
        "Gerald",
        "Ethan",
        "Arthur",
        "Terry",
        "Christian",
        "Sean",
        "Lawrence",
        "Austin",
        "Joe",
        "Noah",
        "Jesse",
        "Albert",
        "Bryan",
        "Billy",
        "Bruce",
        "Willie",
        "Jordan",
        "Dylan",
        "Alan",
        "Ralph",
        "Gabriel",
        "Roy",
        "Juan",
        "Wayne",
        "Eugene",
        "Logan",
        "Randy",
        "Louis",
        "Russell",
        "Vincent",
        "Philip",
        "Bobby",
        "Johnny",
        "Bradley",
        "Martin",
        "Craig",
        "Stanley",
        "Shawn"
    };

    public static string[] galFirstNames = new string[]{
        "Emily",
        "Emma",
        "Olivia",
        "Ava",
        "Isabella",
        "Sophia",
        "Mia",
        "Charlotte",
        "Amelia",
        "Harper",
        "Evelyn",
        "Abigail",
        "Emily",
        "Elizabeth",
        "Mila",
        "Ella",
        "Avery",
        "Sofia",
        "Camila",
        "Aria",
        "Scarlett",
        "Victoria",
        "Madison",
        "Luna",
        "Grace",
        "Chloe",
        "Penelope",
        "Layla",
        "Riley",
        "Zoey",
        "Nora",
        "Lily",
        "Eleanor",
        "Hannah",
        "Lillian",
        "Addison",
        "Aubrey",
        "Ellie",
        "Stella",
        "Natalie",
        "Zoe",
        "Leah",
        "Hazel",
        "Violet",
        "Aurora",
        "Savannah",
        "Audrey",
        "Brooklyn",
        "Bella",
        "Claire",
        "Skylar",
        "Lucy",
        "Paisley",
        "Everly",
        "Anna",
        "Caroline",
        "Nova",
        "Genesis",
        "Emilia",
        "Kennedy",
        "Samantha",
        "Maya",
        "Willow",
        "Kinsley",
        "Naomi",
        "Aaliyah",
        "Elena",
        "Sarah",
        "Ariana",
        "Allison",
        "Gabriella",
        "Alice",
        "Madelyn",
        "Cora",
        "Ruby",
        "Eva",
        "Serenity",
        "Autumn",
        "Adeline",
        "Hailey",
        "Gianna",
        "Valentina",
        "Isla",
        "Eliana",
        "Quinn",
        "Nevaeh",
        "Ivy",
        "Sadie",
        "Piper",
        "Lydia",
        "Alexa",
        "Josephine",
        "Emery",
        "Julia",
        "Delilah",
        "Arianna",
        "Vivian",
        "Kaylee"
    };

    public static string[] lastNames = new string[]{
        "Smith",
        "Johnson",
        "Williams",
        "Jones",
        "Brown",
        "Davis",
        "Miller",
        "Wilson",
        "Moore",
        "Taylor",
        "Anderson",
        "Thomas",
        "Jackson",
        "White",
        "Harris",
        "Martin",
        "Thompson",
        "Garcia",
        "Martinez",
        "Robinson",
        "Clark",
        "Rodriguez",
        "Lewis",
        "Lee",
        "Walker",
        "Hall",
        "Allen",
        "Young",
        "Hernandez",
        "King",
        "Wright",
        "Lopez",
        "Hill",
        "Scott",
        "Green",
        "Adams",
        "Baker",
        "Gonzalez",
        "Nelson",
        "Carter",
        "Mitchell",
        "Perez",
        "Roberts",
        "Turner",
        "Phillips",
        "Campbell",
        "Parker",
        "Evans",
        "Edwards",
        "Collins",
        "Stewart",
        "Sanchez",
        "Morris",
        "Rogers",
        "Reed",
        "Cook",
        "Morgan",
        "Bell",
        "Murphy",
        "Bailey",
        "Rivera",
        "Cooper",
        "Richardson",
        "Cox",
        "Howard",
        "Ward",
        "Torres",
        "Peterson",
        "Gray",
        "Ramirez",
        "James",
        "Watson",
        "Brooks",
        "Kelly",
        "Sanders",
        "Price",
        "Bennett",
        "Wood",
        "Barnes",
        "Ross",
        "Henderson",
        "Coleman",
        "Jenkins",
        "Perry",
        "Powell",
        "Long",
        "Patterson",
        "Hughes",
        "Flores",
        "Washington",
        "Butler",
        "Simmons",
        "Foster",
        "Gonzales",
        "Bryant",
        "Alexander",
        "Russell",
        "Griffin",
        "Diaz",
        "Hayes"
    };

    public static string[] guyAvatars = new string[]{
        "avatar_02.png",
        "avatar_03.png",
        "avatar_05.png",
        "avatar_08.png",
        "avatar_11.png"
    };

    public static string[] galAvatars = new string[]{
        "avatar_04.png",
        "avatar_06.png",
        "avatar_07.png",
        "avatar_09.png",
        "avatar_10.png"
    };

    public static List<Contact> GenerateContacts()
    {
        if (_contacts != null)
            return _contacts;

        int count = 100;
        _contacts = new List<Contact>();
        for (int i = 0; i < count; i++)
        {
            string[] firstNames;
            string[] avatars;

            if (Random.Shared.Next(2) == 0)
            {
                // Generate guy contact
                firstNames = guyFirstNames;
                avatars = guyAvatars;
            }
            else
            {
                // Generate gal contact
                firstNames = galFirstNames;
                avatars = galAvatars;
            }

            _contacts.Add(new Contact
            {
                FirstName = firstNames[Random.Shared.Next(0, firstNames.Length)],
                LastName = lastNames[Random.Shared.Next(0, lastNames.Length)],
                Company = $"Company {i}",
                PhoneNumber = $"Phone Number {i}",
                Email = $"Email {i}",
                ProfilePicture = Random.Shared.Next(2) == 0 ? "" : avatars[Random.Shared.Next(0, avatars.Length)],
                Address = $"Address {i}",
                City = $"City {i}",
                State = $"State {i}",
                ZipCode = $"Zip Code {i}",
                Country = $"Country {i}"
            });
        }
        return _contacts;
    }

    private static string[] PostersLandscape = new string[]{
        "poster_01_landscape.png",
        "poster_02_landscape.png",
        "poster_03_landscape.png",
        "poster_04_landscape.png",
        "poster_05_landscape.png",
    };

    private static string[] PostersPortrait = new string[]{
        "poster_01_portrait.png",
        "poster_02_portrait.png",
        "poster_03_portrait.png",
        "poster_04_portrait.png",
        "poster_05_portrait.png",
        "action_adventure.png",
        "action_heist.png",
        "action_martialarts.png",
        "action_military.png",
        "action_scifi.png",
        "action_movie.png",
        "reality_cooking.png",
        "reality_makeover.png",
        "reality_movie.png",
        "reality_survival.png",
        "reality_talent.png",
        "reality_travel.png"
    };

    private static string[] Categories = new string[]{
        "Action",
        "Adventure",
        "Comedy",
        "Drama",
        "Fantasy",
        "Horror",
        "Mystery",
        "Romance",
        "Sci-Fi",
        "Thriller",
        "Western"
    };

    private static List<string> _movies;
    public static List<string> GenerateFeaturedMovies()
    {
        if (_movies != null)
            return _movies;

        _movies = PostersPortrait.ToList();

        return _movies;
    }

    private static List<string> _continueWatching;
    public static List<string> GenerateContinueWatching()
    {
        if (_continueWatching != null)
            return _continueWatching;

        _continueWatching = PostersLandscape.ToList();
        _continueWatching.Sort((a, b) => Random.Shared.Next(-1, 1));

        return _continueWatching;
    }

    private static List<string> _recomendedShows;
    public static List<string> GenerateRecommendedShows()
    {
        if (_recomendedShows != null)
            return _recomendedShows;

        _recomendedShows = PostersPortrait.ToList();
        _recomendedShows.Sort((a, b) => Random.Shared.Next(-1, 1));

        return _recomendedShows;
    }

    private static List<string> _newShows;
    public static List<string> GenerateNewShows()
    {
        if (_newShows != null)
            return _newShows;

        _newShows = PostersPortrait.ToList();
        _newShows.Sort((a, b) => Random.Shared.Next(-1, 1));

        return _newShows;
    }

    private static List<string> _actionShows;
    public static List<string> GenerateActionShows()
    {
        if (_actionShows != null)
            return _actionShows;

        _actionShows = PostersPortrait.Where(p => p.Contains("action_")).ToList();
        _actionShows.Sort((a, b) => Random.Shared.Next(-1, 1));

        return _actionShows;
    }

    private static List<string> _realityShows;
    public static List<string> GenerateRealityShows()
    {
        if (_realityShows != null)
            return _realityShows;

        _realityShows = PostersPortrait.Where(p => p.Contains("reality_")).ToList();
        _realityShows.Sort((a, b) => Random.Shared.Next(-1, 1));

        return _realityShows;
    }
}