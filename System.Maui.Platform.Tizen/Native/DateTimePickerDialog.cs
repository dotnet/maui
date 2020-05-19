using System;
using ElmSharp;
using EButton = ElmSharp.Button;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class DateTimePickerDialog : Dialog, IDateTimeDialog
	{
		EvasObject _parent;
		/// <summary>
		/// Creates a dialog window.
		/// </summary>
		public DateTimePickerDialog(EvasObject parent) : base(parent)
		{
			_parent = parent;
			Initialize();
		}

		public DateTimePicker Picker { get; protected set; }

		/// <summary>
		/// Occurs when the date of this dialog has changed.
		/// </summary>
		public event EventHandler<DateChangedEventArgs> DateTimeChanged;

		protected virtual DateTimePicker CreatePicker(EvasObject parent)
		{
			return new DateTimePicker(parent);
		}

		void Initialize()
		{
			Picker = CreatePicker(_parent);
			Picker.Show();
			Content = Picker;

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
						if (Picker != null && Picker.IsFocused)
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
			DateTimeChanged?.Invoke(this, new DateChangedEventArgs(Picker.DateTime));
			Hide();
		}
	}
}
