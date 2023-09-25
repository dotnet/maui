using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1747, "Binding to Switch.IsEnabled has no effect", PlatformAffected.WinPhone)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Switch)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	public partial class Issue1747 : TestContentPage
	{
		const string ToggleButtonAutomationId = nameof(ToggleButtonAutomationId);
		const string ToggleSwitchAutomationId = nameof(ToggleSwitchAutomationId);

#if APP
		public Issue1747()
		{
			InitializeComponent();

			ToggleButton.AutomationId = ToggleButtonAutomationId;
			ToggleSwitch.AutomationId = ToggleSwitchAutomationId;
		}
#endif

		protected override void Init()
		{
			BindingContext = new ToggleViewModel();
		}

		public void Button_OnClick(object sender, EventArgs args)
		{
			var button = sender as Button;
			if (!(button?.BindingContext is ToggleViewModel viewModel))
			{
				return;
			}

			viewModel.ShouldBeToggled = !viewModel.ShouldBeToggled;
			viewModel.ShouldBeEnabled = !viewModel.ShouldBeEnabled;
		}

		class ToggleViewModel : ViewModel
		{
			bool _shouldBeToggled;
			public bool ShouldBeToggled
			{
				get => _shouldBeToggled;
				set
				{
					_shouldBeToggled = value;
					OnPropertyChanged();
				}
			}

			bool _shouldBeEnabled;
			public bool ShouldBeEnabled
			{
				get => _shouldBeEnabled;
				set
				{
					_shouldBeEnabled = value;
					OnPropertyChanged();
				}
			}
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue1747Test()
		{
			RunningApp.WaitForElement(q => q.Marked(ToggleButtonAutomationId));
			RunningApp.WaitForElement(q => q.Marked(ToggleSwitchAutomationId));

			var toggleSwitch = RunningApp.Query(q => q.Marked(ToggleSwitchAutomationId))?.FirstOrDefault();
			Assert.AreNotEqual(toggleSwitch, null);
			Assert.AreEqual(toggleSwitch?.Enabled, false);

			RunningApp.Tap(q => q.Marked(ToggleButtonAutomationId));

			toggleSwitch = RunningApp.Query(q => q.Marked(ToggleSwitchAutomationId))?.FirstOrDefault();
			Assert.AreNotEqual(toggleSwitch, null);
			Assert.AreEqual(toggleSwitch?.Enabled, true);
		}
#endif
	}
}

