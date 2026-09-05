using System;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	/// <summary>
	/// Tests for RadioButton ContentAsString() warning behavior.
	/// Issue #33829: Warning should not be logged when ControlTemplate is set because
	/// View content IS supported in that scenario.
	/// </summary>
	[Category("RadioButton")]
	public class RadioButtonContentAsStringTests : BaseTestFixture
	{
		public RadioButtonContentAsStringTests()
		{
			ApplicationExtensions.CreateAndSetMockApplication();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Application.ClearCurrent();
			}
			base.Dispose(disposing);
		}

		[Fact]
		public void ContentAsStringDoesNotLogWarningWhenControlTemplateIsSet()
		{
			// Arrange: RadioButton with ControlTemplate AND View Content
			// This scenario IS supported per documentation, so no warning should be logged
			var radioButton = new RadioButton
			{
				ControlTemplate = RadioButton.DefaultTemplate,
				Content = new Label { Text = "Test Label" }
			};

			// Act
			var result = radioButton.ContentAsString();

			// Assert: No warning should be logged when ControlTemplate is set
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"No warning should be logged when ControlTemplate is set. " +
				$"Found: {MockApplication.MockLogger.Messages.FirstOrDefault()}");
			Assert.NotNull(result);
		}

		[Fact]
		public void ContentAsStringLogsWarningWhenNoControlTemplate()
		{
			// Arrange: RadioButton without ControlTemplate but with View Content
			// This scenario is NOT supported, so warning should be logged
			var radioButton = new RadioButton
			{
				Content = new Label { Text = "Test Label" }
			};

			// Act
			var result = radioButton.ContentAsString();

			// Assert: Warning SHOULD be logged when ControlTemplate is null
			Assert.Single(MockApplication.MockLogger.Messages);
			Assert.Contains("does not support View as the Content property", MockApplication.MockLogger.Messages.First(), StringComparison.Ordinal);
		}
	}
}
