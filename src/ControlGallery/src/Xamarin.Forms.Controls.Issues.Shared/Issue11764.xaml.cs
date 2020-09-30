using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.SwipeView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11764, "[Bug] SwipeView iOS getting offset by 1px down and 1px right", PlatformAffected.iOS)]
	public partial class Issue11764 : TestContentPage
	{
		public Issue11764()
		{
#if APP
			Title = "Issue 11764";
			InitializeComponent();

			var random = new Random();
			for (var i = 0; i < 16; i++)
				Data.Add($"Entry #{i + 1} - {random.Next(0, 999999)}");

			TapCommand = new Command<string>(input => DisplayAlert("Entry tapped", $"Tap: {input}", "OK"));
			PinCommand = new Command<string>(input => DisplayAlert("Pin entry", $"Pin: {input}", "OK"));
			DeleteCommand = new Command<string>(input => DisplayAlert("Delete entry", $"Delete: {input}", "OK"));

			BindingContext = this;
#endif
		}

		public ObservableCollection<string> Data { get; } = new ObservableCollection<string>();
		public ICommand TapCommand { get; }
		public ICommand PinCommand { get; }
		public ICommand DeleteCommand { get; }

		protected override void Init()
		{

		}
	}
}