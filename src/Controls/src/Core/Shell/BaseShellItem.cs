using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls
{
	[DebuggerDisplay("Title = {Title}, Route = {Route}")]
	public class BaseShellItem : NavigableElement, IPropertyPropagationController, IVisualController, IFlowDirectionController, ITabStopElement
	{
		public event EventHandler Appearing;
		public event EventHandler Disappearing;

		bool _hasAppearing;
		const string DefaultFlyoutItemLabelStyle = "Default_FlyoutItemLabelStyle";
		const string DefaultFlyoutItemImageStyle = "Default_FlyoutItemImageStyle";
		const string DefaultFlyoutItemLayoutStyle = "Default_FlyoutItemLayoutStyle";

		#region PropertyKeys

		internal static readonly BindablePropertyKey IsCheckedPropertyKey = BindableProperty.CreateReadOnly(nameof(IsChecked), typeof(bool), typeof(BaseShellItem), false);

		#endregion PropertyKeys

		public static readonly BindableProperty FlyoutIconProperty =
			BindableProperty.Create(nameof(FlyoutIcon), typeof(ImageSource), typeof(BaseShellItem), null, BindingMode.OneTime);

		public static readonly BindableProperty IconProperty =
			BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(BaseShellItem), null, BindingMode.OneWay,
				propertyChanged: OnIconChanged);

		public static readonly BindableProperty IsCheckedProperty = IsCheckedPropertyKey.BindableProperty;

		public static readonly BindableProperty IsEnabledProperty =
			BindableProperty.Create(nameof(IsEnabled), typeof(bool), typeof(BaseShellItem), true, BindingMode.OneWay);

		public static readonly BindableProperty TitleProperty =
			BindableProperty.Create(nameof(Title), typeof(string), typeof(BaseShellItem), null, BindingMode.OneTime);

		public static readonly BindableProperty TabIndexProperty =
			BindableProperty.Create(nameof(TabIndex),
							typeof(int),
							typeof(BaseShellItem),
							defaultValue: 0,
							propertyChanged: OnTabIndexPropertyChanged,
							defaultValueCreator: TabIndexDefaultValueCreator);

		public static readonly BindableProperty IsTabStopProperty =
			BindableProperty.Create(nameof(IsTabStop),
									typeof(bool),
									typeof(BaseShellItem),
									defaultValue: true,
									propertyChanged: OnTabStopPropertyChanged,
									defaultValueCreator: TabStopDefaultValueCreator);

		public static readonly BindableProperty IsVisibleProperty =
			BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(BaseShellItem), true);

		static void OnTabIndexPropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
			((BaseShellItem)bindable).OnTabIndexPropertyChanged((int)oldValue, (int)newValue);

		static object TabIndexDefaultValueCreator(BindableObject bindable) =>
			((BaseShellItem)bindable).TabIndexDefaultValueCreator();

		static void OnTabStopPropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
			((BaseShellItem)bindable).OnTabStopPropertyChanged((bool)oldValue, (bool)newValue);

		static object TabStopDefaultValueCreator(BindableObject bindable) =>
			((BaseShellItem)bindable).TabStopDefaultValueCreator();

		public ImageSource FlyoutIcon
		{
			get { return (ImageSource)GetValue(FlyoutIconProperty); }
			set { SetValue(FlyoutIconProperty, value); }
		}

		public ImageSource Icon
		{
			get { return (ImageSource)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		public bool IsChecked => (bool)GetValue(IsCheckedProperty);

		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		public string Route
		{
			get { return Routing.GetRoute(this); }
			set { Routing.SetRoute(this, value); }
		}

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public int TabIndex
		{
			get => (int)GetValue(TabIndexProperty);
			set => SetValue(TabIndexProperty, value);
		}

		protected virtual void OnTabIndexPropertyChanged(int oldValue, int newValue) { }

		protected virtual int TabIndexDefaultValueCreator() => 0;

		public bool IsTabStop
		{
			get => (bool)GetValue(IsTabStopProperty);
			set => SetValue(IsTabStopProperty, value);
		}

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

		protected virtual void OnTabStopPropertyChanged(bool oldValue, bool newValue) { }

		protected virtual bool TabStopDefaultValueCreator() => true;

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
			PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, this, LogicalChildren);
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

		internal virtual void ApplyQueryAttributes(IDictionary<string, string> query)
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
				if (Device.RuntimePlatform == Device.UWP)
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

				if (Device.RuntimePlatform != Device.UWP)
				{
					selectedState.Setters.Add(new Setter
					{
						Property = VisualElement.BackgroundColorProperty,
						Value = new Color(0.95)

					});
				}

				if (Device.RuntimePlatform == Device.UWP)
				{
					normalState.Setters.Add(new Setter
					{
						Property = VisualElement.BackgroundColorProperty,
						Value = Color.Transparent
					});
				}

				commonGroup.States.Add(selectedState);

				defaultGridClass.Setters.Add(new Setter { Property = VisualStateManager.VisualStateGroupsProperty, Value = groups });

				if (Device.RuntimePlatform == Device.Android)
					defaultGridClass.Setters.Add(new Setter { Property = Grid.HeightRequestProperty, Value = 50 });
				else
					defaultGridClass.Setters.Add(new Setter { Property = Grid.HeightRequestProperty, Value = 44 });


				ColumnDefinitionCollection columnDefinitions = new ColumnDefinitionCollection();

				if (Device.RuntimePlatform == Device.Android)
					columnDefinitions.Add(new ColumnDefinition { Width = 54 });
				else if (Device.RuntimePlatform == Device.iOS)
					columnDefinitions.Add(new ColumnDefinition { Width = 50 });
				else if (Device.RuntimePlatform == Device.UWP)
					columnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

				columnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
				defaultGridClass.Setters.Add(new Setter { Property = Grid.ColumnDefinitionsProperty, Value = columnDefinitions });

				var image = new Image();

				double sizeRequest = -1;
				if (Device.RuntimePlatform == Device.Android)
					sizeRequest = 24;
				else if (Device.RuntimePlatform == Device.iOS)
					sizeRequest = 22;
				else if (Device.RuntimePlatform == Device.UWP)
					sizeRequest = 16;

				if (sizeRequest > 0)
				{
					defaultImageClass.Setters.Add(new Setter() { Property = Image.HeightRequestProperty, Value = sizeRequest });
					defaultImageClass.Setters.Add(new Setter() { Property = Image.WidthRequestProperty, Value = sizeRequest });
				}

				if (Device.RuntimePlatform == Device.UWP)
				{
					defaultImageClass.Setters.Add(new Setter { Property = Image.HorizontalOptionsProperty, Value = LayoutOptions.Start });
					defaultImageClass.Setters.Add(new Setter { Property = Image.MarginProperty, Value = new Thickness(12, 0, 12, 0) });
				}

				Binding imageBinding = new Binding(iconBinding);
				defaultImageClass.Setters.Add(new Setter { Property = Image.SourceProperty, Value = imageBinding });

				grid.Children.Add(image);

				var label = new Label();
				Binding labelBinding = new Binding(textBinding);
				defaultLabelClass.Setters.Add(new Setter { Property = Label.TextProperty, Value = labelBinding });

				grid.Children.Add(label, 1, 0);

				if (Device.RuntimePlatform == Device.Android)
				{
					defaultLabelClass.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = 14 });
					defaultLabelClass.Setters.Add(new Setter { Property = Label.TextColorProperty, Value = Color.Black.MultiplyAlpha(0.87) });
					defaultLabelClass.Setters.Add(new Setter { Property = Label.FontFamilyProperty, Value = "sans-serif-medium" });
					defaultLabelClass.Setters.Add(new Setter { Property = Label.MarginProperty, Value = new Thickness(20, 0, 0, 0) });
				}
				else if (Device.RuntimePlatform == Device.iOS)
				{
					defaultLabelClass.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = Device.GetNamedSize(NamedSize.Small, label) });
					defaultLabelClass.Setters.Add(new Setter { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold });
				}
				else if (Device.RuntimePlatform == Device.UWP)
				{
					defaultLabelClass.Setters.Add(new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.Start });
					defaultLabelClass.Setters.Add(new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start });
				}

				INameScope nameScope = new NameScope();
				NameScope.SetNameScope(grid, nameScope);
				nameScope.RegisterName("FlyoutItemLayout", grid);
				nameScope.RegisterName("FlyoutItemImage", image);
				nameScope.RegisterName("FlyoutItemLabel", label);

				grid.BindingContextChanged += (sender, __) =>
				{
					if (sender is Grid g)
					{
						var bo = g.BindingContext as BindableObject;
						var styleClassSource = Shell.GetBindableObjectWithFlyoutItemTemplate(bo) as IStyleSelectable;
						UpdateFlyoutItemStyles(g, styleClassSource);
					}
				};

				grid.Resources = new ResourceDictionary() { defaultGridClass, defaultLabelClass, defaultImageClass };
				return grid;
			});
		}
	}

	public interface IQueryAttributable
	{
		void ApplyQueryAttributes(IDictionary<string, string> query);
	}
}
