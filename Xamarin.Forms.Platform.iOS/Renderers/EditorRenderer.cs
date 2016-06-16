using System;
using System.Drawing;
using System.ComponentModel;
#if __UNIFIED__
using UIKit;
#else
using MonoTouch.UIKit;
#endif
#if __UNIFIED__
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using PointF = CoreGraphics.CGPoint;

#else
using nfloat=System.Single;
using nint=System.Int32;
using nuint=System.UInt32;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	public class EditorRenderer : ViewRenderer<Editor, UITextView>
	{
		UIToolbar _accessoryView;

		IElementController ElementController => Element as IElementController;

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (!Forms.IsiOS7OrNewer)
			{
				// Avoid crash iOS 6. iOS 6, I hate you. Why you no like Infinite size?
				return base.GetDesiredSize(Math.Min(widthConstraint, 2000), Math.Min(heightConstraint, 2000));
			}
			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Control.Changed -= HandleChanged;
				Control.Started -= OnStarted;
				Control.Ended -= OnEnded;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				SetNativeControl(new UITextView(RectangleF.Empty));

				if (Device.Idiom == TargetIdiom.Phone)
				{
					// iPhone does not have a dismiss keyboard button
					var keyboardWidth = UIScreen.MainScreen.Bounds.Width;
					_accessoryView = new UIToolbar(new RectangleF(0, 0, keyboardWidth, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };

					var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
					var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) =>
					{
						Control.ResignFirstResponder();
						Element.SendCompleted();
					});
					_accessoryView.SetItems(new[] { spacer, doneButton }, false);
					Control.InputAccessoryView = _accessoryView;
				}

				Control.Changed += HandleChanged;
				Control.Started += OnStarted;
				Control.Ended += OnEnded;
			}

			if (e.NewElement != null)
			{
				UpdateText();
				UpdateFont();
				UpdateTextColor();
				UpdateKeyboard();
				UpdateEditable();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Editor.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Xamarin.Forms.InputView.KeyboardProperty.PropertyName)
				UpdateKeyboard();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateEditable();
			else if (e.PropertyName == Editor.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Editor.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.FontSizeProperty.PropertyName)
				UpdateFont();
		}

		void HandleChanged(object sender, EventArgs e)
		{
			ElementController.SetValueFromRenderer(Editor.TextProperty, Control.Text);
		}

		void OnEnded(object sender, EventArgs eventArgs)
		{
			Element.SetValue(VisualElement.IsFocusedPropertyKey, false);
			Element.SendCompleted();
		}

		void OnStarted(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void UpdateEditable()
		{
			Control.Editable = Element.IsEnabled;
			Control.UserInteractionEnabled = Element.IsEnabled;

			if (Control.InputAccessoryView != null)
				Control.InputAccessoryView.Hidden = !Element.IsEnabled;
		}

		void UpdateFont()
		{
			Control.Font = Element.ToUIFont();
		}

		void UpdateKeyboard()
		{
			Control.ApplyKeyboard(Element.Keyboard);
		}

		void UpdateText()
		{
			// ReSharper disable once RedundantCheckBeforeAssignment
			if (Control.Text != Element.Text)
				Control.Text = Element.Text;
		}

		void UpdateTextColor()
		{
			var textColor = Element.TextColor;

			if (textColor.IsDefault)
				Control.TextColor = UIColor.Black;
			else
				Control.TextColor = textColor.ToUIColor();
		}
	}
}