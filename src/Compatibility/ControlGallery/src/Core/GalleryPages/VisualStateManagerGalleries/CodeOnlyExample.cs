using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.VisualStateManagerGalleries
{
	public class CodeOnlyExample : ContentPage
	{
		public CodeOnlyExample()
		{
			var layout = new StackLayout();

			var calendar = new DatePicker();

			VisualStateManager.SetVisualStateGroups(calendar, new VisualStateGroupList { SetUpMonths() });

			calendar.DateSelected += CalendarOnDateSelected;
			calendar.Date = new DateTime(2017, 12, 25);

			layout.Children.Add(calendar);

			var context = new Label { Text = "The DatePicker above changes its colors based on the selected date's month. The colors are all VisualStates created and added in code." };
			layout.Children.Add(context);

			Content = layout;
		}

		static void CalendarOnDateSelected(object o, DateChangedEventArgs dateChangedEventArgs)
		{
			if (o is DatePicker dp)
			{
				VisualStateManager.GoToState(dp, dateChangedEventArgs.NewDate.Month.ToString());
			}
		}

		VisualStateGroup SetUpMonths()
		{
			var stateGroup = new VisualStateGroup { Name = "Months", TargetType = typeof(DatePicker) };

			stateGroup.States.Add(CreateState(1, Colors.CornflowerBlue, Colors.AliceBlue));
			stateGroup.States.Add(CreateState(2, Colors.Red, Colors.WhiteSmoke));
			stateGroup.States.Add(CreateState(3, Colors.GreenYellow, Colors.ForestGreen));
			stateGroup.States.Add(CreateState(4, Colors.GreenYellow, Colors.SaddleBrown));
			stateGroup.States.Add(CreateState(5, Colors.White, Colors.Red));
			stateGroup.States.Add(CreateState(6, Colors.RoyalBlue, Colors.DarkOrange));
			stateGroup.States.Add(CreateState(7, Colors.Red, Colors.DeepSkyBlue));
			stateGroup.States.Add(CreateState(8, Colors.DarkRed, Colors.AliceBlue));
			stateGroup.States.Add(CreateState(9, Colors.SaddleBrown, Colors.AntiqueWhite));
			stateGroup.States.Add(CreateState(10, Colors.Orange, Colors.Black));
			stateGroup.States.Add(CreateState(11, Colors.OrangeRed, Colors.SaddleBrown));
			stateGroup.States.Add(CreateState(12, Colors.Green, Colors.Red));

			return stateGroup;
		}

		static VisualState CreateState(int month, Color textColor, Color backgroundColor)
		{
			var textColorSetter = new Setter { Value = textColor, Property = DatePicker.TextColorProperty };
			var backColorSetter = new Setter { Value = backgroundColor, Property = BackgroundColorProperty };

			return new VisualState
			{
				Name = month.ToString(),
				TargetType = typeof(DatePicker),
				Setters = { textColorSetter, backColorSetter }
			};
		}
	}
}