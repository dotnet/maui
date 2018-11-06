using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{	
#if APP
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1766, "Editor.IsEnabled = false", PlatformAffected.WinPhone)]
	public partial class Issue1766 : ContentPage
	{	
		public Issue1766 ()
		{
			InitializeComponent();
            MyItems.Add(new MyItem() { Reference = DateTime.Now.Ticks.ToString(), ShowButton = true });
            MyItems.Add(new MyItem() { Reference = DateTime.Now.Ticks.ToString(), ShowButton = false });
            MyItems.Add(new MyItem() { Reference = DateTime.Now.Ticks.ToString(), ShowButton = true });
            MyItems.Add(new MyItem() { Reference = DateTime.Now.Ticks.ToString(), ShowButton = false });

            var myListViewList = this.FindByName<ListView>("MyListViewList");

            foreach (var item in myListViewList.ItemTemplate.Values)
            {
                System.Diagnostics.Debug.WriteLine("item: {0}", item);
            }

            if (myListViewList != null)
            {
                myListViewList.ItemTapped += (sender, args) =>
                {
                    DisplayAlert("Item Tapped", "Item Tapped", "Ok");
                };
            }

            BindingContext = this;
		}

		List<MyItem> _myItems;
        public List<MyItem> MyItems
        {
            get
            {
                if (_myItems == null) {
	                _myItems = new List<MyItem> ();
                }
                return _myItems;
            }
        }

        public void OnButtonClicked(object sender, EventArgs e)
        {
            DisplayAlert("Button Tapped", "Button Tapped", "Ok");
        }
	}

	public class MyItem
	{
		public string Reference { get; set; }
		public bool ShowButton { get; set; }
	}
#endif

}

