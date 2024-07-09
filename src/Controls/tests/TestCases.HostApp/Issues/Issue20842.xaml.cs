using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	internal class Person
	{
		public string Name { get; set; } = "";
		public int Age { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string Location { get; set; } = "";

		public override string ToString()
			=> Name;
	}

	public class PersonDataTemplateSelector : DataTemplateSelector
	{
		public required DataTemplate ValidTemplate { get; set; }
		public required DataTemplate InvalidTemplate { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			return ((Person)item).DateOfBirth.Year >= 1980 ? ValidTemplate : InvalidTemplate;
		}
	}

	[Issue(IssueTracker.Github, "20842", "Verify data templates in CollectionView virtualize correctly", PlatformAffected.UWP)]
	public partial class Issue20842 : ContentPage
	{
		private readonly List<Person> people = new();

		public Issue20842()
		{
			InitializeComponent();

			for (int i = 0; i < 100; i++)
			{
				people.Add(new Person { Name = $"Kath {i}", DateOfBirth = new DateTime(1985, 11, 20), Location = "France" });
				people.Add(new Person { Name = $"Steve {i}", DateOfBirth = new DateTime(1975, 1, 15), Location = "USA" });
				people.Add(new Person { Name = $"Lucas {i}", DateOfBirth = new DateTime(1988, 2, 5), Location = "Germany" });
				people.Add(new Person { Name = $"John {i}", DateOfBirth = new DateTime(1976, 2, 20), Location = "USA" });
				people.Add(new Person { Name = $"Tariq {i}", DateOfBirth = new DateTime(1987, 1, 10), Location = "UK" });
				people.Add(new Person { Name = $"Jane {i}", DateOfBirth = new DateTime(1982, 8, 30), Location = "USA" });
				people.Add(new Person { Name = $"Tom {i}", DateOfBirth = new DateTime(1977, 3, 10), Location = "UK" });
			}

			PersonList.ItemsSource = people;
		}

		private void ScrollToTopButton_Clicked(object sender, EventArgs e)
		{
			PersonList.ScrollTo(0, animate: false);
		}

		private void ScrollToBottomButton_Clicked(object sender, EventArgs e)
		{
			PersonList.ScrollTo(people.Count - 1, animate: false);
		}
	}
}