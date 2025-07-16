#nullable disable
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ContentView.xml" path="Type[@FullName='Microsoft.Maui.Controls.ContentView']/Docs/*" />
	[ContentProperty("Content")]
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public partial class ContentView : TemplatedView, IContentView, ISafeAreaPage, ISafeAreaElement
	{
		/// <summary>Bindable property for <see cref="Content"/>.</summary>
		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(ContentView), null, propertyChanged: TemplateUtilities.OnContentChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/ContentView.xml" path="//Member[@MemberName='Content']/Docs/*" />
		public View Content
		{
			get { return (View)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		/// <summary>Bindable property for <see cref="SafeAreaIgnore"/>.</summary>
		public static readonly BindableProperty SafeAreaIgnoreProperty = SafeAreaElement.SafeAreaIgnoreProperty;

		/// <summary>
		/// Gets or sets the safe area edges to ignore for this content view.
		/// The default value is SafeAreaEdges.Default.
		/// </summary>
		/// <remarks>
		/// This property controls which edges of the content view should ignore safe area insets.
		/// Use SafeAreaRegions.Default to respect safe area, SafeAreaRegions.All to ignore all insets, 
		/// SafeAreaRegions.None to ensure content never displays behind blocking UI, or SafeAreaRegions.SoftInput for soft input aware behavior.
		/// </remarks>
		public SafeAreaEdges SafeAreaIgnore
		{
			get => (SafeAreaEdges)GetValue(SafeAreaElement.SafeAreaIgnoreProperty);
			set => SetValue(SafeAreaElement.SafeAreaIgnoreProperty, value);
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

		SafeAreaEdges ISafeAreaElement.SafeAreaIgnoreDefaultValueCreator()
		{
			return SafeAreaEdges.Default;
		}

		private protected override string GetDebuggerDisplay()
		{
			var contentText = DebuggerDisplayHelpers.GetDebugText(nameof(Content), Content);
			return $"{base.GetDebuggerDisplay()}, {contentText}";
		}

		#region ISafeAreaPage

		/// <inheritdoc cref="ISafeAreaPage.SafeAreaInsets"/>
		Thickness ISafeAreaPage.SafeAreaInsets { set { } } // Default no-op implementation for content views

		/// <inheritdoc cref="ISafeAreaPage.IgnoreSafeAreaForEdge"/>
		bool ISafeAreaPage.IgnoreSafeAreaForEdge(int edge)
		{
			// Use direct property first, then fall back to attached property
			var regionForEdge = SafeAreaIgnore.GetEdge(edge);
			
			// Handle the SafeAreaRegions behavior
			if (regionForEdge.HasFlag(SafeAreaRegions.All))
			{
				return true; // Ignore all insets - content may be positioned anywhere
			}

			if (regionForEdge == SafeAreaRegions.None || regionForEdge == SafeAreaRegions.SoftInput)
			{
				// Content will never display behind anything that could block it
				// Or treat SoftInput as respecting safe area for now
				return false;
			}

			if (regionForEdge == SafeAreaRegions.Default)
			{
				// Check if attached property is set, if not fall back to default behavior
				if (SafeAreaElement.GetIgnore(this) != SafeAreaEdges.Default)
				{
					return SafeAreaElement.ShouldIgnoreSafeAreaForEdge(this, edge);
				}
				
				// Default behavior for ContentView is to respect safe area
				return false;
			}

			return false;
		}

		/// <inheritdoc cref="ISafeAreaPage.GetSafeAreaRegionsForEdge"/>
		SafeAreaRegions ISafeAreaPage.GetSafeAreaRegionsForEdge(int edge)
		{
			// Use direct property first, then fall back to attached property
			var regionForEdge = SafeAreaIgnore.GetEdge(edge);
			
			if (regionForEdge != SafeAreaRegions.Default)
			{
				return regionForEdge;
			}
			
			// Fall back to attached property if direct property is Default
			return SafeAreaElement.GetIgnoreForEdge(this, edge);
		}

		#endregion
	}
}