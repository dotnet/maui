using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Layouts;
using Flex = Microsoft.Maui.Layouts.Flex;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="Type[@FullName='Microsoft.Maui.Controls.FlexLayout']/Docs" />
	[ContentProperty(nameof(Children))]
	public class FlexLayout : Layout, IFlexLayout
	{
		Flex.Item _root;

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='DirectionProperty']/Docs" />
		public static readonly BindableProperty DirectionProperty =
			BindableProperty.Create(nameof(Direction), typeof(FlexDirection), typeof(FlexLayout), FlexDirection.Row,
									propertyChanged: OnDirectionPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='JustifyContentProperty']/Docs" />
		public static readonly BindableProperty JustifyContentProperty =
			BindableProperty.Create(nameof(JustifyContent), typeof(FlexJustify), typeof(FlexLayout), FlexJustify.Start,
									propertyChanged: OnJustifyContentPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='AlignContentProperty']/Docs" />
		public static readonly BindableProperty AlignContentProperty =
			BindableProperty.Create(nameof(AlignContent), typeof(FlexAlignContent), typeof(FlexLayout), FlexAlignContent.Stretch,
									propertyChanged: OnAlignContentPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='AlignItemsProperty']/Docs" />
		public static readonly BindableProperty AlignItemsProperty =
			BindableProperty.Create(nameof(AlignItems), typeof(FlexAlignItems), typeof(FlexLayout), FlexAlignItems.Stretch,
									propertyChanged: OnAlignItemsPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='PositionProperty']/Docs" />
		public static readonly BindableProperty PositionProperty =
			BindableProperty.Create(nameof(Position), typeof(FlexPosition), typeof(FlexLayout), FlexPosition.Relative,
									propertyChanged: OnPositionPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='WrapProperty']/Docs" />
		public static readonly BindableProperty WrapProperty =
			BindableProperty.Create(nameof(Wrap), typeof(FlexWrap), typeof(FlexLayout), FlexWrap.NoWrap,
									propertyChanged: OnWrapPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='OrderProperty']/Docs" />
		public static readonly BindableProperty OrderProperty =
			BindableProperty.CreateAttached("Order", typeof(int), typeof(FlexLayout), default(int),
											propertyChanged: OnOrderPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GrowProperty']/Docs" />
		public static readonly BindableProperty GrowProperty =
			BindableProperty.CreateAttached("Grow", typeof(float), typeof(FlexLayout), default(float),
											propertyChanged: OnGrowPropertyChanged, validateValue: (bindable, value) => (float)value >= 0);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='ShrinkProperty']/Docs" />
		public static readonly BindableProperty ShrinkProperty =
			BindableProperty.CreateAttached("Shrink", typeof(float), typeof(FlexLayout), 1f,
											propertyChanged: OnShrinkPropertyChanged, validateValue: (bindable, value) => (float)value >= 0);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='AlignSelfProperty']/Docs" />
		public static readonly BindableProperty AlignSelfProperty =
			BindableProperty.CreateAttached("AlignSelf", typeof(FlexAlignSelf), typeof(FlexLayout), FlexAlignSelf.Auto,
											propertyChanged: OnAlignSelfPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='BasisProperty']/Docs" />
		public static readonly BindableProperty BasisProperty =
			BindableProperty.CreateAttached("Basis", typeof(FlexBasis), typeof(FlexLayout), FlexBasis.Auto,
											propertyChanged: OnBasisPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='Direction']/Docs" />
		public FlexDirection Direction
		{
			get => (FlexDirection)GetValue(DirectionProperty);
			set => SetValue(DirectionProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='JustifyContent']/Docs" />
		public FlexJustify JustifyContent
		{
			get => (FlexJustify)GetValue(JustifyContentProperty);
			set => SetValue(JustifyContentProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='AlignContent']/Docs" />
		public FlexAlignContent AlignContent
		{
			get => (FlexAlignContent)GetValue(AlignContentProperty);
			set => SetValue(AlignContentProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='AlignItems']/Docs" />
		public FlexAlignItems AlignItems
		{
			get => (FlexAlignItems)GetValue(AlignItemsProperty);
			set => SetValue(AlignItemsProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='Position']/Docs" />
		public FlexPosition Position
		{
			get => (FlexPosition)GetValue(PositionProperty);
			set => SetValue(PositionProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='Wrap']/Docs" />
		public FlexWrap Wrap
		{
			get => (FlexWrap)GetValue(WrapProperty);
			set => SetValue(WrapProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetOrder'][1]/Docs" />
		public static int GetOrder(BindableObject bindable)
			=> (int)bindable.GetValue(OrderProperty);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='SetOrder'][1]/Docs" />
		public static void SetOrder(BindableObject bindable, int value)
			=> bindable.SetValue(OrderProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetGrow'][1]/Docs" />
		public static float GetGrow(BindableObject bindable)
			=> (float)bindable.GetValue(GrowProperty);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='SetGrow'][1]/Docs" />
		public static void SetGrow(BindableObject bindable, float value)
			=> bindable.SetValue(GrowProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetShrink'][1]/Docs" />
		public static float GetShrink(BindableObject bindable)
			=> (float)bindable.GetValue(ShrinkProperty);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='SetShrink'][1]/Docs" />
		public static void SetShrink(BindableObject bindable, float value)
			=> bindable.SetValue(ShrinkProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetAlignSelf'][1]/Docs" />
		public static FlexAlignSelf GetAlignSelf(BindableObject bindable)
			=> (FlexAlignSelf)bindable.GetValue(AlignSelfProperty);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='SetAlignSelf'][1]/Docs" />
		public static void SetAlignSelf(BindableObject bindable, FlexAlignSelf value)
			=> bindable.SetValue(AlignSelfProperty, value);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetBasis'][1]/Docs" />
		public static FlexBasis GetBasis(BindableObject bindable)
			=> (FlexBasis)bindable.GetValue(BasisProperty);

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='SetBasis'][1]/Docs" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetOrder'][2]/Docs" />
		public int GetOrder(IView view)
		{
			return view switch
			{
				BindableObject bo => (int)bo.GetValue(OrderProperty),
				_ => _viewInfo[view].Order,
			};
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='SetOrder'][2]/Docs" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetGrow'][2]/Docs" />
		public float GetGrow(IView view)
		{
			return view switch
			{
				BindableObject bo => (float)bo.GetValue(GrowProperty),
				_ => _viewInfo[view].Grow,
			};
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='SetGrow'][2]/Docs" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetShrink'][2]/Docs" />
		public float GetShrink(IView view)
		{
			return view switch
			{
				BindableObject bo => (float)bo.GetValue(ShrinkProperty),
				_ => _viewInfo[view].Shrink,
			};
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='SetShrink'][2]/Docs" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetAlignSelf'][2]/Docs" />
		public FlexAlignSelf GetAlignSelf(IView view)
		{
			return view switch
			{
				BindableObject bo => (FlexAlignSelf)bo.GetValue(AlignSelfProperty),
				_ => _viewInfo[view].AlignSelf,
			};
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='SetAlignSelf'][2]/Docs" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetBasis'][2]/Docs" />
		public FlexBasis GetBasis(IView view)
		{
			return view switch
			{
				BindableObject bo => (FlexBasis)bo.GetValue(BasisProperty),
				_ => _viewInfo[view].Basis,
			};
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='SetBasis'][2]/Docs" />
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

			// TODO ezhart The Core layout interfaces don't have the padding property yet; when that's available, we should add a check for it here
			if (view is FlexLayout && view is Controls.Layout layout)
			{
				var (pleft, ptop, pright, pbottom) = (Thickness)layout.GetValue(Compatibility.Layout.PaddingProperty);
				item.PaddingLeft = (float)pleft;
				item.PaddingTop = (float)ptop;
				item.PaddingRight = (float)pright;
				item.PaddingBottom = (float)pbottom;
			}
		}

		void AddFlexItem(IView child)
		{
			if (_root == null)
				return;
			var item = (child as FlexLayout)?._root ?? new Flex.Item();
			InitItemProperties(child, item);
			if (!(child is FlexLayout))
			{ //inner layouts don't get measured
				item.SelfSizing = (Flex.Item it, ref float w, ref float h) =>
				{
					var sizeConstraints = item.GetConstraints();
					sizeConstraints.Width = (sizeConstraints.Width == 0) ? double.PositiveInfinity : sizeConstraints.Width;
					sizeConstraints.Height = (sizeConstraints.Height == 0) ? double.PositiveInfinity : sizeConstraints.Height;
					var request = child.Measure(sizeConstraints.Width, sizeConstraints.Height);
					w = (float)request.Width;
					h = (float)request.Height;
				};
			}

			_root.InsertAt(Children.IndexOf(child), item);
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='GetFlexFrame']/Docs" />
		public Graphics.Rect GetFlexFrame(IView view)
		{
			return view switch
			{
				BindableObject bo => ((Flex.Item)bo.GetValue(FlexItemProperty)).GetFrame(),
				_ => _viewInfo[view].FlexItem.GetFrame(),
			};
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/FlexLayout.xml" path="//Member[@MemberName='Layout']/Docs" />
		public void Layout(double width, double height)
		{
			if (_root.Parent != null)   //Layout is only computed at root level
				return;
			_root.Width = !double.IsPositiveInfinity((width)) ? (float)width : 0;
			_root.Height = !double.IsPositiveInfinity((height)) ? (float)height : 0;
			_root.Layout();
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
			foreach (var child in Children)
				AddFlexItem(child);
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
			base.OnAdd(index, view);
			AddFlexItem(view);
		}

		protected override void OnInsert(int index, IView view)
		{
			base.OnInsert(index, view);
			AddFlexItem(view);
		}

		protected override void OnUpdate(int index, IView view, IView oldView)
		{
			base.OnUpdate(index, view, oldView);
			RemoveFlexItem(oldView);
			AddFlexItem(view);
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
	}
}
