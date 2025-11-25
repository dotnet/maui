#nullable disable
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ContentView.xml" path="Type[@FullName='Microsoft.Maui.Controls.ContentView']/Docs/*" />
	[ContentProperty("Content")]
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
<<<<<<< HEAD
	[ElementHandler<ContentViewHandler>]
	public partial class ContentView : TemplatedView, IContentView
||||||| 3f26a592b2
	public partial class ContentView : TemplatedView, IContentView
=======
	public partial class ContentView : TemplatedView, IContentView, ISafeAreaView2, ISafeAreaElement
>>>>>>> 485b400ee4a317af11647f3e64085d7d8d4d5f17
	{
		/// <summary>Bindable property for <see cref="Content"/>.</summary>
		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(ContentView), null, propertyChanged: TemplateUtilities.OnContentChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/ContentView.xml" path="//Member[@MemberName='Content']/Docs/*" />
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