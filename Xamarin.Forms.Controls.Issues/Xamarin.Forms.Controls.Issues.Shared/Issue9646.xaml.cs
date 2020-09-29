using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 9646, "[Android] SwipeView items don't fully remove themselves when unrevealed ",
		PlatformAffected.Android)]
	public partial class Issue9646 : TestContentPage
	{
		public Issue9646()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			BindingContext = new Issue9646ViewModel();
		}
	}

	public class Issue9646Model
	{
		public string Title { get; set; }
		public string SubTitle { get; set; }
	}

	public class Issue9646ViewModel
	{
		public Issue9646ViewModel()
		{
			Items = new List<Issue9646Model>();
			LoadItems();
		}

		public List<Issue9646Model> Items { get; set; }

		void LoadItems()
		{
			for (int i = 0; i < 10; i++)
			{
				Items.Add(new Issue9646Model
				{
					Title = $"Title {i + 1}",
					SubTitle = $"SubTitle {i + 1}",
				});
			}
		}
	}
}