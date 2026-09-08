using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using ObjCRuntime;
using UIKit;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class ImageButtonRenderer : ViewRenderer<ImageButton, UIButton>, IImageVisualElementRenderer
	{
		bool _isDisposed;

		// This looks like it should be a const under iOS Classic,
		// but that doesn't work under iOS 
		// ReSharper disable once BuiltInTypeReferenceStyle
		// Under iOS Classic Resharper wants to suggest this use the built-in type ref
		// but under iOS that suggestion won't work
		readonly nfloat _minimumButtonHeight = 44; // Apple docs


		[Preserve(Conditional = true)]
		public ImageButtonRenderer() : base()
		{
			ButtonElementManager.Init(this);
			BorderElementManager.Init(this);
			ImageElementManager.Init(this);
		}

		public override SizeF SizeThatFits(SizeF size)
		{
			var result = base.SizeThatFits(size);

			if (result.Height < _minimumButtonHeight)
			{
				result.Height = _minimumButtonHeight;
			}

			return result;
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing && Control != null)
			{
				ButtonElementManager.Dispose(this);
				BorderElementManager.Dispose(this);
				ImageElementManager.Dispose(this);
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}

		protected async override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ImageButton.SourceProperty.PropertyName)
				await ImageElementManager.SetImage(this, Element).ConfigureAwait(false);
			else if (e.PropertyName == ImageButton.PaddingProperty.PropertyName)
				UpdatePadding();
		}

		protected async override void OnElementChanged(ElementChangedEventArgs<ImageButton> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var control = CreateNativeControl();
					control.ClipsToBounds = true;
					SetNativeControl(control);

					Debug.Assert(Control != null, "Control != null");
				}

				UpdatePadding();
				await UpdateImage().ConfigureAwait(false);
			}
		}

		[PortHandler]
		void UpdatePadding(UIButton button = null)
		{
			var uiElement = button ?? Control;
			if (uiElement == null)
				return;

#pragma warning disable CA1416, CA1422 // TOOO:  UIButton.ContentEdgeInsets' is unsupported on: 'ios' 15.0 and later
			uiElement.ContentEdgeInsets = new UIEdgeInsets(
				(float)(Element.Padding.Top),
				(float)(Element.Padding.Left),
				(float)(Element.Padding.Bottom),
				(float)(Element.Padding.Right)
			);
#pragma warning restore CA1416, CA1422
		}
		async Task UpdateImage()
		{
			try
			{
				await ImageElementManager.SetImage(this, Element).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Forms.MauiContext?.CreateLogger<ImageRenderer>()?.LogWarning(ex, "Error loading image");
			}
		}

		protected override UIButton CreateNativeControl()
		{
			return new UIButton(UIButtonType.System);
		}

		protected override void SetAccessibilityLabel()
		{
			// If we have not specified an AccessibilityLabel and the AccessibiltyLabel is current bound to the Title,
			// exit this method so we don't set the AccessibilityLabel value and break the binding.
			// This may pose a problem for users who want to explicitly set the AccessibilityLabel to null, but this
			// will prevent us from inadvertently breaking UI Tests that are using Query.Marked to get the dynamic Title 
			// of the ImageButton.

			var elemValue = (string)Element?.GetValue(AutomationProperties.NameProperty);
			if (string.IsNullOrWhiteSpace(elemValue) && Control?.AccessibilityLabel == Control?.Title(UIControlState.Normal))
				return;

			base.SetAccessibilityLabel();
		}

		bool IImageVisualElementRenderer.IsDisposed => _isDisposed;
		void IImageVisualElementRenderer.SetImage(UIImage image)
		{
			Control.SetImage(image?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal), UIControlState.Normal);
			Control.HorizontalAlignment = UIControlContentHorizontalAlignment.Fill;
			Control.VerticalAlignment = UIControlContentVerticalAlignment.Fill;
		}

		UIImageView IImageVisualElementRenderer.GetImage()
		{
			return Control?.ImageView;
		}
	}
}
