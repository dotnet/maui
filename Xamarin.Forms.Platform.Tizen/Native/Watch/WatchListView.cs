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

		public IntPtr CircleHandle => _circleGenList.CircleHandle;

		public CircleGenList CircleGenList => _circleGenList;

		public CircleSurface CircleSurface => _surface;

		public IRotaryActionWidget RotaryWidget { get => this; }

		GenItemClass _paddingItemClass;
		protected GenItemClass PaddingItemTemplate => _paddingItemClass ?? (_paddingItemClass = new PaddingItemClass());

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

		protected override void UpdateHeader()
		{
			if (GetHeader() != null)
			{
				base.UpdateHeader();
			}
			else
			{
				var paddingTemplate = PaddingItemTemplate;
				if (!HasHeaderContext())
				{
					InitializeHeaderItemContext(PaddingItemTemplate);
				}
				else
				{
					(HeaderItemContext.Item as GenListItem).UpdateItemClass(paddingTemplate, HeaderItemContext);
				}
				HeaderItemContext.Element = null;
			}
		}

		protected override void UpdateFooter()
		{
			if (GetFooter() != null)
			{
				base.UpdateFooter();
			}
			else
			{
				var paddingTemplate = PaddingItemTemplate;
				if (!HasFooterContext())
				{
					InitializeFooterItemContext(PaddingItemTemplate);
				}
				else
				{
					(FooterItemContext.Item as GenListItem).UpdateItemClass(paddingTemplate, FooterItemContext);
				}
				FooterItemContext.Element = null;
			}
		}


		protected override IntPtr CreateHandle(EvasObject parent)
		{
			_circleGenList = new CircleGenList(parent, _surface);
			RealHandle = _circleGenList.RealHandle;
			return _circleGenList.Handle;
		}

		class PaddingItemClass : GenItemClass
		{
			public PaddingItemClass() : base(ThemeConstants.GenItemClass.Styles.Watch.Padding)
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
