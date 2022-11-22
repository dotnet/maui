using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="Type[@FullName='Microsoft.Maui.Controls.AbsoluteLayout']/Docs/*" />
	public class AbsoluteLayout : Layout, IAbsoluteLayout
	{
		readonly Dictionary<IView, AbsoluteLayoutInfo> _viewInfo = new();

		protected override ILayoutManager CreateLayoutManager()
		{
			return new AbsoluteLayoutManager(this);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='AutoSize']/Docs/*" />
		public static double AutoSize = -1;

		#region Attached Properties

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='LayoutFlagsProperty']/Docs/*" />
		public static readonly BindableProperty LayoutFlagsProperty = BindableProperty.CreateAttached("LayoutFlags",
			typeof(AbsoluteLayoutFlags), typeof(AbsoluteLayout), AbsoluteLayoutFlags.None);

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='LayoutBoundsProperty']/Docs/*" />
		public static readonly BindableProperty LayoutBoundsProperty = BindableProperty.CreateAttached("LayoutBounds",
			typeof(Rect), typeof(AbsoluteLayout), new Rect(0, 0, AutoSize, AutoSize), propertyChanged: LayoutBoundsPropertyChanged);

		static void LayoutBoundsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is View view && view.Parent is Maui.ILayout layout)
			{
				layout.InvalidateMeasure();
			}
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='GetLayoutFlags'][1]/Docs/*" />
		public static AbsoluteLayoutFlags GetLayoutFlags(BindableObject bindable)
		{
			return (AbsoluteLayoutFlags)bindable.GetValue(LayoutFlagsProperty);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='GetLayoutBounds'][1]/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(BoundsTypeConverter))]
		public static Rect GetLayoutBounds(BindableObject bindable)
		{
			return (Rect)bindable.GetValue(LayoutBoundsProperty);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='SetLayoutFlags'][1]/Docs/*" />
		public static void SetLayoutFlags(BindableObject bindable, AbsoluteLayoutFlags flags)
		{
			bindable.SetValue(LayoutFlagsProperty, flags);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='SetLayoutBounds'][1]/Docs/*" />
		public static void SetLayoutBounds(BindableObject bindable, Rect bounds)
		{
			bindable.SetValue(LayoutBoundsProperty, bounds);
		}

		#endregion

		public AbsoluteLayoutFlags GetLayoutFlags(IView view)
		{
			return view switch
			{
				BindableObject bo => (AbsoluteLayoutFlags)bo.GetValue(LayoutFlagsProperty),
				_ => _viewInfo[view].LayoutFlags,
			};
		}

		public Rect GetLayoutBounds(IView view)
		{
			return view switch
			{
				BindableObject bo => (Rect)bo.GetValue(LayoutBoundsProperty),
				_ => _viewInfo[view].LayoutBounds,
			};
		}

		public void SetLayoutFlags(IView view, AbsoluteLayoutFlags flags)
		{
			switch (view)
			{
				case BindableObject bo:
					bo.SetValue(LayoutFlagsProperty, flags);
					break;
				default:
					_viewInfo[view].LayoutFlags = flags;
					break;
			}
		}

		public void SetLayoutBounds(IView view, Rect bounds)
		{
			switch (view)
			{
				case BindableObject bo:
					bo.SetValue(LayoutBoundsProperty, bounds);
					break;
				default:
					_viewInfo[view].LayoutBounds = bounds;
					break;
			}
		}

		protected override void OnAdd(int index, IView view)
		{
			if (view is not BindableObject)
			{
				_viewInfo[view] = new AbsoluteLayoutInfo();
			}

			base.OnAdd(index, view);
		}

		protected override void OnClear()
		{
			_viewInfo.Clear();
			base.OnClear();
		}

		protected override void OnRemove(int index, IView view)
		{
			_viewInfo.Remove(view);
			base.OnRemove(index, view);
		}

		protected override void OnInsert(int index, IView view)
		{
			if (view is not BindableObject)
			{
				_viewInfo[view] = new AbsoluteLayoutInfo();
			}

			base.OnInsert(index, view);
		}

		protected override void OnUpdate(int index, IView view, IView oldView)
		{
			_viewInfo.Remove(oldView);

			if (view is not BindableObject)
			{
				_viewInfo[view] = new AbsoluteLayoutInfo();
			}

			base.OnUpdate(index, view, oldView);
		}

		class AbsoluteLayoutInfo
		{
			public AbsoluteLayoutFlags LayoutFlags { get; set; }
			public Rect LayoutBounds { get; set; }
		}
	}
}
