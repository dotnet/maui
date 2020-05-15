using System;
using ElmSharp;
using EButton = ElmSharp.Button;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class DateTimePickerDialog : Dialog, IDateTimeDialog
	{
		EvasObject _parent;
		DateTimePicker _picker;

		/// <summary>
		/// Creates a dialog window.
		/// </summary>
		public DateTimePickerDialog(EvasObject parent) : base(parent)
		{
			_parent = parent;
			Initialize();
		}

		/// <summary>
		/// Gets or sets picker style
		/// </summary>
		public DateTimePickerMode Mode
		{
			get => _picker.Mode;
			set => _picker.Mode = value;
		}

		/// <summary>
		/// Gets or sets the upper boundary of the DateTime field.
		/// </summary>
		public DateTime MaximumDateTime
		{
			get => _picker.MaximumDateTime;
			set => _picker.MaximumDateTime = value;
		}

		/// <summary>
		/// Gets or sets the lower boundary of the DateTime field.
		/// </summary>
		public DateTime MinimumDateTime
		{
			get => _picker.MinimumDateTime;
			set => _picker.MinimumDateTime = value;
		}

		/// <summary>
		/// Gets or sets the current value of the DateTime field.
		/// </summary>
		public DateTime DateTime
		{
			get => _picker.DateTime;
			set => _picker.DateTime = value;
		}

		/// <summary>
		/// Occurs when the date of this dialog has changed.
		/// </summary>
		public event EventHandler<DateChangedEventArgs> DateTimeChanged;

		/// <summary>
		/// Occurs when the picker dialog has opened.
		/// </summary>
		public event EventHandler PickerOpened;

		/// <summary>
		/// Occurs when the picker dialog has closed.
		/// </summary>
		public event EventHandler PickerClosed;

		void Initialize()
		{
			_picker = new DateTimePicker(_parent);
			_picker.Show();
			Content = _picker;

			//TODO need to add internationalization support
			PositiveButton = new EButton(_parent) { Text = "Set" };
			PositiveButton.Clicked += (s, e) =>
			{
				Confirm();
			};

			//TODO need to add internationalization support
			NegativeButton = new EButton(_parent) { Text = "Cancel" };
			NegativeButton.Clicked += (s, e) =>
			{
				Hide();
				PickerClosed?.Invoke(this, EventArgs.Empty);
			};
			BackButtonPressed += (object s, EventArgs e) =>
			{
				Hide();
				PickerClosed?.Invoke(this, EventArgs.Empty);
			};

			ShowAnimationFinished += (object s, EventArgs e) =>
			{
				PickerOpened?.Invoke(this, EventArgs.Empty);
			};

			// TODO This is Tizen TV Limitation.
			// UX is defined and the focus move processing is complete, it should be removed(After Tizen 5.0)
			if (Device.Idiom == TargetIdiom.TV)
			{
				KeyDown += (s, e) =>
				{
					if (e.KeyName == "Return")
					{
						if (_picker != null && _picker.IsFocused)
						{
							Confirm();
							e.Flags |= EvasEventFlag.OnHold;
						}
					}
				};
			}
		}

		void Confirm()
		{
			DateTimeChanged?.Invoke(this, new DateChangedEventArgs(_picker.DateTime));
			Hide();
			PickerClosed?.Invoke(this, EventArgs.Empty);
		}
	}
}
