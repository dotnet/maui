using System.Collections.ObjectModel;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using System.Maui.Core.UITests;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
    [Issue(IssueTracker.Github, 8689,
		"[Bug] Margin of Contentview like Grid, or Stacklayout doesnt work inside RefreshView",
		PlatformAffected.Android)]
	public partial class Issue8689 : TestContentPage
	{
		public Issue8689()
		{
#if APP
			InitializeComponent();
            BindingContext = new Issue8689ViewModel();
#endif
        }

        protected override void Init()
        {

        }
    }

    [Preserve(AllMembers = true)]
    public class Issue8689ViewModel : BindableObject
    {
        public Issue8689ViewModel()
        {
            Items = new ObservableCollection<string>();
            LoadItems();
        }

        public ObservableCollection<string> Items { get; set; }

        void LoadItems()
        {
            Items.Clear();

            Items.Add("Item 1");
            Items.Add("Item 2");
            Items.Add("Item 3");
            Items.Add("Item 4");
            Items.Add("Item 5");
            Items.Add("Item 6");
            Items.Add("Item 7");
        }
    }
}