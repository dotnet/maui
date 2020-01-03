using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8741, "[Bug] [Shell] [Android] ToolbarItem Enabled/Disabled behavior does not work for Shell apps", PlatformAffected.Android)]
	public class Issue8741 : TestShell
	{
		protected override void Init()
		{
			var page = CreateContentPage();
			var toolbarItem = new ToolbarItem
			{
				Text = "Add",
				AutomationId = "Add"
			};

			toolbarItem.SetBinding(MenuItem.CommandProperty, "ToolbarTappedCommand");
			page.ToolbarItems.Add(toolbarItem);

			var button = new Button
			{
				Text = "Toggle Enabled/Disabled",
				AutomationId = "ToggleEnabled"
			};

			button.SetBinding(Button.CommandProperty, "ChangeToggleCommand");
			var label = new Label();
			label.SetBinding(Label.TextProperty, "EnabledText");

			var clickCount = new Label();
			clickCount.AutomationId = "ClickCount";
			clickCount.SetBinding(Label.TextProperty, "ClickCount");

			page.Content =
				new StackLayout
				{
					Children =
					{
						label,
						clickCount,
						button
					}
				};

			BindingContext = new ViewModelIssue8741();
		}

#if UITEST
		[Test]
		public void Issue8741Test()
		{
			RunningApp.WaitForElement("Add");
			RunningApp.Tap("Add");
#if __ANDROID__
			var toolbarItemColorValue = GetToolbarItemColorValue();
			int disabledAlpha = GetAlphaValue(toolbarItemColorValue);
#endif
			Assert.AreEqual("0", RunningApp.WaitForElement("ClickCount")[0].ReadText());

			RunningApp.Tap("ToggleEnabled");
			RunningApp.Tap("Add");
#if __ANDROID__
			toolbarItemColorValue = GetToolbarItemColorValue();
			int enabledAlpha = GetAlphaValue(toolbarItemColorValue);
			Assert.Less(disabledAlpha, enabledAlpha);
#endif
			Assert.AreEqual("1", RunningApp.WaitForElement("ClickCount")[0].ReadText());			

			RunningApp.Tap("ToggleEnabled");
			RunningApp.Tap("Add");

			Assert.AreEqual("1", RunningApp.WaitForElement("ClickCount")[0].ReadText());
		}

#if __ANDROID__
		private object GetToolbarItemColorValue()
		{
			return RunningApp.Query(x => x.Text("Add").Invoke("getCurrentTextColor"))[0];
		}

		private int GetAlphaValue(object toolbarItemColorValue)
		{
			int color = Convert.ToInt32(toolbarItemColorValue);
			int a = (color >> 24) & 0xff;
			return a;
		}
#endif

#endif

		[Preserve(AllMembers = true)]
		public class ViewModelIssue8741 : INotifyPropertyChanged
		{
			bool _canAddNewItem;
			int _clickCount;

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}

			public bool Enabled
			{
				get => _canAddNewItem;
				set
				{
					_canAddNewItem = value;
					OnPropertyChanged(nameof(Enabled));
					ToolbarTappedCommand.ChangeCanExecute();
				}
			}

			public int ClickCount
			{
				get
				{
					return _clickCount;
				}
				set
				{
					_clickCount = value;
					OnPropertyChanged(nameof(ClickCount));
				}
			}

			public string EnabledText { get; set; }
			public Command ChangeToggleCommand { get; set; }
			public Command ToolbarTappedCommand { get; set; }

			public ViewModelIssue8741()
			{
				ChangeToggleCommand = new Command(ChangeToggle);
				ToolbarTappedCommand = new Command(ToolbarTapped, () => Enabled);
				EnabledText = Enabled ? "Enabled" : "Disabled";
			}

			void ToolbarTapped()
			{
				ClickCount++;
			}

			void ChangeToggle()
			{
				Enabled = !Enabled;
				EnabledText = Enabled ? "Enabled" : "Disabled";
				OnPropertyChanged(nameof(EnabledText));
			}
		}
	}
}
