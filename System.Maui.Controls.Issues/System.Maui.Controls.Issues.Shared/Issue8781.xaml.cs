using System.Maui.Internals;
using System.Maui.Xaml;
using System.Maui.CustomAttributes;
using System;
using System.Windows.Input;
using System.Collections.Generic;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using System.Maui.Core.UITests;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CarouselView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8781, "SwipeViewItem rendering issue on iOS", PlatformAffected.iOS)]
	public partial class Issue8781 : ContentPage
	{
		public Issue8781()
		{
#if APP
			Device.SetFlags(new List<string> { ExperimentalFlags.SwipeViewExperimental });
			InitializeComponent();
			BindingContext = this;
#endif
		}
#if APP
		public ICommand CheckAnswerCommand => new Command<string>(OnCheckAnswer);

		async void OnCheckAnswer(string parameter)
		{
			if (string.IsNullOrEmpty(parameter))
				return;

			if (parameter.Equals("4", StringComparison.InvariantCultureIgnoreCase))
			{
				resultEntry.Text = string.Empty;
				swipeView.Close();
				await DisplayAlert("Correct!", "The answer is 4.", "OK");
			}
			else
			{
				resultEntry.Text = string.Empty;
				await DisplayAlert("Incorrect!", "Try again.", "OK");
			}
		}
#endif
	}
}