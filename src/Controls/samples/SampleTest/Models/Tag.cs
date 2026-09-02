using CommunityToolkit.Maui.Core.Extensions;
using System.Text.Json.Serialization;

namespace SampleTest.Models
{
    public class Tag
    {
        public int ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Color { get; set; } = "#FF0000";

        [JsonIgnore]
        public Brush ColorBrush
        {
            get
            {
                return new SolidColorBrush(Microsoft.Maui.Graphics.Color.FromArgb(Color));
            }
        }

        [JsonIgnore]
        public Color DisplayColor
        {
            get
            {
                return Microsoft.Maui.Graphics.Color.FromArgb(Color);
            }
        }

        [JsonIgnore]
        public Color DisplayDarkColor
        {
            get
            {
                return DisplayColor.WithBlackKey(0.8);
            }
        }

        [JsonIgnore]
        public Color DisplayLightColor
        {
            get
            {
                return DisplayColor.WithBlackKey(0.2);
            }
        }

        [JsonIgnore]
        public bool IsSelected { get; set; }
    }
}