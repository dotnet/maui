using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Gtk.Hosting;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>The pending-label registration of <see cref="GtkControlsAutomation.GetLabeledBy"/>, with stub handlers (no GTK needed).</summary>
#pragma warning disable CS0618 // AutomationProperties is obsolete but still supported.
	public class ControlsAutomationTests
	{
		sealed class CountingHandler : IViewHandler
		{
			public int Updates;
			public object? PlatformView => null;
			public IElement? VirtualView { get; private set; }
			IView? IViewHandler.VirtualView => VirtualView as IView;
			public object? ContainerView => null;
			public bool HasContainer { get => false; set { } }
			public Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint) => default;
			public void PlatformArrange(Graphics.Rect frame) { }
			public IMauiContext? MauiContext => null;
			public void SetMauiContext(IMauiContext mauiContext) { }
			public void SetVirtualView(IElement view) => VirtualView = view;
			public void UpdateValue(string property) { if (property == "LabeledBy") Updates++; }
			public void Invoke(string command, object? args = null) { }
			public void DisconnectHandler() { }
		}

		[Theory]
		[InlineData(1)]
		[InlineData(3)]
		[InlineData(10)]
		public void RepeatedMappingBeforeTheLabelHasAHandlerRelatesOnce(int runs)
		{
			var label = new Label();
			var entry = new Entry();
			var entryHandler = new CountingHandler();
			entry.Handler = entryHandler;
			AutomationProperties.SetLabeledBy(entry, label);
			entryHandler.Updates = 0; // SetLabeledBy itself notifies a present handler

			for (int i = 0; i < runs; i++)
				Assert.Same(label, GtkControlsAutomation.Instance.GetLabeledBy(entry));

			label.Handler = new CountingHandler();

			Assert.Equal(1, entryHandler.Updates);

			// connected: nothing is left pending, a later handler change does not update again
			label.Handler = new CountingHandler();
			Assert.Equal(1, entryHandler.Updates);
		}

		[Fact]
		public void ChangingTheLabelReplacesThePendingRegistration()
		{
			var first = new Label();
			var second = new Label();
			var entry = new Entry();
			var entryHandler = new CountingHandler();
			entry.Handler = entryHandler;

			AutomationProperties.SetLabeledBy(entry, first);
			entryHandler.Updates = 0; // SetLabeledBy itself notifies a present handler
			GtkControlsAutomation.Instance.GetLabeledBy(entry);
			AutomationProperties.SetLabeledBy(entry, second);
			entryHandler.Updates = 0; // SetLabeledBy itself notifies a present handler
			GtkControlsAutomation.Instance.GetLabeledBy(entry);
			GtkControlsAutomation.Instance.GetLabeledBy(entry);

			first.Handler = new CountingHandler();
			Assert.Equal(0, entryHandler.Updates);

			second.Handler = new CountingHandler();
			Assert.Equal(1, entryHandler.Updates);
		}

		[Fact]
		public void OneLabelCanLabelSeveralViews()
		{
			var label = new Label();
			var a = new Entry();
			var b = new Entry();
			var ha = new CountingHandler();
			var hb = new CountingHandler();
			a.Handler = ha;
			b.Handler = hb;
			AutomationProperties.SetLabeledBy(a, label);
			ha.Updates = 0;
			AutomationProperties.SetLabeledBy(b, label);
			hb.Updates = 0;

			GtkControlsAutomation.Instance.GetLabeledBy(a);
			GtkControlsAutomation.Instance.GetLabeledBy(a);
			GtkControlsAutomation.Instance.GetLabeledBy(b);

			label.Handler = new CountingHandler();

			Assert.Equal(1, ha.Updates);
			Assert.Equal(1, hb.Updates);
		}
	}
#pragma warning restore CS0618
}
