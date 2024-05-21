using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 21109, "[Android] MAUI 8.0.3 -> 8.0.6 regression: custom handler with key listener no longer works", PlatformAffected.All)]
	public partial class Issue21109 : ContentPage
	{
		public Issue21109()
		{
			InitializeComponent();

			ReturnTypeResult.Text = $"ReturnType: {ReturnTypeEntry.ReturnType}";
		}

		void OnReturnTypeEntryTextChanged(object sender, TextChangedEventArgs e)
		{
			Random rnd = new Random();
			var returnTypeCount = Enum.GetNames(typeof(ReturnType)).Length;

			ReturnType returnType = ReturnType.Default;

			do
			{
				returnType = (ReturnType)rnd.Next(0, returnTypeCount);
			} while (returnType == ReturnTypeEntry.ReturnType);

			ReturnTypeEntry.ReturnType = returnType;
			ReturnTypeResult.Text = $"ReturnType: {returnType}";
		}
	}

	public class DecimalEntry : Entry
	{
		public DecimalEntry()
		{
			Keyboard = Keyboard.Numeric;
		}
	}
}