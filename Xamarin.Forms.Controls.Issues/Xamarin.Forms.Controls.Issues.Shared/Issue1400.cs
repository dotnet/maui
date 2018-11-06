using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1400, "Group binding errors", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone, NavigationBehavior.PushModalAsync)]
    public class Issue1400 : ContentPage
    {
        public static Entry Editfield { get; set; }
        public static ListView List { get; set; }
        public static List<MyGroup> Data { get; set;  }
        public Issue1400 ()
        {
            Data = new List<MyGroup>();
            Data.Add(new MyGroup(){Headertitle = "Header 1"});
            Data.First().Add(new MyData(){Title = "title 1"});
            Data.First().Add(new MyData() { Title = "title 2" });
            Data.Add(new MyGroup() { Headertitle = "Header 2" });
            Data.Last().Add(new MyData() { Title = "title 2a" });
            Data.Last().Add(new MyData() { Title = "title 2b" });


            Editfield = new Entry();
            Editfield.HorizontalOptions = LayoutOptions.FillAndExpand;
            Editfield.BindingContext = Data.First().First();
            Editfield.SetBinding(Entry.TextProperty, "Title");

            Editfield.TextChanged += (sender, args) =>
            {

                AddCell(null);
            };

            List = new ListView();
            List.HorizontalOptions = LayoutOptions.FillAndExpand;
            List.VerticalOptions = LayoutOptions.FillAndExpand;
            List.BackgroundColor = Color.Yellow;
            List.ItemTemplate = new DataTemplate(typeof (VCTest));
            List.GroupHeaderTemplate = new DataTemplate(typeof(VCHeader));
            List.IsGroupingEnabled = true;
            List.ItemsSource = Data;



			Content = new StackLayout () {
				VerticalOptions = LayoutOptions.StartAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children = { Editfield, List },
				Padding = new Thickness (10, 10, 10, 10)

			};
        }

        public static List<MyGroup> CopyList(List<MyGroup> data)
        {
            var newlist = new List<MyGroup>();
            foreach (var grp in data)
            {
                var grpItem = new MyGroup() { Headertitle = grp.Headertitle };
                foreach (var subItem in grp)
                {
                    var item = new MyData() { Title = subItem.Title };
                    grpItem.Add(item);
                }
                newlist.Add(grpItem);
            }
            return newlist;
        } 

        public static void AddCell(MyData data)
        {
            var newlist = CopyList(Data);

            // just make some changes
            newlist.Last().Add(new MyData() { Title = Editfield.Text });
            newlist.Last().RemoveAt(0);
            newlist.Last().Add(new MyData() { Title = "2nd "+Editfield.Text });

            Data = newlist;

            List.ItemsSource = newlist;
        }
    }

    public class MyData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

	    string _title;

        public const string PropTitle = "Title";

        public string Title
        {
            get { return _title; }
            set
            {
                if (value.Equals(_title, StringComparison.Ordinal)) return;
                _title = value;
                OnPropertyChanged(new PropertyChangedEventArgs(PropTitle));
            }
        }

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null) PropertyChanged(this, e);
        }
    }

    public class MyGroup : ObservableCollection<MyData>, INotifyPropertyChanged
    {
	    string _headertitle;

        public const string PropHeadertitle = "Headertitle";

        public string Headertitle
        {
            get { return _headertitle; }
            set
            {
                if (value.Equals(_headertitle, StringComparison.Ordinal)) return;
                _headertitle = value;
                OnPropertyChanged( new PropertyChangedEventArgs(PropHeadertitle));
            }
        }
    }

	internal class VCTest : ViewCell
    {
        public VCTest()
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, "Title");
            View = label;
        }
    }

	internal class VCHeader : ViewCell
    {
        public VCHeader()
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, "Headertitle");
            View = label;
        }
    }
}
