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
	public partial class ContentView : TemplatedView, IContentView, ISafeAreaPage
	{
		/// <summary>Bindable property for <see cref="Content"/>.</summary>
		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(ContentView), null, propertyChanged: TemplateUtilities.OnContentChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/ContentView.xml" path="//Member[@MemberName='Content']/Docs/*" />
		public View Content
		{
			get { return (View)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
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
			return SafeArea.ShouldIgnoreSafeAreaForEdge(this, edge);
		}

		/// <inheritdoc cref="ISafeAreaPage.GetSafeAreaRegionsForEdge"/>
		SafeAreaRegions ISafeAreaPage.GetSafeAreaRegionsForEdge(int edge)
		{
			return SafeArea.GetIgnoreForEdge(this, edge);
		}

		#endregion
	}
}