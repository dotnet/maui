using System;
using System.Collections.Generic;
using ElmSharp;
using Tizen.NET.MaterialComponents;
using Xamarin.Forms.Platform.Tizen;
using EImage = ElmSharp.Image;

namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialNavigationView : MNavigationView, INavigationView
	{
		ImageSource _bgImageSource;
		EImage _bg;
		Aspect _bgImageAspect;

		IDictionary<MItem, Element> _flyoutMenu = new Dictionary<MItem, Element>();

		public MaterialNavigationView(EvasObject parent) : base(parent)
		{
			MenuItemSelected += OnSelectedItemChanged;
		}

		public Aspect BackgroundImageAspect
		{
			get
			{
				return _bgImageAspect;
			}
			set
			{
				_bgImageAspect = value;
				if (_bg != null)
				{
					_bg.ApplyAspect(_bgImageAspect);
				}
			}
		}

		public ImageSource BackgroundImageSource
		{
			get
			{
				return _bgImageSource;
			}
			set
			{
				_bgImageSource = value;
				if(_bgImageSource != null)
				{
					if(_bg == null)
					{
						_bg = new EImage(this);
					}
					_bg.ApplyAspect(_bgImageAspect);
					BackgroundImage = _bg;
					_ = _bg.LoadFromImageSourceAsync(_bgImageSource);
				}
				else
				{
					BackgroundImage = null;
				}
			}
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
