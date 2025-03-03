using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19127, "Triggers are not working on Frame control", PlatformAffected.iOS)]
	public partial class Issue19127 : ContentPage
	{
		public Issue19127()
		{
			InitializeComponent();
			BindingContext = new Issue19127Settings();
		}
	}

	public class Issue19127Settings : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public Command EnableCamera { get; set; }

		private bool _isCameraEnabled;
		public bool IsCameraEnabled
		{
			get { return _isCameraEnabled; }
			set
			{
				_isCameraEnabled = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCameraEnabled)));
			}
		}

		public Issue19127Settings()
		{
			IsCameraEnabled = false;
			EnableCamera = new Command(() => ToggleEnableCamera());
		}

		private void ToggleEnableCamera()
		{
			IsCameraEnabled = !_isCameraEnabled;
		}
	}
}