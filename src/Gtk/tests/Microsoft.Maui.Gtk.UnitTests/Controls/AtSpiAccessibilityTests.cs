using System;
using Microsoft.Maui.Controls;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>
	/// The accessibility mapping read back through AT-SPI (what Orca sees): values, not just that a property exists. Needs the
	/// accessibility bus (<c>dbus-run-session</c>, as the WSL runner provides); returns without asserting where there is none.
	/// </summary>
#pragma warning disable CS0618 // AutomationProperties is obsolete but still supported.
	public class AtSpiAccessibilityTests
	{
		readonly ITestOutputHelper _output;

		public AtSpiAccessibilityTests(ITestOutputHelper output) => _output = output;

		AtSpiProbe.Node? Find(WindowHost host, string name, string? role = null)
		{
			AtSpiProbe.Node? node = null;
			// The application registers on the bus once its window is mapped: retry until the node shows up.
			GtkTestHost.PumpUntil(() =>
			{
				node = AtSpiProbe.Query(() => AtSpiProbe.FindByName(name, role), host.PlatformWindow, 5000);
				return node is not null;
			}, host.PlatformWindow, 20000);

			if (node is null)
				_output.WriteLine(string.Join("\n", AtSpiProbe.Query(AtSpiProbe.Dump, host.PlatformWindow) ?? new()));

			return node;
		}

		[GtkFact]
		public void LabelDescriptionRoleAndHeadingReachTheAccessibilityBus() => GtkTestHost.Run(() =>
		{
			if (!AtSpiProbe.IsAvailable())
				return;

			var plain = new Button { Text = "plain" };
			SemanticProperties.SetDescription(plain, "atspi-button-name");
			SemanticProperties.SetHint(plain, "atspi-button-hint");

			var title = new Label { Text = "title text" };
			SemanticProperties.SetHeadingLevel(title, SemanticHeadingLevel.Level2);
			SemanticProperties.SetDescription(title, "atspi-heading-name");

			var stepper = new Stepper(0, 10, 4, 1);
			SemanticProperties.SetDescription(stepper, "atspi-stepper-name");

			using var host = new WindowHost(new VerticalStackLayout { title, plain, stepper });
			host.Pump(20);

			var button = Find(host, "atspi-button-name");
			Assert.NotNull(button);
			Assert.Equal("atspi-button-hint", button!.Description);
			Assert.Equal("push button", button.Role);

			// A label with a description is named by it, not by its text; changing the text keeps that.
			title.Text = "changed title";
			host.Pump(20);
			var heading = Find(host, "atspi-heading-name");
			Assert.NotNull(heading);
			Assert.Equal("heading", heading!.Role);

			var spin = Find(host, "atspi-stepper-name");
			Assert.NotNull(spin);
			Assert.Equal("spin button", spin!.Role);
		});

		[GtkFact]
		public void StatesAndRolesOfStockControlsReachTheAccessibilityBus() => GtkTestHost.Run(() =>
		{
			if (!AtSpiProbe.IsAvailable())
				return;

			var check = new CheckBox { IsChecked = true };
			SemanticProperties.SetDescription(check, "atspi-check");
			var entry = new Entry { IsReadOnly = true };
			SemanticProperties.SetDescription(entry, "atspi-entry");
			var disabled = new Button { Text = "d", IsEnabled = false };
			SemanticProperties.SetDescription(disabled, "atspi-disabled");
			var radio = new RadioButton { Content = "r" };
			SemanticProperties.SetDescription(radio, "atspi-radio");

			using var host = new WindowHost(new VerticalStackLayout { check, entry, disabled, radio });
			host.Pump(20);

			var checkNode = Find(host, "atspi-check");
			Assert.NotNull(checkNode);
			Assert.Equal("check box", checkNode!.Role);
			Assert.True(checkNode.Checked);

			check.IsChecked = false;
			host.Pump(20);
			Assert.False(Find(host, "atspi-check")!.Checked);

			var entryNode = Find(host, "atspi-entry");
			Assert.NotNull(entryNode);
			Assert.False(entryNode!.Editable);

			var disabledNode = Find(host, "atspi-disabled");
			Assert.NotNull(disabledNode);
			Assert.False(disabledNode!.Sensitive);

			// GTK before 4.18 reports a grouped check button as a check box; the backend sets the radio role.
			var radioNode = Find(host, "atspi-radio");
			Assert.NotNull(radioNode);
			Assert.Equal("radio button", radioNode!.Role);
		});
	}
#pragma warning restore CS0618
}
