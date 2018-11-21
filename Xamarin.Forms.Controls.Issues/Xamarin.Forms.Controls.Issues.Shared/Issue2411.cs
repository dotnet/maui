using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
    [Issue(IssueTracker.Github, 2411, "ListView.ScrollTo not working in TabbedPage", PlatformAffected.Android)]
    public class Issue2411 : TestTabbedPage
    {
        protected override void Init()
        {
            Children.Add(new XamarinListViewScrollToBugPage1());
            Children.Add(new XamarinListViewScrollToBugPage2());
            Children.Add(new XamarinListViewScrollToBugPage3());
        }

#if UITEST
        [Test]
#if __ANDROID__
        [Ignore("Appearing event is tied to virtualization in TabbedPage for Material")]
#endif
#if __MACOS__
        [Ignore("ScrollTo not implemented on MacOS")]
#endif
        [Issue(IssueTracker.Github, 2411, "ScrollToPosition.MakeVisible not called every time TabbedPage", PlatformAffected.Android)]
        public void Issue2411ScrollToPositionMakeVisible()
        {
            RunningApp.WaitForElement(q => q.Marked("99 99 99 99 99 99"));
            RunningApp.Screenshot("ScrollTo working correctly");
            RunningApp.Tap(q => q.Marked("Crash in ScrollToPosition.End"));
            RunningApp.Screenshot("On Second Tab");
            RunningApp.WaitForElement(q => q.Marked("2 0 0 0 0 0 0"));
            RunningApp.Tap(q => q.Marked("Scroll To in OnAppearing"));
            RunningApp.Screenshot("On First Tab");
            RunningApp.WaitForElement(q => q.Marked("99 99 99 99 99 99"));

            var listViewBound = RunningApp.Query(q => q.Marked("listView"))[0].Rect;
            Xamarin.Forms.Core.UITests.Gestures.ScrollForElement(RunningApp, "* marked:'0 0 0 0 0 0'", new Xamarin.Forms.Core.UITests.Drag(listViewBound, Xamarin.Forms.Core.UITests.Drag.Direction.TopToBottom, Xamarin.Forms.Core.UITests.Drag.DragLength.Long));
            RunningApp.Screenshot("Scrolled to Top");

            RunningApp.Tap(q => q.Marked("Crash in ScrollToPosition.End"));
            RunningApp.Screenshot("On Second Tab");
            RunningApp.WaitForElement(q => q.Marked("2 0 0 0 0 0 0"));
            RunningApp.Tap(q => q.Marked("Scroll To in OnAppearing"));
            RunningApp.Screenshot("On First Tab");
            RunningApp.WaitForElement(q => q.Marked("99 99 99 99 99 99"));
        }

        [Test]
        [Issue(IssueTracker.Github, 2411, "ScrollToPosition.End crashing in TabbedPage", PlatformAffected.Android)]
#if __MACOS__
        [Ignore("ScrollTo not implemented on MacOS")]
#endif
        public void Issue2411ScrollToPositionEndCrash()
        {
            RunningApp.Tap(q => q.Marked("Crash in ScrollToPosition.End"));
            RunningApp.Screenshot("On Second Tab");
            RunningApp.Tap(q => q.Marked("Scroll To in OnAppearing"));
            RunningApp.Screenshot("On First Tab");
            RunningApp.Tap(q => q.Marked("Crash in ScrollToPosition.End"));
            RunningApp.Screenshot("On Second Tab Again");
            RunningApp.Tap(q => q.Marked("ScrollToPosition.End End - Not animated"));
            RunningApp.WaitForElement(q => q.Marked("2 99 99 99 99 99 99"));
        }

        [Test]
        [Issue(IssueTracker.Github, 2411, "ScrollToPositon.End crashing in TabbedPage", PlatformAffected.Android)]
        public void Issue2411ScrollToPositionWrongOnUneven()
        {
            RunningApp.Tap(q => q.Marked("Crash in ScrollToPosition.End"));
            RunningApp.Tap(q => q.Marked("Scroll To in OnAppearing Uneven"));

            var dontRun = RunningApp.Query(q => q.Marked(XamarinListViewScrollToBugPage3.DontRun));
            if (dontRun.Length > 0)
                Assert.Inconclusive("Ignored on iOS < 9 until Bugzilla 28277 is resolved.");

            RunningApp.Screenshot("On Third Tab");
            RunningApp.WaitForElement(q => q.Marked("99 99 99 99 99 99"));
        }
#endif
    }

    [Preserve(AllMembers = true)]
    internal class ListObj
    {
        public string Name { get; set; }
    }

    [Preserve(AllMembers = true)]
    public class CellTemplateScrollTo : ViewCell
    {
        public CellTemplateScrollTo()
        {
            Label cellLabel = new Label()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };

            cellLabel.SetBinding(Label.TextProperty, new Binding("Name", BindingMode.OneWay));

            StackLayout root = new StackLayout()
            {
                Children = {
                    cellLabel
                }
            };

            View = root;
        }
    }

    [Preserve(AllMembers = true)]
    public class CellTemplateScrollToUneven : CellTemplateScrollTo
    {
        public CellTemplateScrollToUneven()
        {

            Height = 60 + new Random().Next(10, 100);
        }
    }

    [Preserve(AllMembers = true)]
    public class XamarinListViewScrollToBugPage1 : ContentPage
    {
        ListView _listView;
        ObservableCollection<ListObj> _collection = new ObservableCollection<ListObj>();

        public XamarinListViewScrollToBugPage1()
        {
            Title = "Scroll To in OnAppearing";

            for (int i = 0; i < 100; i++)
            {
                var item = new ListObj { Name = string.Format("{0} {0} {0} {0} {0} {0}", i) };
                _collection.Add(item);
            }

            _listView = new ListView
            {
                ItemsSource = _collection,
                ItemTemplate = new DataTemplate(typeof(CellTemplateScrollTo))
            };

            _listView.AutomationId = "listView";

            Content = new StackLayout
            {
                Children = {
                    _listView
                }
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _listView.ScrollTo(_collection.Last(), ScrollToPosition.MakeVisible, false);
        }
    }

    [Preserve(AllMembers = true)]
    public class XamarinListViewScrollToBugPage2 : ContentPage
    {
        ListView _listView;
        ObservableCollection<ListObj> _collection = new ObservableCollection<ListObj>();

        public XamarinListViewScrollToBugPage2()
        {
            Padding = new Thickness(10, 50, 10, 0);
            Title = "Crash in ScrollToPosition.End";

            for (int i = 0; i < 100; i++)
            {
                var item = new ListObj { Name = string.Format("2 {0} {0} {0} {0} {0} {0}", i) };
                _collection.Add(item);
            }

            _listView = new ListView
            {
                ItemsSource = _collection,
                ItemTemplate = new DataTemplate(typeof(CellTemplateScrollTo))
            };

            var endButton = new Button
            {
                Text = "ScrollToPosition.End End - Not animated",
                Command = new Command(() =>
                {
                    _listView.ScrollTo(_collection.Last(), ScrollToPosition.End, false);
                })
            };

            var endButtonAnimated = new Button
            {
                Text = "ScrollToPosition.MakeVisible End - Animated",
                Command = new Command(() =>
                {
                    _listView.ScrollTo(_collection.Last(), ScrollToPosition.MakeVisible, true);
                })
            };

            Content = new StackLayout
            {
                Children = {
                    endButton,
                    endButtonAnimated,
                    _listView
                }
            };
        }
    }

    public class XamarinListViewScrollToBugPage3 : ContentPage
    {
        ListView _listView;
        ObservableCollection<ListObj> _collection = new ObservableCollection<ListObj>();
        int _i = 0;
        public const string DontRun = "Don't run";
        public XamarinListViewScrollToBugPage3()
        {
            Title = "Scroll To in OnAppearing Uneven";

            bool runTest = true;
            // This test will fail in iOS < 9 because using ScrollTo with UnevenRows with estimation is currently not working.
            // It did not previously fail because this test used `TakePerformanceHit` to turn off row estimation. However, as
            // that was never a public feature, it was never a valid fix for the test.
            // https://bugzilla.xamarin.com/show_bug.cgi?id=28277
#if !UITEST
                if (App.IOSVersion < 9)
                    runTest = false;
#endif

            if (!runTest)
                _collection.Add(new ListObj { Name = DontRun });
            else
            {
                for (_i = 0; _i < 100; _i++)
                {
                    var item = new ListObj { Name = string.Format("{0} {0} {0} {0} {0} {0}", _i) };
                    _collection.Add(item);
                }
            }

            var btnAdd = new Button
            {
                Text = "Add item",
                WidthRequest = 100
            };
            btnAdd.Clicked += BtnAddOnClicked;

            var btnBottom = new Button
            {
                Text = "Scroll to end",
                WidthRequest = 100
            };
            btnBottom.Clicked += BtnBottomOnClicked;

            var btnPanel = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                Children = {
                    btnAdd,
                    btnBottom
                }
            };

            _listView = new ListView
            {
                ItemsSource = _collection,
                BackgroundColor = Color.Transparent,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HasUnevenRows = true,
                ItemTemplate = new DataTemplate(typeof(CellTemplateScrollToUneven))
            };
            _listView.ItemTapped += (sender, e) => ((ListView)sender).SelectedItem = null;

            _listView.AutomationId = "listView";

            Content = new StackLayout
            {
                Children = {
                    btnPanel,
                    _listView
                }
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _listView.ScrollTo(_collection.Last(), ScrollToPosition.MakeVisible, false);
        }

        void BtnBottomOnClicked(object sender, EventArgs e)
        {
            var item = _collection.Last();
            _listView.ScrollTo(item, ScrollToPosition.End, true);
        }

        void BtnAddOnClicked(object sender, EventArgs eventArgs)
        {
            var str = string.Format("Item {0}", _i++);
            var item = new ListObj { Name = string.Format("{0} {0} {0} {0} {0} {0}", _i) };
            _collection.Add(item);

            _listView.ScrollTo(item, ScrollToPosition.End, true);
        }
    }
}
