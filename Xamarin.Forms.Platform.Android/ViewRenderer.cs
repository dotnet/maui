using System;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Xamarin.Forms.Platform.Android.FastRenderers;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public interface IViewRenderer
	{
		void MeasureExactly();
	}

	public abstract class ViewRenderer : ViewRenderer<View, AView>
	{
		protected ViewRenderer(Context context) : base(context)
		{
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use ViewRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected ViewRenderer()

		{
		}
	}

	public abstract class ViewRenderer<TView, TNativeView> : VisualElementRenderer<TView>, IViewRenderer, ITabStop, AView.IOnFocusChangeListener where TView : View where TNativeView : AView
	{
		protected ViewRenderer(Context context) : base(context)
		{
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use ViewRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected ViewRenderer()
		{

		}

		protected virtual TNativeView CreateNativeControl()
		{
			return default(TNativeView);
		}

		ViewGroup _container;
		string _defaultContentDescription;
		bool? _defaultFocusable;
		ImportantForAccessibility? _defaultImportantForAccessibility;
		string _defaultHint;

		bool _disposed;
		EventHandler<VisualElement.FocusRequestArgs> _focusChangeHandler;

		SoftInput _startingInputMode;

		public TNativeView Control { get; private set; }
		protected virtual AView ControlUsedForAutomation => Control;

		AView ITabStop.TabStop => Control;

		void IViewRenderer.MeasureExactly()
		{
			MeasureExactly(Control, Element, Context);
		}

		// This is static so it's also available for use by the fast renderers
		internal static void MeasureExactly(AView control, VisualElement element, Context context)
		{
			if (control == null || element == null)
			{
				return;
			}

			var width = element.Width;
			var height = element.Height;

			if (width <= 0 || height <= 0)
			{
				return;
			}

			var realWidth = (int)context.ToPixels(width);
			var realHeight = (int)context.ToPixels(height);

			var widthMeasureSpec = MeasureSpecFactory.MakeMeasureSpec(realWidth, MeasureSpecMode.Exactly);
			var heightMeasureSpec = MeasureSpecFactory.MakeMeasureSpec(realHeight, MeasureSpecMode.Exactly);

			control.Measure(widthMeasureSpec, heightMeasureSpec);
		}

		void AView.IOnFocusChangeListener.OnFocusChange(AView v, bool hasFocus)
		{
			if (Element is Entry || Element is SearchBar || Element is Editor)
			{
				var isInViewCell = false;
				Element parent = Element.RealParent;
				while (!(parent is Page) && parent != null)
				{
					if (parent is Cell)
					{
						isInViewCell = true;
						break;
					}
					parent = parent.RealParent;
				}

				if (isInViewCell)
				{
					Window window = Context.GetActivity().Window;
					if (hasFocus)
					{
						_startingInputMode = window.Attributes.SoftInputMode;
						window.SetSoftInputMode(SoftInput.AdjustPan);
					}
					else
						window.SetSoftInputMode(_startingInputMode);
				}
			}

			OnNativeFocusChanged(hasFocus);

			((IElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, hasFocus);
		}

		public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			if (Control == null)
				return (base.GetDesiredSize(widthConstraint, heightConstraint));

			AView view = _container == this ? (AView)Control : _container;
			view.Measure(widthConstraint, heightConstraint);

			return new SizeRequest(new Size(Control.MeasuredWidth, Control.MeasuredHeight), MinimumSize());
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				if (Control != null && ManageNativeControlLifetime)
				{
					Control.OnFocusChangeListener = null;
				}

				if (Element != null && _focusChangeHandler != null)
				{
					Element.FocusChangeRequested -= _focusChangeHandler;
				}
				_focusChangeHandler = null;
			}

			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				if (_container != null && _container != this)
				{
					if (_container.Handle != IntPtr.Zero)
					{
						_container.RemoveFromParent();
						_container.Dispose();
					}
					_container = null;
				}
				_disposed = true;
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TView> e)
		{
			base.OnElementChanged(e);

			if (_focusChangeHandler == null)
				_focusChangeHandler = OnFocusChangeRequested;

			if (e.OldElement != null)
				e.OldElement.FocusChangeRequested -= _focusChangeHandler;

			if (e.NewElement != null)
				e.NewElement.FocusChangeRequested += _focusChangeHandler;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateIsEnabled();
			else if (e.PropertyName == AutomationProperties.LabeledByProperty.PropertyName)
				SetLabeledBy();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);
			if (Control == null)
				return;

			AView view = _container == this ? (AView)Control : _container;

			view.Measure(MeasureSpecFactory.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly), MeasureSpecFactory.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly));
			view.Layout(0, 0, r - l, b - t);
		}

		protected override void OnRegisterEffect(PlatformEffect effect)
		{
			base.OnRegisterEffect(effect);
			effect.SetControl(Control);
		}

		protected override void SetAutomationId(string id)
		{
			if (Control == null)
			{
				base.SetAutomationId(id);
				return;
			}

			ContentDescription = id + "_Container";
			AutomationPropertiesProvider.SetAutomationId(ControlUsedForAutomation, Element, id);
		}

		protected override void SetContentDescription()
		{
			if (Control == null)
			{
				base.SetContentDescription();
				return;
			}

			AutomationPropertiesProvider.SetContentDescription(
				ControlUsedForAutomation, Element, ref _defaultContentDescription, ref _defaultHint);
		}

		protected override void SetFocusable()
		{
			if (Control == null)
			{
				base.SetFocusable();
				return;
			}

			AutomationPropertiesProvider.SetFocusable(ControlUsedForAutomation, Element, ref _defaultFocusable, ref _defaultImportantForAccessibility);
		}

		protected void SetNativeControl(TNativeView control)
		{
			SetNativeControl(control, this);
		}

		protected virtual void OnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			if (Control == null)
				return;

			e.Result = true;

			if (e.Focus)
			{
				// Android does the actual focus/unfocus work on the main looper
				// So in case we're setting the focus in response to another control's un-focusing,
				// we need to post the handling of it to the main looper so that it happens _after_ all the other focus
				// work is done; otherwise, a call to ClearFocus on another control will kill the focus we set here
				Device.BeginInvokeOnMainThread(() =>
				{
					if (Control == null || Control.IsDisposed())
						return;

					if (Control is IPopupTrigger popupElement)
						popupElement.ShowPopupOnFocus = true;

					Control?.RequestFocus();
				});
			}
			else
			{
				Control.ClearFocus();
			}
		}

		internal virtual void OnNativeFocusChanged(bool hasFocus)
		{
		}

		internal override void SendVisualElementInitialized(VisualElement element, AView nativeView)
		{
			base.SendVisualElementInitialized(element, Control);
		}

		internal void SetNativeControl(TNativeView control, ViewGroup container)
		{
			if (Control != null)
			{
				Control.OnFocusChangeListener = null;
				RemoveView(Control);
			}

			_container = container;

			Control = control;
			if (Control.Id == NoId)
			{
				Control.Id = Platform.GenerateViewId();
			}

			AView toAdd = container == this ? control : (AView)container;
			AddView(toAdd, LayoutParams.MatchParent);

			Control.OnFocusChangeListener = this;

			UpdateIsEnabled();
			UpdateFlowDirection();
			SetLabeledBy();
		}

		void SetLabeledBy()
			=> AutomationPropertiesProvider.SetLabeledBy(Control, Element);

		void UpdateIsEnabled()
		{
			if (Control != null)
				Control.Enabled = Element.IsEnabled;
		}

		void UpdateFlowDirection()
		{
			Control.UpdateFlowDirection(Element);
		}
	}
}