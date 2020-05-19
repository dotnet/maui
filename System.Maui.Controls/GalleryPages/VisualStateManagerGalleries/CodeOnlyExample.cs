using System;

namespace Xamarin.Forms.Controls.GalleryPages.VisualStateManagerGalleries
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

			stateGroup.States.Add(CreateState(1, Color.CornflowerBlue, Color.AliceBlue));
			stateGroup.States.Add(CreateState(2, Color.Red, Color.WhiteSmoke));
			stateGroup.States.Add(CreateState(3, Color.GreenYellow, Color.ForestGreen));
			stateGroup.States.Add(CreateState(4, Color.GreenYellow, Color.SaddleBrown));
			stateGroup.States.Add(CreateState(5, Color.White, Color.Red));
			stateGroup.States.Add(CreateState(6, Color.RoyalBlue, Color.DarkOrange));
			stateGroup.States.Add(CreateState(7, Color.Red, Color.DeepSkyBlue));
			stateGroup.States.Add(CreateState(8, Color.DarkRed, Color.AliceBlue));
			stateGroup.States.Add(CreateState(9, Color.SaddleBrown, Color.AntiqueWhite));
			stateGroup.States.Add(CreateState(10, Color.Orange, Color.Black));
			stateGroup.States.Add(CreateState(11, Color.OrangeRed, Color.SaddleBrown));
			stateGroup.States.Add(CreateState(12, Color.Green, Color.Red));

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