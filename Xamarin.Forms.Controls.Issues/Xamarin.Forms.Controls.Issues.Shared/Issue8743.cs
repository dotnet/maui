using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;
using System.Threading.Tasks;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif
namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8743, "[Bug][UWP] SearchBar does not respect FontSize on 4.3.0",
		PlatformAffected.UWP)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.SearchBar)]
#endif
	public class Issue8743 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Issue 8743";

			StackLayout layout = new StackLayout();

			Label instructions = new Label
			{ 
				Text = "Check that the font size in the search bars below matches the size specified in the placeholders."
			};

			SearchBar normalSearchBar = new SearchBar
			{
				Placeholder = "FontSize = default"
			};

			SearchBar largeSearchBar = new SearchBar
			{
				FontSize = Device.GetNamedSize(NamedSize.Large, typeof(SearchBar)),
				Placeholder = "FontSize = Large"
			};

			SearchBar size100SearchBar = new SearchBar
			{
				FontSize = 100f,
				Placeholder = "FontSize = 100.0"
			};

			layout.Children.Add(instructions);
			layout.Children.Add(normalSearchBar);
			layout.Children.Add(largeSearchBar);
			layout.Children.Add(size100SearchBar);

			Content = layout;
		}
	}
}
