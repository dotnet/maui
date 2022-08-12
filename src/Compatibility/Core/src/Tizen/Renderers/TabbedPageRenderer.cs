#nullable enable

using System;
using System.Linq;
using Microsoft.Maui.Controls.Platform;
using Tizen.NUI;
using Tizen.UIExtensions.NUI;
using GColors = Microsoft.Maui.Graphics.Colors;
using NCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using NLayoutParamPolicies = Tizen.NUI.BaseComponents.LayoutParamPolicies;
using NView = Tizen.NUI.BaseComponents.View;
using XLabel = Microsoft.Maui.Controls.Label;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[Obsolete("Use Microsoft.Maui.Controls.Platform.Compatibility.TabbedRenderer instead")]
	public class TabbedPageRenderer : VisualElementRenderer<TabbedPage>
	{
		NCollectionView? _tabbedView;
		TabbedPageAdaptor? _adaptor;
		ViewGroup? _content;

		public TabbedPageRenderer()
		{
			RegisterPropertyHandler(nameof(Element.CurrentPage), OnCurrentPageChanged);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
		{
			if (NativeView == null && e.NewElement != null)
			{
				SetNativeView(new NView
				{
					BackgroundColor = Color.White,
					HeightSpecification = NLayoutParamPolicies.MatchParent,
					WidthSpecification = NLayoutParamPolicies.MatchParent,
				});

				_tabbedView = new NCollectionView
				{
					SizeHeight = 40d.ToScaledPixel(),
					WidthSpecification = NLayoutParamPolicies.MatchParent,
					LayoutManager = new GridLayoutManager(true, 1, global::Tizen.UIExtensions.NUI.ItemSizingStrategy.MeasureAllItems),
					SelectionMode = CollectionViewSelectionMode.SingleAlways,
				};
				_tabbedView.Adaptor = _adaptor = new TabbedPageAdaptor(e.NewElement);
				_content = new ViewGroup
				{
					WidthSpecification = NLayoutParamPolicies.MatchParent,
					HeightSpecification = NLayoutParamPolicies.MatchParent,
					Position = new Position(0, 40d.ToScaledPixel())
				};

				NativeView!.Add(_tabbedView);
				NativeView!.Add(_content);
				_adaptor.SelectionChanged += OnTabItemSelected;
			}
			base.OnElementChanged(e);
		}

		protected override void OnElementReady()
		{
			base.OnElementReady();
			_tabbedView!.RequestItemSelect(Element.InternalChildren.IndexOf(Element.CurrentPage));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (var child in Element.InternalChildren)
				{
					var renderer = Platform.GetRenderer(child);
					renderer?.Dispose();
				}
				_tabbedView?.Dispose();
				_content?.Dispose();
			}
			base.Dispose(disposing);
		}

		void OnCurrentPageChanged()
		{
			var current = Platform.GetOrCreateRenderer(Element.CurrentPage).NativeView;
			current.WidthSpecification = NLayoutParamPolicies.MatchParent;
			current.HeightSpecification = NLayoutParamPolicies.MatchParent;
			var old = _content!.Children.FirstOrDefault();
			if (current != old)
			{
				if (old != null)
				{
					_content.Remove(old);
				}
				_content.Add(current);
				Element.CurrentPage?.SendAppearing();
			}
		}

		void OnTabItemSelected(object? sender, CollectionViewSelectionChangedEventArgs e)
		{
			if (e.SelectedItems.Count == 0)
				return;

			Page? current = e.SelectedItems[0] as Page;
			if (Element.CurrentPage != current)
			{
				Element.CurrentPage?.SendDisappearing();
				Element.CurrentPage = current;
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

			TabbedPage _page;
			BoxView _bar;

			public bool IsSelected
			{
				get => (bool)GetValue(SelectedStateProperty);
				set => SetValue(SelectedStateProperty, value);
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
				BackgroundColor = GColors.White;

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

				var grid = new Controls.Grid
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
					TargetType = typeof(ItemView),
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
					TargetType = typeof(ItemView),
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
