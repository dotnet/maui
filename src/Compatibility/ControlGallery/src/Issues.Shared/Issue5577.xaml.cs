using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
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
	[Issue(IssueTracker.Github, 5577, "CollectionView XAML API suggestion", PlatformAffected.All)]
	public partial class Issue5577 : TestContentPage
	{
#if APP
		public Issue5577()
		{
			InitializeComponent();

			BindingContext = new ViewModel5577();
		}
#endif

		protected override void Init()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public class ViewModel5577
	{
		public ViewModel5577()
		{
			AddAnimals();
		}

		public ObservableCollection<Model5577> Animals { get; private set; } = new ObservableCollection<Model5577>();

		private void AddAnimals()
		{
			Animals.Add(new Model5577
			{
				Name = "Afghan Hound",
				Location = "Afghanistan",
			});
			Animals.Add(new Model5577
			{
				Name = "Alpine Dachsbracke",
				Location = "Austria",
			});
			Animals.Add(new Model5577
			{
				Name = "American Bulldog",
				Location = "United States",
			});
			Animals.Add(new Model5577
			{
				Name = "Bearded Collie",
				Location = "Scotland",
			});
			Animals.Add(new Model5577
			{
				Name = "Boston Terrier",
				Location = "United States",
			});
			Animals.Add(new Model5577
			{
				Name = "Canadian Eskimo",
				Location = "Canada",
			});
			Animals.Add(new Model5577
			{
				Name = "Eurohound",
				Location = "Scandinavia",
			});
			Animals.Add(new Model5577
			{
				Name = "Irish Terrier",
				Location = "Ireland",
			});
			Animals.Add(new Model5577
			{
				Name = "Kerry Beagle",
				Location = "Ireland",
			});
			Animals.Add(new Model5577
			{
				Name = "Norwegian Buhund",
				Location = "Norway",
			});
		}
	}

	[Preserve(AllMembers = true)]
	public class Model5577
	{
		public string Name { get; set; }
		public string Location { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}