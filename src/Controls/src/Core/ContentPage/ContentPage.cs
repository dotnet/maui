#nullable disable

using System;
using System.Diagnostics;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.HotReload;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ContentPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.ContentPage']/Docs/*" />
	[ContentProperty("Content")]
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public partial class ContentPage : TemplatedPage, IContentView, HotReload.IHotReloadableView, ISafeAreaElement, ISafeAreaView2
	{
		/// <summary>Bindable property for <see cref="Content"/>.</summary>
		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(ContentPage), null, propertyChanged: TemplateUtilities.OnContentChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/ContentPage.xml" path="//Member[@MemberName='Content']/Docs/*" />
		public View Content
		{
			get { return (View)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		/// <summary>Bindable property for <see cref="HideSoftInputOnTapped"/>.</summary>
		public static readonly BindableProperty HideSoftInputOnTappedProperty
			= BindableProperty.Create(nameof(HideSoftInputOnTapped), typeof(bool), typeof(ContentPage), false);

		/// <summary>Bindable property for <see cref="SafeAreaEdges"/>.</summary>
		public static readonly BindableProperty SafeAreaEdgesProperty = SafeAreaElement.SafeAreaEdgesProperty;

		/// <summary>
		/// Gets or sets a value that indicates whether tapping anywhere on the page will cause the soft input to hide.
		/// </summary>
		public bool HideSoftInputOnTapped
		{
			get { return (bool)GetValue(HideSoftInputOnTappedProperty); }
			set { SetValue(HideSoftInputOnTappedProperty, value); }
		}

		/// <summary>
		/// Gets or sets the safe area edges to obey for this content page.
		/// The default value is SafeAreaEdges.Default (None - edge to edge).
		/// </summary>
		/// <remarks>
		/// This property controls which edges of the content page should obey safe area insets.
		/// Use SafeAreaRegions.None for edge-to-edge content, SafeAreaRegions.All to obey all safe area insets, 
		/// SafeAreaRegions.Container for content that flows under keyboard but stays out of bars/notch, or SafeAreaRegions.SoftInput for keyboard-aware behavior.
		/// </remarks>
		public SafeAreaEdges SafeAreaEdges
		{
			get => (SafeAreaEdges)GetValue(SafeAreaElement.SafeAreaEdgesProperty);
			set => SetValue(SafeAreaElement.SafeAreaEdgesProperty, value);
		}

		public ContentPage()
		{
			this.NavigatedTo += (_, _) => UpdateHideSoftInputOnTapped();
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			View content = Content;
			ControlTemplate controlTemplate = ControlTemplate;
			if (content != null && controlTemplate != null)
			{
				SetInheritedBindingContext(content, BindingContext);
			}
		}

		internal override void OnControlTemplateChanged(ControlTemplate oldValue, ControlTemplate newValue)
		{
			if (oldValue == null)
				return;

			base.OnControlTemplateChanged(oldValue, newValue);
			View content = Content;
			ControlTemplate controlTemplate = ControlTemplate;
			if (content != null && controlTemplate != null)
			{
				SetInheritedBindingContext(content, BindingContext);
			}
		}

		object IContentView.Content => Content;
		IView IContentView.PresentedContent => ((this as IControlTemplated).TemplateRoot as IView) ?? Content;

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			return this.ComputeDesiredSize(widthConstraint, heightConstraint);
		}

		[Obsolete("Use ArrangeOverride instead")]
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			// This call is redundant when a handler is attached because the handler just calls
			// CrossPlatformArrange
			if (Handler is null)
				base.LayoutChildren(x, y, width, height);
		}

		protected override Size ArrangeOverride(Rect bounds)
		{
			Frame = this.ComputeFrame(bounds);
			Handler?.PlatformArrange(Frame);
			return Frame.Size;
		}

		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			_ = this.MeasureContent(widthConstraint, heightConstraint);
			return new Size(widthConstraint, heightConstraint);
		}

		Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds)
		{
			Frame = bounds;
			this.ArrangeContent(bounds);
			return bounds.Size;
		}

		protected override void InvalidateMeasureOverride()
		{
			base.InvalidateMeasureOverride();
			if (Content is IView view)
			{
				view.InvalidateMeasure();
			}
		}

		#region HotReload

		IView IReplaceableView.ReplacedView => HotReload.MauiHotReloadHelper.GetReplacedView(this) ?? this;

		HotReload.IReloadHandler HotReload.IHotReloadableView.ReloadHandler { get; set; }

		void HotReload.IHotReloadableView.TransferState(IView newView)
		{
			//TODO: Let you hot reload the the ViewModel
			//TODO: Lets do a real state transfer
			if (newView is View v)
				v.BindingContext = BindingContext;
		}

		void HotReload.IHotReloadableView.Reload()
		{
			Dispatcher.Dispatch(() =>
			{
				this.CheckHandlers();
				var reloadHandler = ((IHotReloadableView)this).ReloadHandler;
				reloadHandler?.Reload();
				//TODO: if reload handler is null, Do a manual reload?
			});
		}

		#endregion

		Size IContentView.CrossPlatformArrange(Rect bounds)
		{
			return (this as ICrossPlatformLayout).CrossPlatformArrange(bounds);
		}

		Size IContentView.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return (this as ICrossPlatformLayout).CrossPlatformMeasure(widthConstraint, heightConstraint);
		}

		/// <inheritdoc cref="ISafeAreaView2.GetSafeAreaRegionsForEdge"/>
		SafeAreaRegions ISafeAreaView2.GetSafeAreaRegionsForEdge(int edge)
		{
			// Check if the developer has explicitly set SafeAreaEdges
			if (IsSet(SafeAreaEdgesProperty))
			{
				// Developer has explicitly set SafeAreaEdges, use it directly
				return SafeAreaEdges.GetEdge(edge);
			}


#if IOS || MACCATALYST

			// Developer hasn't set SafeAreaEdges, fall back to legacy IgnoreSafeArea behavior
			var ignoreSafeArea = ((ISafeAreaView)this).IgnoreSafeArea;
			if (ignoreSafeArea)
			{
				return SafeAreaRegions.None; // If legacy says "ignore", return None (edge-to-edge)
			}
			else
			{
				return SafeAreaRegions.Container; // If legacy says "don't ignore", return Container
			}

#else

			// By default on android it was never edge to edge so we set this to container by default
			return SafeAreaRegions.Container;

#endif
		}

		SafeAreaEdges ISafeAreaElement.SafeAreaEdgesDefaultValueCreator()
		{
			return SafeAreaEdges.None;
		}

		private protected override string GetDebuggerDisplay()
		{
			var contentText = DebuggerDisplayHelpers.GetDebugText(nameof(Content), Content);
			return $"{base.GetDebuggerDisplay()}, {contentText}";
		}
	}
}