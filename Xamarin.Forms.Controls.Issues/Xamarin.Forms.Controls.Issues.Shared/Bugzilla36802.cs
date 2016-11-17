using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;
using System.Collections.ObjectModel;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{

    [Preserve(AllMembers = true)]
    [Issue(IssueTracker.Bugzilla, 36802, "[iOS] AccessoryView Partially Hidden When Using RecycleElement and GroupShortName", PlatformAffected.iOS)]
    public class Bugzilla36802 : TestContentPage
    {
        const string Instructions = "On iOS, all the list items below should have an AccessoryView visible. If any are not visible or are covered by the section index list then this test has failed.";
        ObservableCollection<GroupedItem> grouped { get; set; }
        ListView lstView;

        public class AccessoryViewCell : ViewCell
        {
            public AccessoryViewCell()
            {
                var label = new Label();
                label.SetBinding(Label.TextProperty, ".");
                View = label;
            }
        }

        public class GroupedItem : ObservableCollection<string>
        {
            public string LongName { get; set; }
            public string ShortName { get; set; }
        }

        protected override void Init()
        {
            var label = new Label { Text = Instructions };
            grouped = new ObservableCollection<GroupedItem>();
            lstView = new ListView(ListViewCachingStrategy.RecycleElement) {
                IsGroupingEnabled = true,
                ItemTemplate = new DataTemplate(typeof(AccessoryViewCell)),
                ItemsSource = grouped,
                GroupDisplayBinding = new Binding("LongName"),
                GroupShortNameBinding = new Binding("ShortName")
            };

            var grp1 = new GroupedItem() { LongName = "Group 1", ShortName = "1" };
            var grp2 = new GroupedItem() { LongName = "Group 2", ShortName = "2" };

            for (int i = 1; i < 4; i++)
            {
                grp1.Add($"Item #{i}");
                grp2.Add($"Item #{i}");
            }

            grouped.Add(grp1); 
            grouped.Add(grp2);

            Content = new StackLayout
            {
                Children = {
                    label,
                    lstView
                }
            };
        }

#if (UITEST && __IOS__)
        [Test]
		[Category(UITestCategories.ManualReview)]
        public void Bugzilla36802Test()
        {
            RunningApp.Screenshot("AccessoryView partially hidden test");
        }
#endif
    }
}
