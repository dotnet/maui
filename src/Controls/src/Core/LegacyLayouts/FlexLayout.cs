#nullable disable
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Flex = Microsoft.Maui.Layouts.Flex;

namespace Microsoft.Maui.Controls.Compatibility
{
	[ContentProperty(nameof(Children))]
	[Obsolete("Use Microsoft.Maui.Controls.FlexLayout instead. For more information, see https://learn.microsoft.com/dotnet/maui/migration/layouts")]
	public class FlexLayout : Layout<View>
	{
		/// <summary>Bindable property for <see cref="Direction"/>.</summary>
		public static readonly BindableProperty DirectionProperty =
			BindableProperty.Create(nameof(Direction), typeof(FlexDirection), typeof(FlexLayout), FlexDirection.Row,
									propertyChanged: OnDirectionPropertyChanged);

		/// <summary>Bindable property for <see cref="JustifyContent"/>.</summary>
		public static readonly BindableProperty JustifyContentProperty =
			BindableProperty.Create(nameof(JustifyContent), typeof(FlexJustify), typeof(FlexLayout), FlexJustify.Start,
									propertyChanged: OnJustifyContentPropertyChanged);

		/// <summary>Bindable property for <see cref="AlignContent"/>.</summary>
		public static readonly BindableProperty AlignContentProperty =
			BindableProperty.Create(nameof(AlignContent), typeof(FlexAlignContent), typeof(FlexLayout), FlexAlignContent.Stretch,
									propertyChanged: OnAlignContentPropertyChanged);

		/// <summary>Bindable property for <see cref="AlignItems"/>.</summary>
		public static readonly BindableProperty AlignItemsProperty =
			BindableProperty.Create(nameof(AlignItems), typeof(FlexAlignItems), typeof(FlexLayout), FlexAlignItems.Stretch,
									propertyChanged: OnAlignItemsPropertyChanged);

		/// <summary>Bindable property for <see cref="Position"/>.</summary>
		public static readonly BindableProperty PositionProperty =
			BindableProperty.Create(nameof(Position), typeof(FlexPosition), typeof(FlexLayout), FlexPosition.Relative,
									propertyChanged: OnPositionPropertyChanged);

		/// <summary>Bindable property for <see cref="Wrap"/>.</summary>
		public static readonly BindableProperty WrapProperty =
			BindableProperty.Create(nameof(Wrap), typeof(FlexWrap), typeof(FlexLayout), FlexWrap.NoWrap,
									propertyChanged: OnWrapPropertyChanged);

		static readonly BindableProperty FlexItemProperty =
			BindableProperty.CreateAttached("FlexItem", typeof(Flex.Item), typeof(FlexLayout), null);

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

		public FlexLayout() =>
			Hosting.CompatibilityCheck.CheckForCompatibility();

		public FlexDirection Direction
		{
			get => (FlexDirection)GetValue(DirectionProperty);
			set => SetValue(DirectionProperty, value);
		}

		public FlexJustify JustifyContent
		{
			get => (FlexJustify)GetValue(JustifyContentProperty);
			set => SetValue(JustifyContentProperty, value);
		}

		public FlexAlignContent AlignContent
		{
			get => (FlexAlignContent)GetValue(AlignContentProperty);
			set => SetValue(AlignContentProperty, value);
		}

		public FlexAlignItems AlignItems
		{
			get => (FlexAlignItems)GetValue(AlignItemsProperty);
			set => SetValue(AlignItemsProperty, value);
		}

		public FlexPosition Position
		{
			get => (FlexPosition)GetValue(PositionProperty);
			set => SetValue(PositionProperty, value);
		}

		public FlexWrap Wrap
		{
			get => (FlexWrap)GetValue(WrapProperty);
			set => SetValue(WrapProperty, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static Flex.Item GetFlexItem(BindableObject bindable)
			=> (Flex.Item)bindable.GetValue(FlexItemProperty);

		static void SetFlexItem(BindableObject bindable, Flex.Item node)
			=> bindable.SetValue(FlexItemProperty, node);

		public static int GetOrder(BindableObject bindable)
			=> (int)bindable.GetValue(OrderProperty);

		public static void SetOrder(BindableObject bindable, int value)
			=> bindable.SetValue(OrderProperty, value);

		public static float GetGrow(BindableObject bindable)
			=> (float)bindable.GetValue(GrowProperty);

		public static void SetGrow(BindableObject bindable, float value)
			=> bindable.SetValue(GrowProperty, value);

		public static float GetShrink(BindableObject bindable)
			=> (float)bindable.GetValue(ShrinkProperty);

		public static void SetShrink(BindableObject bindable, float value)
			=> bindable.SetValue(ShrinkProperty, value);

		public static FlexAlignSelf GetAlignSelf(BindableObject bindable)
			=> (FlexAlignSelf)bindable.GetValue(AlignSelfProperty);

		public static void SetAlignSelf(BindableObject bindable, FlexAlignSelf value)
			=> bindable.SetValue(AlignSelfProperty, value);

		public static FlexBasis GetBasis(BindableObject bindable)
			=> (FlexBasis)bindable.GetValue(BasisProperty);

		public static void SetBasis(BindableObject bindable, FlexBasis value)
			=> bindable.SetValue(BasisProperty, value);

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
			flexLayout.InvalidateLayout();
		}

		static void OnJustifyContentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var flexLayout = bindable as FlexLayout;
			if (flexLayout._root == null)
				return;
			flexLayout._root.JustifyContent = (Flex.Justify)(FlexJustify)newValue;
			flexLayout.InvalidateLayout();
		}

		static void OnAlignContentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var flexLayout = bindable as FlexLayout;
			if (flexLayout._root == null)
				return;
			flexLayout._root.AlignContent = (Flex.AlignContent)(FlexAlignContent)newValue;
			flexLayout.InvalidateLayout();
		}

		static void OnAlignItemsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var flexLayout = (FlexLayout)bindable;
			if (flexLayout._root == null)
				return;
			flexLayout._root.AlignItems = (Flex.AlignItems)(FlexAlignItems)newValue;
			flexLayout.InvalidateLayout();
		}

		static void OnPositionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var flexLayout = (FlexLayout)bindable;
			if (flexLayout._root == null)
				return;
			flexLayout._root.Position = (Flex.Position)(FlexPosition)newValue;
			flexLayout.InvalidateLayout();
		}

		static void OnWrapPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var flexLayout = bindable as FlexLayout;
			if (flexLayout._root == null)
				return;
			flexLayout._root.Wrap = (Flex.Wrap)(FlexWrap)newValue;
			flexLayout.InvalidateLayout();
		}

		Flex.Item _root;
		//this should only be used in unitTests. layout creation will normally happen on OnParentSet
		internal override void OnIsPlatformEnabledChanged()
		{
			base.OnIsPlatformEnabledChanged();
			if (IsPlatformEnabled && _root == null)
				PopulateLayout();
			else if (!IsPlatformEnabled && _root != null)
				ClearLayout();
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
				AddChild(child);
		}

		void InitLayoutProperties(Flex.Item item)
		{
			item.AlignContent = (Flex.AlignContent)(FlexAlignContent)GetValue(AlignContentProperty);
			item.AlignItems = (Flex.AlignItems)(FlexAlignItems)GetValue(AlignItemsProperty);
			item.Direction = (Flex.Direction)(FlexDirection)GetValue(DirectionProperty);
			item.JustifyContent = (Flex.Justify)(FlexJustify)GetValue(JustifyContentProperty);
			item.Wrap = (Flex.Wrap)(FlexWrap)GetValue(WrapProperty);
		}

		void ClearLayout()
		{
			foreach (var child in Children)
				RemoveChild(child);
			_root = null;
		}

		protected override void OnAdded(View view)
		{
			AddChild(view);
			view.PropertyChanged += OnChildPropertyChanged;
			base.OnAdded(view);
		}

		protected override void OnRemoved(View view)
		{
			view.PropertyChanged -= OnChildPropertyChanged;
			RemoveChild(view);
			base.OnRemoved(view);
		}

		void AddChild(View view)
		{
			if (_root == null)
				return;
			var item = (view as FlexLayout)?._root ?? new Flex.Item();
			InitItemProperties(view, item);
			if (!(view is FlexLayout))
			{ //inner layouts don't get measured
				item.SelfSizing = (Flex.Item it, ref float w, ref float h, bool inMeasureMode) =>
				{
					var sizeConstrains = item.GetConstraints();
					sizeConstrains.Width = (inMeasureMode && sizeConstrains.Width == 0) ? double.PositiveInfinity : sizeConstrains.Width;
					sizeConstrains.Height = (inMeasureMode && sizeConstrains.Height == 0) ? double.PositiveInfinity : sizeConstrains.Height;
					var request = view.Measure(sizeConstrains.Width, sizeConstrains.Height, MeasureFlags.None).Request;
					w = (float)request.Width;
					h = (float)request.Height;
				};
			}

			_root.InsertAt(Children.IndexOf(view), item);
			SetFlexItem(view, item);
		}

		void InitItemProperties(View view, Flex.Item item)
		{
			item.Order = (int)view.GetValue(OrderProperty);
			item.Grow = (float)view.GetValue(GrowProperty);
			item.Shrink = (float)view.GetValue(ShrinkProperty);
			item.Basis = ((FlexBasis)view.GetValue(BasisProperty)).ToFlexBasis();
			item.AlignSelf = (Flex.AlignSelf)(FlexAlignSelf)view.GetValue(AlignSelfProperty);
			var (mleft, mtop, mright, mbottom) = (Thickness)view.GetValue(MarginProperty);
			item.MarginLeft = (float)mleft;
			item.MarginTop = (float)mtop;
			item.MarginRight = (float)mright;
			item.MarginBottom = (float)mbottom;
			var width = (double)view.GetValue(WidthRequestProperty);
			item.Width = width < 0 ? float.NaN : (float)width;
			var height = (double)view.GetValue(HeightRequestProperty);
			item.Height = height < 0 ? float.NaN : (float)height;
			item.IsVisible = (bool)view.GetValue(IsVisibleProperty);
			if (view is FlexLayout)
			{
				var (pleft, ptop, pright, pbottom) = (Thickness)view.GetValue(PaddingProperty);
				item.PaddingLeft = (float)pleft;
				item.PaddingTop = (float)ptop;
				item.PaddingRight = (float)pright;
				item.PaddingBottom = (float)pbottom;
			}
		}

		void RemoveChild(View view)
		{
			if (_root == null)
				return;
			var item = GetFlexItem(view);
			_root.Remove(item);
			view.ClearValue(FlexItemProperty);
		}

		void OnChildPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == WidthRequestProperty.PropertyName
				|| e.PropertyName == HeightRequestProperty.PropertyName)
			{
				var item = (sender as FlexLayout)?._root ?? GetFlexItem((BindableObject)sender);
				if (item == null)
					return;
				item.Width = ((View)sender).WidthRequest < 0 ? float.NaN : (float)((View)sender).WidthRequest;
				item.Height = ((View)sender).HeightRequest < 0 ? float.NaN : (float)((View)sender).HeightRequest;
				InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
				return;
			}

			if (e.PropertyName == MarginProperty.PropertyName)
			{
				var item = (sender as FlexLayout)?._root ?? GetFlexItem((BindableObject)sender);
				if (item == null)
					return;
				var margin = (Thickness)((View)sender).GetValue(MarginProperty);
				item.MarginLeft = (float)margin.Left;
				item.MarginTop = (float)margin.Top;
				item.MarginRight = (float)margin.Right;
				item.MarginBottom = (float)margin.Bottom;
				InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
				return;
			}

			if (e.PropertyName == PaddingProperty.PropertyName)
			{
				var item = (sender as FlexLayout)?._root ?? GetFlexItem((BindableObject)sender);
				if (item == null)
					return;
				var padding = (Thickness)((View)sender).GetValue(PaddingProperty);
				item.PaddingLeft = (float)padding.Left;
				item.PaddingTop = (float)padding.Top;
				item.PaddingRight = (float)padding.Right;
				item.PaddingBottom = (float)padding.Bottom;
				InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
				return;
			}

			if (e.PropertyName == IsVisibleProperty.PropertyName)
			{
				var item = (sender as FlexLayout)?._root ?? GetFlexItem((BindableObject)sender);
				if (item == null)
					return;
				item.IsVisible = (bool)((View)sender).GetValue(IsVisibleProperty);
				InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
				return;
			}
		}

#pragma warning disable CS0672 // Member overrides obsolete member
		protected override void LayoutChildren(double x, double y, double width, double height)
#pragma warning restore CS0672 // Member overrides obsolete member
		{
			if (_root == null)
				return;

			Layout(width, height);
			foreach (var child in Children)
			{
				var frame = GetFlexItem(child).GetFrame();
				if (double.IsNaN(frame.X)
					|| double.IsNaN(frame.Y)
					|| double.IsNaN(frame.Width)
					|| double.IsNaN(frame.Height))
					throw new Exception("something is deeply wrong");
				frame = frame.Offset(x, y); //flex doesn't support offset on _root
				child.Layout(frame);
			}
		}

		bool _measuring;
#pragma warning disable CS0672 // Member overrides obsolete member
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
#pragma warning restore CS0672 // Member overrides obsolete member
			if (_root == null)
				return new SizeRequest(new Size(widthConstraint, heightConstraint));

			//All of this is a HACK as X.Flex doesn't supports measuring
			if (!double.IsPositiveInfinity(widthConstraint) && !double.IsPositiveInfinity(heightConstraint))
				return new SizeRequest(new Size(widthConstraint, heightConstraint));

			_measuring = true;
			//1. Set Shrink to 0, set align-self to start (to avoid stretching)
			//   Set Image.Aspect to Fill to get the value we expect in measuring
			foreach (var child in Children)
			{
				if (GetFlexItem(child) is Flex.Item item)
				{
					item.Shrink = 0;
					item.AlignSelf = Flex.AlignSelf.Start;
				}
			}
			Layout(widthConstraint, heightConstraint);

			//2. look at the children location
			if (double.IsPositiveInfinity(widthConstraint))
			{
				widthConstraint = 0;
				foreach (var item in _root)
					widthConstraint = Math.Max(widthConstraint, item.Frame[0] + item.Frame[2] + item.MarginRight);
			}
			if (double.IsPositiveInfinity(heightConstraint))
			{
				heightConstraint = 0;
				foreach (var item in _root)
					heightConstraint = Math.Max(heightConstraint, item.Frame[1] + item.Frame[3] + item.MarginBottom);
			}

			//3. reset Shrink, align-self, and image.aspect
			foreach (var child in Children)
			{
				if (GetFlexItem(child) is Flex.Item item)
				{
					item.Shrink = (float)child.GetValue(ShrinkProperty);
					item.AlignSelf = (Flex.AlignSelf)(FlexAlignSelf)child.GetValue(AlignSelfProperty);
				}
			}
			_measuring = false;
			return new SizeRequest(new Size(widthConstraint, heightConstraint));
		}

		void Layout(double width, double height)
		{
			if (_root.Parent != null)   //Layout is only computed at root level
				return;
			_root.Width = !double.IsPositiveInfinity((width)) ? (float)width : 0;
			_root.Height = !double.IsPositiveInfinity((height)) ? (float)height : 0;
			_root.Layout(_measuring);
		}
	}
}