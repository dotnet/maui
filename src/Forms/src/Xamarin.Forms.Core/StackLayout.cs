using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public class StackLayout : Layout<View>, IElementConfiguration<StackLayout>
	{
		public static readonly BindableProperty OrientationProperty = BindableProperty.Create(nameof(Orientation), typeof(StackOrientation), typeof(StackLayout), StackOrientation.Vertical,
			propertyChanged: (bindable, oldvalue, newvalue) => ((StackLayout)bindable).InvalidateLayout());

		public static readonly BindableProperty SpacingProperty = BindableProperty.Create(nameof(Spacing), typeof(double), typeof(StackLayout), 6d,
			propertyChanged: (bindable, oldvalue, newvalue) => ((StackLayout)bindable).InvalidateLayout());

		LayoutInformation _layoutInformation = new LayoutInformation();
		readonly Lazy<PlatformConfigurationRegistry<StackLayout>> _platformConfigurationRegistry;

		public StackLayout()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<StackLayout>>(() =>
				new PlatformConfigurationRegistry<StackLayout>(this));
		}

		public IPlatformElementConfiguration<T, StackLayout> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		public StackOrientation Orientation
		{
			get { return (StackOrientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		public double Spacing
		{
			get { return (double)GetValue(SpacingProperty); }
			set { SetValue(SpacingProperty, value); }
		}

		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			if (!HasVisibleChildren())
			{
				return;
			}

			LayoutInformation layoutInformationCopy = _layoutInformation;
			if (width == layoutInformationCopy.Constraint.Width && height == layoutInformationCopy.Constraint.Height)
			{
				StackOrientation orientation = Orientation;

				AlignOffAxis(layoutInformationCopy, orientation, width, height);
				ProcessExpanders(layoutInformationCopy, orientation, x, y, width, height);
			}
			else
			{
				CalculateLayout(layoutInformationCopy, x, y, width, height, true);
			}

			for (var i = 0; i < LogicalChildrenInternal.Count; i++)
			{
				var child = (View)LogicalChildrenInternal[i];
				if (child.IsVisible && layoutInformationCopy.Plots != null)
					LayoutChildIntoBoundingRegion(child, layoutInformationCopy.Plots[i], layoutInformationCopy.Requests[i]);
			}
		}

		[Obsolete("OnSizeRequest is obsolete as of version 2.2.0. Please use OnMeasure instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
		{
			if (!HasVisibleChildren())
			{
				return new SizeRequest();
			}

			// calculate with padding inset for X,Y so we can hopefully re-use this in the layout pass
			Thickness padding = Padding;
			CalculateLayout(_layoutInformation, padding.Left, padding.Top, widthConstraint, heightConstraint, false);
			var result = new SizeRequest(_layoutInformation.Bounds.Size, _layoutInformation.MinimumSize);
			return result;
		}

		internal override void ComputeConstraintForView(View view)
		{
			ComputeConstraintForView(view, false);
		}

		internal override void InvalidateMeasureInternal(InvalidationTrigger trigger)
		{
			_layoutInformation = new LayoutInformation();
			base.InvalidateMeasureInternal(trigger);
		}

		void AlignOffAxis(LayoutInformation layout, StackOrientation orientation, double widthConstraint, double heightConstraint)
		{
			for (var i = 0; i < layout.Plots?.Length; i++)
			{
				if (!((View)LogicalChildrenInternal[i]).IsVisible)
					continue;
				if (orientation == StackOrientation.Vertical)
				{
					layout.Plots[i].Width = widthConstraint;
				}
				else
				{
					layout.Plots[i].Height = heightConstraint;
				}
			}
		}

		void CalculateLayout(LayoutInformation layout, double x, double y, double widthConstraint, double heightConstraint, bool processExpanders)
		{
			layout.Constraint = new Size(widthConstraint, heightConstraint);
			layout.Expanders = 0;
			layout.CompressionSpace = 0;
			layout.Plots = new Rectangle[Children.Count];
			layout.Requests = new SizeRequest[Children.Count];

			StackOrientation orientation = Orientation;

			CalculateNaiveLayout(layout, orientation, x, y, widthConstraint, heightConstraint);
			CompressNaiveLayout(layout, orientation, widthConstraint, heightConstraint);

			if (processExpanders)
			{
				AlignOffAxis(layout, orientation, widthConstraint, heightConstraint);
				ProcessExpanders(layout, orientation, x, y, widthConstraint, heightConstraint);
			}
		}

		void CalculateNaiveLayout(LayoutInformation layout, StackOrientation orientation, double x, double y, double widthConstraint, double heightConstraint)
		{
			layout.CompressionSpace = 0;

			double xOffset = x;
			double yOffset = y;
			double boundsWidth = 0;
			double boundsHeight = 0;
			double minimumWidth = 0;
			double minimumHeight = 0;
			double spacing = Spacing;
			if (orientation == StackOrientation.Vertical)
			{
				View expander = null;
				for (var i = 0; i < LogicalChildrenInternal.Count; i++)
				{
					var child = (View)LogicalChildrenInternal[i];
					if (!child.IsVisible)
						continue;

					if (child.VerticalOptions.Expands)
					{
						layout.Expanders++;
						if (expander != null)
						{
							// we have multiple expanders, make sure previous expanders are reset to not be fixed because they no logner are
							ComputeConstraintForView(child, false);
						}
						expander = child;
					}
					SizeRequest request = child.Measure(widthConstraint, double.PositiveInfinity, MeasureFlags.IncludeMargins);

					var bounds = new Rectangle(x, yOffset, request.Request.Width, request.Request.Height);
					layout.Plots[i] = bounds;
					layout.Requests[i] = request;
					layout.CompressionSpace += Math.Max(0, request.Request.Height - request.Minimum.Height);
					yOffset = bounds.Bottom + spacing;

					boundsWidth = Math.Max(boundsWidth, request.Request.Width);
					boundsHeight = bounds.Bottom - y;
					minimumHeight += request.Minimum.Height + spacing;
					minimumWidth = Math.Max(minimumWidth, request.Minimum.Width);
				}
				minimumHeight -= spacing;
				if (expander != null)
					ComputeConstraintForView(expander, layout.Expanders == 1); // warning : slightly obtuse, but we either need to setup the expander or clear the last one
			}
			else
			{
				View expander = null;
				for (var i = 0; i < LogicalChildrenInternal.Count; i++)
				{
					var child = (View)LogicalChildrenInternal[i];
					if (!child.IsVisible)
						continue;

					if (child.HorizontalOptions.Expands)
					{
						layout.Expanders++;
						if (expander != null)
						{
							ComputeConstraintForView(child, false);
						}
						expander = child;
					}
					SizeRequest request = child.Measure(double.PositiveInfinity, heightConstraint, MeasureFlags.IncludeMargins);

					var bounds = new Rectangle(xOffset, y, request.Request.Width, request.Request.Height);
					layout.Plots[i] = bounds;
					layout.Requests[i] = request;
					layout.CompressionSpace += Math.Max(0, request.Request.Width - request.Minimum.Width);
					xOffset = bounds.Right + spacing;

					boundsWidth = bounds.Right - x;
					boundsHeight = Math.Max(boundsHeight, request.Request.Height);
					minimumWidth += request.Minimum.Width + spacing;
					minimumHeight = Math.Max(minimumHeight, request.Minimum.Height);
				}
				minimumWidth -= spacing;
				if (expander != null)
					ComputeConstraintForView(expander, layout.Expanders == 1);
			}

			layout.Bounds = new Rectangle(x, y, boundsWidth, boundsHeight);
			layout.MinimumSize = new Size(minimumWidth, minimumHeight);
		}

		void CompressHorizontalLayout(LayoutInformation layout, double widthConstraint, double heightConstraint)
		{
			double xOffset = 0;

			if (widthConstraint >= layout.Bounds.Width)
			{
				// no need to compress
				return;
			}

			double requiredCompression = layout.Bounds.Width - widthConstraint;
			double compressionSpace = layout.CompressionSpace;
			double compressionPressure = (requiredCompression / layout.CompressionSpace).Clamp(0, 1);

			for (var i = 0; i < layout.Plots.Length; i++)
			{
				var child = (View)LogicalChildrenInternal[i];
				if (!child.IsVisible)
					continue;

				Size minimum = layout.Requests[i].Minimum;

				layout.Plots[i].X -= xOffset;

				Rectangle plot = layout.Plots[i];
				double availableSpace = plot.Width - minimum.Width;
				if (availableSpace <= 0)
					continue;

				compressionSpace -= availableSpace;

				double compression = availableSpace * compressionPressure;
				xOffset += compression;

				double newWidth = plot.Width - compression;
				SizeRequest newRequest = child.Measure(newWidth, heightConstraint, MeasureFlags.IncludeMargins);

				layout.Requests[i] = newRequest;

				plot.Height = newRequest.Request.Height;

				if (newRequest.Request.Width < newWidth)
				{
					double delta = newWidth - newRequest.Request.Width;
					newWidth = newRequest.Request.Width;
					xOffset += delta;
					requiredCompression = requiredCompression - xOffset;
					compressionPressure = (requiredCompression / compressionSpace).Clamp(0, 1);
				}
				plot.Width = newWidth;

				layout.Bounds.Height = Math.Max(layout.Bounds.Height, plot.Height);

				layout.Plots[i] = plot;
			}
		}

		void CompressNaiveLayout(LayoutInformation layout, StackOrientation orientation, double widthConstraint, double heightConstraint)
		{
			if (layout.CompressionSpace <= 0)
				return;

			if (orientation == StackOrientation.Vertical)
			{
				CompressVerticalLayout(layout, widthConstraint, heightConstraint);
			}
			else
			{
				CompressHorizontalLayout(layout, widthConstraint, heightConstraint);
			}
		}

		void CompressVerticalLayout(LayoutInformation layout, double widthConstraint, double heightConstraint)
		{
			double yOffset = 0;

			if (heightConstraint >= layout.Bounds.Height)
			{
				// no need to compress
				return;
			}

			double requiredCompression = layout.Bounds.Height - heightConstraint;
			double compressionSpace = layout.CompressionSpace;
			double compressionPressure = (requiredCompression / layout.CompressionSpace).Clamp(0, 1);

			for (var i = 0; i < layout.Plots.Length; i++)
			{
				var child = (View)LogicalChildrenInternal[i];
				if (!child.IsVisible)
					continue;

				Size minimum = layout.Requests[i].Minimum;

				layout.Plots[i].Y -= yOffset;

				Rectangle plot = layout.Plots[i];
				double availableSpace = plot.Height - minimum.Height;
				if (availableSpace <= 0)
					continue;

				compressionSpace -= availableSpace;

				double compression = availableSpace * compressionPressure;
				yOffset += compression;

				double newHeight = plot.Height - compression;
				SizeRequest newRequest = child.Measure(widthConstraint, newHeight, MeasureFlags.IncludeMargins);

				layout.Requests[i] = newRequest;

				plot.Width = newRequest.Request.Width;

				if (newRequest.Request.Height < newHeight)
				{
					double delta = newHeight - newRequest.Request.Height;
					newHeight = newRequest.Request.Height;
					yOffset += delta;
					requiredCompression = requiredCompression - yOffset;
					compressionPressure = (requiredCompression / compressionSpace).Clamp(0, 1);
				}
				plot.Height = newHeight;

				layout.Bounds.Width = Math.Max(layout.Bounds.Width, plot.Width);

				layout.Plots[i] = plot;
			}
		}

		void ComputeConstraintForView(View view, bool isOnlyExpander)
		{
			if (Orientation == StackOrientation.Horizontal)
			{
				if ((Constraint & LayoutConstraint.VerticallyFixed) != 0 && view.VerticalOptions.Alignment == LayoutAlignment.Fill)
				{
					if (isOnlyExpander && view.HorizontalOptions.Alignment == LayoutAlignment.Fill && Constraint == LayoutConstraint.Fixed)
					{
						view.ComputedConstraint = LayoutConstraint.Fixed;
					}
					else
					{
						view.ComputedConstraint = LayoutConstraint.VerticallyFixed;
					}
				}
				else
				{
					view.ComputedConstraint = LayoutConstraint.None;
				}
			}
			else
			{
				if ((Constraint & LayoutConstraint.HorizontallyFixed) != 0 && view.HorizontalOptions.Alignment == LayoutAlignment.Fill)
				{
					if (isOnlyExpander && view.VerticalOptions.Alignment == LayoutAlignment.Fill && Constraint == LayoutConstraint.Fixed)
					{
						view.ComputedConstraint = LayoutConstraint.Fixed;
					}
					else
					{
						view.ComputedConstraint = LayoutConstraint.HorizontallyFixed;
					}
				}
				else
				{
					view.ComputedConstraint = LayoutConstraint.None;
				}
			}
		}

		bool HasVisibleChildren()
		{
			for (var index = 0; index < InternalChildren.Count; index++)
			{
				var child = (VisualElement)InternalChildren[index];
				if (child.IsVisible)
					return true;
			}
			return false;
		}

		void ProcessExpanders(LayoutInformation layout, StackOrientation orientation, double x, double y, double widthConstraint, double heightConstraint)
		{
			if (layout.Expanders <= 0)
				return;

			if (orientation == StackOrientation.Vertical)
			{
				double extraSpace = heightConstraint - layout.Bounds.Height;
				if (extraSpace <= 0)
					return;

				double spacePerExpander = extraSpace / layout.Expanders;
				double yOffset = 0;

				for (var i = 0; i < LogicalChildrenInternal.Count; i++)
				{
					var child = (View)LogicalChildrenInternal[i];
					if (!child.IsVisible)
						continue;
					Rectangle plot = layout.Plots[i];
					plot.Y += yOffset;

					if (child.VerticalOptions.Expands)
					{
						plot.Height += spacePerExpander;
						yOffset += spacePerExpander;
					}

					layout.Plots[i] = plot;
				}

				layout.Bounds.Height = heightConstraint;
			}
			else
			{
				double extraSpace = widthConstraint - layout.Bounds.Width;
				if (extraSpace <= 0)
					return;

				double spacePerExpander = extraSpace / layout.Expanders;
				double xOffset = 0;

				for (var i = 0; i < LogicalChildrenInternal.Count; i++)
				{
					var child = (View)LogicalChildrenInternal[i];
					if (!child.IsVisible)
						continue;
					Rectangle plot = layout.Plots[i];
					plot.X += xOffset;

					if (child.HorizontalOptions.Expands)
					{
						plot.Width += spacePerExpander;
						xOffset += spacePerExpander;
					}

					layout.Plots[i] = plot;
				}

				layout.Bounds.Width = widthConstraint;
			}
		}

		class LayoutInformation
		{
			public Rectangle Bounds;
			public double CompressionSpace;
			public Size Constraint;
			public int Expanders;
			public Size MinimumSize;
			public Rectangle[] Plots;
			public SizeRequest[] Requests;
		}
	}
}