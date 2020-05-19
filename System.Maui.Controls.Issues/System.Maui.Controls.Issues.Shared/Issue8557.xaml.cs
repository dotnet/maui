using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
using System.Linq;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CollectionView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8557, "[Bug] Grouped header with incorrect size when use GridItemsLayout with two columns on the CollectionView", PlatformAffected.iOS)]
	public partial class Issue8557 : TestContentPage
	{
#if APP
		public Issue8557()
		{
			InitializeComponent();

			BindingContext = new Issue8557ViewModel();
		}
#endif

		protected override void Init()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public class Issue8557Model
	{
		public string Description { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue8557GroupModel : List<Issue8557Model>
	{
		public string Name { get; set; }

		public Issue8557GroupModel(string name, List<Issue8557Model> data) : base(data)
		{
			Name = name;
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue8557ViewModel 
	{
		public List<Issue8557GroupModel> GroupContent { get; private set; }

		public Issue8557ViewModel()
		{
			GroupContent = new List<Issue8557GroupModel>
			{
				new Issue8557GroupModel(
					"Group 1",
					new List<Issue8557Model>
					{
						new Issue8557Model { Description = "Description 1.1" },
						new Issue8557Model { Description = "Description 1.2" },
						new Issue8557Model { Description = "Description 1.3" },
						new Issue8557Model { Description = "Description 1.4" }
					}
				),
				new Issue8557GroupModel(
					"Group 2",
					new List<Issue8557Model>
					{
						new Issue8557Model { Description = "Description 2.1" },
						new Issue8557Model { Description = "Description 2.2" },
						new Issue8557Model { Description = "Description 2.3" },
						new Issue8557Model { Description = "Description 2.4" }
					}
				)
			};
		}
	}
}
