#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// An element used to display a content object within a templated control. This allows you to customize how content is displayed. 
	/// </summary>
	public class ContentPresenter : Layout, IContentView, IControlsView
	{
		/// <summary>
		/// Bindable property for Content.
		/// </summary>
		public static BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View),
			typeof(ContentPresenter), null, propertyChanged: OnContentChanged);

		/// <summary>
		/// Initializes a new instance of the ContentPresenter class.
		/// </summary>
		public ContentPresenter()
		{
			this.SetBinding(
				ContentProperty,
				static (IContentView view) => view.Content,
				source: RelativeBindingSource.TemplatedParent,
				converter: new ContentConverter(),
				converterParameter: this);
		}

		/// <summary>
		/// Gets or sets the content to be displayed.
		/// </summary>
		public View Content
		{
			get { return (View)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		object IContentView.Content => Content;
		IView IContentView.PresentedContent => Content;

		/// <summary>
		/// Clears the content.
		/// </summary>
		internal new virtual void Clear()
		{
			Content = null;
		}

		/// <summary>
		/// Sets the inherited binding context for a child element.
		/// </summary>
		/// <param name="child">The child element.</param>
		/// <param name="context">The binding context.</param>
		internal override void SetChildInheritedBindingContext(Element child, object context)
		{
			// We never want to use the standard inheritance mechanism, we will get this set by our parent
		}

		static async void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = (ContentPresenter)bindable;

			var oldView = (View)oldValue;
			var newView = (View)newValue;
			if (oldView != null)
			{
				self.Remove(oldView);
				oldView.ParentOverride = null;
			}

			if (newView != null)
			{
				self.Add(newView);
				newView.ParentOverride = await TemplateUtilities.FindTemplatedParentAsync((Element)bindable);
			}
		}

		/// <summary>
		/// Measures the desired size of the content.
		/// </summary>
		/// <param name="widthConstraint">The width constraint.</param>
		/// <param name="heightConstraint">The height constraint.</param>
		/// <returns>The desired size.</returns>
		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			return this.ComputeDesiredSize(widthConstraint, heightConstraint);
		}

		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return this.MeasureContent(widthConstraint, heightConstraint);
		}

		/// <summary>
		/// Arranges the content within the specified bounds.
		/// </summary>
		/// <param name="bounds">The bounds for arrangement.</param>
		/// <returns>The arranged size.</returns>
		protected override Size ArrangeOverride(Rect bounds)
		{
			Frame = this.ComputeFrame(bounds);
			Handler?.PlatformArrange(Frame);
			return Frame.Size;
		}

		Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds)
		{
			this.ArrangeContent(bounds);
			return bounds.Size;
		}

		/// <summary>
		/// Creates the layout manager for this ContentPresenter.
		/// </summary>
		/// <returns>The layout manager.</returns>
		protected override ILayoutManager CreateLayoutManager()
		{
			return new ContentLayoutManager(this);
		}

		private class ContentLayoutManager : ILayoutManager
		{
			private readonly IContentView _contentView;

			public ContentLayoutManager(IContentView contentView)
			{
				_contentView = contentView;
			}

			public Size Measure(double widthConstraint, double heightConstraint)
			{
				return _contentView.MeasureContent(widthConstraint, heightConstraint);
			}

			public Size ArrangeChildren(Rect bounds)
			{
				_contentView.ArrangeContent(bounds);
				return bounds.Size;
			}
		}
	}
}