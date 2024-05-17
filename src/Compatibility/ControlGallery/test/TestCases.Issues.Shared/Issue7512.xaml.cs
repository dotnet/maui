using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CollectionView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7512, "RTL CollectionView looks odd", PlatformAffected.Android)]
	public partial class Issue7512 : TestContentPage
	{
		bool isRTL = false;

#if APP
		public Issue7512()
		{
			InitializeComponent();

			BindingContext = new ViewModel7512();
		}
#endif

		protected override void Init()
		{

		}

		void HandleButtonClick(object sender, EventArgs e)
		{
			var button = sender as Button;
			var stackLayout = button.Parent as StackLayout;
			var grid = stackLayout.Parent as Grid;
			var collectionView = grid.Children[1] as CollectionView;

			isRTL = !isRTL;

			button.Text = isRTL ? "Switch to LTR" : "Switch to RTL";
			collectionView.FlowDirection = isRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
		}
	}

	[Preserve(AllMembers = true)]
	public class ViewModel7512
	{
		public ObservableCollection<Model7512> Monkeys { get; private set; }

		public ViewModel7512()
		{
			Monkeys = CreateMonkeyCollection();
		}

		ObservableCollection<Model7512> CreateMonkeyCollection()
		{
			var source = new ObservableCollection<Model7512>();

			source.Add(new Model7512
			{
				Name = "Baboon",
				Location = "Africa & Asia",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/f/fc/Papio_anubis_%28Serengeti%2C_2009%29.jpg/200px-Papio_anubis_%28Serengeti%2C_2009%29.jpg"
			});

			source.Add(new Model7512
			{
				Name = "Capuchin Monkey",
				Location = "Central & South America",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/4/40/Capuchin_Costa_Rica.jpg/200px-Capuchin_Costa_Rica.jpg"
			});

			source.Add(new Model7512
			{
				Name = "Blue Monkey",
				Location = "Central and East Africa",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/8/83/BlueMonkey.jpg/220px-BlueMonkey.jpg"
			});

			source.Add(new Model7512
			{
				Name = "Squirrel Monkey",
				Location = "Central & South America",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/2/20/Saimiri_sciureus-1_Luc_Viatour.jpg/220px-Saimiri_sciureus-1_Luc_Viatour.jpg"
			});

			source.Add(new Model7512
			{
				Name = "Golden Lion Tamarin",
				Location = "Brazil",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/8/87/Golden_lion_tamarin_portrait3.jpg/220px-Golden_lion_tamarin_portrait3.jpg"
			});

			source.Add(new Model7512
			{
				Name = "Howler Monkey",
				Location = "South America",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/0/0d/Alouatta_guariba.jpg/200px-Alouatta_guariba.jpg"
			});

			source.Add(new Model7512
			{
				Name = "Japanese Macaque",
				Location = "Japan",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/c/c1/Macaca_fuscata_fuscata1.jpg/220px-Macaca_fuscata_fuscata1.jpg"
			});

			source.Add(new Model7512
			{
				Name = "Mandrill",
				Location = "Southern Cameroon, Gabon, Equatorial Guinea, and Congo",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/7/75/Mandrill_at_san_francisco_zoo.jpg/220px-Mandrill_at_san_francisco_zoo.jpg"
			});

			source.Add(new Model7512
			{
				Name = "Proboscis Monkey",
				Location = "Borneo",
				ImageUrl = "http://upload.wikimedia.org/wikipedia/commons/thumb/e/e5/Proboscis_Monkey_in_Borneo.jpg/250px-Proboscis_Monkey_in_Borneo.jpg"
			});

			source.Add(new Model7512
			{
				Name = "Red-shanked Douc",
				Location = "Vietnam, Laos",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9f/Portrait_of_a_Douc.jpg/159px-Portrait_of_a_Douc.jpg"
			});

			source.Add(new Model7512
			{
				Name = "Gray-shanked Douc",
				Location = "Vietnam",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0b/Cuc.Phuong.Primate.Rehab.center.jpg/320px-Cuc.Phuong.Primate.Rehab.center.jpg"
			});

			source.Add(new Model7512
			{
				Name = "Golden Snub-nosed Monkey",
				Location = "China",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c8/Golden_Snub-nosed_Monkeys%2C_Qinling_Mountains_-_China.jpg/165px-Golden_Snub-nosed_Monkeys%2C_Qinling_Mountains_-_China.jpg"
			});

			source.Add(new Model7512
			{
				Name = "Black Snub-nosed Monkey",
				Location = "China",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/59/RhinopitecusBieti.jpg/320px-RhinopitecusBieti.jpg"
			});

			source.Add(new Model7512
			{
				Name = "Tonkin Snub-nosed Monkey",
				Location = "Vietnam",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9c/Tonkin_snub-nosed_monkeys_%28Rhinopithecus_avunculus%29.jpg/320px-Tonkin_snub-nosed_monkeys_%28Rhinopithecus_avunculus%29.jpg"
			});

			source.Add(new Model7512
			{
				Name = "Thomas's Langur",
				Location = "Indonesia",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/31/Thomas%27s_langur_Presbytis_thomasi.jpg/142px-Thomas%27s_langur_Presbytis_thomasi.jpg"
			});

			source.Add(new Model7512
			{
				Name = "Purple-faced Langur",
				Location = "Sri Lanka",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/02/Semnopithèque_blanchâtre_mâle.JPG/192px-Semnopithèque_blanchâtre_mâle.JPG"
			});

			source.Add(new Model7512
			{
				Name = "Gelada",
				Location = "Ethiopia",
				ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/13/Gelada-Pavian.jpg/320px-Gelada-Pavian.jpg"
			});

			return source;
		}
	}

	[Preserve(AllMembers = true)]
	public class Model7512
	{
		public string Name { get; set; }
		public string Location { get; set; }
		public string ImageUrl { get; set; }
	}
}
