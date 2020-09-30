using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8449, "[Android] CollectionView.EmptyView not displaying when IsGrouped", PlatformAffected.iOS)]
	public partial class Issue8449 : TestContentPage
	{
		public Issue8449()
		{
#if APP
			Title = "Issue 8449";
			InitializeComponent();
			BindingContext = new Issue8449ViewModel();
#endif
		}

		protected override void Init()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public class Issue8449CategoryModel : List<Issue8449Model>
	{
		public string Title { get; set; }

		public Issue8449CategoryModel(string title, List<Issue8449Model> items) : base(items)
		{
			Title = title;
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue8449Model
	{
		public string Title { get; set; }
		public string Comment { get; set; }
		public string Combo { get; set; }
		public string Type { get; set; }
		public bool IsStock { get; set; }
		public int FighterId { get; set; }
		public int UniqueId { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue8449ViewModel
	{
		public List<Issue8449CategoryModel> Items { get; private set; } = new List<Issue8449CategoryModel>();

		public Issue8449ViewModel()
		{
			//CreateCollection();
		}

		private void CreateCollection()
		{
			Items.Add(new Issue8449CategoryModel("Title_1", new List<Issue8449Model>
			{
				new Issue8449Model { Title ="title_1", Comment="comment_1",Combo="combo_1",Type="type_1",IsStock=false,FighterId=1, UniqueId=1},
				new Issue8449Model { Title ="title_2", Comment="comment_2",Combo="combo_2",Type="type_2",IsStock=false,FighterId=2, UniqueId=2}
			}));

			Items.Add(new Issue8449CategoryModel("Title_2", new List<Issue8449Model>
			{
				new Issue8449Model { Title ="title_1", Comment="comment_1",Combo="combo_1",Type="type_1",IsStock=false,FighterId=1, UniqueId=1},
				new Issue8449Model { Title ="title_2", Comment="comment_2",Combo="combo_2",Type="type_2",IsStock=false,FighterId=2, UniqueId=2}
			}));

			Items.Add(new Issue8449CategoryModel("Title_3", new List<Issue8449Model>
			{
				new Issue8449Model { Title ="title_1", Comment="comment_1",Combo="combo_1",Type="type_1",IsStock=false,FighterId=1, UniqueId=1},
				new Issue8449Model { Title ="title_2", Comment="comment_2",Combo="combo_2",Type="type_2",IsStock=false,FighterId=2, UniqueId=2}
			}));
		}
	}
}