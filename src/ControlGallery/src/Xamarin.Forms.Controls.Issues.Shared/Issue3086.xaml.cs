using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 3086, "On iOS, groupheadertemplate is broken")]
	public partial class Issue3086 : ContentPage
	{
		public Issue3086()
		{
#if APP
			InitializeComponent();
			TestListView.ItemsSource = new ObservableCollection<GroupedItems>() {
				new GroupedItems ("Header 1") { "1.1", "1.2", "1.3" },
				new GroupedItems ("Header 2") { "2.1", "2.2", "2.3" },
				new GroupedItems ("Header 3") { "3.1", "3.2", "3.3" },
				new GroupedItems ("Header 4") { "4.1", "4.2", "4.3" },
			};
#endif
		}

		internal class GroupedItems : ObservableCollection<string>
		{
			public GroupedItems(string groupName) { GroupName = groupName; }
			public string GroupName { get; private set; }
		}
	}
}