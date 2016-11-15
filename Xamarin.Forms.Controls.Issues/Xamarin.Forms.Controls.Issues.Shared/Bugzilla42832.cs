using System;
using System.Collections.Generic;
using Xamarin.Forms.Internals;
using Xamarin.Forms.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
#endif

    [Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 42832, "Scrolling a ListView with active ContextAction Items causes NRE", PlatformAffected.Android)]
    public class Bugzilla42832 : TestContentPage
	{
        ListView listview;

        protected override void Init()
        {
            var items = new List<string>();
            for(int i=0; i<20; i++)
                items.Add($"Item #{i}");

            var template = new DataTemplate(typeof(TestCell));
            template.SetBinding(TextCell.TextProperty, ".");

            listview = new ListView(ListViewCachingStrategy.RetainElement)
            {
                AutomationId = "mainList",
                ItemsSource = items,
                ItemTemplate = template
            };

            Content = listview;
        }

        [Preserve(AllMembers = true)]
        public class TestCell : TextCell
        {
            public TestCell()
            {
                var menuItem = new MenuItem { Text = "Test Item" };
                ContextActions.Add(menuItem);
            }
        }

#if UITEST && __ANDROID__
        [Test]
        public void ContextActionsScrollNRE()
        {
            RunningApp.TouchAndHold(q => q.Marked("Item #0"));
            RunningApp.WaitForElement(q => q.Marked("Test Item"));

            int counter = 0;
            while(counter < 5)
            {
                RunningApp.ScrollDownTo("Item #15", "mainList", ScrollStrategy.Gesture, timeout:TimeSpan.FromMinutes(1));
                RunningApp.ScrollUpTo("Item #0", "mainList", ScrollStrategy.Gesture, timeout: TimeSpan.FromMinutes(1));
                counter++;
            }

            RunningApp.Screenshot("If the app did not crash, then the test has passed.");
        }
#endif
    }
}
