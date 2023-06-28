#nullable enable
using System;
using System.Linq;
using Microsoft.Maui.Controls.Handlers.Items;
using Tizen.NUI;
using Tizen.UIExtensions.NUI;
using GColor = Microsoft.Maui.Graphics.Color;
using GColors = Microsoft.Maui.Graphics.Colors;
using NCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using NItemSizingStrategy = Tizen.UIExtensions.NUI.ItemSizingStrategy;
using NLayoutParamPolicies = Tizen.NUI.BaseComponents.LayoutParamPolicies;
using NView = Tizen.NUI.BaseComponents.View;
using XLabel = Microsoft.Maui.Controls.Label;

namespace Microsoft.Maui.Controls
{
	public partial class TabbedPage
	{
		NCollectionView _tabbedView;
		TabbedPageAdaptor _adaptor;
		ViewGroup _content;
		IMauiContext MauiContext => this.Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null here");

		NView CreatePlatformView(TabbedPage tabbedPage)
		{
			var view = new NView
			{
				HeightSpecification = NLayoutParamPolicies.MatchParent,
				WidthSpecification = NLayoutParamPolicies.MatchParent,
				Layout = new LinearLayout
				{
					LinearOrientation = LinearLayout.Orientation.Vertical
				}
			};

			_tabbedView = new NCollectionView
			{
				SizeHeight = 40d.ToScaledPixel(),
				WidthSpecification = NLayoutParamPolicies.MatchParent,
				LayoutManager = new GridLayoutManager(true, 1, NItemSizingStrategy.MeasureAllItems),
				SelectionMode = CollectionViewSelectionMode.SingleAlways,
			};
			_tabbedView.Adaptor = _adaptor = new TabbedPageAdaptor(tabbedPage);

			_content = new ViewGroup
			{
				WidthSpecification = NLayoutParamPolicies.MatchParent,
				HeightSpecification = NLayoutParamPolicies.MatchParent
			};

			view.Add(_tabbedView);
			view.Add(_content);
			_adaptor.SelectionChanged += OnTabItemSelected;
			var currentPageIndex = tabbedPage.InternalChildren.IndexOf(tabbedPage.CurrentPage);
			if (currentPageIndex != -1)
				_tabbedView!.RequestItemSelect(currentPageIndex);
			return view;
		}

		static NView? OnCreatePlatformView(ViewHandler<ITabbedView, NView> arg)
		{
			if (arg.VirtualView is TabbedPage tabbedPage)
			{
				return tabbedPage.CreatePlatformView(tabbedPage);
			}

			return null;
		}

		partial void OnHandlerChangingPartial(HandlerChangingEventArgs args)
		{
			if (args.OldHandler != null && args.NewHandler == null)
				DisconnectHandler();
		}

		void DisconnectHandler()
		{
			foreach (var child in InternalChildren)
			{
				var handler = child.ToHandler(MauiContext);
				handler?.DisconnectHandler();
			}
			_tabbedView?.Dispose();
			_content?.Dispose();
		}

		void OnTabItemSelected(object? sender, CollectionViewSelectionChangedEventArgs e)
		{
			if (e.SelectedItems is null || e.SelectedItems.Count == 0)
				return;

			Page? current = e.SelectedItems[0] as Page;
			if (CurrentPage != current)
			{
				CurrentPage?.SendDisappearing();
				CurrentPage = current;
			}
		}

		internal static void MapBarBackground(ITabbedViewHandler handler, TabbedPage view)
		{
		}
		internal static void MapBarBackgroundColor(ITabbedViewHandler handler, TabbedPage view)
		{
		}
		internal static void MapBarTextColor(ITabbedViewHandler handler, TabbedPage view)
		{
		}
		internal static void MapUnselectedTabColor(ITabbedViewHandler handler, TabbedPage view)
		{
		}
		internal static void MapSelectedTabColor(ITabbedViewHandler handler, TabbedPage view)
		{
		}

		internal static void MapItemsSource(ITabbedViewHandler handler, TabbedPage view)
		{
		}
		internal static void MapItemTemplate(ITabbedViewHandler handler, TabbedPage view)
		{
		}
		internal static void MapSelectedItem(ITabbedViewHandler handler, TabbedPage view)
		{
		}
		internal static void MapCurrentPage(ITabbedViewHandler handler, TabbedPage view)
		{
			if (view.MauiContext == null)
				return;

			var currentHandler = view.CurrentPage.ToHandler(view.MauiContext);
			if (currentHandler != null && currentHandler.ToPlatform() is NView current)
			{
				current.WidthSpecification = NLayoutParamPolicies.MatchParent;
				current.HeightSpecification = NLayoutParamPolicies.MatchParent;
				var old = view._content.Children.FirstOrDefault();
				if (current != old)
				{
					if (old != null)
					{
						view._content.Remove(old);
					}
					view._content.Add(current);
					view.CurrentPage?.SendAppearing();
				}
			}
		}

		class TabbedPageAdaptor : ItemTemplateAdaptor
		{
			public TabbedPageAdaptor(TabbedPage page) : base(page, page.InternalChildren, GetTemplate(page)) { }

			protected override bool IsSelectable => true;

			static DataTemplate GetTemplate(TabbedPage page)
			{
				return new DataTemplate(() =>
				{
					return new TabbedItem(page);
				});
			}
		}

		class TabbedItem : Frame
		{
			static readonly BindableProperty SelectedStateProperty = BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(TabbedItem), false, propertyChanged: (b, o, n) => ((TabbedItem)b).UpdateSelectedState());
			static readonly BindableProperty SelectedTabColorProperty = BindableProperty.Create(nameof(SelectedTabColor), typeof(GColor), typeof(TabbedItem), default(Color), propertyChanged: (b, o, n) => ((TabbedItem)b).UpdateSelectedState());
			static readonly BindableProperty UnselectedTabColorProperty = BindableProperty.Create(nameof(UnselectedTabColor), typeof(GColor), typeof(TabbedItem), default(Color), propertyChanged: (b, o, n) => ((TabbedItem)b).UpdateSelectedState());

			TabbedPage _page;
			BoxView _bar;

			public bool IsSelected
			{
				get => (bool)GetValue(SelectedStateProperty);
				set => SetValue(SelectedStateProperty, value);
			}

			public GColor SelectedTabColor
			{
				get => (GColor)GetValue(SelectedTabColorProperty);
				set => SetValue(SelectedTabColorProperty, value);
			}

			public GColor UnselectedTabColor
			{
				get => (GColor)GetValue(UnselectedTabColorProperty);
				set => SetValue(UnselectedTabColorProperty, value);
			}

#pragma warning disable CS8618
			public TabbedItem(TabbedPage page)
#pragma warning restore CS8618
			{
				_page = page;
				InitializeComponent();
			}

			void InitializeComponent()
			{
				Padding = new Thickness(0);
				HasShadow = false;
				BorderColor = GColors.DarkGray;
				SetBinding(BackgroundProperty, new Binding("BarBackground", source: _page));
				SetBinding(BackgroundColorProperty, new Binding("BarBackgroundColor", source: _page));
				SetBinding(SelectedTabColorProperty, new Binding("SelectedTabColor", source: _page));
				SetBinding(UnselectedTabColorProperty, new Binding("UnselectedTabColor", source: _page));

				var label = new XLabel
				{
					Margin = new Thickness(20, 0),
					FontSize = 16,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center,
				};
				label.SetBinding(XLabel.TextProperty, new Binding("Title"));
				label.SetBinding(XLabel.TextColorProperty, new Binding("BarTextColor", source: _page));

				_bar = new BoxView
				{
					Color = GColors.Transparent,
				};

				var grid = new Grid
				{
					RowDefinitions =
					{
						new RowDefinition
						{
							Height = GridLength.Star,
						},
						new RowDefinition
						{
							Height = 5,
						}
					}
				};
				grid.Add(label, 0, 0);
				grid.Add(_bar, 0, 1);
				Content = grid;

				var groups = new VisualStateGroupList();

				VisualStateGroup group = new VisualStateGroup()
				{
					Name = "CommonStates",
				};

				VisualState selected = new VisualState()
				{
					Name = VisualStateManager.CommonStates.Selected,
					TargetType = typeof(TabbedItem),
					Setters =
					{
						new Setter
						{
							Property = SelectedStateProperty,
							Value = true,
						},
					},
				};

				VisualState normal = new VisualState()
				{
					Name = VisualStateManager.CommonStates.Normal,
					TargetType = typeof(TabbedItem),
					Setters =
					{
						new Setter
						{
							Property = SelectedStateProperty,
							Value = false,
						},
					}
				};
				group.States.Add(normal);
				group.States.Add(selected);
				groups.Add(group);
				VisualStateManager.SetVisualStateGroups(this, groups);
			}

			void UpdateSelectedState()
			{
				if (IsSelected)
				{
					_bar.Color = _page.SelectedTabColor.IsNotDefault() ? _page.SelectedTabColor : GColors.DarkGray;
				}
				else
				{
					_bar.Color = _page.UnselectedTabColor.IsNotDefault() ? _page.UnselectedTabColor : GColors.Transparent;
				}
			}
		}
	}
}
