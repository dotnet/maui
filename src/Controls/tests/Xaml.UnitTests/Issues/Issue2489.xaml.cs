using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Issue2489 : ContentPage
	{
		public Issue2489()
		{
			InitializeComponent();
		}

		public Issue2489(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public public class Tests
		{
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void DataTriggerTargetType(bool useCompiledXaml)
			{
				var layout = new Issue2489(useCompiledXaml);
				Assert.NotNull(layout.wimage);
				Assert.NotNull(layout.wimage.Triggers);
				Assert.IsType<DataTrigger>(layout.wimage.Triggers.Any());
				Assert.True(layout.wimage.Triggers[0]);
				var trigger = (DataTrigger)layout.wimage.Triggers[0];
				Assert.Equal(typeof(WImage), trigger.TargetType);
			}
		}
	}

	public class WImage : View
	{
		public ImageSource Source { get; set; }
		public Aspect Aspect { get; set; }
	}
}