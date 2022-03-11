using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/NavigableElement.xml" path="Type[@FullName='Microsoft.Maui.Controls.NavigableElement']/Docs" />
	public class NavigableElement : Element, INavigationProxy, IStyleSelectable
	{
		static readonly BindablePropertyKey NavigationPropertyKey =
			BindableProperty.CreateReadOnly("Navigation", typeof(INavigation), typeof(VisualElement), default(INavigation));

		/// <include file="../../../docs/Microsoft.Maui.Controls/NavigableElement.xml" path="//Member[@MemberName='NavigationProperty']/Docs" />
		public static readonly BindableProperty NavigationProperty = NavigationPropertyKey.BindableProperty;

		/// <include file="../../../docs/Microsoft.Maui.Controls/NavigableElement.xml" path="//Member[@MemberName='StyleProperty']/Docs" />
		public static readonly BindableProperty StyleProperty =
			BindableProperty.Create("Style", typeof(Style), typeof(VisualElement), default(Style),
				propertyChanged: (bindable, oldvalue, newvalue) => ((NavigableElement)bindable)._mergedStyle.Style = (Style)newvalue);

		internal readonly MergedStyle _mergedStyle;

		internal NavigableElement()
		{
			Navigation = new NavigationProxy();
			_mergedStyle = new MergedStyle(GetType(), this);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/NavigableElement.xml" path="//Member[@MemberName='Navigation']/Docs" />
		public INavigation Navigation
		{
			get { return (INavigation)GetValue(NavigationProperty); }
			internal set { SetValue(NavigationPropertyKey, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/NavigableElement.xml" path="//Member[@MemberName='Style']/Docs" />
		public Style Style
		{
			get { return (Style)GetValue(StyleProperty); }
			set { SetValue(StyleProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/NavigableElement.xml" path="//Member[@MemberName='StyleClass']/Docs" />
		[System.ComponentModel.TypeConverter(typeof(ListStringTypeConverter))]
		public IList<string> StyleClass
		{
			get { return @class; }
			set { @class = value; }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/NavigableElement.xml" path="//Member[@MemberName='class']/Docs" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/NavigableElement.xml" path="//Member[@MemberName='NavigationProxy']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public NavigationProxy NavigationProxy
		{
			get { return Navigation as NavigationProxy; }
		}

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