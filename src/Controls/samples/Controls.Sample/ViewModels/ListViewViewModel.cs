using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Maui.Controls.Sample.Models;

namespace Maui.Controls.Sample.ViewModels
{
	public class ListViewViewModel
	{
		public ObservableCollection<Grouping<char, Person>> GroupedEmployees { get; private set; }

		public ListViewViewModel(int num = 2000)
		{
			var rnd = new Random();
			var employees = new List<Person>();

			for (int i = 0; i < num; i++)
			{
				employees.Add(new Person($"Name {i + 1}", rnd.Next(18, 65), $"Location {i + 1}"));
			}

			GroupedEmployees = new ObservableCollection<Grouping<char, Person>>(employees.OrderBy(e => e.Name[0]).GroupBy(e => e.Name[0]).Select(e => new Grouping<char, Person>(e.Key, e)));
		}
	}
}