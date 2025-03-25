#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <summary>Positions child elements at absolute positions.</summary>
	///	<remarks>
	///	Application developers can control the placement of child elements by providing proportional coordinates, device coordinates, or a combination of both, 
	///	depending on the <see cref="AbsoluteLayoutFlags" /> values that are passed to 
	///	<see cref="SetLayoutFlags(BindableObject,AbsoluteLayoutFlags)" /> method.
	///	When one of the proportional <see cref="AbsoluteLayoutFlags" /> enumeration values is provided, the corresponding X, or Y arguments that
	///	range between 0.0 and 1.0 will always cause the child to be displayed completely on screen. That is, you do not need to subtract or add the height or width of a
	///	child in order to display it flush with the left, right, top, or bottom of the <see cref="AbsoluteLayout" />. For width, height, X, or
	///	Y values that are not specified proportionally, application developers use device-dependent units to locate and size the child element.
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
		/// Application developers can set the width or height of the <see cref="VisualElement.Bounds" /> property to <see cref="AutoSize" />
		/// on a visual element when adding to the layout to cause that element to be measured during the layout pass and sized appropriately.
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

		/// <summary>
		/// Gets the layout flags of a view that will be used to interpret the layout bounds set on it when it is added to the layout.
		/// </summary>
		/// <param name="bindable">The bindable object to retrieve the layout flags for.</param>
		/// <returns>The layout flags applied to the given bindable object.</returns>
		public static AbsoluteLayoutFlags GetLayoutFlags(BindableObject bindable)
		{
			return (AbsoluteLayoutFlags)bindable.GetValue(LayoutFlagsProperty);
		}

		/// <summary>
		/// Gets the layout bounds of a view that will be used to interpret the layout bounds set on it when it is added to the layout.
		/// </summary>
		/// <param name="bindable">The bindable object to determine the layout bounds for.</param>
		/// <returns>A <see cref="Rect"/> with the layout bounds for the given bindable object.</returns>
		[System.ComponentModel.TypeConverter(typeof(BoundsTypeConverter))]
		public static Rect GetLayoutBounds(BindableObject bindable)
		{
			return (Rect)bindable.GetValue(LayoutBoundsProperty);
		}

		/// <summary>
		/// Sets the layout flags of a view that will be used to interpret the layout bounds set on it when it is added to the layout.
		/// </summary>
		/// <remarks>
		/// This method supports the <c>AbsoluteLayout.LayoutFlags</c> XAML attached property.
		/// In XAML, application developers can specify one or more of the <see cref="AbsoluteLayoutFlags" /> enumeration value names for the value of this property on the children of a <see cref="AbsoluteLayout" />.
		/// </remarks>
		public static void SetLayoutFlags(BindableObject bindable, AbsoluteLayoutFlags flags)
		{
			bindable.SetValue(LayoutFlagsProperty, flags);
		}

		/// <summary>
		/// Sets the layout bounds of a view that will be used to interpret the layout bounds set on it when it is added to the layout.
		/// </summary>
		/// <param name="bindable">The bindable object to set the layout bounds for.</param>
		/// <param name="bounds">The bounds to set on the given bindable object.</param>
		public static void SetLayoutBounds(BindableObject bindable, Rect bounds)
		{
			bindable.SetValue(LayoutBoundsProperty, bounds);
		}

		#endregion

		/// <summary>
		/// Gets the layout flags of a view that will be used to interpret the layout bounds set on it when it is added to the layout.
		/// </summary>
		/// <param name="view">The view to retrieve the layout flags for.</param>
		/// <returns>The layout flags applied to the given view.</returns>
		public AbsoluteLayoutFlags GetLayoutFlags(IView view)
		{
			return view switch
			{
				BindableObject bo => (AbsoluteLayoutFlags)bo.GetValue(LayoutFlagsProperty),
				_ => _viewInfo[view].LayoutFlags,
			};
		}

		/// <summary>
		/// Gets the layout bounds of a view that will be used to interpret the layout bounds set on it when it is added to the layout.
		/// </summary>
		/// <param name="view">The view to determine the layout bounds for.</param>
		/// <returns>A <see cref="Rect"/> with the layout bounds for the given view.</returns>
		public Rect GetLayoutBounds(IView view)
		{
			return view switch
			{
				BindableObject bo => (Rect)bo.GetValue(LayoutBoundsProperty),
				_ => _viewInfo[view].LayoutBounds,
			};
		}

		/// <summary>
		/// Sets the layout flags of a view that will be used to interpret the layout bounds set on it when it is added to the layout.
		/// </summary>
		/// <param name="view">The view to apply the layout flags to.</param>
		/// <param name="flags">The flags to apply to the view.</param>
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

		/// <summary>
		/// Sets the layout bounds of a view that will be used to interpret the layout bounds set on it when it is added to the layout.
		/// </summary>
		/// <param name="view">The view to set the layout bounds for.</param>
		/// <param name="bounds">The bounds to set on the given view.</param>
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
