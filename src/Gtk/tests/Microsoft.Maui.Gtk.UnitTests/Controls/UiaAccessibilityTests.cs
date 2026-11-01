using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>
	/// The accessibility mapping read back through UI Automation on Windows (GTK's AccessKit backend): what Narrator sees.
	/// Returns without asserting where UI Automation is not available (Linux). Windows GTK 4.22.4 publishes the window only (see the test).
	/// </summary>
#pragma warning disable CS0618 // AutomationProperties is obsolete but still supported.
	public class UiaAccessibilityTests
	{
		const int WindowControlType = 50032;

		readonly ITestOutputHelper _output;

		public UiaAccessibilityTests(ITestOutputHelper output) => _output = output;

		[GtkFact]
		public void LabelsAndDescriptionsReachUiAutomation() => GtkTestHost.Run(() =>
		{
			if (!OperatingSystem.IsWindows())
				return;

			var button = new Button { Text = "visible text" };
			SemanticProperties.SetDescription(button, "uia-button-name");
			SemanticProperties.SetHint(button, "uia-button-hint");

			var stepper = new Stepper(0, 10, 4, 1);
			SemanticProperties.SetDescription(stepper, "uia-stepper-name");

			using var host = new WindowHost(new VerticalStackLayout { button, stepper });
			var title = "uia-probe-" + Guid.NewGuid().ToString("N");
			host.PlatformWindow.Title = title;
			host.Pump(20);

			List<UiaProbe.Node>? nodes = null;
			GtkTestHost.PumpUntil(() =>
			{
				nodes = UiaProbe.Query(() => UiaProbe.Elements(title), host.PlatformWindow, 5000);
				return nodes is { Count: > 1 };
			}, host.PlatformWindow, 3000);

			Assert.NotNull(nodes);
			foreach (var n in nodes!)
				_output.WriteLine($"{n.ControlType} '{n.Name}' help='{n.HelpText}' desc='{n.FullDescription}' enabled={n.IsEnabled}");

			// GTK 4.22.4 for Windows ships the AccessKit backend but runs the null (test) accessibility context, also with GTK_A11Y=accesskit:
			// UI Automation sees the toplevel window only. When a runtime publishes the tree, the labels must be in it.
			Assert.Contains(nodes, n => n.ControlType == WindowControlType && n.Name == title);
			if (nodes.Count > 1)
			{
				Assert.Contains(nodes, n => n.Name == "uia-button-name");
				Assert.Contains(nodes, n => n.Name == "uia-stepper-name");
			}
		});
	}
#pragma warning restore CS0618
}
