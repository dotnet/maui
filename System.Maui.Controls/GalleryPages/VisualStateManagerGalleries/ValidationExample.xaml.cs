using System;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.VisualStateManagerGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ValidationExample : ContentPage
	{
		public ValidationExample ()
		{
			InitializeComponent ();

			Message.TextChanged += EditorTextChanged;
		}

		void EditorTextChanged(object sender, TextChangedEventArgs e)
		{
			var count = e.NewTextValue.Length;

			CharacterCount.Text = $"{count} characters";
			CheckMessageValid();
		}

		bool IsValid()
		{
			var messageValid = CheckMessageValid();
			var subjectValid = CheckSubjectValid();

			return messageValid && subjectValid;
		}

		bool CheckMessageValid()
		{
			var isValid = Message.Text == null || Message.Text.Length <= 40;
			var state = isValid ? "Normal" : "Invalid";

			VisualStateManager.GoToState(Message, state);
			VisualStateManager.GoToState(MessageError, state);
			VisualStateManager.GoToState(CharacterCount, state);
			VisualStateManager.GoToState(MessageLabel, state);

			return isValid;
		}

		bool CheckSubjectValid()
		{
			var isValid = Subject.Text?.Length > 0;
			var state = isValid ? "Normal" : "Invalid";

			VisualStateManager.GoToState(Subject, state);
			VisualStateManager.GoToState(SubjectError, state);
			VisualStateManager.GoToState(SubjectLabel, state);

			return isValid;
		}

		void Submit_OnClicked(object sender, EventArgs e)
		{
			if (IsValid())
			{
				DisplayAlert("Submitted", "Thank you for submitting", "OK");
			}
		}
	}
}