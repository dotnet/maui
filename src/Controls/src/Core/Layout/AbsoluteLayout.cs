#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <summary>Positions child elements at absolute positions.</summary>
	///	<remarks>
	///	<para>Application developers can control the placement of child elements by providing proportional coordinates, device coordinates, or a combination of both, 
	///	depending on the <see cref="T:Microsoft.Maui.Layouts.AbsoluteLayoutFlags" /> values that are passed to 
	///	<see cref = "M:Microsoft.Maui.Controls.AbsoluteLayout.SetLayoutFlags(Microsoft.Maui.Controls.BindableObject,Microsoft.Maui.Layouts.AbsoluteLayoutFlags)" /> method.
	///	When one of the proportional <see cref = "T:Microsoft.Maui.Layouts.AbsoluteLayoutFlags" /> enumeration values is provided, the corresponding X, or Y arguments that
	///	range between 0.0 and 1.0 will always cause the child to be displayed completely on screen. That is, you do not need to subtract or add the height or width of a
	///	child in order to display it flush with the left, right, top, or bottom of the <see cref = "T:Microsoft.Maui.Controls.AbsoluteLayout" />. For width, height, X, or
	///	Y values that are not specified proportionally, application developers use device-dependent units to locate and size the child element.</para>
	///	</remarks>
	public class AbsoluteLayout : Layout, IAbsoluteLayout
	{
		readonly Dictionary<IView, AbsoluteLayoutInfo> _viewInfo = new();

		protected override ILayoutManager CreateLayoutManager()
		{
			return new AbsoluteLayoutManager(this);
		}

		/// <summary>A value that indicates that the width or height of the child should be sized to that child's native size.</summary>
		/// <remarks>
		/// <para>Application developers can set the width or height of the <see cref="P:Microsoft.Maui.Controls.VisualElement.Bounds" /> property to <see cref="P:Microsoft.Maui.Controls.AbsoluteLayout.AutoSize" /> on a visual element when adding to the layout to cause that element to be measured during the layout pass and sized appropriately.</para>
		/// </remarks>
		public static double AutoSize = -1;

		#region Attached Properties

		/// <summary>Bindable property for attached property <c>LayoutFlags</c>.</summary>
		public static readonly BindableProperty LayoutFlagsProperty = BindableProperty.CreateAttached("LayoutFlags",
			typeof(AbsoluteLayoutFlags), typeof(AbsoluteLayout), AbsoluteLayoutFlags.None);

		/// <summary>Bindable property for attached property <c>LayoutBounds</c>.</summary>
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

		/// <summary> Sets the layout flags of a view that will be used to interpret the layout bounds set on it when it is added to the layout.</summary>
		/// <remarks>
		/// <para>This method supports the <c>AbsoluteLayout.LayoutFlags</c> XAML attached property. In XAML, Application developers can specify the following <see cref = "T:Microsoft.Maui.Layouts.AbsoluteLayoutFlags" /> enumeration value names for the value of this property on the children of a <see cref = "T:Microsoft.Maui.Controls.AbsoluteLayout" />:</para>
		/// <list type = "bullet">
		///  <item><c> All </c></item>
		///  <item><c> None </c></item>
		///  <item><c> HeightProportional </c></item>
		///  <item><c> WidthProportional </c></item>
		///  <item><c> SizeProportional </c></item>
		///  <item><c> XProportional </c></item>
		///  <item><c> YProportional </c></item>
		///  <item><c> PositionProportional </c></item>
		/// </list>
		/// <para> Combine any of the above values by supplying a comma-separated list; call this method again to update the layout flags of a view after it is added.</para>
		/// </remarks>
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
			public AbsoluteLayoutInfo()
			{
				LayoutBounds = new Rect(0, 0, AutoSize, AutoSize);
			}

			public AbsoluteLayoutFlags LayoutFlags { get; set; }
			public Rect LayoutBounds { get; set; }
		}
	}
}
