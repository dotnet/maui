using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 2248, "ListView.ScrollTo crashes app", PlatformAffected.WinPhone)]
	public class Issue2248 : ContentPage
    {
		ObservableCollection<Item> _items;

        public Issue2248()
        {
            _items = new ObservableCollection<Item>()
            {
                new Item() {Id = 1, Name = "First"},
                new Item() {Id = 2, Name = "Second"},
                new Item() {Id = 3, Name = "Third"},
                new Item() {Id = 4, Name = "Fourth"},
                new Item() {Id = 5, Name = "Fifth"}
            };


            var listView = new ListView()
            {
                ItemsSource = _items,
                ItemTemplate = new DataTemplate(typeof (ItemCell))
            };

            Content = listView;
        }

        public void RemoveItemFromCollection(Item item)
        {
            _items.Remove(item);
        }
		public class Item
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}

		public class ItemCell : ViewCell
		{
			public ItemCell()
			{
				var nameLabel = new Label();
				nameLabel.SetBinding(Label.TextProperty, "Name");
				nameLabel.GestureRecognizers.Add(new TapGestureRecognizer()
				{
					Command = new Command(DeleteItem),
					NumberOfTapsRequired = 1
				});

				View = nameLabel;
			}

			void DeleteItem()
			{
				var parent = Parent.Parent as Issue2248;

				if (parent != null)
				{
					parent.RemoveItemFromCollection((Item) BindingContext);
				}
			}
		}
    }
}
