using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms.Internals;
using Xamarin.Forms.StyleSheets;

namespace Xamarin.Forms
{
	public class NavigableElement : Element, INavigationProxy, IStyleSelectable
	{
		static readonly BindablePropertyKey NavigationPropertyKey =
			BindableProperty.CreateReadOnly("Navigation", typeof(INavigation), typeof(VisualElement), default(INavigation));

		public static readonly BindableProperty NavigationProperty = NavigationPropertyKey.BindableProperty;

		public static readonly BindableProperty StyleProperty =
			BindableProperty.Create("Style", typeof(Style), typeof(VisualElement), default(Style),
				propertyChanged: (bindable, oldvalue, newvalue) => ((NavigableElement)bindable)._mergedStyle.Style = (Style)newvalue);

		internal readonly MergedStyle _mergedStyle;

		internal NavigableElement()
		{
			Navigation = new NavigationProxy();
			_mergedStyle = new MergedStyle(GetType(), this);
		}

		public INavigation Navigation
		{
			get { return (INavigation)GetValue(NavigationProperty); }
			internal set { SetValue(NavigationPropertyKey, value); }
		}

		public Style Style
		{
			get { return (Style)GetValue(StyleProperty); }
			set { SetValue(StyleProperty, value); }
		}

		[TypeConverter(typeof(ListStringTypeConverter))]
		public IList<string> StyleClass
		{
			get { return @class; }
			set { @class = value; }
		}

		[TypeConverter(typeof(ListStringTypeConverter))]
		public IList<string> @class
		{
			get { return _mergedStyle.StyleClass; }
			set
			{
				_mergedStyle.StyleClass = value;
			}
		}

		IList<string> IStyleSelectable.Classes => StyleClass;

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