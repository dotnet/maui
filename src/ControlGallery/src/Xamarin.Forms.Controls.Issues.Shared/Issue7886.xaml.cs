using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
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
				BackgroundColor = Color.Orange;

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