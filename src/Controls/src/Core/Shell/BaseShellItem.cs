#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.StyleSheets;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/BaseShellItem.xml" path="Type[@FullName='Microsoft.Maui.Controls.BaseShellItem']/Docs/*" />
	[DebuggerDisplay("Title = {Title}, Route = {Route}")]
	public class BaseShellItem : NavigableElement, IPropertyPropagationController, IVisualController, IFlowDirectionController, IWindowController
	{
		public event EventHandler Appearing;
		public event EventHandler Disappearing;

		bool _hasAppearing;
		const string DefaultFlyoutItemLabelStyle = "Default_FlyoutItemLabelStyle";
		const string DefaultFlyoutItemImageStyle = "Default_FlyoutItemImageStyle";
		const string DefaultFlyoutItemLayoutStyle = "Default_FlyoutItemLayoutStyle";

		protected private ObservableCollection<Element> DeclaredChildren { get; } = new ObservableCollection<Element>();

		#region PropertyKeys

		internal static readonly BindablePropertyKey IsCheckedPropertyKey = BindableProperty.CreateReadOnly(nameof(IsChecked), typeof(bool), typeof(BaseShellItem), false);

		#endregion PropertyKeys

		/// <summary>Bindable property for <see cref="FlyoutIcon"/>.</summary>
		public static readonly BindableProperty FlyoutIconProperty =
			BindableProperty.Create(nameof(FlyoutIcon), typeof(ImageSource), typeof(BaseShellItem), null, BindingMode.OneTime);

		/// <summary>Bindable property for <see cref="Icon"/>.</summary>
		public static readonly BindableProperty IconProperty =
			BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(BaseShellItem), null, BindingMode.OneWay,
				propertyChanged: OnIconChanged);

		/// <summary>Bindable property for <see cref="IsChecked"/>.</summary>
		public static readonly BindableProperty IsCheckedProperty = IsCheckedPropertyKey.BindableProperty;

		/// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
		public static readonly BindableProperty IsEnabledProperty =
			BindableProperty.Create(nameof(IsEnabled), typeof(bool), typeof(BaseShellItem), true, BindingMode.OneWay);

		/// <summary>Bindable property for <see cref="Title"/>.</summary>
		public static readonly BindableProperty TitleProperty =
			BindableProperty.Create(nameof(Title), typeof(string), typeof(BaseShellItem), null, BindingMode.OneTime, propertyChanged: OnTitlePropertyChanged);

		/// <summary>Bindable property for <see cref="IsVisible"/>.</summary>
		public static readonly BindableProperty IsVisibleProperty =
			BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(BaseShellItem), true);

		public BaseShellItem()
		{
			DeclaredChildren.CollectionChanged += (_, args) =>
			{
				if (args.NewItems != null)
					foreach (Element element in args.NewItems)
						AddLogicalChild(element);

				if (args.OldItems != null)
					foreach (Element element in args.OldItems)
						RemoveLogicalChild(element);
			};
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/BaseShellItem.xml" path="//Member[@MemberName='FlyoutIcon']/Docs/*" />
		public ImageSource FlyoutIcon
		{
			get { return (ImageSource)GetValue(FlyoutIconProperty); }
			set { SetValue(FlyoutIconProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/BaseShellItem.xml" path="//Member[@MemberName='Icon']/Docs/*" />
		public ImageSource Icon
		{
			get { return (ImageSource)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/BaseShellItem.xml" path="//Member[@MemberName='IsChecked']/Docs/*" />
		public bool IsChecked => (bool)GetValue(IsCheckedProperty);

		/// <include file="../../../docs/Microsoft.Maui.Controls/BaseShellItem.xml" path="//Member[@MemberName='IsEnabled']/Docs/*" />
		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/BaseShellItem.xml" path="//Member[@MemberName='Route']/Docs/*" />
		public string Route
		{
			get { return Routing.GetRoute(this); }
			set { Routing.SetRoute(this, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/BaseShellItem.xml" path="//Member[@MemberName='Title']/Docs/*" />
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/BaseShellItem.xml" path="//Member[@MemberName='IsVisible']/Docs/*" />
		public bool IsVisible
		{
			get => (bool)GetValue(IsVisibleProperty);
			set => SetValue(IsVisibleProperty, value);
		}

		public bool FlyoutItemIsVisible
		{
			get => (bool)GetValue(Shell.FlyoutItemIsVisibleProperty);
			set => SetValue(Shell.FlyoutItemIsVisibleProperty, value);
		}


		internal bool IsPartOfVisibleTree()
		{
			if (Parent is IShellController shell)
				return shell.GetItems().Contains(this);
			else if (Parent is ShellGroupItem sgi)
				return sgi.ShellElementCollection.Contains(this);

			return false;
		}

		internal virtual void SendAppearing()
		{
			if (_hasAppearing)
				return;

			_hasAppearing = true;
			OnAppearing();
			Appearing?.Invoke(this, EventArgs.Empty);
		}

		internal virtual void SendDisappearing()
		{
			if (!_hasAppearing)
				return;

			_hasAppearing = false;
			OnDisappearing();
			Disappearing?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnAppearing()
		{
		}

		protected virtual void OnDisappearing()
		{
		}

		internal void OnAppearing(Action action)
		{
			if (_hasAppearing)
				action();
			else
			{
				if (Navigation.ModalStack.Count > 0)
				{
					Navigation.ModalStack[Navigation.ModalStack.Count - 1]
						.OnAppearing(action);

					return;
				}
				else if (Navigation.NavigationStack.Count > 1)
				{
					Navigation.NavigationStack[Navigation.NavigationStack.Count - 1]
						.OnAppearing(action);

					return;
				}

				EventHandler eventHandler = null;
				eventHandler = (_, __) =>
				{
					this.Appearing -= eventHandler;
					action();
				};

				this.Appearing += eventHandler;
			}
		}

		IVisual _effectiveVisual = Microsoft.Maui.Controls.VisualMarker.Default;
		IVisual IVisualController.EffectiveVisual
		{
			get { return _effectiveVisual; }
			set
			{
				if (value == _effectiveVisual)
					return;

				_effectiveVisual = value;
				OnPropertyChanged(VisualElement.VisualProperty.PropertyName);
			}
		}
		IVisual IVisualController.Visual => Microsoft.Maui.Controls.VisualMarker.MatchParent;


		static void OnTitlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var shellItem = (BaseShellItem)bindable;
			if (shellItem.FindParentOfType<Shell>()?.Toolbar is ShellToolbar st)
				st.UpdateTitle();
		}

		static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (newValue == null || bindable.IsSet(FlyoutIconProperty))
				return;

			var shellItem = (BaseShellItem)bindable;
			shellItem.FlyoutIcon = (ImageSource)newValue;
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			if (Parent != null)
			{
				if (propertyName == Shell.ItemTemplateProperty.PropertyName || propertyName == nameof(Parent))
					Propagate(Shell.ItemTemplateProperty, this, Parent, true);
			}
		}

		internal static void PropagateFromParent(BindableProperty property, Element me)
		{
			if (me == null || me.Parent == null)
				return;

			Propagate(property, me.Parent, me, false);
		}

		internal static void Propagate(BindableProperty property, BindableObject from, BindableObject to, bool onlyToImplicit)
		{
			if (from == null || to == null)
				return;

			if (onlyToImplicit && Routing.IsImplicit(from))
				return;

			if (to is Shell)
				return;

			if (from.IsSet(property) && !to.IsSet(property))
				to.SetValue(property, from.GetValue(property));
		}

		void IPropertyPropagationController.PropagatePropertyChanged(string propertyName)
		{
			PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, this, ((IVisualTreeElement)this).GetVisualChildren());
		}

		EffectiveFlowDirection _effectiveFlowDirection = default(EffectiveFlowDirection);
		EffectiveFlowDirection IFlowDirectionController.EffectiveFlowDirection
		{
			get { return _effectiveFlowDirection; }
			set
			{
				if (value == _effectiveFlowDirection)
					return;

				_effectiveFlowDirection = value;

				var ve = (Parent as VisualElement);
				ve?.InvalidateMeasureInternal(InvalidationTrigger.Undefined);
				OnPropertyChanged(VisualElement.FlowDirectionProperty.PropertyName);
			}
		}

		bool IFlowDirectionController.ApplyEffectiveFlowDirectionToChildContainer => true;
		double IFlowDirectionController.Width => (Parent as VisualElement)?.Width ?? 0;

		static readonly BindablePropertyKey WindowPropertyKey = BindableProperty.CreateReadOnly(
			nameof(Window), typeof(Window), typeof(BaseShellItem), null);

		/// <summary>Bindable property for <see cref="Window"/>.</summary>
		public static readonly BindableProperty WindowProperty = WindowPropertyKey.BindableProperty;

		public Window Window => (Window)GetValue(WindowProperty);

		Window IWindowController.Window
		{
			get => (Window)GetValue(WindowProperty);
			set => SetValue(WindowPropertyKey, value);
		}

		internal virtual void ApplyQueryAttributes(ShellRouteParameters query)
		{
		}

		static void UpdateFlyoutItemStyles(Grid flyoutItemCell, IStyleSelectable source)
		{
			List<string> bindableObjectStyle = new List<string>() {
				DefaultFlyoutItemLabelStyle,
				DefaultFlyoutItemImageStyle,
				DefaultFlyoutItemLayoutStyle,
				FlyoutItem.LabelStyle,
				FlyoutItem.ImageStyle,
				FlyoutItem.LayoutStyle };

			if (source?.Classes != null)
				foreach (var styleClass in source.Classes)
					bindableObjectStyle.Add(styleClass);

			flyoutItemCell
				.StyleClass = bindableObjectStyle;
			flyoutItemCell.Children.OfType<Label>().First()
				.StyleClass = bindableObjectStyle;
			flyoutItemCell.Children.OfType<Image>().First()
				.StyleClass = bindableObjectStyle;
		}

		BindableObject NonImplicitParent
		{
			get
			{
				if (Parent is Shell)
					return Parent;

				var parent = (BaseShellItem)Parent;

				if (!Routing.IsImplicit(parent))
					return parent;

				return parent.NonImplicitParent;
			}
		}

		internal static DataTemplate CreateDefaultFlyoutItemCell(string textBinding, string iconBinding)
		{
			return new DataTemplate(() =>
			{
				var grid = new Grid();

				if (DeviceInfo.Platform == DevicePlatform.WinUI)
					grid.ColumnSpacing = grid.RowSpacing = 0;

				grid.Resources = new ResourceDictionary();

				var defaultLabelClass = new Style(typeof(Label))
				{
					Setters = {
						new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center }
					},
					Class = DefaultFlyoutItemLabelStyle,
				};

				var defaultImageClass = new Style(typeof(Image))
				{
					Setters = {
						new Setter { Property = Image.VerticalOptionsProperty, Value = LayoutOptions.Center }
					},
					Class = DefaultFlyoutItemImageStyle,
				};

				var defaultGridClass = new Style(typeof(Grid))
				{
					Class = DefaultFlyoutItemLayoutStyle,
				};

				var groups = new VisualStateGroupList();

				var commonGroup = new VisualStateGroup();
				commonGroup.Name = "CommonStates";
				groups.Add(commonGroup);

				var normalState = new VisualState();
				normalState.Name = "Normal";
				commonGroup.States.Add(normalState);

				var selectedState = new VisualState();
				selectedState.Name = "Selected";

				if (DeviceInfo.Platform != DevicePlatform.WinUI)
				{
					selectedState.Setters.Add(new Setter
					{
						Property = VisualElement.BackgroundColorProperty,
						Value = new AppThemeBinding() { Light = Colors.Black.MultiplyAlpha(0.1f), Dark = Colors.White.MultiplyAlpha(0.1f) }
					});
				}

				normalState.Setters.Add(new Setter
				{
					Property = VisualElement.BackgroundColorProperty,
					Value = Colors.Transparent
				});

				commonGroup.States.Add(selectedState);

				defaultGridClass.Setters.Add(new Setter { Property = VisualStateManager.VisualStateGroupsProperty, Value = groups });

				if (DeviceInfo.Platform == DevicePlatform.Android)
					defaultGridClass.Setters.Add(new Setter { Property = Grid.HeightRequestProperty, Value = 50 });
				else
					defaultGridClass.Setters.Add(new Setter { Property = Grid.HeightRequestProperty, Value = 44 });


				ColumnDefinitionCollection columnDefinitions = new ColumnDefinitionCollection();

				if (DeviceInfo.Platform == DevicePlatform.Android)
					columnDefinitions.Add(new ColumnDefinition { Width = 54 });
				else if (DeviceInfo.Platform == DevicePlatform.iOS)
					columnDefinitions.Add(new ColumnDefinition { Width = 50 });
				else if (DeviceInfo.Platform == DevicePlatform.WinUI)
					columnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
				else if (DeviceInfo.Platform == DevicePlatform.Tizen)
					columnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

				columnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
				defaultGridClass.Setters.Add(new Setter { Property = Grid.ColumnDefinitionsProperty, Value = columnDefinitions });

				Binding automationIdBinding = new Binding(Element.AutomationIdProperty.PropertyName);
				defaultGridClass.Setters.Add(new Setter { Property = Element.AutomationIdProperty, Value = automationIdBinding });

				var image = new Image();

				double sizeRequest = -1;
				if (DeviceInfo.Platform == DevicePlatform.Android)
					sizeRequest = 24;
				else if (DeviceInfo.Platform == DevicePlatform.iOS)
					sizeRequest = 22;
				else if (DeviceInfo.Platform == DevicePlatform.WinUI)
					sizeRequest = 16;
				else if (DeviceInfo.Platform == DevicePlatform.Tizen)
					sizeRequest = 25;

				if (sizeRequest > 0)
				{
					defaultImageClass.Setters.Add(new Setter() { Property = Image.HeightRequestProperty, Value = sizeRequest });
					defaultImageClass.Setters.Add(new Setter() { Property = Image.WidthRequestProperty, Value = sizeRequest });
				}

				if (DeviceInfo.Platform == DevicePlatform.WinUI)
				{
					defaultImageClass.Setters.Add(new Setter { Property = Image.HorizontalOptionsProperty, Value = LayoutOptions.Start });
					defaultImageClass.Setters.Add(new Setter { Property = Image.MarginProperty, Value = new Thickness(12, 0, 12, 0) });
				}

				Binding imageBinding = new Binding(iconBinding);
				defaultImageClass.Setters.Add(new Setter { Property = Image.SourceProperty, Value = imageBinding });

				grid.Add(image);

				var label = new Label();
				Binding labelBinding = new Binding(textBinding);
				defaultLabelClass.Setters.Add(new Setter { Property = Label.TextProperty, Value = labelBinding });

				grid.Add(label, 1, 0);

				if (DeviceInfo.Platform == DevicePlatform.Android)
				{
					object textColor;

					if (Application.Current == null)
					{
						textColor = Colors.Black.MultiplyAlpha(0.87f);
					}
					else
					{
						textColor = new AppThemeBinding { Light = Colors.Black.MultiplyAlpha(0.87f), Dark = Colors.White };
					}

					defaultLabelClass.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = 14 });
					defaultLabelClass.Setters.Add(new Setter { Property = Label.TextColorProperty, Value = textColor });
					defaultLabelClass.Setters.Add(new Setter { Property = Label.FontFamilyProperty, Value = "sans-serif-medium" });
					defaultLabelClass.Setters.Add(new Setter { Property = Label.MarginProperty, Value = new Thickness(20, 0, 0, 0) });
				}
				else if (DeviceInfo.Platform == DevicePlatform.iOS)
				{
					defaultLabelClass.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = 14 });
					defaultLabelClass.Setters.Add(new Setter { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold });
				}
				else if (DeviceInfo.Platform == DevicePlatform.WinUI)
				{
					defaultLabelClass.Setters.Add(new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.Start });
					defaultLabelClass.Setters.Add(new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start });
				}
				else if (DeviceInfo.Platform == DevicePlatform.Tizen)
				{
					defaultLabelClass.Setters.Add(new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.Start });
					defaultLabelClass.Setters.Add(new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start });
				}

				INameScope nameScope = new NameScope();
				NameScope.SetNameScope(grid, nameScope);
				nameScope.RegisterName("FlyoutItemLayout", grid);
				nameScope.RegisterName("FlyoutItemImage", image);
				nameScope.RegisterName("FlyoutItemLabel", label);


				ActionDisposable previousBindingContext = null;
				grid.BindingContextChanged += (sender, _) =>
				{
					previousBindingContext?.Dispose();
					previousBindingContext = null;

					if (sender is Grid g)
					{
						var bo = g.BindingContext as BindableObject;
						var styleClassSource = Shell.GetBindableObjectWithFlyoutItemTemplate(bo) as IStyleSelectable;
						UpdateFlyoutItemStyles(g, styleClassSource);

						// this means they haven't changed the BaseShellItemContext so we are
						// going to propagate the semantic properties to the default template
						if (g.BindingContext is BaseShellItem bsi)
						{
							previousBindingContext = SemanticProperties.FakeBindSemanticProperties(bsi, g);

							// If the user hasn't set a semantic property on the flyout item then we'll
							// just bind the semantic description to the title
							if (!g.IsSet(SemanticProperties.DescriptionProperty))
							{
								g.SetBinding(SemanticProperties.DescriptionProperty, TitleProperty.PropertyName);
							}
						}
					}
				};

				grid.Resources = new ResourceDictionary() { defaultGridClass, defaultLabelClass, defaultImageClass };
				return grid;
			});
		}
	}

	public interface IQueryAttributable
	{
		void ApplyQueryAttributes(IDictionary<string, object> query);
	}
}
