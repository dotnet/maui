using System;
using Xamarin.Forms.Platform.Tizen.Native;
using EEntry = ElmSharp.Entry;
using Specific = Xamarin.Forms.PlatformConfiguration.TizenSpecific.Application;
using WatchDateTimePickerDialog = Xamarin.Forms.Platform.Tizen.Native.Watch.WatchDateTimePickerDialog;

namespace Xamarin.Forms.Platform.Tizen
{
	public class DatePickerRenderer : ViewRenderer<DatePicker, EEntry>
	{
		//TODO need to add internationalization support
		const string DialogTitle = "Choose Date";
		Lazy<IDateTimeDialog> _lazyDialog;

		public DatePickerRenderer()
		{
			RegisterPropertyHandler(DatePicker.DateProperty, UpdateDate);
			RegisterPropertyHandler(DatePicker.FormatProperty, UpdateDate);
			RegisterPropertyHandler(DatePicker.TextColorProperty, UpdateTextColor);
			RegisterPropertyHandler(DatePicker.FontAttributesProperty, UpdateFontAttributes);
			RegisterPropertyHandler(DatePicker.FontFamilyProperty, UpdateFontFamily);
			RegisterPropertyHandler(DatePicker.FontSizeProperty, UpdateFontSize);
		}

		protected virtual IDateTimeDialog CreateDialog()
		{
			if (Device.Idiom == TargetIdiom.Watch)
			{
				return new WatchDateTimePickerDialog(Forms.NativeParent);
			}
			else
			{
				return new DateTimePickerDialog(Forms.NativeParent);
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
		{
			if (Control == null)
			{
				var entry = CreateNativeControl();
				entry.SetVerticalTextAlignment(0.5);
				SetNativeControl(entry);

				if (entry is IEntry ie)
				{
					ie.TextBlockFocused += OnTextBlockFocused;
				}

				_lazyDialog = new Lazy<IDateTimeDialog>(() =>
				{
					var dialog = CreateDialog();
					dialog.Title = DialogTitle;
					dialog.DateTimeChanged += OnDateTimeChanged;
					dialog.PickerOpened += OnPickerOpened;
					dialog.PickerClosed += OnPickerClosed;
					return dialog;
				});
			}
			base.OnElementChanged(e);
		}

		protected virtual EEntry CreateNativeControl()
		{
			return new Native.EditfieldEntry(Forms.NativeParent)
			{
				IsSingleLine = true,
				HorizontalTextAlignment = Native.TextAlignment.Center,
				InputPanelShowByOnDemand = true,
				IsEditable = false
			};
		}

		protected override Size MinimumSize()
		{
			if (Control is IMeasurable im)
			{
				return im.Measure(Control.MinimumWidth, Control.MinimumHeight).ToDP();
			}
			else
			{
				return base.MinimumSize();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					if (Control is IEntry ie)
					{
						ie.TextBlockFocused -= OnTextBlockFocused;
					}
				}
				if (_lazyDialog.IsValueCreated)
				{
					_lazyDialog.Value.DateTimeChanged -= OnDateTimeChanged;
					_lazyDialog.Value.PickerOpened -= OnPickerOpened;
					_lazyDialog.Value.PickerClosed -= OnPickerClosed;
					_lazyDialog.Value.Unrealize();
				}
			}
			base.Dispose(disposing);
		}

		void OnTextBlockFocused(object sender, EventArgs e)
		{
			// For EFL Entry, the event will occur even if it is currently disabled.
			// If the problem is resolved, no conditional statement is required.
			if (Element.IsEnabled)
			{
				var dialog = _lazyDialog.Value;
				dialog.DateTime = Element.Date;
				dialog.MaximumDateTime = Element.MaximumDate;
				dialog.MinimumDateTime = Element.MinimumDate;
				// You need to call Show() after ui thread occupation because of EFL problem.
				// Otherwise, the content of the popup will not receive focus.
				Device.BeginInvokeOnMainThread(() => dialog.Show());
			}
		}

		protected virtual void OnDateTimeChanged(object sender, Native.DateChangedEventArgs dcea)
		{
			Element.Date = dcea.NewDate;
			Control.Text = dcea.NewDate.ToString(Element.Format);
		}

		protected virtual void UpdateDate()
		{
			Control.Text = Element.Date.ToString(Element.Format);
		}

		protected virtual void UpdateTextColor()
		{
			if (Control is IEntry ie)
			{
				ie.TextColor = Element.TextColor.ToNative();
			}
		}

		protected virtual void OnPickerOpened(object sender, EventArgs args)
		{
			if (Specific.GetUseBezelInteraction(Application.Current))
			{
				// picker included in WatchDatePickedDialog has been activated, whenever the dialog is opend.
				Forms.RotaryFocusObject = Element;
				Specific.SetActiveBezelInteractionElement(Application.Current, Element);
			}
		}

		protected virtual void OnPickerClosed(object sender, EventArgs args)
		{
			if (Specific.GetUseBezelInteraction(Application.Current))
			{
				if (Forms.RotaryFocusObject == Element)
					Forms.RotaryFocusObject = null;
				if (Specific.GetActiveBezelInteractionElement(Application.Current) == Element)
					Specific.SetActiveBezelInteractionElement(Application.Current, null);
			}
		}

		void UpdateFontSize()
		{
			if (Control is IEntry ie)
			{
				ie.FontSize = Element.FontSize;
			}
		}

		void UpdateFontFamily()
		{
			if (Control is IEntry ie)
			{
				ie.FontFamily = Element.FontFamily;
			}
		}

		void UpdateFontAttributes()
		{
			if (Control is IEntry ie)
			{
				ie.FontAttributes = Element.FontAttributes;
			}
		}
	}
}
