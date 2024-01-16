using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19689, "Java.Lang.IndexOutOfBoundsException: setSpan ( ... ) ends beyond length", PlatformAffected.Android)]
	public partial class Issue19689 : ContentPage
	{
		string _text = string.Empty;

		public Issue19689()
		{
			InitializeComponent();
			
			BindingContext = this;

			Text = "Bla";
		}

		public string Text
		{
			get => _text;
			set
			{
				if (value != _text)
				{
					_text = value;
					OnPropertyChanged();
				}
			}
		}
	}
}