using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 8902, "CarouselView Layout on orientation change", PlatformAffected.iOS)]
	public partial class Issue8902 : TestContentPage
	{
		public Issue8902()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			BindingContext = new Issue8902ViewModel();
		}

		public class Issue8902ViewModel
		{
			public Issue8902ViewModel()
			{
				Persons = new List<Issue8902Person>();
				Persons.Add(new Issue8902Person()
				{
					Age = 38,
					Name = "User 1"
				});
				Persons.Add(new Issue8902Person()
				{
					Age = 22,
					Name = "User 2"
				});
				Persons.Add(new Issue8902Person()
				{
					Age = 51,
					Name = "User 3"
				});
			}
			public List<Issue8902Person> Persons { get; set; }
		}

		public class Issue8902Person
		{
			public string Name { get; set; }
			public int Age { get; set; }
		}
	}
}