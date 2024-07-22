using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

#pragma warning disable RS0016 // Add public types and members to the declared API

namespace Microsoft.Maui.Controls;

public abstract class Layout<T> : Layout
	where T : IView, ICrossPlatformLayout
{
	readonly CastingList<T, IView> _castChildren;

	public Layout()
	{
		_castChildren = new CastingList<T, IView>(_children);
	}

	public new IList<T> Children => _castChildren;

	protected override void OnChildAdded(Element child)
	{
		base.OnChildAdded(child);

		if (child is T typedChild)
		{
			OnChildAdded(typedChild);
#pragma warning disable CS0618 // Type or member is obsolete
			if (child is VisualElement ve)
			{
				ve.MeasureInvalidated += OnChildMeasureInvalidated;
			}
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}

	protected override void OnChildRemoved(Element child, int oldLogicalIndex)
	{
		base.OnChildRemoved(child, oldLogicalIndex);

		if (child is T typedChild)
		{
			OnChildRemoved(typedChild, oldLogicalIndex);
#pragma warning disable CS0618 // Type or member is obsolete
			if (child is VisualElement ve)
			{
				ve.MeasureInvalidated -= OnChildMeasureInvalidated;
			}
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}

	protected virtual void OnChildAdded(T child)
	{
#pragma warning disable CS0618 // Type or member is obsolete
		OnAdded(child);
#pragma warning restore CS0618 // Type or member is obsolete
	}

	protected virtual void OnChildRemoved(T child, int oldLogicalIndex)
	{
#pragma warning disable CS0618 // Type or member is obsolete
		OnRemoved(child);
#pragma warning restore CS0618 // Type or member is obsolete
	}

	/// <summary>
	/// Invoked when a child is added to the layout. Implement this method to add class handling for this event.
	/// </summary>
	/// <param name="view">The view which was added.</param>
	[Obsolete("Use OnChildAdded")]
	protected virtual void OnAdded(T view)
	{
	}

	/// <summary>
	/// Invoked when a child is removed the layout. Implement this method to add class handling for this event.
	/// </summary>
	/// <param name="view">The view which was removed.</param>
	[Obsolete("Use OnChildRemoved")]
	protected virtual void OnRemoved(T view)
	{
	}

	Rect _previousBounds;
	protected override Size ArrangeOverride(Rect bounds)
	{
		var result = base.ArrangeOverride(bounds);

		if (_previousBounds != bounds)
		{
			_previousBounds = bounds;
			LayoutChanged?.Invoke(this, EventArgs.Empty);                
		}

		return result;
	}

	[Obsolete("Use MeasureOverride")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
	protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
	{
		return MeasureOverride(widthConstraint, heightConstraint);
	}

	[Obsolete("Use IView.ZIndex instead.")]
	public void LowerChild(View view)
	{
		if (!this.Contains(view) || this.First() == view)
		{
			return;
		}

		this.Remove(view);
		this.Insert(view, 0);
		OnChildrenReordered();
	}

	[Obsolete("Use IView.ZIndex instead.")]
	public void RaiseChild(View view)
	{
		if (!this.Contains(view) || this.Last() == view)
		{
			return;
		}

		this.Remove(view);
		this.Insert(view,this.Count - 1);
		OnChildrenReordered();
	}

	[Obsolete("Use InvalidateMeasure")]
	protected virtual void InvalidateLayout()
	{
		// Todo call this when InvalidateMeasure is called?
		(this as IView).InvalidateMeasure();
	}

	protected override ILayoutManager CreateLayoutManager()
	{
#pragma warning disable CS0618 // Type or member is obsolete
		return new PaddingOnlyLayoutManager(this, LayoutChildren);
#pragma warning restore CS0618 // Type or member is obsolete
	}

	[Obsolete("override CreateLayoutManager and return a new LayoutManager")]
	protected virtual void LayoutChildren(double x, double y, double width, double height)
	{
		
	}

	[Obsolete("This method on the original class makes no sense and was probably accidently made protected. I'll write a better message before merging.")]
	protected void OnChildMeasureInvalidated(object? sender, EventArgs e)
	{
		OnChildMeasureInvalidated();
	}

	[Obsolete("If you're interested in knowing when a child element is invalidated than just subscribe to MeasureInvalidated inside OnChildAdded.")]
	protected virtual void OnChildMeasureInvalidated()
	{
	}

	
	[Obsolete("Obsolete. This never really did anything because the platform is going to invalidate the layout anyway when views are removed/added.")]
	protected virtual bool ShouldInvalidateOnChildAdded(View child) => true;

	[Obsolete("Obsolete. This never really did anything because the platform is going to invalidate the layout anyway when views are removed/added.")]
	protected virtual bool ShouldInvalidateOnChildRemoved(View child) => true;
	
	[Obsolete("Obsolete. Use InvalidateMeasure")]
	protected void UpdateChildrenLayout()
	{
		(this as IView).InvalidateArrange();
	}
	
	[Obsolete("Obsolete. Use InvalidateMeasure")]
	public void ForceLayout() => (this as IView).InvalidateMeasure();
	
	// TODO Obsolete in NET10. I was going to obsolete and tell people to use SizeChanged
	// But the different with SizeChanged and LayoutChanged is that LayoutChanged fires after
	// the arrange pass has happen on all the children, where SizeChanged fires before the arrange pass
	public event EventHandler? LayoutChanged;

	/// <summary>
	/// The implementation in the legacy Layout class would call LayoutChildren with the padding applied
	/// </summary>
	class PaddingOnlyLayoutManager : LayoutManager
	{
		readonly Action<double, double, double, double> _layoutChildren;

		public PaddingOnlyLayoutManager(Layout layout, Action<double, double, double, double> layoutChildren) : base(layout)
		{
			_layoutChildren = layoutChildren;
		}

		public override Size ArrangeChildren(Rect bounds)
		{
			double width = Layout.Width;
			double height = Layout.Height;

			double x = Layout.Padding.Left;
			double y = Layout.Padding.Top;
			double w = Math.Max(0, width - Layout.Padding.HorizontalThickness);
			double h = Math.Max(0, height - Layout.Padding.VerticalThickness);

			_layoutChildren(x, y, w, h);
			return bounds.Size;
		}

		public override Size Measure(double widthConstraint, double heightConstraint)
		{
			return new Size(widthConstraint, heightConstraint);
		}
	}
}
