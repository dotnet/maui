#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/FlyoutItem.xml" path="Type[@FullName='Microsoft.Maui.Controls.FlyoutItem']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class FlyoutItem : ShellItem
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/FlyoutItem.xml" path="//Member[@MemberName='LabelStyle']/Docs/*" />
		public const string LabelStyle = "FlyoutItemLabelStyle";
		/// <include file="../../../docs/Microsoft.Maui.Controls/FlyoutItem.xml" path="//Member[@MemberName='ImageStyle']/Docs/*" />
		public const string ImageStyle = "FlyoutItemImageStyle";
		/// <include file="../../../docs/Microsoft.Maui.Controls/FlyoutItem.xml" path="//Member[@MemberName='LayoutStyle']/Docs/*" />
		public const string LayoutStyle = "FlyoutItemLayoutStyle";

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlyoutItem.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public FlyoutItem()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlyoutItem.xml" path="//Member[@MemberName='IsVisibleProperty']/Docs/*" />
		public static readonly new BindableProperty IsVisibleProperty = BaseShellItem.IsVisibleProperty;
		/// <include file="../../../docs/Microsoft.Maui.Controls/FlyoutItem.xml" path="//Member[@MemberName='GetIsVisible']/Docs/*" />
		public static bool GetIsVisible(BindableObject obj) => (bool)obj.GetValue(IsVisibleProperty);
		/// <include file="../../../docs/Microsoft.Maui.Controls/FlyoutItem.xml" path="//Member[@MemberName='SetIsVisible']/Docs/*" />
		public static void SetIsVisible(BindableObject obj, bool isVisible) => obj.SetValue(IsVisibleProperty, isVisible);
	}

	/// <include file="../../../docs/Microsoft.Maui.Controls/TabBar.xml" path="Type[@FullName='Microsoft.Maui.Controls.TabBar']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class TabBar : ShellItem
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/TabBar.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public TabBar()
		{
		}
	}


	/// <include file="../../../docs/Microsoft.Maui.Controls/ShellItem.xml" path="Type[@FullName='Microsoft.Maui.Controls.ShellItem']/Docs/*" />
	[ContentProperty(nameof(Items))]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ShellItem : ShellGroupItem, IShellItemController, IElementConfiguration<ShellItem>, IPropertyPropagationController, IVisualTreeElement
	{
		#region PropertyKeys

		static readonly BindablePropertyKey ItemsPropertyKey = BindableProperty.CreateReadOnly(nameof(Items), typeof(ShellSectionCollection), typeof(ShellItem), null,
				defaultValueCreator: bo => new ShellSectionCollection { Inner = new ElementCollection<ShellSection>(((ShellItem)bo).DeclaredChildren) });

		#endregion PropertyKeys

		#region IShellItemController

		IShellItemController ShellItemController => this;

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
		ReadOnlyCollection<ShellSection> IShellItemController.GetItems() => ((ShellSectionCollection)Items).VisibleItemsReadOnly;

		event NotifyCollectionChangedEventHandler IShellItemController.ItemsCollectionChanged
		{
			add { ((ShellSectionCollection)Items).VisibleItemsChanged += value; }
			remove { ((ShellSectionCollection)Items).VisibleItemsChanged -= value; }
		}

		bool IShellItemController.ShowTabs
		{
			get
			{
				Shell shell = Parent as Shell;
				if (shell == null)
					return true;

				var displayedPage = shell.GetCurrentShellPage();

				bool defaultShowTabs = true;

#if WINDOWS
				// Windows supports nested tabs so we want the tabs to display
				// if the current shell section has multiple contents
				if (ShellItemController.GetItems().Count > 1 ||
					(CurrentItem as IShellSectionController)?.GetItems()?.Count > 1)
				{
					defaultShowTabs = true;
				}
				else
				{
					defaultShowTabs = false;
				}
#else

				if (ShellItemController.GetItems().Count <= 1)
					defaultShowTabs = false;
#endif

				return shell.GetEffectiveValue<bool>(Shell.TabBarIsVisibleProperty, () => defaultShowTabs, null, displayedPage);
			}
		}

		#endregion IShellItemController

		#region IPropertyPropagationController
		void IPropertyPropagationController.PropagatePropertyChanged(string propertyName)
		{
			PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, this, ((IVisualTreeElement)this).GetVisualChildren());
		}
		#endregion

		/// <summary>Bindable property for <see cref="CurrentItem"/>.</summary>
		public static readonly BindableProperty CurrentItemProperty =
			BindableProperty.Create(nameof(CurrentItem), typeof(ShellSection), typeof(ShellItem), null, BindingMode.TwoWay,
				propertyChanged: OnCurrentItemChanged);


		/// <summary>Bindable property for <see cref="Items"/>.</summary>

		public static readonly BindableProperty ItemsProperty = ItemsPropertyKey.BindableProperty;
		Lazy<PlatformConfigurationRegistry<ShellItem>> _platformConfigurationRegistry;

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellItem.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellItem.xml" path="//Member[@MemberName='CurrentItem']/Docs/*" />
		public ShellSection CurrentItem
		{
			get { return (ShellSection)GetValue(CurrentItemProperty); }
			set { SetValue(CurrentItemProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellItem.xml" path="//Member[@MemberName='Items']/Docs/*" />
		public IList<ShellSection> Items => (IList<ShellSection>)GetValue(ItemsProperty);
		internal override ShellElementCollection ShellElementCollection => (ShellElementCollection)Items;

		internal bool IsVisibleItem => Parent is Shell shell && shell?.CurrentItem == this;

		internal void SendStructureChanged()
		{
			if (Parent is Shell shell)
			{
				if (IsVisibleItem)
					shell.SendStructureChanged();

				shell.SendFlyoutItemsChanged();
			}
		}

		internal static ShellItem CreateFromShellSection(ShellSection shellSection)
		{
			if (shellSection.Parent != null)
			{
				var current = (ShellItem)shellSection.Parent;

				if (current.Items.Contains(shellSection))
					current.CurrentItem = shellSection;

				return current;
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

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, ShellItem> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);
			OnVisibleChildAdded(child);
		}

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
					ClearValue(CurrentItemProperty, specificity: SetterSpecificity.FromHandler);
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
			CurrentItem?.SendDisappearing();
		}

		protected override void OnParentSet()
		{
			base.OnParentSet();
			if (this.IsVisibleItem && CurrentItem != null)
				((IShellController)Parent)?.AppearanceChanged(CurrentItem, false);
		}
	}
}
