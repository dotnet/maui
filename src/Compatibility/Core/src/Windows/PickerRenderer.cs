using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Core;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public partial class PickerRenderer : ViewRenderer<Picker, FormsComboBox>
	{
		bool _fontApplied;
		bool _isAnimating;
		WBrush _defaultBrush;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					_isAnimating = false;
					Control.SelectionChanged -= OnControlSelectionChanged;
					Control.DropDownOpened -= OnDropDownOpenStateChanged;
					Control.DropDownClosed -= OnDropDownOpenStateChanged;
					Control.OpenAnimationCompleted -= ControlOnOpenAnimationCompleted;
					Control.Loaded -= ControlOnLoaded;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new FormsComboBox());
					Control.SelectionChanged += OnControlSelectionChanged;
					Control.DropDownOpened += OnDropDownOpenStateChanged;
					Control.DropDownClosed += OnDropDownOpenStateChanged;
					Control.OpenAnimationCompleted += ControlOnOpenAnimationCompleted;
					Control.ClosedAnimationStarted += ControlOnClosedAnimationStarted;
					Control.Loaded += ControlOnLoaded;
				}
				else
				{
					WireUpFormsVsm();
				}

				Control.ItemsSource = ((LockableObservableListWrapper)Element.Items)._list;
				UpdateTitle();
				UpdateSelectedIndex();
				UpdateCharacterSpacing();
				UpdateHorizontalTextAlignment();
				UpdateVerticalTextAlignment();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Picker.SelectedIndexProperty.PropertyName)
				UpdateSelectedIndex();
			else if (e.PropertyName == Picker.TitleProperty.PropertyName || e.PropertyName == Picker.TitleColorProperty.PropertyName)
				UpdateTitle();
			else if (e.PropertyName == Picker.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == Picker.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Picker.FontAttributesProperty.PropertyName || e.PropertyName == Picker.FontFamilyProperty.PropertyName || e.PropertyName == Picker.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Picker.HorizontalTextAlignmentProperty.PropertyName || e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateHorizontalTextAlignment();
			else if (e.PropertyName == Picker.VerticalTextAlignmentProperty.PropertyName)
				UpdateVerticalTextAlignment();
		}

		void ControlOnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			WireUpFormsVsm();

			// The defaults from the control template won't be available
			// right away; we have to wait until after the template has been applied
			_defaultBrush = Control.Foreground;
			UpdateFont();
			UpdateTextColor();
		}

		void WireUpFormsVsm()
		{
			if (Element.UseFormsVsm())
			{
				InterceptVisualStateManager.Hook(Control.GetFirstDescendant<Microsoft.UI.Xaml.Controls.Grid>(), Control, Element);
			}
		}

		void ControlOnClosedAnimationStarted(object sender, EventArgs eventArgs)
		{
			if (!Control.IsFullScreen)
			{
				// Start refreshing while the control's closing animation runs;
				// OnDropDownOpenStateChanged will take care of stopping the refresh
				StartAnimationRefresh();
			}
		}

		void ControlOnOpenAnimationCompleted(object sender, EventArgs eventArgs)
		{
			_isAnimating = false;
			if (!Control.IsFullScreen)
			{
				// Force a final redraw after the closing animation has completed
				((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.MeasureChanged);
			}
		}

		[PortHandler]
		void OnControlSelectionChanged(object sender, WSelectionChangedEventArgs e)
		{
			Element?.SelectedIndex = Control.SelectedIndex;
		}

		void OnDropDownOpenStateChanged(object sender, object o)
		{
			if (Control.IsDropDownOpen)
			{
				if (Control.IsOpeningAnimated && !Control.IsFullScreen)
				{
					// Start running the animation refresh; 
					// ControlOnOpenAnimationCompleted will take care of stopping it
					StartAnimationRefresh();
				}
				else
				{
					((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.MeasureChanged);
				}
			}
			else
			{
				// The ComboBox is now closed; if we were animating the closure, stop
				_isAnimating = false;
				// and force the final redraw
				((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.MeasureChanged);
			}
		}

		/// <summary>
		///     Forces redraw of the control during opening/closing animations to provide
		///     a smoother sliding animation for the surrounding controls
		///     Only applies on the phone and only when there are fewer than 6 items in the picker
		/// </summary>
		void StartAnimationRefresh()
		{
			_isAnimating = true;
			Task.Factory.StartNew(async () =>
			{
				while (_isAnimating)
				{
					await Task.Delay(16);
					await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.MeasureChanged));
				}
			});
		}

		[PortHandler]
		void UpdateCharacterSpacing()
		{
			Control.CharacterSpacing = Element.CharacterSpacing.ToEm();
		}

		[PortHandler]
		void UpdateFont()
		{
			if (Control == null)
				return;

			Picker picker = Element;

			if (picker == null)
				return;

			bool pickerIsDefault =
				picker.FontFamily == null &&
#pragma warning disable CS0612 // Type or member is obsolete
				picker.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Picker), true) &&
#pragma warning restore CS0612 // Type or member is obsolete
				picker.FontAttributes == FontAttributes.None;

			if (pickerIsDefault && !_fontApplied)
				return;

			if (pickerIsDefault)
			{
				// ReSharper disable AccessToStaticMemberViaDerivedType
				Control.ClearValue(ComboBox.FontStyleProperty);
				Control.ClearValue(ComboBox.FontSizeProperty);
				Control.ClearValue(ComboBox.FontFamilyProperty);
				Control.ClearValue(ComboBox.FontWeightProperty);
				Control.ClearValue(ComboBox.FontStretchProperty);
				// ReSharper restore AccessToStaticMemberViaDerivedType
			}
			else
			{
				Control.ApplyFont(picker);
			}

			_fontApplied = true;
		}

		[PortHandler]
		void UpdateSelectedIndex()
		{
			Control.SelectedIndex = Element.SelectedIndex;
		}

		[PortHandler]
		void UpdateTextColor()
		{
			Color color = Element.TextColor;
			Control.Foreground = color.IsDefault() ? (_defaultBrush ?? color.ToPlatform()) : color.ToPlatform();
		}

		[PortHandler]
		void UpdateTitle()
		{
			Control.Header = null;
			Control.HeaderTemplate = string.IsNullOrEmpty(Element.Title) ? null : (Microsoft.UI.Xaml.DataTemplate)Microsoft.UI.Xaml.Application.Current.Resources["ComboBoxHeader"];
			Control.DataContext = Element;
		}

		[PortHandler]
		void UpdateHorizontalTextAlignment()
		{
			Control.HorizontalContentAlignment = Element.HorizontalTextAlignment.ToPlatformHorizontalAlignment();
		}
		[PortHandler]
		void UpdateVerticalTextAlignment()
		{
			Control.VerticalContentAlignment = Element.VerticalTextAlignment.ToPlatformVerticalAlignment();
		}
	}
}