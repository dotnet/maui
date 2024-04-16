#nullable disable
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls
{
	/// <summary>Represents an <see cref="Element"/> with base functionality for <see cref="Page"/> navigation. Does not necessarily render on screen.</summary>
	/// <remarks>Not meant to be used directly. Instead, opt to use derived types, such as <see cref="View"/>.</remarks>
	public class NavigableElement : Element, INavigationProxy, IStyleSelectable
	{
		static readonly BindablePropertyKey NavigationPropertyKey =
			BindableProperty.CreateReadOnly("Navigation", typeof(INavigation), typeof(VisualElement), default(INavigation));

		/// <summary>Bindable property for <see cref="Navigation"/>.</summary>
		public static readonly BindableProperty NavigationProperty = NavigationPropertyKey.BindableProperty;

		/// <summary>Bindable property for <see cref="Style"/>.</summary>
		public static readonly BindableProperty StyleProperty =
			BindableProperty.Create("Style", typeof(Style), typeof(VisualElement), default(Style),
				propertyChanged: (bindable, oldvalue, newvalue) => ((NavigableElement)bindable)._mergedStyle.Style = (Style)newvalue);

		internal readonly MergedStyle _mergedStyle;

		internal NavigableElement()
		{
			Navigation = new NavigationProxy();
			_mergedStyle = new MergedStyle(GetType(), this);
		}

		/// <summary>Gets the object responsible for handling stack-based navigation.</summary>
		/// <remarks>Binds to the <see cref="NavigationProperty"/> <see cref="BindableProperty"/>.</remarks>
		public INavigation Navigation
		{
			get { return (INavigation)GetValue(NavigationProperty); }
			internal set { SetValue(NavigationPropertyKey, value); }
		}

		/// <summary>Gets or sets the unique <see cref="Style"/> for this element.</summary>
		public Style Style
		{
			get { return (Style)GetValue(StyleProperty); }
			set { SetValue(StyleProperty, value); }
		}

		/// <summary>Gets or sets the style classes for the element.</summary>
		/// <remarks>
		///		<para>Equiavalent to <see cref="@class"/>.</para>
		///		<para>Style classes enable multiple styles to be applied to a control, without resorting to style inheritance.</para>
		/// </remarks>
		/// <seealso href="https://learn.microsoft.com/dotnet/maui/user-interface/styles/xaml?view=net-maui-8.0#style-classes">Conceptual documentation on style classes</seealso>
		[System.ComponentModel.TypeConverter(typeof(ListStringTypeConverter))]
		public IList<string> StyleClass
		{
			get { return @class; }
			set { @class = value; }
		}

		/// <summary>Gets or sets the style classes for the element.</summary>
		/// <remarks>
		///		<para>Equiavalent to <see cref="StyleClass"/>.</para>
		///		<para>Style classes enable multiple styles to be applied to a control, without resorting to style inheritance.</para>
		/// </remarks>
		/// <seealso href="https://learn.microsoft.com/dotnet/maui/user-interface/styles/xaml?view=net-maui-8.0#style-classes">Conceptual documentation on style classes</seealso>
		[System.ComponentModel.TypeConverter(typeof(ListStringTypeConverter))]
		public IList<string> @class
		{
			get { return _mergedStyle.StyleClass; }
			set
			{
				_mergedStyle.StyleClass = value;
			}
		}

		IList<string> IStyleSelectable.Classes => StyleClass;

		/// <summary>Gets the cast of <see cref="Navigation"/> to a <see cref="Maui.Controls.Internals.NavigationProxy"/>.</summary>
		/// <remarks>
		///		<para>Determines whether the element will proxy navigation calls.</para>
		///		<para>For internal use by .NET MAUI.</para>
		/// </remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public NavigationProxy NavigationProxy
		{
			get { return Navigation as NavigationProxy; }
		}


		/// <summary>Raises the (internal) <c>ParentSet</c> event.</summary>
		/// <remarks>
		/// Will set the <see cref="NavigationProxy">NavigationProxy's</see> inner navigation object to closest topmost element capable of handling navigation calls.
		/// </remarks>
		/// <seealso cref="Element.OnParentSet"/>
		protected override void OnParentSet()
		{
			base.OnParentSet();

			Element parent = Parent;
			INavigationProxy navProxy = null;
			while (parent != null)
			{
				if (parent is INavigationProxy proxy)
				{
					navProxy = proxy;
					break;
				}
				parent = parent.RealParent;
			}

			if (navProxy != null)
			{
				NavigationProxy.Inner = navProxy.NavigationProxy;
			}
			else
			{
				NavigationProxy.Inner = null;
			}
		}
	}
}
