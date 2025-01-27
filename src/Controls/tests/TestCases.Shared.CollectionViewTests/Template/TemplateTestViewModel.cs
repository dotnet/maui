using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.TestCases.CollectionViewTests.Template
{
	public class TemplateViewModel
	{
		public List<Monkey> Monkeys { get; set; }

		public TemplateViewModel()
		{
			Monkeys = new List<Monkey>
			{
				new Monkey { Name = "Baboon", Location = "Africa & Asia" },
				new Monkey { Name = "Capuchin Monkey", Location = "Central & South America" },
				new Monkey { Name = "Blue Monkey", Location = "Central and East Africa" },
				new Monkey { Name = "Squirrel Monkey", Location = "Central & South America" },
				new Monkey { Name = "Golden Lion Tamarin", Location = "Brazil" },
				new Monkey { Name = "Howler Monkey", Location = "South America" },
				new Monkey { Name = "Japanese Macaque", Location = "Japan" }
			};
		}
	}

	public class Monkey
	{
		public string Name { get; set; }
		public string Location { get; set; }
	}
}
