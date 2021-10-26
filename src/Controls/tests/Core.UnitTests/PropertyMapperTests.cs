using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class PropertyMapperTests : BaseTestFixture
	{
		[Test]
		public void ValidateCorrectChainingReplaced()
		{
			// This simulates mappers being initialized by the Application class early on
			// If the App.Xaml class defines styles for 
			_ = new Application();

			VisualElement.Mapper.Add("ValidateCorrectChainingReplaced.VisualElement", (_, __) => { });
			Element.Mapper.Add("ValidateCorrectChainingReplaced.Element", (_, __) => { });


			Assert.NotNull(LabelHandler.LabelMapper.GetProperty("ValidateCorrectChainingReplaced.VisualElement"));
			Assert.NotNull(LabelHandler.LabelMapper.GetProperty("ValidateCorrectChainingReplaced.Element"));
		}


		[Test]
		public void ValidateGetKeyWorksIfAddedBeforeRemap()
		{
			// This simulates mappers being initialized by the Application class early on
			// If the App.Xaml class defines styles for 
			_ = new Application();

			LabelHandler.LabelMapper.Add("ValidateGetKeyWorksForPrependMapperExtensionMethod.LabelHandler.LabelMapper", (_, __) => { });
			Element.Mapper.Add("ValidateGetKeyWorksForPrependMapperExtensionMethod.Element", (_, __) => { });
			VisualElement.Mapper.Add("ValidateGetKeyWorksForPrependMapperExtensionMethod.VisualElement", (_, __) => { });


			Assert.NotNull(LabelHandler.LabelMapper.GetProperty("ValidateGetKeyWorksForPrependMapperExtensionMethod.VisualElement"));
			Assert.NotNull(LabelHandler.LabelMapper.GetProperty("ValidateGetKeyWorksForPrependMapperExtensionMethod.LabelHandler.LabelMapper"));
			Assert.NotNull(LabelHandler.LabelMapper.GetProperty("ValidateGetKeyWorksForPrependMapperExtensionMethod.Element"));
		}


		[Test]
		public void ValidateParentKeyIsPresent()
		{
			_ = new Application();
			Assert.NotNull(Label.ControlsLabelMapper.GetProperty(nameof(IView.MaximumHeight)));
			Assert.NotNull(VisualElement.Mapper.GetProperty(nameof(IView.MaximumHeight)));
		}
	}
}
