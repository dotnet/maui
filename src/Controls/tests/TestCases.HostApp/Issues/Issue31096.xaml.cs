namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31096, "Changing IsGrouped on runtime with CollectionViewHandler2 does not properly work", PlatformAffected.iOS)]
public partial class Issue31096 : ContentPage
{
	public Issue31096()
	{
		InitializeComponent();
		BindingContext = new GroupedAnimalsViewModel();
	}

	private void Switch_Toggled(object sender, ToggledEventArgs e) => cv.IsGrouped = e.Value;

	class GroupedAnimalsViewModel
	{
		public List<AnimalGroup> Animals { get; private set; } = new List<AnimalGroup>();

		public GroupedAnimalsViewModel()
		{
			Animals.Add(new AnimalGroup("Bears", new List<Animal>
			{
				new() {
					Name = "American Black Bear",
					Location = "North America",
				},
				new() {
					Name = "Asian Black Bear",
					Location = "Asia",
				}
			}));
		}
	}

	class Animal
	{
		public string Name { get; set; }
		public string Location { get; set; }
		public string Details { get; set; }
	}

	class AnimalGroup(string name, List<Animal> animals) : List<Animal>(animals)
	{
		public string Name { get; private set; } = name;
	}
}