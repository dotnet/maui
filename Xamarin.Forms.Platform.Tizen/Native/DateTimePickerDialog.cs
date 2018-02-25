using System;
using ElmSharp;
using EButton = ElmSharp.Button;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class DateTimePickerDialog<T> : Dialog where T : DateTimeSelector
	{
		T _dateTimePicker;
		EvasObject _parent;

		/// <summary>
		/// Creates a dialog window.
		/// </summary>
		public DateTimePickerDialog(EvasObject parent) : base(parent)
		{
			_parent = parent;
			Initialize();
		}

		/// <summary>
		/// Occurs when the date of this dialog has changed.
		/// </summary>
		public event EventHandler<DateChangedEventArgs> DateTimeChanged;

		/// <summary>
		/// Gets the <see cref="DateTimePicker"/> contained in this dialog.
		/// </summary>
		public T DateTimePicker
		{
			get
			{
				return _dateTimePicker;
			}
			private set
			{
				if (_dateTimePicker != value)
				{
					ApplyDateTimePicker(value);
				}
			}
		}

		void ApplyDateTimePicker(T dateTimePicker)
		{
			_dateTimePicker = dateTimePicker;
			Content = _dateTimePicker;
		}

		void Initialize()
		{
			DateTimePicker = (T)Activator.CreateInstance(typeof(T), new object[] { _parent });

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
			};
			BackButtonPressed += (object s, EventArgs e) =>
			{
				Hide();
			};

			// TODO This is Tizen TV Limitation.
			// UX is defined and the focus move processing is complete, it should be removed(After Tizen 5.0)
			if (Device.Idiom == TargetIdiom.TV)
			{
				KeyDown += (s, e) =>
				{
					if (e.KeyName == "Return")
					{
						if (DateTimePicker != null && DateTimePicker.IsFocused)
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
			DateTime oldDate = DateTimePicker.DateTime;
			DateTimeChanged?.Invoke(this, new DateChangedEventArgs(oldDate, DateTimePicker.DateTime));
			Hide();
		}
	}
}
