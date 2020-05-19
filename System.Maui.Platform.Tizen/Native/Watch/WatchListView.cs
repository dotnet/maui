using System;
using System.Collections;
using ElmSharp;
using ElmSharp.Wearable;

namespace Xamarin.Forms.Platform.Tizen.Native.Watch
{
	public class WatchListView : Native.ListView, IRotaryActionWidget, IRotaryInteraction
	{
		CircleGenList _circleGenList;
		CircleSurface _surface;

		GenItem _headerPadding;
		GenItem _footerPadding;

		public IntPtr CircleHandle => _circleGenList.CircleHandle;

		public CircleSurface CircleSurface => _surface;

		public IRotaryActionWidget RotaryWidget { get => this; }

		public override ScrollBarVisiblePolicy VerticalScrollBarVisibility
		{
			get => _circleGenList.VerticalScrollBarVisiblePolicy;
			set => _circleGenList.VerticalScrollBarVisiblePolicy = value;
		}

		public WatchListView(EvasObject parent, CircleSurface surface)
		{
			_surface = surface;
			Realize(parent);

			Scroller = new CircleScrollerExtension(this);
			Scroller.Scrolled += OnScrolled;
		}

		public override void AddSource(IEnumerable source, Cell beforeCell = null)
		{
			base.AddSource(source, beforeCell);
			AddHeaderPadding();
			AddFooterPadding();
		}

		public override void SetHeader(VisualElement header)
		{
			if (_headerPadding != null)
				RemovePaddingItem(_headerPadding);
			
			base.SetHeader(header);

			if (!HasHeader())
				AddHeaderPadding();
		}

		public override void SetFooter(VisualElement footer)
		{
			if(_footerPadding != null)
				RemovePaddingItem(_footerPadding);

			base.SetFooter(footer);

			if (!HasFooter())
				AddFooterPadding();
		}

		protected override IntPtr CreateHandle(EvasObject parent)
		{
			_circleGenList = new CircleGenList(parent, _surface);
			RealHandle = _circleGenList.RealHandle;
			return _circleGenList.Handle;
		}

		void RemovePaddingItem(GenItem item)
		{
			item?.Delete();
			item = null;
		}

		void AddHeaderPadding()
		{
			var cls = new WatchListView.PaddingItemClass();

			if (FirstItem == null)
			{
				_headerPadding = Append(cls, null);
			}
			else
			{
				_headerPadding = InsertBefore(cls, null, FirstItem);
			}
		}

		void AddFooterPadding()
		{
			var cls = new WatchListView.PaddingItemClass();

			if (Count > 1)
			{
				_footerPadding = Append(cls, null);
			}
		}

		class PaddingItemClass : GenItemClass
		{
			public PaddingItemClass() : base("padding")
			{
			}
		}

		class CircleScrollerExtension : CircleScroller
		{
			WatchListView _list;

			public override IntPtr CircleHandle => _list.CircleHandle;

			public CircleScrollerExtension(WatchListView parent) : base(parent, parent.CircleSurface)
			{
				_list = parent;
			}

			protected override IntPtr CreateHandle(EvasObject parent)
			{
				return parent.RealHandle;
			}
		}
	}
}
