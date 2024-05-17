using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10086,
		"[Bug] SwipeView.RightItems Label is Missing/Hidden Due to Bad Alignment",
		PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue10086 : TestContentPage
	{
		public Issue10086()
		{
#if APP
			InitializeComponent();
			BindingContext = new Issue10086ViewModel();
#endif
		}

		protected override void Init()
		{
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue10086Model
	{
		public string Title { get; set; }
		public string SubTitle { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue10086ViewModel : BindableObject
	{
		public Issue10086ViewModel()
		{
			Items = new List<Issue10086Model>();
			LoadItems();
		}

		public List<Issue10086Model> Items { get; set; }

		void LoadItems()
		{
			for (int i = 0; i < 10; i++)
			{
				Items.Add(new Issue10086Model
				{
					Title = $"Title {i + 1}",
					SubTitle = $"SubTitle {i + 1}",
				});
			}
		}
	}
}