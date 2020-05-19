using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using System.Windows.Input;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.SwipeView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
    [Preserve(AllMembers = true)]
    [Issue(IssueTracker.Github, 8779, "[iOS][Android] Entry in custom SwipeItemView can't gain focus", PlatformAffected.Android | PlatformAffected.iOS)]
    public partial class Issue8779 : TestContentPage
    {
        public Issue8779()
        {
#if APP
			Title = "Issue 8779";
			InitializeComponent();

			CheckAnswerCommand = new Command<string>(CheckAnswer);
            BindingContext = this;
#endif
        }

        public ICommand CheckAnswerCommand { get; private set; }

        protected override void Init()
        {

        }

#if APP
        async void OnIncorrectAnswerInvoked(object sender, EventArgs e)
        {
			((SwipeView)sender).Close();
            await DisplayAlert("Incorrect!", "Try again.", "OK");
        }

        async void OnCorrectAnswerInvoked(object sender, EventArgs e)
        {
			((SwipeView)sender).Close();
            await DisplayAlert("Correct!", "The answer is 4.", "OK");
        }

        void CheckAnswer(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
				Int32.TryParse(resultEntry.Text, out int number);

                if (number.Equals(4))
                    OnCorrectAnswerInvoked(swipeView, EventArgs.Empty);
                else
                    OnIncorrectAnswerInvoked(swipeView, EventArgs.Empty);

				resultEntry.Text = string.Empty;
            }
        }
#endif
    }
}
