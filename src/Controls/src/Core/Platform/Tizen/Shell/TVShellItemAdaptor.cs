#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using ElmSharp;
using Microsoft.Maui.Controls.Internals;
using Tizen.UIExtensions.ElmSharp;
using DPExtensions = Tizen.UIExtensions.ElmSharp.DPExtensions;
using ITNavigtaionView = Tizen.UIExtensions.ElmSharp.INavigationView;
using TViewHolderState = Tizen.UIExtensions.ElmSharp.ViewHolderState;

namespace Microsoft.Maui.Controls.Platform
{
	public class TVShellItemAdaptor : ItemAdaptor
	{
		Dictionary<EvasObject, View> _nativeFormsTable = new Dictionary<EvasObject, View>();
		Dictionary<object, View?> _dataBindedViewTable = new Dictionary<object, View?>();

		Element _element;
		IMauiContext _context;
		ITNavigtaionView? _navigationView;

		protected virtual bool IsSelectable { get; }

		public DataTemplate DefaultTemplate { get; private set; }

		public TVShellItemAdaptor(Element element, ITNavigtaionView? nv, IMauiContext context, IEnumerable items, bool isCollapsed) : base(items)
		{
			_element = element;
			_context = context;
			_navigationView = nv;
			IsSelectable = true;
			DefaultTemplate = CreateDafaultTemplate(nv, isCollapsed);
		}

		public override EvasObject? CreateNativeView(EvasObject parent)
		{
			return CreateNativeView(0, parent);
		}

		public override EvasObject? CreateNativeView(int index, EvasObject parent)
		{
			View? view = GetTemplatedView(index);
			if (view != null)
			{
				var native = view.ToPlatform(_context);
				_nativeFormsTable[native] = view;
				return native;
			}

			return null;
		}

		public override EvasObject? GetFooterView(EvasObject parent)
		{
			return null;
		}

		public override EvasObject? GetHeaderView(EvasObject parent)
		{
			return null;
		}

		public override Size MeasureFooter(int widthConstraint, int heightConstraint)
		{
			return new Size(0, 0);
		}

		public override Size MeasureHeader(int widthConstraint, int heightConstraint)
		{
			return new Size(0, 0);
		}

		public override Size MeasureItem(int widthConstraint, int heightConstraint)
		{
			return MeasureItem(0, widthConstraint, heightConstraint);
		}

		public override Size MeasureItem(int index, int widthConstraint, int heightConstraint)
		{
			View? view = GetTemplatedView(index);
			if (view != null)
			{
				var native = view.ToPlatform(_context);
				view.Parent = _element;

				if (Count > index)
					view.BindingContext = this[index];

				var size = view.Measure(DPExtensions.ConvertToScaledDP(widthConstraint), DPExtensions.ConvertToScaledDP(heightConstraint), MeasureFlags.IncludeMargins).Request;
				native.Unrealize();

				return size.ToEFLPixel();
			}

			return new Size(0, 0);
		}

		public override void RemoveNativeView(EvasObject native)
		{
			native?.Unrealize();
		}

		public override void SetBinding(EvasObject native, int index)
		{
			if (_nativeFormsTable.TryGetValue(native, out View? view))
			{
				ResetBindedView(view);
				var item = this[index];
				view.BindingContext = item;
				if (item != null)
					_dataBindedViewTable[item] = view;

				view.MeasureInvalidated += OnItemMeasureInvalidated;
				view.Parent = _element;
			}
		}

		public override void UnBinding(EvasObject native)
		{
			if (_nativeFormsTable.TryGetValue(native, out View? view))
			{
				view.MeasureInvalidated -= OnItemMeasureInvalidated;
				ResetBindedView(view);
			}
		}

		public override void UpdateViewState(EvasObject native, TViewHolderState state)
		{
			base.UpdateViewState(native, state);
			if (_nativeFormsTable.TryGetValue(native, out View? view))
			{
				switch (state)
				{
					case TViewHolderState.Focused:
						VisualStateManager.GoToState(view, VisualStateManager.CommonStates.Focused);
						view.SetValue(VisualElement.IsFocusedPropertyKey, true);
						break;
					case TViewHolderState.Normal:
						VisualStateManager.GoToState(view, VisualStateManager.CommonStates.Normal);
						view.SetValue(VisualElement.IsFocusedPropertyKey, false);
						break;
					case TViewHolderState.Selected:
						if (IsSelectable)
							VisualStateManager.GoToState(view, VisualStateManager.CommonStates.Selected);
						break;
				}
			}
		}

		DataTemplate CreateDafaultTemplate(ITNavigtaionView? nv, bool isCollapsed)
		{
			return new DataTemplate(() =>
			{
				var grid = new Grid
				{
					HeightRequest = nv.GetTvFlyoutItemHeight(),
					WidthRequest = nv.GetTvFlyoutItemWidth(),
					BackgroundColor = Graphics.Colors.Transparent
				};

				ColumnDefinitionCollection columnDefinitions = new ColumnDefinitionCollection();
				columnDefinitions.Add(new ColumnDefinition { Width = nv.GetTvFlyoutIconColumnSize() });
				columnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
				grid.ColumnDefinitions = columnDefinitions;

				var image = new Image
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					HeightRequest = nv.GetTvFlyoutIconSize(),
					WidthRequest = nv.GetTvFlyoutIconSize(),
					Margin = new Thickness(nv.GetTvFlyoutMargin(), 0, 0, 0),
				};
				image.SetBinding(Image.SourceProperty, new Binding("FlyoutIcon"));
				grid.Add(image);
				grid.SetColumn(image, 0);

				var label = new Label
				{
					FontSize = nv.GetTvFlyoutItemFontSize(),
					VerticalTextAlignment = TextAlignment.Center,
					Margin = new Thickness(nv.GetTvFlyoutMargin(), 0, 0, 0),
				};

				label.SetBinding(Label.TextProperty, new Binding("Title"));
				label.SetBinding(Label.TextColorProperty, new Binding("BackgroundColor", converter: new TextColorConverter(), source: grid));

				if (isCollapsed)
				{
					label.Opacity = 0;
					label.SetBinding(Label.OpacityProperty, new Binding("Width", converter: new OpacityConverter(label.Opacity), source: label));
				}

				grid.Add(label);
				grid.SetColumn(label, 1);

				var groups = new VisualStateGroupList();

				var commonGroup = new VisualStateGroup();
				commonGroup.Name = "CommonStates";
				groups.Add(commonGroup);

				var normalState = new VisualState();
				normalState.Name = "Normal";
				normalState.Setters.Add(new Setter
				{
					Property = VisualElement.BackgroundColorProperty,
					Value = nv.GetTvFlyoutItemColor(),
				});

				var focusedState = new VisualState();
				focusedState.Name = "Focused";
				focusedState.Setters.Add(new Setter
				{
					Property = VisualElement.BackgroundColorProperty,
					Value = nv.GetTvFlyoutItemFocusedColor()
				});

				var selectedState = new VisualState();
				selectedState.Name = "Selected";
				selectedState.Setters.Add(new Setter
				{
					Property = VisualElement.BackgroundColorProperty,
					Value = nv.GetTvFlyoutItemColor()
				});

				commonGroup.States.Add(normalState);
				commonGroup.States.Add(focusedState);
				commonGroup.States.Add(selectedState);

				VisualStateManager.SetVisualStateGroups(grid, groups);
				return grid;
			});
		}

		View? GetTemplatedView(int index)
		{
			var dataTemplate = DefaultTemplate;
			var item = this[index];
			if (item is BindableObject bo)
			{
				dataTemplate = (_element as IShellController)?.GetFlyoutItemDataTemplate(bo);
				if (dataTemplate != null)
				{
					if (item is IMenuItemController)
					{
						if (_element is Shell shell && shell.MenuItemTemplate != dataTemplate)
							dataTemplate = DefaultTemplate;
					}
					else
					{
						if (_element is Shell shell && shell.ItemTemplate != dataTemplate)
							dataTemplate = DefaultTemplate;
					}
				}
				else
				{
					dataTemplate = DefaultTemplate;
				}
			}

			var view = dataTemplate.SelectDataTemplate(this[index], _element).CreateContent() as View;
			return view;
		}

		void ResetBindedView(View view)
		{
			if (view.BindingContext != null && _dataBindedViewTable.ContainsKey(view.BindingContext))
			{
				_dataBindedViewTable[view.BindingContext] = null;
				view.Parent = null;
				view.BindingContext = null;
			}
		}

		void OnItemMeasureInvalidated(object? sender, EventArgs e)
		{
			var data = (sender as View)?.BindingContext ?? null;
			if (data != null)
			{
				int index = GetItemIndex(data);
				if (index != -1)
				{
					CollectionView?.ItemMeasureInvalidated(index);
				}
			}
		}

		class TextColorConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (value is Graphics.Color c && c == Graphics.Colors.Transparent)
				{
					return Graphics.Colors.White;
				}
				else
				{
					return Graphics.Colors.Black;
				}
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (value is Graphics.Color c && c == Graphics.Colors.White)
				{
					return Graphics.Colors.Transparent;
				}
				else
				{
					return Graphics.Colors.Black;
				}
			}
		}

		class OpacityConverter : IValueConverter
		{
			double _opacity;
			double _itemWidth = -1;

			public OpacityConverter(double opacity)
			{
				_opacity = opacity;
			}

			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				var width = (double)value;

				if (_itemWidth == -1)
				{
					_itemWidth = width;
					return _opacity;
				}

				_itemWidth = (_itemWidth < width) ? width : _itemWidth;
				return ((width / _itemWidth) > 1) ? 1 : (width / _itemWidth);
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				var width = (double)value;

				if (_itemWidth == -1)
				{
					_itemWidth = width;
					return _opacity;
				}

				_itemWidth = (_itemWidth < width) ? width : _itemWidth;
				return width * _itemWidth;
			}
		}
	}
}
