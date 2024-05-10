#nullable disable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Flex = Microsoft.Maui.Layouts.Flex;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="Type[@FullName='Microsoft.Maui.Controls.FlexLayout']/Docs/*" />
	[ContentProperty(nameof(Children))]
	public class FlexLayout : Layout, IFlexLayout
	{
		Flex.Item _root;

		/// <summary>Bindable property for attached property <see cref="Direction"/>.</summary>
		public static readonly BindableProperty DirectionProperty =
			BindableProperty.Create(nameof(Direction), typeof(FlexDirection), typeof(FlexLayout), FlexDirection.Row,
									propertyChanged: OnDirectionPropertyChanged);

		/// <summary>Bindable property for attached property <see cref="JustifyContent"/>.</summary>
		public static readonly BindableProperty JustifyContentProperty =
			BindableProperty.Create(nameof(JustifyContent), typeof(FlexJustify), typeof(FlexLayout), FlexJustify.Start,
									propertyChanged: OnJustifyContentPropertyChanged);

		/// <summary>Bindable property for attached property <see cref="AlignContent"/>.</summary>
		public static readonly BindableProperty AlignContentProperty =
			BindableProperty.Create(nameof(AlignContent), typeof(FlexAlignContent), typeof(FlexLayout), FlexAlignContent.Stretch,
									propertyChanged: OnAlignContentPropertyChanged);

		/// <summary>Bindable property for attached property <see cref="AlignItems"/>.</summary>
		public static readonly BindableProperty AlignItemsProperty =
			BindableProperty.Create(nameof(AlignItems), typeof(FlexAlignItems), typeof(FlexLayout), FlexAlignItems.Stretch,
									propertyChanged: OnAlignItemsPropertyChanged);

		/// <summary>Bindable property for attached property <see cref="Position"/>.</summary>
		public static readonly BindableProperty PositionProperty =
			BindableProperty.Create(nameof(Position), typeof(FlexPosition), typeof(FlexLayout), FlexPosition.Relative,
									propertyChanged: OnPositionPropertyChanged);

		/// <summary>Bindable property for attached property <see cref="Wrap"/>.</summary>
		public static readonly BindableProperty WrapProperty =
			BindableProperty.Create(nameof(Wrap), typeof(FlexWrap), typeof(FlexLayout), FlexWrap.NoWrap,
									propertyChanged: OnWrapPropertyChanged);

		/// <summary>Bindable property for attached property <c>Order</c>.</summary>
		public static readonly BindableProperty OrderProperty =
			BindableProperty.CreateAttached("Order", typeof(int), typeof(FlexLayout), default(int),
											propertyChanged: OnOrderPropertyChanged);

		/// <summary>Bindable property for attached property <c>Grow</c>.</summary>
		public static readonly BindableProperty GrowProperty =
			BindableProperty.CreateAttached("Grow", typeof(float), typeof(FlexLayout), default(float),
											propertyChanged: OnGrowPropertyChanged, validateValue: (bindable, value) => (float)value >= 0);

		/// <summary>Bindable property for attached property <c>Shrink</c>.</summary>
		public static readonly BindableProperty ShrinkProperty =
			BindableProperty.CreateAttached("Shrink", typeof(float), typeof(FlexLayout), 1f,
											propertyChanged: OnShrinkPropertyChanged, validateValue: (bindable, value) => (float)value >= 0);

		/// <summary>Bindable property for attached property <c>AlignSelf</c>.</summary>
		public static readonly BindableProperty AlignSelfProperty =
			BindableProperty.CreateAttached("AlignSelf", typeof(FlexAlignSelf), typeof(FlexLayout), FlexAlignSelf.Auto,
											propertyChanged: OnAlignSelfPropertyChanged);

		/// <summary>Bindable property for attached property <c>Basis</c>.</summary>
		public static readonly BindableProperty BasisProperty =
			BindableProperty.CreateAttached("Basis", typeof(FlexBasis), typeof(FlexLayout), FlexBasis.Auto,
											propertyChanged: OnBasisPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='Direction']/Docs/*" />
		public FlexDirection Direction
		{
			get => (FlexDirection)GetValue(DirectionProperty);
			set => SetValue(DirectionProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='JustifyContent']/Docs/*" />
		public FlexJustify JustifyContent
		{
			get => (FlexJustify)GetValue(JustifyContentProperty);
			set => SetValue(JustifyContentProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='AlignContent']/Docs/*" />
		public FlexAlignContent AlignContent
		{
			get => (FlexAlignContent)GetValue(AlignContentProperty);
			set => SetValue(AlignContentProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='AlignItems']/Docs/*" />
		public FlexAlignItems AlignItems
		{
			get => (FlexAlignItems)GetValue(AlignItemsProperty);
			set => SetValue(AlignItemsProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='Position']/Docs/*" />
		public FlexPosition Position
		{
			get => (FlexPosition)GetValue(PositionProperty);
			set => SetValue(PositionProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='Wrap']/Docs/*" />
		public FlexWrap Wrap
		{
			get => (FlexWrap)GetValue(WrapProperty);
			set => SetValue(WrapProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetOrder'][1]/Docs/*" />
		public static int GetOrder(BindableObject bindable)
			=> (int)bindable.GetValue(OrderProperty);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='SetOrder'][1]/Docs/*" />
		public static void SetOrder(BindableObject bindable, int value)
			=> bindable.SetValue(OrderProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetGrow'][1]/Docs/*" />
		public static float GetGrow(BindableObject bindable)
			=> (float)bindable.GetValue(GrowProperty);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='SetGrow'][1]/Docs/*" />
		public static void SetGrow(BindableObject bindable, float value)
			=> bindable.SetValue(GrowProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetShrink'][1]/Docs/*" />
		public static float GetShrink(BindableObject bindable)
			=> (float)bindable.GetValue(ShrinkProperty);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='SetShrink'][1]/Docs/*" />
		public static void SetShrink(BindableObject bindable, float value)
			=> bindable.SetValue(ShrinkProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetAlignSelf'][1]/Docs/*" />
		public static FlexAlignSelf GetAlignSelf(BindableObject bindable)
			=> (FlexAlignSelf)bindable.GetValue(AlignSelfProperty);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='SetAlignSelf'][1]/Docs/*" />
		public static void SetAlignSelf(BindableObject bindable, FlexAlignSelf value)
			=> bindable.SetValue(AlignSelfProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetBasis'][1]/Docs/*" />
		public static FlexBasis GetBasis(BindableObject bindable)
			=> (FlexBasis)bindable.GetValue(BasisProperty);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='SetBasis'][1]/Docs/*" />
		public static void SetBasis(BindableObject bindable, FlexBasis value)
			=> bindable.SetValue(BasisProperty, value);

		static readonly BindableProperty FlexItemProperty =
			BindableProperty.CreateAttached("FlexItem", typeof(Flex.Item), typeof(FlexLayout), null);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static Flex.Item GetFlexItem(BindableObject bindable)
			=> (Flex.Item)bindable.GetValue(FlexItemProperty);

		static void OnOrderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (!bindable.IsSet(FlexItemProperty))
				return;
			GetFlexItem(bindable).Order = (int)newValue;
			((VisualElement)bindable).InvalidateMeasureInternal(InvalidationTrigger.Undefined);
		}

		static void OnGrowPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (!bindable.IsSet(FlexItemProperty))
				return;
			GetFlexItem(bindable).Grow = (float)newValue;
			((VisualElement)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		static void OnShrinkPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (!bindable.IsSet(FlexItemProperty))
				return;
			GetFlexItem(bindable).Shrink = (float)newValue;
			((VisualElement)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		static void OnAlignSelfPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (!bindable.IsSet(FlexItemProperty))
				return;
			GetFlexItem(bindable).AlignSelf = (Flex.AlignSelf)(FlexAlignSelf)newValue;
			((VisualElement)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		static void OnBasisPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (!bindable.IsSet(FlexItemProperty))
				return;
			GetFlexItem(bindable).Basis = ((FlexBasis)newValue).ToFlexBasis();
			((VisualElement)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		static void OnDirectionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var flexLayout = bindable as FlexLayout;
			if (flexLayout._root == null)
				return;
			flexLayout._root.Direction = (Flex.Direction)(FlexDirection)newValue;
			flexLayout.InvalidateMeasure();
		}

		static void OnJustifyContentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var flexLayout = bindable as FlexLayout;
			if (flexLayout._root == null)
				return;
			flexLayout._root.JustifyContent = (Flex.Justify)(FlexJustify)newValue;
			flexLayout.InvalidateMeasure();
		}

		static void OnAlignContentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var flexLayout = bindable as FlexLayout;
			if (flexLayout._root == null)
				return;
			flexLayout._root.AlignContent = (Flex.AlignContent)(FlexAlignContent)newValue;
			flexLayout.InvalidateMeasure();
		}

		static void OnAlignItemsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var flexLayout = (FlexLayout)bindable;
			if (flexLayout._root == null)
				return;
			flexLayout._root.AlignItems = (Flex.AlignItems)(FlexAlignItems)newValue;
			flexLayout.InvalidateMeasure();
		}

		static void OnPositionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var flexLayout = (FlexLayout)bindable;
			if (flexLayout._root == null)
				return;
			flexLayout._root.Position = (Flex.Position)(FlexPosition)newValue;
			flexLayout.InvalidateMeasure();
		}

		static void OnWrapPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var flexLayout = bindable as FlexLayout;
			if (flexLayout._root == null)
				return;
			flexLayout._root.Wrap = (Flex.Wrap)(FlexWrap)newValue;
			flexLayout.InvalidateMeasure();
		}

		readonly Dictionary<IView, FlexInfo> _viewInfo = new();

		class FlexInfo
		{
			public int Order { get; set; }
			public float Grow { get; set; }
			public float Shrink { get; set; }
			public FlexAlignSelf AlignSelf { get; set; }
			public FlexBasis Basis { get; set; }
			public Flex.Item FlexItem { get; set; }
		}

		public int GetOrder(IView view)
		{
			return view switch
			{
				BindableObject bo => (int)bo.GetValue(OrderProperty),
				_ => _viewInfo[view].Order,
			};
		}

		public void SetOrder(IView view, int order)
		{
			switch (view)
			{
				case BindableObject bo:
					bo.SetValue(OrderProperty, order);
					break;
				default:
					_viewInfo[view].Order = order;
					break;
			}
		}

		public float GetGrow(IView view)
		{
			return view switch
			{
				BindableObject bo => (float)bo.GetValue(GrowProperty),
				_ => _viewInfo[view].Grow,
			};
		}

		public void SetGrow(IView view, float grow)
		{
			switch (view)
			{
				case BindableObject bo:
					bo.SetValue(GrowProperty, grow);
					break;
				default:
					_viewInfo[view].Grow = grow;
					break;
			}
		}

		public float GetShrink(IView view)
		{
			return view switch
			{
				BindableObject bo => (float)bo.GetValue(ShrinkProperty),
				_ => _viewInfo[view].Shrink,
			};
		}

		public void SetShrink(IView view, float shrink)
		{
			switch (view)
			{
				case BindableObject bo:
					bo.SetValue(ShrinkProperty, shrink);
					break;
				default:
					_viewInfo[view].Shrink = shrink;
					break;
			}
		}

		public FlexAlignSelf GetAlignSelf(IView view)
		{
			return view switch
			{
				BindableObject bo => (FlexAlignSelf)bo.GetValue(AlignSelfProperty),
				_ => _viewInfo[view].AlignSelf,
			};
		}

		public void SetAlignSelf(IView view, FlexAlignSelf alignSelf)
		{
			switch (view)
			{
				case BindableObject bo:
					bo.SetValue(AlignSelfProperty, alignSelf);
					break;
				default:
					_viewInfo[view].AlignSelf = alignSelf;
					break;
			}
		}

		public FlexBasis GetBasis(IView view)
		{
			return view switch
			{
				BindableObject bo => (FlexBasis)bo.GetValue(BasisProperty),
				_ => _viewInfo[view].Basis,
			};
		}

		public void SetBasis(IView view, FlexBasis basis)
		{
			switch (view)
			{
				case BindableObject bo:
					bo.SetValue(BasisProperty, basis);
					break;
				default:
					_viewInfo[view].Basis = basis;
					break;
			}
		}

		internal Flex.Item GetFlexItem(IView view)
		{
			return view switch
			{
				BindableObject bo => (Flex.Item)bo.GetValue(FlexItemProperty),
				_ => _viewInfo[view].FlexItem,
			};
		}

		void SetFlexItem(IView view, Flex.Item flexItem)
		{
			switch (view)
			{
				case BindableObject bo:
					bo.SetValue(FlexItemProperty, flexItem);
					break;
				default:
					_viewInfo[view].FlexItem = flexItem;
					break;
			}
		}

		Thickness GetMargin(IView view)
		{
			return view switch
			{
				BindableObject bo => (Thickness)bo.GetValue(MarginProperty),
				_ => view.Margin
			};
		}

		double GetWidth(IView view)
		{
			return view switch
			{
				BindableObject bo => (double)bo.GetValue(WidthRequestProperty),
				_ => view.Width
			};
		}

		double GetHeight(IView view)
		{
			return view switch
			{
				BindableObject bo => (double)bo.GetValue(HeightRequestProperty),
				_ => view.Height
			};
		}

		bool GetIsVisible(IView view)
		{
			return view switch
			{
				BindableObject bo => (bool)bo.GetValue(IsVisibleProperty),
				_ => view.Visibility != Visibility.Collapsed
			};
		}

		void InitItemProperties(IView view, Flex.Item item)
		{
			item.Order = GetOrder(view);
			item.Grow = GetGrow(view);
			item.Shrink = GetShrink(view);
			item.Basis = GetBasis(view).ToFlexBasis();
			item.AlignSelf = (Flex.AlignSelf)GetAlignSelf(view);
			var (mleft, mtop, mright, mbottom) = GetMargin(view);
			item.MarginLeft = (float)mleft;
			item.MarginTop = (float)mtop;
			item.MarginRight = (float)mright;
			item.MarginBottom = (float)mbottom;
			var width = GetWidth(view);
			item.Width = width < 0 ? float.NaN : (float)width;
			var height = GetHeight(view);
			item.Height = height < 0 ? float.NaN : (float)height;
			item.IsVisible = GetIsVisible(view);

			if (view is IPadding viewWithPadding)
			{
				var (pleft, ptop, pright, pbottom) = viewWithPadding.Padding;
				item.PaddingLeft = (float)pleft;
				item.PaddingTop = (float)ptop;
				item.PaddingRight = (float)pright;
				item.PaddingBottom = (float)pbottom;
			}
		}

		// Until we can rewrite the FlexLayout engine to handle measurement properly (without the "in measure mode" hacks)
		// we need to replace the default implementation of CrossPlatformMeasure.
		// And we need to disable the public API analyzer briefly, because it doesn't understand hiding.
#pragma warning disable RS0016 // Add public types and members to the declared API
		new public Graphics.Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
#pragma warning restore RS0016 // Add public types and members to the declared API
		{
			var layoutManager = _layoutManager ??= CreateLayoutManager();

			InMeasureMode = true;
			var result = layoutManager.Measure(widthConstraint, heightConstraint);
			InMeasureMode = false;

			return result;
		}

		internal bool InMeasureMode { get; set; }

		void AddFlexItem(int index, IView child)
		{
			if (_root == null)
				return;

			if (child is not BindableObject)
			{
				// If this is a pure Core IView, we need to track all the flex properties
				// locally because we don't have attached properties for them
				_viewInfo.Add(child, new FlexInfo());
			}

			var item = (child as FlexLayout)?._root ?? new Flex.Item();
			InitItemProperties(child, item);
			if (child is not FlexLayout)
			{
				item.SelfSizing = (Flex.Item it, ref float w, ref float h) =>
				{
					var sizeConstraints = item.GetConstraints();

					sizeConstraints.Width = (InMeasureMode && sizeConstraints.Width == 0) ? double.PositiveInfinity : sizeConstraints.Width;
					sizeConstraints.Height = (InMeasureMode && sizeConstraints.Height == 0) ? double.PositiveInfinity : sizeConstraints.Height;

					if (child is Image)
					{
						// This is a hack to get FlexLayout to behave like it did in Forms
						// Forms always did its initial image measure unconstrained, which would return
						// the intrinsic size of the image (no scaling or aspect ratio adjustments)

						sizeConstraints.Width = double.PositiveInfinity;
						sizeConstraints.Height = double.PositiveInfinity;
					}

					var request = child.Measure(sizeConstraints.Width, sizeConstraints.Height);
					w = (float)request.Width;
					h = (float)request.Height;
				};
			}

			_root.InsertAt(index, item);
			SetFlexItem(child, item);
		}

		void RemoveFlexItem(IView child)
		{
			if (_root == null)
				return;

			var item = GetFlexItem(child);
			_root.Remove(item);

			switch (child)
			{
				case BindableObject bo:
					bo.ClearValue(FlexItemProperty);
					break;
				default:
					_viewInfo.Remove(child);
					break;
			}
		}

		protected override ILayoutManager CreateLayoutManager()
		{
			return new FlexLayoutManager(this);
		}

		public Graphics.Rect GetFlexFrame(IView view) =>
			GetFlexItem(view).GetFrame();

		void EnsureFlexItemPropertiesUpdated()
		{
			for (int n = 0; n < this.Count; n++)
			{
				var child = this[n];
				var flexItem = GetFlexItem(child);

				InitItemProperties(child, flexItem);
			}
		}

		public void Layout(double width, double height)
		{
			if (_root.Parent != null)   //Layout is only computed at root level
				return;

			var useMeasureHack = NeedsMeasureHack(width, height);
			if (useMeasureHack)
			{
				PrepareMeasureHack();
			}

			EnsureFlexItemPropertiesUpdated();

			_root.Width = !double.IsPositiveInfinity((width)) ? (float)width : 0;
			_root.Height = !double.IsPositiveInfinity((height)) ? (float)height : 0;
			_root.Layout();

			if (useMeasureHack)
			{
				RestoreValues();
			}
		}

		protected override void OnParentSet()
		{
			base.OnParentSet();
			if (Parent != null && _root == null)
				PopulateLayout();
			else if (Parent == null && _root != null)
				ClearLayout();
		}

		void PopulateLayout()
		{
			InitLayoutProperties(_root = new Flex.Item());
			for (var i = 0; i < Children.Count; i++)
			{
				AddFlexItem(i, Children[i]);
			}
		}

		void ClearLayout()
		{
			foreach (var child in Children)
				RemoveFlexItem(child);
			_root = null;
		}

		void InitLayoutProperties(Flex.Item item)
		{
			item.AlignContent = (Flex.AlignContent)(FlexAlignContent)GetValue(AlignContentProperty);
			item.AlignItems = (Flex.AlignItems)(FlexAlignItems)GetValue(AlignItemsProperty);
			item.Direction = (Flex.Direction)(FlexDirection)GetValue(DirectionProperty);
			item.JustifyContent = (Flex.Justify)(FlexJustify)GetValue(JustifyContentProperty);
			item.Wrap = (Flex.Wrap)(FlexWrap)GetValue(WrapProperty);
		}

		protected override void OnAdd(int index, IView view)
		{
			AddFlexItem(index, view);
			base.OnAdd(index, view);
		}

		protected override void OnInsert(int index, IView view)
		{
			AddFlexItem(index, view);
			base.OnInsert(index, view);
		}

		protected override void OnUpdate(int index, IView view, IView oldView)
		{
			RemoveFlexItem(oldView);
			AddFlexItem(index, view);
			base.OnUpdate(index, view, oldView);
		}

		protected override void OnRemove(int index, IView view)
		{
			base.OnRemove(index, view);
			RemoveFlexItem(view);
		}

		protected override void OnClear()
		{
			base.OnClear();
			ClearLayout();
			PopulateLayout();
		}

		static bool NeedsMeasureHack(double widthConstraint, double heightConstraint)
		{
			return double.IsInfinity(widthConstraint) || double.IsInfinity(heightConstraint);
		}

		void PrepareMeasureHack()
		{
			// FlexLayout's Shrink and Stretch features require a fixed area to measure/layout correctly;
			// when the dimensions they are working in are infinite, they don't really make sense. We can
			// get a sensible measure by temporarily setting the Shrink values of all items to 0 and the 
			// Stretch alignment values to Start. So we prepare for that here.

			foreach (var child in Children)
			{
				if (GetFlexItem(child) is Flex.Item item)
				{
					item.Shrink = 0;
					item.AlignSelf = Flex.AlignSelf.Start;
				}
			}
		}

		void RestoreValues()
		{
			// If we had to modify the Shrink and Stretch values of the FlexItems for measurement, we 
			// restore them to their original values.

			foreach (var child in Children)
			{
				if (GetFlexItem(child) is Flex.Item item)
				{
					item.Shrink = GetShrink(child);
					item.AlignSelf = (Flex.AlignSelf)GetAlignSelf(child);
				}
			}
		}
	}
}
