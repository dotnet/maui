#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls.Compatibility
{
	[ContentProperty(nameof(Children))]
	[Obsolete("Use Microsoft.Maui.Controls.AbsoluteLayout instead. For more information, see https://learn.microsoft.com/dotnet/maui/migration/layouts")]
	public class AbsoluteLayout : Layout<View>, IElementConfiguration<AbsoluteLayout>
	{
		/// <summary>Bindable property for attached property <c>LayoutFlags</c>.</summary>
		public static readonly BindableProperty LayoutFlagsProperty = BindableProperty.CreateAttached("LayoutFlags", typeof(AbsoluteLayoutFlags), typeof(AbsoluteLayout), AbsoluteLayoutFlags.None);

		/// <summary>Bindable property for attached property <c>LayoutBounds</c>.</summary>
		public static readonly BindableProperty LayoutBoundsProperty = BindableProperty.CreateAttached("LayoutBounds", typeof(Rect), typeof(AbsoluteLayout), new Rect(0, 0, AutoSize, AutoSize));

		readonly AbsoluteElementCollection _children;
		readonly Lazy<PlatformConfigurationRegistry<AbsoluteLayout>> _platformConfigurationRegistry;

		public AbsoluteLayout()
		{
			Hosting.CompatibilityCheck.CheckForCompatibility();
			_children = new AbsoluteElementCollection(InternalChildren, this);
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<AbsoluteLayout>>(() =>
				new PlatformConfigurationRegistry<AbsoluteLayout>(this));
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, AbsoluteLayout> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		public static double AutoSize => -1;

		public new IAbsoluteList<View> Children
		{
			get { return _children; }
		}

		[System.ComponentModel.TypeConverter(typeof(BoundsTypeConverter))]
		public static Rect GetLayoutBounds(BindableObject bindable)
		{
			return (Rect)bindable.GetValue(LayoutBoundsProperty);
		}

		public static AbsoluteLayoutFlags GetLayoutFlags(BindableObject bindable)
		{
			return (AbsoluteLayoutFlags)bindable.GetValue(LayoutFlagsProperty);
		}

		public static void SetLayoutBounds(BindableObject bindable, Rect bounds)
		{
			bindable.SetValue(LayoutBoundsProperty, bounds);
		}

		public static void SetLayoutFlags(BindableObject bindable, AbsoluteLayoutFlags flags)
		{
			bindable.SetValue(LayoutFlagsProperty, flags);
		}

#pragma warning disable CS0672 // Member overrides obsolete member
		protected override void LayoutChildren(double x, double y, double width, double height)
#pragma warning restore CS0672 // Member overrides obsolete member
		{
			foreach (View child in LogicalChildrenInternal)
			{
				Rect rect = ComputeLayoutForRegion(child, new Size(width, height));
				rect.X += x;
				rect.Y += y;

				LayoutChildIntoBoundingRegion(child, rect);
			}
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);
			child.PropertyChanged += ChildOnPropertyChanged;
		}

		protected override void OnChildRemoved(Element child, int oldLogicalIndex)
		{
			child.PropertyChanged -= ChildOnPropertyChanged;
			base.OnChildRemoved(child, oldLogicalIndex);
		}

		[Obsolete("Use MeasureOverride instead")]
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			var bestFitSize = new Size();
			var minimum = new Size();
			foreach (View child in LogicalChildrenInternal)
			{
				SizeRequest desiredSize = ComputeBoundingRegionDesiredSize(child);

				bestFitSize.Width = Math.Max(bestFitSize.Width, desiredSize.Request.Width);
				bestFitSize.Height = Math.Max(bestFitSize.Height, desiredSize.Request.Height);
				minimum.Width = Math.Max(minimum.Width, desiredSize.Minimum.Width);
				minimum.Height = Math.Max(minimum.Height, desiredSize.Minimum.Height);
			}

			return new SizeRequest(bestFitSize, minimum);
		}

		protected override LayoutConstraint ComputeConstraintForView(View view)
		{
			AbsoluteLayoutFlags layoutFlags = GetLayoutFlags(view);

			if ((layoutFlags & AbsoluteLayoutFlags.SizeProportional) == AbsoluteLayoutFlags.SizeProportional)
			{
				if (view.VerticalOptions.Alignment == LayoutAlignment.Fill &&
					view.HorizontalOptions.Alignment == LayoutAlignment.Fill)
					return Constraint;

				return LayoutConstraint.None;
			}

			var result = LayoutConstraint.None;
			Rect layoutBounds = GetLayoutBounds(view);
			if ((layoutFlags & AbsoluteLayoutFlags.HeightProportional) != 0)
			{
				bool widthLocked = layoutBounds.Width != AutoSize;
				result = Constraint & LayoutConstraint.VerticallyFixed;
				if (widthLocked)
					result |= LayoutConstraint.HorizontallyFixed;
			}
			else if ((layoutFlags & AbsoluteLayoutFlags.WidthProportional) != 0)
			{
				bool heightLocked = layoutBounds.Height != AutoSize;
				result = Constraint & LayoutConstraint.HorizontallyFixed;
				if (heightLocked)
					result |= LayoutConstraint.VerticallyFixed;
			}
			else
			{
				if (layoutBounds.Width != AutoSize)
					result |= LayoutConstraint.HorizontallyFixed;
				if (layoutBounds.Height != AutoSize)
					result |= LayoutConstraint.VerticallyFixed;
			}

			return result;
		}

		void ChildOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == LayoutFlagsProperty.PropertyName || e.PropertyName == LayoutBoundsProperty.PropertyName)
			{
				InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
				UpdateChildrenLayout();
			}
		}

		static SizeRequest ComputeBoundingRegionDesiredSize(View view)
		{
			var width = 0.0;
			var height = 0.0;

			var sizeRequest = new Lazy<SizeRequest>(() => view.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins));

			Rect bounds = GetLayoutBounds(view);
			AbsoluteLayoutFlags absFlags = GetLayoutFlags(view);
			bool widthIsProportional = (absFlags & AbsoluteLayoutFlags.WidthProportional) != 0;
			bool heightIsProportional = (absFlags & AbsoluteLayoutFlags.HeightProportional) != 0;
			bool xIsProportional = (absFlags & AbsoluteLayoutFlags.XProportional) != 0;
			bool yIsProportional = (absFlags & AbsoluteLayoutFlags.YProportional) != 0;

			// add in required x values
			if (!xIsProportional)
			{
				width += bounds.X;
			}

			if (!yIsProportional)
			{
				height += bounds.Y;
			}

			double minWidth = width;
			double minHeight = height;

			if (!widthIsProportional && bounds.Width != AutoSize)
			{
				// fixed size
				width += bounds.Width;
				minWidth += bounds.Width;
			}
			else if (!widthIsProportional)
			{
				// auto size
				width += sizeRequest.Value.Request.Width;
				minWidth += sizeRequest.Value.Minimum.Width;
			}
			else
			{
				// proportional size
				width += sizeRequest.Value.Request.Width / Math.Max(0.25, bounds.Width);
				//minWidth += 0;
			}

			if (!heightIsProportional && bounds.Height != AutoSize)
			{
				// fixed size
				height += bounds.Height;
				minHeight += bounds.Height;
			}
			else if (!heightIsProportional)
			{
				// auto size
				height += sizeRequest.Value.Request.Height;
				minHeight += sizeRequest.Value.Minimum.Height;
			}
			else
			{
				// proportional size
				height += sizeRequest.Value.Request.Height / Math.Max(0.25, bounds.Height);
				//minHeight += 0;
			}

			return new SizeRequest(new Size(width, height), new Size(minWidth, minHeight));
		}

		static Rect ComputeLayoutForRegion(View view, Size region)
		{
			var result = new Rect();

			SizeRequest sizeRequest;
			Rect bounds = GetLayoutBounds(view);
			AbsoluteLayoutFlags absFlags = GetLayoutFlags(view);
			bool widthIsProportional = (absFlags & AbsoluteLayoutFlags.WidthProportional) != 0;
			bool heightIsProportional = (absFlags & AbsoluteLayoutFlags.HeightProportional) != 0;
			bool xIsProportional = (absFlags & AbsoluteLayoutFlags.XProportional) != 0;
			bool yIsProportional = (absFlags & AbsoluteLayoutFlags.YProportional) != 0;

			if (widthIsProportional)
			{
				result.Width = DeviceDisplay.MainDisplayInfo.DisplayRound(region.Width * bounds.Width);
			}
			else if (bounds.Width != AutoSize)
			{
				result.Width = bounds.Width;
			}

			if (heightIsProportional)
			{
				result.Height = DeviceDisplay.MainDisplayInfo.DisplayRound(region.Height * bounds.Height);
			}
			else if (bounds.Height != AutoSize)
			{
				result.Height = bounds.Height;
			}

			if (!widthIsProportional && bounds.Width == AutoSize)
			{
				if (!heightIsProportional && bounds.Width == AutoSize)
				{
					// Width and Height are auto
					sizeRequest = view.Measure(region.Width, region.Height, MeasureFlags.IncludeMargins);
					result.Width = sizeRequest.Request.Width;
					result.Height = sizeRequest.Request.Height;
				}
				else
				{
					// Only width is auto
					sizeRequest = view.Measure(region.Width, result.Height, MeasureFlags.IncludeMargins);
					result.Width = sizeRequest.Request.Width;
				}
			}
			else if (!heightIsProportional && bounds.Height == AutoSize)
			{
				// Only height is auto
				sizeRequest = view.Measure(result.Width, region.Height, MeasureFlags.IncludeMargins);
				result.Height = sizeRequest.Request.Height;
			}

			if (xIsProportional)
			{
				result.X = Math.Round((region.Width - result.Width) * bounds.X);
			}
			else
			{
				result.X = bounds.X;
			}

			if (yIsProportional)
			{
				result.Y = Math.Round((region.Height - result.Height) * bounds.Y);
			}
			else
			{
				result.Y = bounds.Y;
			}

			return result;
		}

		public interface IAbsoluteList<T> : IList<T> where T : View
		{
			void Add(View view, Rect bounds, AbsoluteLayoutFlags flags = AbsoluteLayoutFlags.None);

			void Add(View view, Point position);
		}

		class AbsoluteElementCollection : ElementCollection<View>, IAbsoluteList<View>
		{
			public AbsoluteElementCollection(ObservableCollection<Element> inner, AbsoluteLayout parent) : base(inner)
			{
				Parent = parent;
			}

			internal AbsoluteLayout Parent { get; set; }

			public void Add(View view, Rect bounds, AbsoluteLayoutFlags flags = AbsoluteLayoutFlags.None)
			{
				SetLayoutBounds(view, bounds);
				SetLayoutFlags(view, flags);
				Add(view);
			}

			public void Add(View view, Point position)
			{
				SetLayoutBounds(view, new Rect(position.X, position.Y, AutoSize, AutoSize));
				Add(view);
			}
		}
	}
}