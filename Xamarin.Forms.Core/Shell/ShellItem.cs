using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class FlyoutItem : ShellItem
	{
		public const string LabelStyle = "FlyoutItemLabelStyle";
		public const string ImageStyle = "FlyoutItemImageStyle";
		public const string LayoutStyle = "FlyoutItemLayoutStyle";

		public FlyoutItem()
		{

		}

		public static readonly new BindableProperty IsVisibleProperty =
			BindableProperty.CreateAttached(nameof(IsVisible), typeof(bool), typeof(FlyoutItem), true, propertyChanged: OnFlyoutItemIsVisibleChanged);

		public static bool GetIsVisible(BindableObject obj) => (bool)obj.GetValue(IsVisibleProperty);
		public static void SetIsVisible(BindableObject obj, bool isVisible) => obj.SetValue(IsVisibleProperty, isVisible);

		static void OnFlyoutItemIsVisibleChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is Element element)
				element
					.FindParentOfType<Shell>()
					?.SendStructureChanged();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Always)]
	public class TabBar : ShellItem
	{
		public TabBar()
		{
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

		// we want the list returned from here to remain point in time accurate
		ReadOnlyCollection<ShellSection> IShellItemController.GetItems() =>
			new ReadOnlyCollection<ShellSection>(((ShellSectionCollection)Items).VisibleItemsReadOnly.ToList());

		event NotifyCollectionChangedEventHandler IShellItemController.ItemsCollectionChanged
		{
			add { ((ShellSectionCollection)Items).VisibleItemsChanged += value; }
			remove { ((ShellSectionCollection)Items).VisibleItemsChanged -= value; }
		}

		bool IShellItemController.ShowTabs
		{
			get
			{
				var displayedPage = CurrentItem?.DisplayedPage;
				if (displayedPage == null)
					return true;

				Shell shell = Parent as Shell;
				if (shell == null)
					return true;

				if (ShellItemController.GetItems().Count <= 1)
					return false;

				return shell.GetEffectiveValue<bool>(Shell.TabBarIsVisibleProperty, () => true, null, displayedPage);
			}
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
			((ShellElementCollection)Items).VisibleItemsChangedInternal += (_, args) =>
			{
				if (args.OldItems != null)
				{
					foreach (Element item in args.OldItems)
					{
						OnVisibleChildRemoved(item);
					}
				}

				if (args.NewItems != null)
				{
					foreach (Element item in args.NewItems)
					{
						OnVisibleChildAdded(item);
					}
				}

				SendStructureChanged();
			};

			(Items as INotifyCollectionChanged).CollectionChanged += ItemsCollectionChanged;

			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ShellItem>>(() => new PlatformConfigurationRegistry<ShellItem>(this));
		}

		public ShellSection CurrentItem
		{
			get { return (ShellSection)GetValue(CurrentItemProperty); }
			set { SetValue(CurrentItemProperty, value); }
		}

		public IList<ShellSection> Items => (IList<ShellSection>)GetValue(ItemsProperty);
		internal override ShellElementCollection ShellElementCollection => (ShellElementCollection)Items;

		internal bool IsVisibleItem => Parent is Shell shell && shell?.CurrentItem == this;

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal => _logicalChildren ?? (_logicalChildren = new ReadOnlyCollection<Element>(_children));

		internal void SendStructureChanged()
		{
			if (Parent is Shell shell && IsVisibleItem)
			{
				shell.SendStructureChanged();
			}
		}

		internal static ShellItem CreateFromShellSection(ShellSection shellSection)
		{
			if (shellSection.Parent != null)
			{
				return (ShellItem)shellSection.Parent;
			}

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

		public static implicit operator ShellItem(ShellSection shellSection)
		{
			return CreateFromShellSection(shellSection);
		}

		public static implicit operator ShellItem(ShellContent shellContent) => (ShellSection)shellContent;

		public static implicit operator ShellItem(TemplatedPage page) => (ShellSection)(ShellContent)page;

		public static implicit operator ShellItem(MenuItem menuItem) => new MenuShellItem(menuItem);

		public IPlatformElementConfiguration<T, ShellItem> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);
			OnVisibleChildAdded(child);
		}

		[Obsolete("OnChildRemoved(Element) is obsolete as of version 4.8.0. Please use OnChildRemoved(Element, int) instead.")]
		protected override void OnChildRemoved(Element child) => OnChildRemoved(child, -1);

		protected override void OnChildRemoved(Element child, int oldLogicalIndex)
		{
			base.OnChildRemoved(child, oldLogicalIndex);
			OnVisibleChildRemoved(child);
		}

		void OnVisibleChildAdded(Element child)
		{
			if (CurrentItem == null && ((IShellItemController)this).GetItems().Contains(child))
				SetValueFromRenderer(CurrentItemProperty, child);
		}

		void OnVisibleChildRemoved(Element child)
		{
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

			if (newValue == null)
				return;

			if (shellItem.Parent is Shell)
			{
				if (newValue is BaseShellItem newShellItem)
					newShellItem.SendAppearing();
			}

			if (shellItem.Parent is IShellController shell && shellItem.IsVisibleItem)
			{
				shell.UpdateCurrentState(ShellNavigationSource.ShellSectionChanged);
			}

			shellItem.SendStructureChanged();

			if (shellItem.IsVisibleItem)
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
				for (var i = 0; i < e.OldItems.Count; i++)
				{
					var element = (Element)e.OldItems[i];
					OnChildRemoved(element, e.OldStartingIndex + i);
				}
			}
		}

		internal override void SendAppearing()
		{
			base.SendAppearing();
			if (CurrentItem != null && Parent is Shell shell && shell.CurrentItem == this)
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