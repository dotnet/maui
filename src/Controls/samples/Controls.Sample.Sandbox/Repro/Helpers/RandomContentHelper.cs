using System;
using System.Collections.Generic;
using System.Text;

namespace CollectionViewPerformanceMaui.Helpers
{
	public static class RandomContentHelper
	{
        private static Random random = new Random();

        private static readonly List<string> RestaurantNameStart = new()
		{
            "Starlight",
            "Gourmet",
            "Classic",
		    "Tasty",
		    "Happy",
		    "Royal",
		    "Grand",
		    "Fresh",
		    "Local",
		    "Bella"
		};

        private static readonly List<string> RestaurantNameEnd = new()
		{
		    "Kitchen",
		    "Bistro",
		    "Eatery",
		    "Diner",
		    "Grill",
		    "Place",
		    "House",
		    "Spot",
		    "Cafe",
		    "Bar"
		};

        private static readonly List<string> AddressStreetStart = new()
        {
            "Maple",
            "Willow",
            "Oak",
            "Crescent",
            "Birch",
            "Elm",
            "Pine",
            "Rose",
            "Lily",
            "Magnolia",
            "Juniper",
            "Cedar",
            "Hazel",
            "Beech",
            "Poplar"
        };

        private static readonly List<string> AddressStreetEnd = new()
        {
            "Street",
            "Avenue",
            "Boulevard",
            "Lane",
            "Road",
            "Terrace",
            "Place",
            "Grove",
            "Drive",
            "Circuit",
            "Court"
        };

        private static readonly List<string> AddressSuburb = new()
        {
            "Willowvale",
            "Greenwood",
            "Riverside",
            "Meadowbrook",
            "Hillview",
            "Sunset Hills",
            "Oakwood",
            "Brookfield",
            "Riverview",
            "Pinecrest",
            "Springvale",
            "Glenwood",
            "Woodlands",
            "Fairview",
            "Parkside"
        };

        private static readonly List<string> AddressState = new()
        {
            "SA",
            "WA",
            "NT",
            "QLD",
            "NSW",
            "VIC",
            "TAS",
            "ACT"
        };

        private static readonly List<string> Ratings = new()
        {
            "★",
            "★★",
            "★★★",
            "★★★★",
            "★★★★★"
        };

        private static readonly List<string> Words = new()
        {
            "delicious",
            "cozy",
            "fresh",
            "spicy",
            "ambient",
            "friendly",
            "clean",
            "service",
            "menu",
            "culinary",
            "local",
            "flavorful",
            "creative",
            "traditional",
            "experience",
            "romantic",
            "casual",
            "upscale",
            "wine",
            "desserts",
            "reservation",
            "recommend",
            "popular",
            "dine",
            "enjoy",
            "dish",
            "beverage",
            "exquisite",
            "warm",
            "organic",
            "seasonal"
        };

        public static string GenerateRandomRestaurantName()
        {
            return $"{RestaurantNameStart[random.Next(RestaurantNameStart.Count)]} {RestaurantNameEnd[random.Next(RestaurantNameEnd.Count)]}";
        }

        public static string GenerateRandomAddress()
        {
            return $"{random.Next(1, 100)} {AddressStreetStart[random.Next(AddressStreetStart.Count)]} {AddressStreetEnd[random.Next(AddressStreetEnd.Count)]}, {AddressSuburb[random.Next(AddressSuburb.Count)]}, {AddressState[random.Next(AddressState.Count)]}";
        }

        public static string GenerateRandomRating()
		{
			return Ratings[random.Next(Ratings.Count)];
		}

        public static string GenerateRandomSentence(int length)
        {
            var sentence = new StringBuilder();

            for (var i = 0; i < length; i++)
            {
                var word = Words[random.Next(Words.Count)];

                if (i == 0)
                {
                    word = char.ToUpper(word[0]) + word.Substring(1);
                }

                sentence.Append(word);

                if (i < length - 1)
                {
                    sentence.Append(" ");
                }
            }

            return sentence.ToString();
        }
    }
}
