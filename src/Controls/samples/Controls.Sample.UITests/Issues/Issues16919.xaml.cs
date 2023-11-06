using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 16919, "Shapes without handlers shouldn't be added as LogicalChildren", PlatformAffected.All)]
	public partial class Issue16919 : ContentPage
	{
		public Issue16919()
		{
			InitializeComponent();
		}

		void OnTestClicked(object sender, EventArgs e)
		{
			var strokeShape = TestBorder.StrokeShape as RoundRectangle;

			if (strokeShape is not null)
			{
				var handler = strokeShape.Handler;

				if (handler is not null)
					TestButton.Text = "Passed";
				else
					TestButton.Text = "Failed";
			}
		}
	}
}