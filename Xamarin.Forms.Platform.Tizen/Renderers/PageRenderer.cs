using System;
using System.Collections.Specialized;
using ElmSharp.Wearable;
using Xamarin.Forms.Platform.Tizen.Native.Watch;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	/// <summary>
	/// Renderer of ContentPage.
	/// </summary>
	public class PageRenderer : VisualElementRenderer<Page>
	{
		/// <summary>
		/// Native control which holds the contents.
		/// </summary>
		Native.Page _page;
		Lazy<MoreOption> _moreOption;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public PageRenderer()
		{
			RegisterPropertyHandler(Page.BackgroundImageSourceProperty, UpdateBackgroundImage);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			if (null == _page)
			{
				_page = new Native.Page(Forms.NativeParent);
				_page.LayoutUpdated += OnLayoutUpdated;
				SetNativeView(_page);
			}
			base.OnElementChanged(e);
		}

		protected override void OnElementReady()
		{
			if (Device.Idiom == TargetIdiom.Watch)
			{
				_moreOption = new Lazy<MoreOption>(CreateMoreOption);
				if (Element.ToolbarItems is INotifyCollectionChanged items)
				{
					items.CollectionChanged += OnToolbarCollectionChanged;
				}
				if (Element.ToolbarItems.Count > 0)
				{
					UpdateToolbarItems(true);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_page != null)
				{
					_page.LayoutUpdated -= OnLayoutUpdated;
				}

				if (Device.Idiom == TargetIdiom.Watch)
				{
					if (Element.ToolbarItems is INotifyCollectionChanged items)
					{
						items.CollectionChanged -= OnToolbarCollectionChanged;
					}

					if (_moreOption.IsValueCreated)
					{
						_moreOption.Value.Clicked -= OnMoreOptionItemClicked;
						_moreOption.Value.Closed -= SendMoreOptionClosed;
						_moreOption.Value.Opened -= SendMoreOptionOpened;
						_moreOption.Value.Items.Clear();
						_moreOption.Value.Unrealize();
					}
				}
			}
			base.Dispose(disposing);
		}

		protected override void UpdateBackgroundColor(bool initialize)
		{
			if (initialize && Element.BackgroundColor.IsDefault)
				return;

			// base.UpdateBackgroundColor() is not called on purpose, we don't want the regular background setting
			if (Element.BackgroundColor.IsDefault || Element.BackgroundColor.A == 0)
				_page.Color = EColor.Transparent;
			else
				_page.Color = Element.BackgroundColor.ToNative();
		}

		protected override void UpdateLayout()
		{
			// empty on purpose
		}

		protected virtual FormsMoreOptionItem CreateMoreOptionItem(ToolbarItem item)
		{
			var moreOptionItem = new FormsMoreOptionItem
			{
				MainText = item.Text,
				ToolbarItem = item
			};
			var icon = item.IconImageSource as FileImageSource;
			if (icon != null)
			{
				var img = new ElmSharp.Image(_moreOption.Value);
				img.Load(ResourcePath.GetPath(icon));
				moreOptionItem.Icon = img;
			}
			return moreOptionItem;
		}

		protected virtual void OnMoreOptionClosed()
		{
		}

		protected virtual void OnMoreOptionOpened()
		{
		}

		void UpdateBackgroundImage(bool initialize)
		{
			if (initialize && Element.BackgroundImageSource.IsNullOrEmpty())
				return;

			// TODO: investigate if we can use the other image source types: stream, font, uri

			var bgImage = Element.BackgroundImageSource as FileImageSource;
			if (bgImage.IsNullOrEmpty())
				_page.File = null;
			else
				_page.File = ResourcePath.GetPath(bgImage);
		}

		void OnLayoutUpdated(object sender, Native.LayoutEventArgs e)
		{
			Element.Layout(e.Geometry.ToDP());

			if (_moreOption != null && _moreOption.IsValueCreated)
			{
				_moreOption.Value.Geometry = _page.Geometry;
			}
		}

		MoreOption CreateMoreOption()
		{
			var moreOption = new MoreOption(_page);
			_page.Children.Add(moreOption);
			moreOption.Show();
			moreOption.Clicked += OnMoreOptionItemClicked;
			moreOption.Closed += SendMoreOptionClosed;
			moreOption.Opened += SendMoreOptionOpened;
			return moreOption;
		}

		void SendMoreOptionClosed(object sender, EventArgs e)
		{
			OnMoreOptionClosed();
		}

		void SendMoreOptionOpened(object sender, EventArgs e)
		{
			OnMoreOptionOpened();
		}

		void OnToolbarCollectionChanged(object sender, EventArgs eventArgs)
		{
			if (Element.ToolbarItems.Count > 0 || _moreOption.IsValueCreated)
			{
				UpdateToolbarItems(false);
			}
		}

		void UpdateToolbarItems(bool initialize)
		{
			//clear existing more option items and add toolbar item again on purpose.
			if (!initialize && _moreOption.Value.Items.Count > 0)
			{
				_moreOption.Value.Items.Clear();
			}

			foreach (var item in Element.ToolbarItems)
			{
				_moreOption.Value.Items.Add(CreateMoreOptionItem(item));
			}
		}

		void OnMoreOptionItemClicked(object sender, MoreOptionItemEventArgs e)
		{
			var formsMoreOptionItem = e.Item as FormsMoreOptionItem;
			if (formsMoreOptionItem != null)
			{
				((IMenuItemController)formsMoreOptionItem.ToolbarItem)?.Activate();
			}
			_moreOption.Value.IsOpened = false;
		}
	}
}
