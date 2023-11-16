using System.Collections.Generic;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Controls.Sample.UITests.Elements
{
	[Preserve(AllMembers = true)]
	class Team : List<Member>
	{
		public Team(string name, List<Member> members) : base(members)
		{
			Name = name;
		}

		public string Name { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}

	[Preserve(AllMembers = true)]
	class Member
	{
		public Member(string name) => Name = name;

		public string Name { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}

	[Preserve(AllMembers = true)]
	class SuperTeams : List<Team>
	{
		public SuperTeams()
		{
			Add(new Team("Avengers",
				new List<Member>
				{
					new Member("Thor"),
					new Member("Captain America"),
					new Member("Iron Man"),
					new Member("The Hulk"),
					new Member("Ant-Man"),
					new Member("Wasp"),
					new Member("Hawkeye"),
					new Member("Black Panther"),
					new Member("Black Widow"),
					new Member("Doctor Druid"),
					new Member("She-Hulk"),
					new Member("Mockingbird"),
				}
			));

			Add(new Team("Fantastic Four",
				new List<Member>
				{
					new Member("The Thing"),
					new Member("The Human Torch"),
					new Member("The Invisible Woman"),
					new Member("Mr. Fantastic"),
				}
			));

			Add(new Team("Defenders",
				new List<Member>
				{
					new Member("Doctor Strange"),
					new Member("Namor"),
					new Member("Hulk"),
					new Member("Silver Surfer"),
					new Member("Hellcat"),
					new Member("Nighthawk"),
					new Member("Yellowjacket"),
				}
			));

			Add(new Team("Heroes for Hire",
				new List<Member>
				{
					new Member("Luke Cage"),
					new Member("Iron Fist"),
					new Member("Misty Knight"),
					new Member("Colleen Wing"),
					new Member("Shang-Chi"),
				}
			));

			Add(new Team("West Coast Avengers",
				new List<Member>
				{
					new Member("Hawkeye"),
					new Member("Mockingbird"),
					new Member("War Machine"),
					new Member("Wonder Man"),
					new Member("Tigra"),
				}
			));

			Add(new Team("Great Lakes Avengers",
				new List<Member>
				{
					new Member("Squirrel Girl"),
					new Member("Dinah Soar"),
					new Member("Mr. Immortal"),
					new Member("Flatman"),
					new Member("Doorman"),
				}
			));
		}
	}

	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Preserve(AllMembers = true)]
	public partial class ListGrouping : ContentPage
	{
		public ListGrouping()
		{
			InitializeComponent();

			CollectionView.ItemsSource = new SuperTeams();
		}
	}
}