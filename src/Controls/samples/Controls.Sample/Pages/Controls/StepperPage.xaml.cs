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

		void OnValueChanged(object sender, ValueChangedEventArgs args)
		{
			Debug.WriteLine($"Stepper Value: {args.NewValue}");
		}
	}
}