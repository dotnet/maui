#nullable disable
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls
{
	/// <summary>Represents an <see cref="Element"/> with base functionality for <see cref="Page"/> navigation. Does not necessarily render on screen.</summary>
	/// <remarks>Not meant to be used directly. Instead, opt to use derived types, such as <see cref="View"/>.</remarks>
	public class NavigableElement : StyleableElement, INavigationProxy
	{
		static readonly BindablePropertyKey NavigationPropertyKey =
			BindableProperty.CreateReadOnly(nameof(Navigation), typeof(INavigation), typeof(NavigableElement), default(INavigation));

		/// <summary>Bindable property for <see cref="Navigation"/>.</summary>
		public static readonly BindableProperty NavigationProperty = NavigationPropertyKey.BindableProperty;

		/// <inheritdoc/>
		public static readonly new BindableProperty StyleProperty = StyleableElement.StyleProperty;

		internal NavigableElement()
		{
			Navigation = new NavigationProxy();
		}

		/// <summary>Gets the object responsible for handling stack-based navigation.</summary>
		/// <remarks>Binds to the <see cref="NavigationProperty"/> <see cref="BindableProperty"/>.</remarks>
		public INavigation Navigation
		{
			get { return (INavigation)GetValue(NavigationProperty); }
			internal set { SetValue(NavigationPropertyKey, value); }
		}

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
