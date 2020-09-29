using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public class RelativeLayout : Layout<View>, IElementConfiguration<RelativeLayout>
	{
		public static readonly BindableProperty XConstraintProperty = BindableProperty.CreateAttached("XConstraint", typeof(Constraint), typeof(RelativeLayout), null, propertyChanged: ConstraintChanged);

		public static readonly BindableProperty YConstraintProperty = BindableProperty.CreateAttached("YConstraint", typeof(Constraint), typeof(RelativeLayout), null, propertyChanged: ConstraintChanged);

		public static readonly BindableProperty WidthConstraintProperty = BindableProperty.CreateAttached("WidthConstraint", typeof(Constraint), typeof(RelativeLayout), null, propertyChanged: ConstraintChanged);

		public static readonly BindableProperty HeightConstraintProperty = BindableProperty.CreateAttached("HeightConstraint", typeof(Constraint), typeof(RelativeLayout), null, propertyChanged: ConstraintChanged);

		public static readonly BindableProperty BoundsConstraintProperty = BindableProperty.CreateAttached("BoundsConstraint", typeof(BoundsConstraint), typeof(RelativeLayout), null);

		readonly RelativeElementCollection _children;

		IEnumerable<View> _childrenInSolveOrder;
		readonly Lazy<PlatformConfigurationRegistry<RelativeLayout>> _platformConfigurationRegistry;

		public RelativeLayout()
		{
			VerticalOptions = HorizontalOptions = LayoutOptions.FillAndExpand;
			_children = new RelativeElementCollection(InternalChildren, this);
			_children.Parent = this;

			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<RelativeLayout>>(() =>
				new PlatformConfigurationRegistry<RelativeLayout>(this));
		}

		public IPlatformElementConfiguration<T, RelativeLayout> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		public new IRelativeList<View> Children
		{
			get { return _children; }
		}

		IEnumerable<View> ChildrenInSolveOrder
		{
			get
			{
				if (_childrenInSolveOrder != null)
					return _childrenInSolveOrder;

				var result = new List<View>();
				var solveTable = new Dictionary<View, bool>();
				foreach (View child in Children.Cast<View>())
				{
					solveTable[child] = false;
				}

				List<View> unsolvedChildren = Children.Cast<View>().ToList();
				while (unsolvedChildren.Any())
				{
					List<View> copy = unsolvedChildren.ToList();
					var solvedChild = false;
					foreach (View child in copy)
					{
						if (CanSolveView(child, solveTable))
						{
							result.Add(child);
							solveTable[child] = true;
							unsolvedChildren.Remove(child);
							solvedChild = true;
						}
					}
					if (!solvedChild)
						throw new UnsolvableConstraintsException("Constraints as specified contain an unsolvable loop.");
				}

				_childrenInSolveOrder = result;
				return _childrenInSolveOrder;
			}
		}

		static void ConstraintChanged(BindableObject bindable, object oldValue, object newValue)
		{
			View view = bindable as View;

			(view?.Parent as RelativeLayout)?.UpdateBoundsConstraint(view);
		}

		void UpdateBoundsConstraint(View view)
		{
			if (GetBoundsConstraint(view) == null)
				return; // Bounds constraint hasn't been calculated yet, no need to update just yet

			CreateBoundsFromConstraints(view, GetXConstraint(view), GetYConstraint(view), GetWidthConstraint(view), GetHeightConstraint(view));

			_childrenInSolveOrder = null; // New constraints may have impact on solve order

			InvalidateLayout();
		}

		public static BoundsConstraint GetBoundsConstraint(BindableObject bindable)
		{
			return (BoundsConstraint)bindable.GetValue(BoundsConstraintProperty);
		}

		public static Constraint GetHeightConstraint(BindableObject bindable)
		{
			return (Constraint)bindable.GetValue(HeightConstraintProperty);
		}

		public static Constraint GetWidthConstraint(BindableObject bindable)
		{
			return (Constraint)bindable.GetValue(WidthConstraintProperty);
		}

		public static Constraint GetXConstraint(BindableObject bindable)
		{
			return (Constraint)bindable.GetValue(XConstraintProperty);
		}

		public static Constraint GetYConstraint(BindableObject bindable)
		{
			return (Constraint)bindable.GetValue(YConstraintProperty);
		}

		public static void SetBoundsConstraint(BindableObject bindable, BoundsConstraint value)
		{
			bindable.SetValue(BoundsConstraintProperty, value);
		}

		public static void SetHeightConstraint(BindableObject bindable, Constraint value)
		{
			bindable.SetValue(HeightConstraintProperty, value);
		}

		public static void SetWidthConstraint(BindableObject bindable, Constraint value)
		{
			bindable.SetValue(WidthConstraintProperty, value);
		}

		public static void SetXConstraint(BindableObject bindable, Constraint value)
		{
			bindable.SetValue(XConstraintProperty, value);
		}

		public static void SetYConstraint(BindableObject bindable, Constraint value)
		{
			bindable.SetValue(YConstraintProperty, value);
		}

		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			foreach (View child in ChildrenInSolveOrder)
			{
				LayoutChildIntoBoundingRegion(child, SolveView(child));
			}
		}

		protected override void OnAdded(View view)
		{
			BoundsConstraint boundsConstraint = GetBoundsConstraint(view);
			if (boundsConstraint == null || !boundsConstraint.CreatedFromExpression)
			{
				// user probably added the view through the strict Add method.
				CreateBoundsFromConstraints(view, GetXConstraint(view), GetYConstraint(view), GetWidthConstraint(view), GetHeightConstraint(view));
			}

			_childrenInSolveOrder = null;
			base.OnAdded(view);
		}

		protected override void OnRemoved(View view)
		{
			_childrenInSolveOrder = null;
			base.OnRemoved(view);
		}

		[Obsolete("OnSizeRequest is obsolete as of version 2.2.0. Please use OnMeasure instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
		{
			double mockWidth = double.IsPositiveInfinity(widthConstraint) ? ParentView.Width : widthConstraint;
			double mockHeight = double.IsPositiveInfinity(heightConstraint) ? ParentView.Height : heightConstraint;
			MockBounds(new Rectangle(0, 0, mockWidth, mockHeight));

			var boundsRectangle = new Rectangle();
			var set = false;
			foreach (View child in ChildrenInSolveOrder)
			{
				Rectangle bounds = SolveView(child);
				child.MockBounds(bounds);
				if (!set)
				{
					boundsRectangle = bounds;
					set = true;
				}
				else
				{
					boundsRectangle.Left = Math.Min(boundsRectangle.Left, bounds.Left);
					boundsRectangle.Top = Math.Min(boundsRectangle.Top, bounds.Top);
					boundsRectangle.Right = Math.Max(boundsRectangle.Right, bounds.Right);
					boundsRectangle.Bottom = Math.Max(boundsRectangle.Bottom, bounds.Bottom);
				}
			}

			foreach (View child in ChildrenInSolveOrder)
				child.UnmockBounds();

			UnmockBounds();

			return new SizeRequest(new Size(boundsRectangle.Right, boundsRectangle.Bottom));
		}

		bool CanSolveView(View view, Dictionary<View, bool> solveTable)
		{
			BoundsConstraint boundsConstraint = GetBoundsConstraint(view);
			var parents = new List<View>();
			if (boundsConstraint == null)
			{
				throw new Exception("BoundsConstraint should not be null at this point");
			}
			parents.AddRange(boundsConstraint.RelativeTo);
			// expressions probably referenced the base layout somewhere
			while (parents.Remove(this)) // because winphone does not have RemoveAll...
				;

			if (!parents.Any())
				return true;

			for (var i = 0; i < parents.Count; i++)
			{
				View p = parents[i];

				bool solvable;
				if (!solveTable.TryGetValue(p, out solvable))
				{
					throw new InvalidOperationException("Views that have relationships to or from them must be kept in the RelativeLayout.");
				}

				if (!solvable)
					return false;
			}

			return true;
		}

		void CreateBoundsFromConstraints(View view, Constraint xConstraint, Constraint yConstraint, Constraint widthConstraint, Constraint heightConstraint)
		{
			var parents = new List<View>();

			Func<double> x;
			if (xConstraint != null)
			{
				x = () => xConstraint.Compute(this);
				if (xConstraint.RelativeTo != null)
					parents.AddRange(xConstraint.RelativeTo);
			}
			else
				x = () => 0;

			Func<double> y;
			if (yConstraint != null)
			{
				y = () => yConstraint.Compute(this);
				if (yConstraint.RelativeTo != null)
					parents.AddRange(yConstraint.RelativeTo);
			}
			else
				y = () => 0;

			Func<double> width;
			Func<double> height = null;

			if (widthConstraint != null)
			{
				width = () => widthConstraint.Compute(this);
				if (widthConstraint.RelativeTo != null)
					parents.AddRange(widthConstraint.RelativeTo);
			}
			else
				width = () => view.Measure(Width, heightConstraint != null ? height() : Height, MeasureFlags.IncludeMargins).Request.Width;

			if (heightConstraint != null)
			{
				height = () => heightConstraint.Compute(this);
				if (heightConstraint.RelativeTo != null)
					parents.AddRange(heightConstraint.RelativeTo);
			}
			else
				height = () => view.Measure(widthConstraint != null ? width() : Width, Height, MeasureFlags.IncludeMargins).Request.Height;

			BoundsConstraint bounds = BoundsConstraint.FromExpression(() => new Rectangle(x(), y(), width(), height()), parents.Distinct().ToArray());
			SetBoundsConstraint(view, bounds);
		}

		static Rectangle SolveView(View view)
		{
			BoundsConstraint boundsConstraint = GetBoundsConstraint(view);

			if (boundsConstraint == null)
			{
				throw new Exception("BoundsConstraint should not be null at this point");
			}

			var result = boundsConstraint.Compute();

			return result;
		}

		public interface IRelativeList<T> : IList<T> where T : View
		{
			void Add(T view, Expression<Func<Rectangle>> bounds);

			void Add(T view, Expression<Func<double>> x = null, Expression<Func<double>> y = null, Expression<Func<double>> width = null, Expression<Func<double>> height = null);

			void Add(T view, Constraint xConstraint = null, Constraint yConstraint = null, Constraint widthConstraint = null, Constraint heightConstraint = null);
		}

		class RelativeElementCollection : ElementCollection<View>, IRelativeList<View>
		{
			public RelativeElementCollection(ObservableCollection<Element> inner, RelativeLayout parent) : base(inner)
			{
				Parent = parent;
			}

			internal RelativeLayout Parent { get; set; }

			public void Add(View view, Expression<Func<Rectangle>> bounds)
			{
				if (bounds == null)
					throw new ArgumentNullException(nameof(bounds));
				SetBoundsConstraint(view, BoundsConstraint.FromExpression(bounds, fromExpression: true));

				base.Add(view);
			}

			public void Add(View view, Expression<Func<double>> x = null, Expression<Func<double>> y = null, Expression<Func<double>> width = null, Expression<Func<double>> height = null)
			{
				Func<double> xCompiled = x != null ? x.Compile() : () => 0;
				Func<double> yCompiled = y != null ? y.Compile() : () => 0;
				Func<double> widthCompiled = width != null ? width.Compile() : () => view.Measure(Parent.Width, Parent.Height, MeasureFlags.IncludeMargins).Request.Width;
				Func<double> heightCompiled = height != null ? height.Compile() : () => view.Measure(Parent.Width, Parent.Height, MeasureFlags.IncludeMargins).Request.Height;

				var parents = new List<View>();
				parents.AddRange(ExpressionSearch.Default.FindObjects<View>(x));
				parents.AddRange(ExpressionSearch.Default.FindObjects<View>(y));
				parents.AddRange(ExpressionSearch.Default.FindObjects<View>(width));
				parents.AddRange(ExpressionSearch.Default.FindObjects<View>(height));

				BoundsConstraint bounds = BoundsConstraint.FromExpression(() => new Rectangle(xCompiled(), yCompiled(), widthCompiled(), heightCompiled()), fromExpression: true, parents: parents.Distinct().ToArray());

				SetBoundsConstraint(view, bounds);

				base.Add(view);
			}

			public void Add(View view, Constraint xConstraint = null, Constraint yConstraint = null, Constraint widthConstraint = null, Constraint heightConstraint = null)
			{
				view.BatchBegin();

				RelativeLayout.SetXConstraint(view, xConstraint);
				RelativeLayout.SetYConstraint(view, yConstraint);
				RelativeLayout.SetWidthConstraint(view, widthConstraint);
				RelativeLayout.SetHeightConstraint(view, heightConstraint);

				view.BatchCommit();

				base.Add(view);
			}
		}
	}
}