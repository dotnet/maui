using System;
using System.Collections.ObjectModel;

using System.Maui;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 38416, "ListView Sized Incorrectly After Containing Layout's Visibility is Toggled")]
    public partial class Bugzilla38416 : TestContentPage
    {
		[Preserve (AllMembers = true)]
	    public class ListItem
	    {
		    public string Name { get; set; }
	    }

#if !UITEST
		void SwapVisibilityClicked(object sender, EventArgs e)
        {
            Box.IsVisible = !Box.IsVisible;
            FirstLayout.IsVisible = !FirstLayout.IsVisible;
            //FirstListView.IsVisible = !FirstListView.IsVisible; //Workaround. Has to be called after the layout's visibility
        }
#endif

		protected override void Init ()
		{
#if !UITEST
			InitializeComponent();

            var items = new ObservableCollection<ListItem>();
            FirstListView.ItemsSource = items;

            for(int i=0; i<70; i++)
            {
                items.Add(new ListItem { Name = string.Format("List Item {0}", i+1) });
            }

            Box.IsVisible = true;
            //FirstListView.IsVisible = false; //Workaround
            FirstLayout.IsVisible = false;
#endif
		}

	}
}
