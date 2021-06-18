using System;
using Microsoft.Maui.Controls;

namespace MauiProfilingApp
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

			label.Text = $"IntPtr.Size == " + IntPtr.Size;
			Console.WriteLine($"IntPtr.Size == " + IntPtr.Size);
			Console.WriteLine($"DOTNET_DiagnosticPorts == " + Environment.GetEnvironmentVariable("DOTNET_DiagnosticPorts"));
		}
	}
}
