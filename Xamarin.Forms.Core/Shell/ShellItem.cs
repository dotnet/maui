using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms
{
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class FlyoutItem : ShellItem
	{
		public FlyoutItem()
		{
			Shell.SetFlyoutBehavior(this, FlyoutBehavior.Flyout);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Always)]
	public class TabBar : ShellItem
	{
		public TabBar()
		{
			Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
		}
	}


	[ContentProperty(nameof(Items))]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ShellItem : ShellGroupItem, IShellItemController, IElementConfiguration<ShellItem>, IPropertyPropagationController
	{
		#region PropertyKeys

		static readonly BindablePropertyKey ItemsPropertyKey = BindableProperty.CreateReadOnly(nameof(Items), typeof(ShellSectionCollection), typeof(ShellItem), null,
				defaultValueCreator: bo => new ShellSectionCollection { Inner = new ElementCollection<ShellSection>(((ShellItem)bo)._children) });

		#endregion PropertyKeys

		#region IShellItemController

		IShellItemController ShellItemController => this;

		internal Task GoToPart(NavigationRequest request, Dictionary<string, string> queryData)
		{
			var shellSection = request.Request.Section;

			if (shellSection == null)
				shellSection = ShellItemController.GetItems()[0];

			Shell.ApplyQueryAttributes(shellSection, queryData, request.Request.Content == null);

			if (CurrentItem != shellSection)
				SetValueFromRenderer(CurrentItemProperty, shellSection);

			return shellSection.GoToPart(request, queryData);
		}

		bool IShellItemController.ProposeSection(ShellSection shellSection, bool setValue)
		{
			var controller = (IShellController)Parent;

			if (controller == null)
				return false;

			bool accept = controller.ProposeNavigation(ShellNavigationSource.ShellSectionChanged,
				this,
				shellSection,
				shellSection?.CurrentItem,
				shellSection?.Stack,
				true
			);

			if (accept && setValue)
				SetValueFromRenderer(CurrentItemProperty, shellSection);

			return accept;
		}

		ReadOnlyCollection<ShellSection> IShellItemController.GetItems() => ((ShellSectionCollection)Items).VisibleItems;

		event NotifyCollectionChangedEventHandler IShellItemController.ItemsCollectionChanged
		{
			add { ((ShellSectionCollection)Items).VisibleItemsChanged += value; }
			remove { ((ShellSectionCollection)Items).VisibleItemsChanged -= value; }
		}

		#endregion IShellItemController

		#region IPropertyPropagationController
		void IPropertyPropagationController.PropagatePropertyChanged(string propertyName)
		{
			PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, this, Items);
		}
		#endregion

		public static readonly BindableProperty CurrentItemProperty =
			BindableProperty.Create(nameof(CurrentItem), typeof(ShellSection), typeof(ShellItem), null, BindingMode.TwoWay,
				propertyChanged: OnCurrentItemChanged);


		public static readonly BindableProperty ItemsProperty = ItemsPropertyKey.BindableProperty;

		readonly ObservableCollection<Element> _children = new ObservableCollection<Element>();
		ReadOnlyCollection<Element> _logicalChildren;
		Lazy<PlatformConfigurationRegistry<ShellItem>> _platformConfigurationRegistry;

		public ShellItem()
		{
			ShellItemController.ItemsCollectionChanged += (_, __) => SendStructureChanged();
			(Items as INotifyCollectionChanged).CollectionChanged += ItemsCollectionChanged;

			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ShellItem>>(() => new PlatformConfigurationRegistry<ShellItem>(this));
		}

		public ShellSection CurrentItem
		{
			get { return (ShellSection)GetValue(CurrentItemProperty); }
			set { SetValue(CurrentItemProperty, value); }
		}

		public IList<ShellSection> Items => (IList<ShellSection>)GetValue(ItemsProperty);

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal => _logicalChildren ?? (_logicalChildren = new ReadOnlyCollection<Element>(_children));

		internal void SendStructureChanged()
		{
			if (Parent is Shell shell)
			{
				shell.SendStructureChanged();
			}
		}

		internal static ShellItem CreateFromShellSection(ShellSection shellSection)
		{
			ShellItem result = null;

			if (shellSection is Tab)
				result = new TabBar();
			else
				result = new ShellItem();

			result.Route = Routing.GenerateImplicitRoute(shellSection.Route);

			result.Items.Add(shellSection);
			result.SetBinding(TitleProperty, new Binding(nameof(Title), BindingMode.OneWay, source: shellSection));
			result.SetBinding(IconProperty, new Binding(nameof(Icon), BindingMode.OneWay, source: shellSection));
			result.SetBinding(FlyoutDisplayOptionsProperty, new Binding(nameof(FlyoutDisplayOptions), BindingMode.OneTime, source: shellSection));
			result.SetBinding(FlyoutIconProperty, new Binding(nameof(FlyoutIcon), BindingMode.OneWay, source: shellSection));
			
			return result;
		}

#if DEBUG
		[Obsolete ("Please dont use this in core code... its SUPER hard to debug when this happens", true)]
#endif
		public static implicit operator ShellItem(ShellSection shellSection)
		{
			return CreateFromShellSection(shellSection);
		}

		internal static ShellItem GetShellItemFromRouteName(string route)
		{
			var shellContent = new ShellContent { Route = route, Content = Routing.GetOrCreateContent(route) };
			var result = new ShellItem();
			var shellSection = new ShellSection();
			shellSection.Items.Add(shellContent);
			result.Route = Routing.GenerateImplicitRoute(shellSection.Route);
			result.Items.Add(shellSection);
			result.SetBinding(TitleProperty, new Binding(nameof(Title), BindingMode.OneWay, source: shellSection));
			result.SetBinding(IconProperty, new Binding(nameof(Icon), BindingMode.OneWay, source: shellSection));
			result.SetBinding(FlyoutDisplayOptionsProperty, new Binding(nameof(FlyoutDisplayOptions), BindingMode.OneTime, source: shellSection));
			result.SetBinding(FlyoutIconProperty, new Binding(nameof(FlyoutIcon), BindingMode.OneWay, source: shellSection));
			return result;
		}

#if DEBUG
		[Obsolete("Please dont use this in core code... its SUPER hard to debug when this happens", true)]
#endif
		public static implicit operator ShellItem(ShellContent shellContent) => (ShellSection)shellContent;

#if DEBUG
		[Obsolete("Please dont use this in core code... its SUPER hard to debug when this happens", true)]
#endif
		public static implicit operator ShellItem(TemplatedPage page) => (ShellSection)(ShellContent)page;

#if DEBUG
		[Obsolete("Please dont use this in core code... its SUPER hard to debug when this happens", true)]
#endif
		public static implicit operator ShellItem(MenuItem menuItem) => new MenuShellItem(menuItem);

		public IPlatformElementConfiguration<T, ShellItem> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);
			if (CurrentItem == null)
				SetValueFromRenderer(CurrentItemProperty, child);
		}

		protected override void OnChildRemoved(Element child)
		{
			base.OnChildRemoved(child);
			if (CurrentItem == child)
			{
				if (ShellItemController.GetItems().Count == 0)
					ClearValue(CurrentItemProperty);
				else
					SetValueFromRenderer(CurrentItemProperty, ShellItemController.GetItems()[0]);
			}
		}

		static void OnCurrentItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue is BaseShellItem oldShellItem)
				oldShellItem.SendDisappearing();

			var shellItem = (ShellItem)bindable;
			if (shellItem.Parent is Shell parentShell && parentShell.CurrentItem == shellItem)
			{
				if (newValue is BaseShellItem newShellItem)
					newShellItem.SendAppearing();
			}

			if (shellItem.Parent is IShellController shell)
			{
				shell.UpdateCurrentState(ShellNavigationSource.ShellSectionChanged);
			}

			shellItem.SendStructureChanged();
			((IShellController)shellItem?.Parent)?.AppearanceChanged(shellItem, false);
		}

		void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (Element element in e.NewItems)
					OnChildAdded(element);
			}

			if (e.OldItems != null)
			{
				foreach (Element element in e.OldItems)
					OnChildRemoved(element);
			}
		}

		internal override void SendAppearing()
		{
			base.SendAppearing();
			if(CurrentItem != null && Parent is Shell shell && shell.CurrentItem == this)
			{
				CurrentItem.SendAppearing();
			}
		}

		internal override void SendDisappearing()
		{
			base.SendDisappearing();
			if (CurrentItem != null)
			{
				CurrentItem.SendDisappearing();
			}
		}
	}
}
