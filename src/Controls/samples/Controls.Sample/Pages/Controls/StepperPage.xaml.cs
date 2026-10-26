using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class StepperPage
	{
		public StepperPage()
		{
			InitializeComponent();
		}

		void OnValueChanged(object? sender, ValueChangedEventArgs args)
		{
			Debug.WriteLine($"Stepper Value: {args.NewValue}");
		}

		void OnEnableButtonClicked(object? sender, System.EventArgs e)
		{
			if (EnableStepper.IsEnabled)
			{
				EnableStepper.IsEnabled = false;
				EnableButton.Text = "Enable Stepper";
			}
			else
			{
				EnableStepper.IsEnabled = true;
				EnableButton.Text = "Disable Stepper";
			}
		}
	}
}