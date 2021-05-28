using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7886, "PushModalAsync modal page with Entry crashes on close for MacOS (NRE)", PlatformAffected.macOS)]
	public partial class Issue7886 : TestContentPage
	{

		const string TriggerModalAutomationId = "TriggerModal";
		const string PopModalAutomationId = "PopModal";

		public string ButtonAutomationId { get => TriggerModalAutomationId; }

		protected override void Init()
		{
		}

#if APP
		public Issue7886()
		{
			InitializeComponent();
			BindingContext = this;
		}

		void Handle_Clicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new ModalPage()));
		}

		class ModalPage : ContentPage
		{
			public ModalPage()
			{
				BackgroundColor = Colors.Orange;

				var tbi = new ToolbarItem("Done", null, () => Navigation.PopModalAsync())
				{
					AutomationId = PopModalAutomationId
				};

				ToolbarItems.Add(tbi);

				Content = new Entry
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};
			}
		}
#endif
#if UITEST && __MACOS__
		[Test]
		public void NoNREOnPushModalAsyncAndBack()
		{
			RunningApp.WaitForElement(TriggerModalAutomationId);
			RunningApp.Tap(TriggerModalAutomationId);
			RunningApp.WaitForElement(PopModalAutomationId);
			RunningApp.Tap(PopModalAutomationId);
		}

		
#endif
	}

}