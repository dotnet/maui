using System;
using System.Collections.Generic;
using System.Reflection;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Navigation)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9767, "[Bug] [iOS] NavigationBar resetting TextColor to black at every change of BarBackgroundColor", PlatformAffected.iOS)]
	public class Issue9767 : TestNavigationPage
	{
		public Issue9767()
		{
			var page = new ContentPage
			{
				Title = "Issue 9767"
			};

			var layout = new StackLayout();

			var updateBarBackgroundButton = new Button
			{
				Text = "Update BarBackgroundColor"
			};

			updateBarBackgroundButton.Clicked += (sender, args) =>
			{
				BarBackgroundColor = GetRandomColor();
			};

			var updateBarTextButton = new Button
			{
				Text = "Update BarTextColor"
			};

			updateBarTextButton.Clicked += (sender, args) =>
			{
				BarTextColor = GetRandomColor();
			};

			var resetBarBackgroundButton = new Button
			{
				Text = "Reset BarBackgroundColor"
			};

			resetBarBackgroundButton.Clicked += (sender, args) =>
			{
				BarBackgroundColor = Color.Default;
			};

			var resetBarTextButton = new Button
			{
				Text = "Reset BarTextColor"
			};

			resetBarTextButton.Clicked += (sender, args) =>
			{
				BarTextColor = Color.Default;
			};

			var navigateButton = new Button
			{
				Text = "Navigate"
			};

			navigateButton.Clicked += (sender, args) =>
			{
				Navigation.PushAsync(new Issue9767NavigationPage(new Issue9767()));
			};

			layout.Children.Add(updateBarBackgroundButton);
			layout.Children.Add(updateBarTextButton);
			layout.Children.Add(resetBarBackgroundButton);
			layout.Children.Add(resetBarTextButton);
			layout.Children.Add(navigateButton);

			page.Content = layout;

			PushAsync(page);
		}

		protected override void Init()
		{

		}

		Color GetRandomColor()
		{
			var colors = new List<string>();

			foreach (var field in typeof(Color).GetFields(BindingFlags.Static | BindingFlags.Public))
			{
				if (field != null && !string.IsNullOrEmpty(field.Name))
					colors.Add(field.Name);
			}

			Random random = new Random();
			var randomColorName = colors[random.Next(colors.Count)];

			var colorConverter = new ColorTypeConverter();
			var result = colorConverter.ConvertFromInvariantString(randomColorName);

			return (Color)result;
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue9767NavigationPage : NavigationPage
	{
		public Issue9767NavigationPage()
		{

		}

		public Issue9767NavigationPage(Page root) : base(root)
		{

		}
	}
}