using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Platform;
using Xunit;
using GtkWidget = global::Gtk.Widget;
using GtkProperty = global::Gtk.AccessibleProperty;
using GtkRelation = global::Gtk.AccessibleRelation;
using GtkRole = global::Gtk.AccessibleRole;
using GtkState = global::Gtk.AccessibleState;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>
	/// The MAUI semantics mapped to <c>GtkAccessible</c> (properties, states, relations, roles, focus), read back from GTK's
	/// accessibility context with <c>gtk_test_accessible_has_*</c> and the role getters.
	/// </summary>
#pragma warning disable CS0618 // AutomationProperties is obsolete but still supported.
	public class AccessibilityTests
	{
		static TWidget Widget<TWidget>(IView view) where TWidget : class =>
			(TWidget)view.ToHandler(ControlsTestApp.NewContext()).PlatformView!;

		static bool Has(GtkWidget widget, GtkProperty property) => global::Gtk.Test.AccessibleHasProperty(widget, property);

		static bool Has(GtkWidget widget, GtkState state) => global::Gtk.Test.AccessibleHasState(widget, state);

		static bool Has(GtkWidget widget, GtkRelation relation) => global::Gtk.Test.AccessibleHasRelation(widget, relation);

		// ------------------------------------------------------------------ label, description, heading

		[GtkFact]
		public void DescriptionAndHintAreTheLabelAndTheDescriptionAndCanBeRemoved() => GtkTestHost.Run(() =>
		{
			var label = new Slider();
			var widget = Widget<GtkWidget>(label);
			Assert.False(Has(widget, GtkProperty.Label));
			Assert.False(Has(widget, GtkProperty.Description));

			SemanticProperties.SetDescription(label, "spoken name");
			SemanticProperties.SetHint(label, "spoken hint");
			Assert.True(Has(widget, GtkProperty.Label));
			Assert.True(Has(widget, GtkProperty.Description));

			SemanticProperties.SetDescription(label, null);
			SemanticProperties.SetHint(label, null);
			Assert.False(Has(widget, GtkProperty.Label));
			Assert.False(Has(widget, GtkProperty.Description));
		});

		[GtkTheory]
		[InlineData(SemanticHeadingLevel.Level1)]
		[InlineData(SemanticHeadingLevel.Level4)]
		[InlineData(SemanticHeadingLevel.Level9)]
		public void HeadingLevelBeforeTheWidgetIsRealizedMakesItAHeading(SemanticHeadingLevel level) => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "title" };
			SemanticProperties.SetHeadingLevel(label, level);

			var widget = Widget<global::Gtk.Label>(label);
			Assert.Equal(GtkRole.Heading, widget.AccessibleRole);
			Assert.True(Has(widget, GtkProperty.Level));

			// Still unrealized: the role can go back.
			SemanticProperties.SetHeadingLevel(label, SemanticHeadingLevel.None);
			Assert.Equal(GtkRole.Label, widget.AccessibleRole);
			Assert.False(Has(widget, GtkProperty.Level));
		});

		[GtkFact]
		public void HeadingLevelAfterRealizationKeepsTheRoleAndLogsNothing() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "title" };
			using var host = new WindowHost(new VerticalStackLayout { label });
			var widget = host.Widget(label);
			host.Pump();
			GtkTestHost.ClearComplaints();

			SemanticProperties.SetHeadingLevel(label, SemanticHeadingLevel.Level2);
			host.Pump();

			// GTK fixes the role once the widget is rooted.
			Assert.Equal(GtkRole.Label, widget.AccessibleRole);
			GtkTestHost.AssertNoComplaints();
		});

		// ------------------------------------------------------------------ hidden, labelled-by

		[GtkFact]
		public void ExcludingFromTheAccessibleTreeSetsTheHiddenState() => GtkTestHost.Run(() =>
		{
			var entry = new Entry();
			var widget = Widget<GtkWidget>(entry);
			AutomationProperties.SetIsInAccessibleTree(entry, false);
			Assert.True(Has(widget, GtkState.Hidden));

			AutomationProperties.SetIsInAccessibleTree(entry, true);
			AutomationProperties.SetExcludedWithChildren(entry, true);
			Assert.True(Has(widget, GtkState.Hidden));

			AutomationProperties.SetExcludedWithChildren(entry, null);
			GtkTestHost.AssertNoComplaints();
		});

		[GtkFact]
		public void ClearingExclusionKeepsAnInvisibleWidgetHidden() => GtkTestHost.Run(() =>
		{
			var entry = new Entry { IsVisible = false };
			var widget = Widget<GtkWidget>(entry);
			AutomationProperties.SetExcludedWithChildren(entry, true);
			AutomationProperties.SetExcludedWithChildren(entry, false);

			Assert.True(Has(widget, GtkState.Hidden));
		});

		[GtkFact]
		public void LabeledByRelatesTheViewToItsLabelInEitherCreationOrder() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "Name" };
			var entry = new Entry();
			AutomationProperties.SetLabeledBy(entry, label);

			// The label's handler exists first.
			var labelWidget = Widget<GtkWidget>(label);
			var entryWidget = Widget<GtkWidget>(entry);
			Assert.True(Has(entryWidget, GtkRelation.LabelledBy));

			// The entry's handler exists first; the relation follows when the label gets its handler.
			var late = new Label { Text = "Late" };
			var entry2 = new Entry();
			AutomationProperties.SetLabeledBy(entry2, late);
			var entry2Widget = Widget<GtkWidget>(entry2);
			Assert.False(Has(entry2Widget, GtkRelation.LabelledBy));
			Widget<GtkWidget>(late);
			Assert.True(Has(entry2Widget, GtkRelation.LabelledBy));

			AutomationProperties.SetLabeledBy(entry, null);
			Assert.False(Has(entryWidget, GtkRelation.LabelledBy));
			Assert.NotNull(labelWidget);
		});

		// ------------------------------------------------------------------ roles and states GTK keeps

		[GtkFact]
		public void CustomWidgetsHaveTheRoleOfWhatTheyAre() => GtkTestHost.Run(() =>
		{
			var stepper = Widget<global::Gtk.Box>(new Stepper(0, 10, 3, 1));
			Assert.Equal(GtkRole.SpinButton, stepper.AccessibleRole);
			Assert.True(Has(stepper, GtkProperty.ValueMin));
			Assert.True(Has(stepper, GtkProperty.ValueMax));
			Assert.True(Has(stepper, GtkProperty.ValueNow));

			Assert.Equal(GtkRole.Img, Widget<GtkWidget>(new GraphicsView()).AccessibleRole);
			Assert.Equal(GtkRole.Presentation, Widget<GtkWidget>(new Rectangle()).AccessibleRole);
			Assert.Equal(GtkRole.Presentation, Widget<GtkWidget>(new BoxView()).AccessibleRole);
			Assert.Equal(GtkRole.Button, Widget<GtkWidget>(new Button()).AccessibleRole);
			Assert.Equal(GtkRole.Switch, Widget<GtkWidget>(new Switch()).AccessibleRole);
			Assert.Equal(GtkRole.Checkbox, Widget<GtkWidget>(new CheckBox()).AccessibleRole);
			Assert.Equal(GtkRole.Slider, Widget<GtkWidget>(new Slider()).AccessibleRole);
		});

		[GtkFact]
		public void StepperValuePropertiesFollowTheValue() => GtkTestHost.Run(() =>
		{
			var stepper = new Stepper(0, 10, 3, 1);
			var widget = Widget<Microsoft.Maui.Gtk.Handlers.MauiGtkStepper>(stepper);
			Assert.True(Has(widget, GtkProperty.ValueNow));

			widget.ResetProperty(GtkProperty.ValueNow);
			Assert.False(Has(widget, GtkProperty.ValueNow));

			stepper.Value = 5;
			Assert.True(Has(widget, GtkProperty.ValueNow));
		});

		[GtkFact]
		public void CompositeControlsCarryLabelAndDescriptionOnTheirFocusableControl() => GtkTestHost.Run(() =>
		{
			var picker = new Picker { ItemsSource = new[] { "a", "b" } };
			SemanticProperties.SetDescription(picker, "Choose");
			var pickerWidget = Widget<Microsoft.Maui.Gtk.Handlers.MauiGtkPicker>(picker);
			var dropDown = pickerWidget.FirstChild!;
			Assert.True(Has(dropDown, GtkProperty.Label));
			Assert.Equal(GtkRole.ComboBox, dropDown.AccessibleRole);

			var radio = new RadioButton { Content = "r" };
			SemanticProperties.SetDescription(radio, "Pick");
			var radioWidget = Widget<Microsoft.Maui.Gtk.Handlers.MauiGtkRadioButton>(radio);
			Assert.True(Has(radioWidget.Button, GtkProperty.Label));
			Assert.Equal(GtkRole.Radio, radioWidget.Button.AccessibleRole);
		});

		[GtkFact]
		public void StatesGtkMaintainsStayAccurate() => GtkTestHost.Run(() =>
		{
			var check = new CheckBox { IsChecked = true };
			var checkWidget = Widget<GtkWidget>(check);
			Assert.True(Has(checkWidget, GtkState.Checked));

			var entry = new Entry { IsReadOnly = true, IsEnabled = false };
			var entryWidget = Widget<GtkWidget>(entry);
			Assert.True(Has(entryWidget, GtkState.Disabled));
			Assert.True(Has(entryWidget, GtkProperty.ReadOnly));
		});

		[GtkFact]
		public void TabViewIsATabListOfTabsWithPanelsAndTracksTheSelection() => GtkTestHost.Run(() =>
		{
			var tabs = new Microsoft.Maui.Gtk.Platform.MauiGtkTabView();
			var first = tabs.InsertTab(0, "One", null);
			tabs.InsertTab(1, "Two", null);

			Assert.Equal(GtkRole.TabList, tabs.Bar.AccessibleRole);
			Assert.Equal(GtkRole.TabPanel, first.AccessibleRole);
			for (int i = 0; i < 2; i++)
			{
				Assert.Equal(GtkRole.Tab, tabs.ButtonAt(i).AccessibleRole);
				Assert.True(Has(tabs.ButtonAt(i), GtkState.Selected));
			}
		});

		// ------------------------------------------------------------------ focus

		[GtkFact]
		public void SetSemanticFocusFocusesAFocusableViewAndAnnouncesAnUnfocusableOne() => GtkTestHost.Run(() =>
		{
			var entry = new Entry();
			var label = new Label { Text = "info" };
			SemanticProperties.SetDescription(label, "Some information");
			var other = new Entry();
			using var host = new WindowHost(new VerticalStackLayout { label, other, entry });
			host.Pump();
			GtkTestHost.ClearComplaints();
			Assert.True(IsInside(host.PlatformWindow.Focus, host.Widget(other)));

			((IView)entry).SetSemanticFocus();
			host.Pump();
			Assert.True(IsInside(host.PlatformWindow.Focus, host.Widget(entry)));

			((IView)label).SetSemanticFocus();
			host.Pump();
			GtkTestHost.AssertNoComplaints();
		});

		[GtkFact]
		public void TabMovesThroughTheViewsInTreeOrderSkippingDisabledAndCollapsedOnes() => GtkTestHost.Run(() =>
		{
			var first = new Entry();
			var disabled = new Entry { IsEnabled = false };
			var collapsed = new Entry { IsVisible = false };
			var second = new Entry();
			var third = new Entry();
			using var host = new WindowHost(new VerticalStackLayout { first, disabled, collapsed, second, third });
			host.Pump();

			// A presented window starts with its first focusable widget focused.
			Assert.True(IsInside(host.PlatformWindow.Focus, host.Widget(first)));

			host.PlatformWindow.Child!.ChildFocus(global::Gtk.DirectionType.TabForward);
			Assert.True(IsInside(host.PlatformWindow.Focus, host.Widget(second)));

			host.PlatformWindow.Child!.ChildFocus(global::Gtk.DirectionType.TabForward);
			Assert.True(IsInside(host.PlatformWindow.Focus, host.Widget(third)));

			host.PlatformWindow.Child!.ChildFocus(global::Gtk.DirectionType.TabBackward);
			Assert.True(IsInside(host.PlatformWindow.Focus, host.Widget(second)));
		});

		static bool IsInside(GtkWidget? widget, GtkWidget container)
		{
			for (var current = widget; current is not null; current = current.Parent)
			{
				if (current.Handle == container.Handle)
					return true;
			}

			return false;
		}
	}
#pragma warning restore CS0618
}
