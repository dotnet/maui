using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	public class Person
	{
		public string Name { get; set; } = "";
		public int Age { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string Location { get; set; } = "";

		public override string ToString()
		{
			return Name;
		}
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

	public partial class MainPage : ContentPage
	{
		private ObservableCollection<Person> people = new ObservableCollection<Person>();

		public MainPage()
		{
			InitializeComponent();

			for (int i = 0; i < 10; i++)
			{
				people.Add(new Person { Name = $"Kath {i}", DateOfBirth = new DateTime(1985, 11, 20), Location = "France" });
				people.Add(new Person { Name = $"Steve {i}", DateOfBirth = new DateTime(1975, 1, 15), Location = "USA" });
				people.Add(new Person { Name = $"Lucas {i}", DateOfBirth = new DateTime(1988, 2, 5), Location = "Germany" });
				people.Add(new Person { Name = $"John {i}", DateOfBirth = new DateTime(1976, 2, 20), Location = "USA" });
				people.Add(new Person { Name = $"Tariq {i}", DateOfBirth = new DateTime(1987, 1, 10), Location = "UK" });
				people.Add(new Person { Name = $"Jane {i}", DateOfBirth = new DateTime(1982, 8, 30), Location = "USA" });
				people.Add(new Person { Name = $"Tom {i}", DateOfBirth = new DateTime(1977, 3, 10), Location = "UK" });

			}
			collectionView.ItemsSource = people;
		}

		private void AddButton_Clicked(object sender, EventArgs e)
		{
			people.Add(new Person { Name = "Kath (added)", DateOfBirth = new DateTime(1985, 11, 20), Location = "France" });
		}

		private void RemoveButton_Clicked(object sender, EventArgs e)
		{
			var person = collectionView.SelectedItem as Person;
			if (person != null)
			{
				people.Remove(person);
			}
		}
	}
}