using System;
using System.Collections.Generic;
using ElmSharp;
using Tizen.NET.MaterialComponents;
using Xamarin.Forms.Platform.Tizen;

namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialNavigationView : MNavigationView, INavigationView
	{
		IDictionary<MItem, Element> _flyoutMenu = new Dictionary<MItem, Element>();

		public MaterialNavigationView(EvasObject parent) : base(parent)
		{
			MenuItemSelected += OnSelectedItemChanged;
		}

		public event EventHandler<SelectedItemChangedEventArgs> SelectedItemChanged;

		public void BuildMenu(List<List<Element>> flyoutGroups)
		{
			var groups = new List<MGroup>();
			_flyoutMenu.Clear();

			for (int i = 0; i < flyoutGroups.Count; i++)
			{
				var flyoutGroup = flyoutGroups[i];
				var items = new List<MItem>();
				for (int j = 0; j < flyoutGroup.Count; j++)
				{
					string title = null;
					string icon = null;
					if (flyoutGroup[j] is BaseShellItem shellItem)
					{
						title = shellItem.Title;

						if (shellItem.FlyoutIcon is FileImageSource flyoutIcon)
						{
							icon = flyoutIcon.File;
						}
					}
					else if (flyoutGroup[j] is MenuItem menuItem)
					{
						title = menuItem.Text;
						if (menuItem.IconImageSource != null && menuItem.IconImageSource is FileImageSource fileImageSource)
						{
							icon = fileImageSource.File;
						}
					}
					MItem item = new MItem(title, icon);
					items.Add(item);

					_flyoutMenu.Add(item, flyoutGroup[j]);
				}
				var group = new MGroup(items);
				groups.Add(group);
			}
			GroupMenu = groups;
		}

		void OnSelectedItemChanged(object sender, GenListItemEventArgs e)
		{
			_flyoutMenu.TryGetValue(e.Item.Data as MItem, out Element element);

			SelectedItemChanged?.Invoke(this, new SelectedItemChangedEventArgs(element, -1));
		}
	}
}
