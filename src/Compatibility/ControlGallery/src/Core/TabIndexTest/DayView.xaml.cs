using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.TabIndexTest
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DayView : ContentView
	{
		Color _buttonBackgroundColor;
		public Color ButtonBackgroundColor
		{
			get => _buttonBackgroundColor;
			set
			{
				_buttonBackgroundColor = value;
				OnPropertyChanged(nameof(ButtonBackgroundColor));
			}
		}

		Color _textColor;
		public Color TextColor
		{
			get => _textColor;
			set
			{
				_textColor = value;
				OnPropertyChanged(nameof(TextColor));
			}
		}

		public static readonly BindableProperty DayProperty = BindableProperty.Create(
			nameof(Day),
			typeof(DaysOfWeek),
			typeof(DayView),
			DaysOfWeek.None);

		public DaysOfWeek Day
		{
			get => (DaysOfWeek)GetValue(DayProperty);
			set => SetValue(DayProperty, value);
		}

		public static readonly BindableProperty DayTouchedProperty = BindableProperty.Create(
			nameof(DayTouched),
			typeof(Command<DaysOfWeek>),
			typeof(DayView),
			default(Command<DaysOfWeek>));

		public Command<DaysOfWeek> DayTouched
		{
			get => (Command<DaysOfWeek>)GetValue(DayTouchedProperty);
			set => SetValue(DayTouchedProperty, value);
		}

		public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
			nameof(IsSelected),
			typeof(bool),
			typeof(DayView),
			default(bool),
			propertyChanged: (bindable, value, newValue) =>
			{
				if (bindable is DayView view)
				{
					view.RenderSelection((bool)newValue);
				}
			});
		public bool IsSelected
		{
			get => (bool)GetValue(IsSelectedProperty);
			set => SetValue(IsSelectedProperty, value);
		}

		public ICommand TouchCommand { get; }

		public DayView()
		{
			TouchCommand = new Command(DoDayTouched);
			RenderSelection(false);
			InitializeComponent();
		}

		public void RenderSelection(bool selected)
		{
			if (selected)
			{
				ButtonBackgroundColor = Colors.Cerulean;
				TextColor = Colors.White;
			}
			else
			{
				ButtonBackgroundColor = Colors.Gray;
				TextColor = Colors.Black;
			}
		}

		public void Toggle()
		{
			IsSelected = !IsSelected;
			RenderSelection(IsSelected);
		}

		void DoDayTouched()
		{
			Toggle();
			DayTouched?.Execute(Day);
		}
	}
}