using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class LayoutIsEnabledPage : ContentPage
	{
		bool isLayoutEnabled;
		bool isButtonEnabled;
		bool isCommandEnabled;

		public LayoutIsEnabledPage()
		{
			InitializeComponent();

			TheCommand = new Command(OnThe, OnTheCanExecute);
			BindingContext = this;
		}

		public bool IsLayoutEnabled
		{
			get => isLayoutEnabled;
			set
			{
				isLayoutEnabled = value;
				OnPropertyChanged();
			}
		}

		public bool IsButtonEnabled
		{
			get => isButtonEnabled;
			set
			{
				isButtonEnabled = value;
				OnPropertyChanged();
			}
		}

		public bool IsCommandEnabled
		{
			get => isCommandEnabled;
			set
			{
				isCommandEnabled = value;
				OnPropertyChanged();
				TheCommand.ChangeCanExecute();
			}
		}

		public Command TheCommand { get; }

		void OnDisableLayoutBtnClicked(object? sender, EventArgs e)
		{
			MainLayout.IsEnabled = !MainLayout.IsEnabled;

			((Button)sender!).Text = MainLayout.IsEnabled ? "Disable Layout" : "Enable Layout";
		}

		void OnDisableButtonBtnClicked(object? sender, EventArgs e)
		{
			DisabledButton.IsEnabled = !DisabledButton.IsEnabled;
			DisabledCommandButton.IsEnabled = !DisabledCommandButton.IsEnabled;

			DisabledButton.Text = DisabledButton.IsEnabled ? "Enabled" : "Disabled";
			DisabledCommandButton.Text = DisabledCommandButton.IsEnabled ? "Enabled" : "Disabled";

			((Button)sender!).Text = DisabledButton.IsEnabled ? "Disable Button" : "Enable Button";
		}

		void OnThe()
		{
			System.Diagnostics.Debug.WriteLine("On THE clicked!");
		}

		bool OnTheCanExecute() => isCommandEnabled;
	}
}