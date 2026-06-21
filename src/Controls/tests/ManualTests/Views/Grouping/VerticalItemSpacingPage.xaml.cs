namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalItemSpacingPage : ContentPage
	{
		public VerticalItemSpacingPage()
		{
			InitializeComponent();
			BindingContext = this;
		}
		public List<Items> PodcastsGroup => new List<Items>
		{
			new Items
			{
				Name = "Small",
				Children={ "Rat", "Mouse", "Bird", "Fish", "Carrot"}
			},
			new Items
			{
				Name = "Big",
				Children={ "Cat", "Dog", "Rabbit", "Car", "Jet" }
			},
			new Items
			{
				Name = "Awesome",
				Children={ "Code", "School", "Not doing drugs" }
			},
		};
	}

	public class Items : List<string>
	{
		public string Name { get; set; }
		public List<string> Children => this;
	}
}