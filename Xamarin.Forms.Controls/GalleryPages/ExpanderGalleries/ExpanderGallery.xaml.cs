using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace Xamarin.Forms.Controls.GalleryPages.ExpanderGalleries
{
	public partial class ExpanderGallery : ContentPage
	{
		ICommand _command;

		public ExpanderGallery()
		{
			InitializeComponent();
		}

		public ICommand Command => _command ?? (_command = new Command(p =>
		{
			var sender = (Item)p;
			if(!sender.IsExpanded)
			{
				return;
			}

			foreach (var item in Items)
			{
				item.IsExpanded = sender == item;
			}
		}));

		public Item[] Items { get; } = new Item[]
		{
			new Item
			{
				Name = "The First",
			},
			new Item
			{
				Name = "The Second",
				IsExpanded = true
			},
			new Item
			{
				Name = "The Third",
			},
			new Item
			{
				Name = "The Fourth",
			},
			new Item
			{
				Name = "The Fifth"
			},
		};

		public sealed class Item: INotifyPropertyChanged {
			public event PropertyChangedEventHandler PropertyChanged;
			bool _isExpanded;
			bool _isEnabled = true;

			public string Name { get; set; }
			public bool IsExpanded
			{
				get => _isExpanded;
				set
				{
					if(value == _isExpanded)
						return;

					_isExpanded = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
				}
			}

			public bool IsEnabled
			{
				get => _isEnabled;
				set
				{
					if (value == _isEnabled)
						return;

					_isEnabled = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
				}
			}
		}
	}
}
