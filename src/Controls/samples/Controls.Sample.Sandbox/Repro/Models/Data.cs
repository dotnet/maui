using System;
using System.Collections.Generic;
using CollectionViewPerformanceMaui.Enums;
using CollectionViewPerformanceMaui.Helpers;
using CollectionViewPerformanceMaui.Resources.Fonts;

namespace CollectionViewPerformanceMaui.Models
{
	public sealed class Data
	{
		public Template Template { get; set; }

        public string RestaurantName { get; set; } = string.Empty;

        public string RestaurantDescription { get; set; } = string.Empty;

        public string RestaurantAddress { get; set; } = string.Empty;

        public string Rating { get; set; } = string.Empty;

		public string Review { get; set; } = string.Empty;

		public List<string> Reviews { get; set; } = new();

        public List<string> SocialMedia { get; set; } = new();

        public Data()
		{
			var random = new Random();

			this.Template = Template.CardWithTheLot;
			this.RestaurantName = RandomContentHelper.GenerateRandomRestaurantName();
			this.RestaurantDescription = RandomContentHelper.GenerateRandomSentence(5);
			this.RestaurantAddress = RandomContentHelper.GenerateRandomAddress();

			this.Rating = RandomContentHelper.GenerateRandomRating();

            this.Review = RandomContentHelper.GenerateRandomSentence(random.Next(6, 12));
            this.Reviews = new List<string>()
            {
                RandomContentHelper.GenerateRandomSentence(random.Next(6, 12)),
                RandomContentHelper.GenerateRandomSentence(random.Next(6, 12)),
                RandomContentHelper.GenerateRandomSentence(random.Next(6, 12))
            };

			this.SocialMedia = new List<string>()
			{
				FontAwesome.Instagram,
				FontAwesome.Facebook,
				FontAwesome.Tiktok,
			};

			// random.Next(0, 2) == 1; // 50/50 chance
		}
	}
}
