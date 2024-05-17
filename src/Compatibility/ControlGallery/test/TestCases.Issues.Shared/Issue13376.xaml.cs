using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	[Issue(IssueTracker.Github, 13376,
		"[Bug] [iOS] Brush color lost on swiping",
		PlatformAffected.iOS)]
	public partial class Issue13376 : TestContentPage
	{
		public Issue13376()
		{
#if APP
			InitializeComponent();
			BindingContext = new Issue13376ViewModel();
#endif
		}

		protected override void Init()
		{
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue13376Model
	{
		public Issue13376Model(string text)
		{
			Text = text;
		}

		public string Text { get; }
	}

	[Preserve(AllMembers = true)]
	public class Issue13376ViewModel
	{
		public Issue13376ViewModel()
		{
			Items = new List<Issue13376Model>
			{
				new Issue13376Model("Item 1"),
				new Issue13376Model("Item 2"),
				new Issue13376Model("Item 3"),
				new Issue13376Model("Item 4"),
				new Issue13376Model("Item 5"),
			};
		}

		public List<Issue13376Model> Items { get; }
	}
}