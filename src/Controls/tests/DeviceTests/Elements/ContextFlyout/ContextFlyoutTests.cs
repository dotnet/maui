using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{

	[Category(TestCategory.MenuFlyout)]
	public partial class ContextFlyoutTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Layout, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<Editor, EditorHandler>();
					handlers.AddHandler<SearchBar, SearchBarHandler>();
					handlers.AddHandler<Picker, PickerHandler>();
					handlers.AddHandler<DatePicker, DatePickerHandler>();
					handlers.AddHandler<Button, ButtonHandler>();
					handlers.AddHandler<Page, PageHandler>();

					handlers.AddHandler<MenuFlyout, MenuFlyoutHandler>();
					handlers.AddHandler<MenuFlyoutItem, MenuFlyoutItemHandler>();
					handlers.AddHandler<MenuFlyoutSubItem, MenuFlyoutSubItemHandler>();
					handlers.AddHandler<MenuFlyoutSeparator, MenuFlyoutSeparatorHandler>();
				});
			});
		}

		const string MobileContextFlyoutsSwitch = "Microsoft.Maui.RuntimeFeature.AreMobileContextFlyoutsEnabled";

		// Runs the action with context flyouts on iOS and Android turned off, like MauiEnableMobileContextFlyouts=false
		static async Task WithMobileContextFlyoutsDisabled(Func<Task> action)
		{
			AppContext.SetSwitch(MobileContextFlyoutsSwitch, false);
			try
			{
				await action();
			}
			finally
			{
				AppContext.SetSwitch(MobileContextFlyoutsSwitch, true);
			}
		}

		// Copy, Disabled, ---, Edit > Nested
		static MenuFlyout CreateFlyout(out MenuFlyoutItem nested)
		{
			nested = new MenuFlyoutItem { Text = "Nested" };
			var subItem = new MenuFlyoutSubItem { Text = "Edit" };
			subItem.Add(nested);

			return new MenuFlyout
			{
				new MenuFlyoutItem { Text = "Copy" },
				new MenuFlyoutItem { Text = "Disabled", IsEnabled = false },
				new MenuFlyoutSeparator(),
				subItem,
			};
		}

		class ContextMenuViewModel : INotifyPropertyChanged
		{
			string _title;

			public string Title
			{
				get => _title;
				set
				{
					_title = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;
		}
	}
}
