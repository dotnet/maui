#nullable disable
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// An element that contains a single child element.
	/// </summary>
	/// <remarks>
	/// ContentView is a simple container that hosts a single <see cref="View"/> element through its <see cref="Content"/> property.
	/// It is commonly used as a base class for custom views or to apply templates to content.
	/// </remarks>
	[ContentProperty("Content")]
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler(typeof(ContentViewHandler))]
	public partial class ContentView : TemplatedView, IContentView, ISafeAreaView2, ISafeAreaElement
	{
		/// <summary>Bindable property for <see cref="Content"/>.</summary>
		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(ContentView), null, propertyChanged: TemplateUtilities.OnContentChanged);

		/// <summary>
		/// Gets or sets the content of the ContentView. This is a bindable property.
		/// </summary>
		/// <value>A <see cref="View"/> that contains the content.</value>
		public View Content
		{
			get { return (View)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		/// <summary>Bindable property for <see cref="SafeAreaEdges"/>.</summary>
		public static readonly BindableProperty SafeAreaEdgesProperty = SafeAreaElement.SafeAreaEdgesProperty;

		/// <summary>
		/// Gets or sets the safe area edges to obey for this content view.
		/// The default value is SafeAreaEdges.Default (None - edge to edge).
		/// </summary>
		/// <remarks>
		/// This property controls which edges of the content view should obey safe area insets.
		/// Use SafeAreaRegions.None for edge-to-edge content, SafeAreaRegions.All to obey all safe area insets, 
		/// SafeAreaRegions.Container for content that flows under keyboard but stays out of bars/notch, or SafeAreaRegions.Keyboard for keyboard-aware behavior.
		/// </remarks>
		public SafeAreaEdges SafeAreaEdges
		{
			get => (SafeAreaEdges)GetValue(SafeAreaElement.SafeAreaEdgesProperty);
			set => SetValue(SafeAreaElement.SafeAreaEdgesProperty, value);
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			if (Content is View content)
			{
				SetInheritedBindingContext(content, BindingContext);
			}
		}

		internal override void OnControlTemplateChanged(ControlTemplate oldValue, ControlTemplate newValue)
		{
			base.OnControlTemplateChanged(oldValue, newValue);

			if (Content is View content)
			{
				SetInheritedBindingContext(content, BindingContext);
			}
		}

		internal override void SetChildInheritedBindingContext(Element child, object context)
		{
			SetInheritedBindingContext(child, context);
		}

		object IContentView.Content => Content;

		IView IContentView.PresentedContent => ((this as IControlTemplated).TemplateRoot as IView) ?? Content;

		SafeAreaEdges ISafeAreaElement.SafeAreaEdgesDefaultValueCreator()
		{
			return SafeAreaEdges.None;
		}

		private protected override string GetDebuggerDisplay()
		{
			var contentText = DebuggerDisplayHelpers.GetDebugText(nameof(Content), Content);
			return $"{base.GetDebuggerDisplay()}, {contentText}";
		}

		/// <inheritdoc cref="ISafeAreaView2.SafeAreaInsets"/>
		Thickness ISafeAreaView2.SafeAreaInsets { set { } } // Default no-op implementation for content views

		/// <inheritdoc cref="ISafeAreaView2.GetSafeAreaRegionsForEdge"/>
		SafeAreaRegions ISafeAreaView2.GetSafeAreaRegionsForEdge(int edge)
		{
			// Use direct property
			var regionForEdge = SafeAreaEdges.GetEdge(edge);

			if (regionForEdge == SafeAreaRegions.Default)
			{
				// If no safe area edges are set, return None
				return SafeAreaRegions.None;
			}

			// For ContentView, return the region directly
			return regionForEdge;
		}
	}
}