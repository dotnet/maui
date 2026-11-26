using System;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.PropertyMapping)]
	public class PropertyMapperExtensionTests
	{
		[Fact]
		public void AddAfterMapping()
		{
			string log = string.Empty;

			var msg1 = "original mapping should have run";
			var msg2 = "and also this one";

			var mapper1 = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (r, v) => log += msg1
			};

			mapper1.AppendToMapping(nameof(IView.Background), (h, v) => log += msg2);

			mapper1.UpdateProperties(null, new Button());

			Assert.Contains(msg1, log, StringComparison.Ordinal);
			Assert.Contains(msg2, log, StringComparison.Ordinal);

			var originalIndex = log.IndexOf(msg1);
			var additionalIndex = log.IndexOf(msg2);

			Assert.True(originalIndex < additionalIndex);
		}

		[Fact]
		public void AddBeforeMapping()
		{
			string log = string.Empty;

			var msg1 = "original mapping should have run";
			var msg2 = "and also this one";

			var mapper1 = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (r, v) => log += msg1
			};

			mapper1.PrependToMapping(nameof(IView.Background), (h, v) => log += msg2);

			mapper1.UpdateProperties(null, new Button());

			Assert.Contains(msg1, log, StringComparison.Ordinal);
			Assert.Contains(msg2, log, StringComparison.Ordinal);

			var originalIndex = log.IndexOf(msg1);
			var additionalIndex = log.IndexOf(msg2);

			Assert.True(additionalIndex < originalIndex);
		}

		[Fact]
		public void ModifyMapping()
		{
			string log = string.Empty;

			var msg1 = "original";
			var msg2 = "modification";

			var mapper1 = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (r, v) => log += msg1
			};

			mapper1.ModifyMapping(nameof(IView.Background), (h, v, a) => log += msg2);

			mapper1.UpdateProperties(null, new Button());

			Assert.DoesNotContain(msg1, log, StringComparison.Ordinal);
			Assert.Contains(msg2, log, StringComparison.Ordinal);
		}

		[Theory]
		[InlineData(typeof(ContentView), false)]
		[InlineData(typeof(Label), true)]
		[InlineData(typeof(Button), false)]
		public void AddAfterMappingWhen(Type controlType, bool shouldRun)
		{
			string log = string.Empty;

			var msg1 = "original mapping should have run";
			var msg2 = "and also this one";

			var mapper1 = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (r, v) => log += msg1
			};

			mapper1.AppendToMapping<Label, IViewHandler>(nameof(Label.Background), (h, v) => log += msg2);

			mapper1.UpdateProperties(null, (IView)Activator.CreateInstance(controlType));

			Assert.Contains(msg1, log, StringComparison.Ordinal);
			if (shouldRun)
				Assert.Contains(msg2, log, StringComparison.Ordinal);
			else
				Assert.DoesNotContain(msg2, log, StringComparison.Ordinal);

			var originalIndex = log.IndexOf(msg1);
			var additionalIndex = log.IndexOf(msg2);

			if (shouldRun)
			{
				Assert.True(originalIndex < additionalIndex);
			}
			else
			{
				Assert.Equal(0, originalIndex);
				Assert.Equal(-1, additionalIndex);
			}
		}

		[Theory]
		[InlineData(typeof(ContentView), false)]
		[InlineData(typeof(Label), true)]
		[InlineData(typeof(Button), false)]
		public void AddBeforeMappingWhen(Type controlType, bool shouldRun)
		{
			string log = string.Empty;

			var msg1 = "original mapping should have run";
			var msg2 = "and also this one";

			var mapper1 = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (r, v) => log += msg1
			};

			mapper1.PrependToMapping<Label, IViewHandler>(nameof(Label.Background), (h, v) => log += msg2);

			mapper1.UpdateProperties(null, (IView)Activator.CreateInstance(controlType));

			Assert.Contains(msg1, log, StringComparison.Ordinal);
			if (shouldRun)
				Assert.Contains(msg2, log, StringComparison.Ordinal);
			else
				Assert.DoesNotContain(msg2, log, StringComparison.Ordinal);

			var originalIndex = log.IndexOf(msg1);
			var additionalIndex = log.IndexOf(msg2);

			if (shouldRun)
			{
				Assert.True(additionalIndex < originalIndex);
			}
			else
			{
				Assert.Equal(0, originalIndex);
				Assert.Equal(-1, additionalIndex);
			}
		}

		[Theory]
		[InlineData(typeof(ContentView), false)]
		[InlineData(typeof(Label), true)]
		[InlineData(typeof(Button), false)]
		public void ReplaceMappingWhen(Type controlType, bool shouldRun)
		{
			string log = string.Empty;

			var msg1 = "original";
			var msg2 = "modification";

			var mapper1 = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (r, v) => log += msg1
			};

			mapper1.ReplaceMapping<Label, IViewHandler>(nameof(IView.Background), (h, v) => log += msg2);

			mapper1.UpdateProperties(null, (IView)Activator.CreateInstance(controlType));

			if (shouldRun)
			{
				Assert.DoesNotContain(msg1, log, StringComparison.Ordinal);
				Assert.Contains(msg2, log, StringComparison.Ordinal);
			}
			else
			{
				Assert.Contains(msg1, log, StringComparison.Ordinal);
				Assert.DoesNotContain(msg2, log, StringComparison.Ordinal);
			}
		}

		[Theory]
		[InlineData(typeof(ContentView), false)]
		[InlineData(typeof(Label), true)]
		[InlineData(typeof(Button), false)]
		public void ModifyMappingWhen(Type controlType, bool shouldRun)
		{
			string log = string.Empty;

			var msg1 = "original";
			var msg2 = "modification";

			var mapper1 = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (r, v) => log += msg1
			};

			mapper1.ModifyMapping<Label, IViewHandler>(nameof(IView.Background), (h, v, a) => log += msg2);

			mapper1.UpdateProperties(null, (IView)Activator.CreateInstance(controlType));

			if (shouldRun)
			{
				Assert.DoesNotContain(msg1, log, StringComparison.Ordinal);
				Assert.Contains(msg2, log, StringComparison.Ordinal);
			}
			else
			{
				Assert.Contains(msg1, log, StringComparison.Ordinal);
				Assert.DoesNotContain(msg2, log, StringComparison.Ordinal);
			}
		}

		[Fact]
		public void FrameworkReplacementPreservesEarlierAppend()
		{
			var log = string.Empty;
			var mapper = CreateMapper(() => log += "core;");

			mapper.AppendToMapping(nameof(IView.Background), (_, _) => log += "append;");
			mapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => log += "controls;");

			mapper.UpdateProperties(null, new Button());

			Assert.Equal("controls;append;", log);
		}

		[Fact]
		public void FrameworkReplacementPreservesEarlierPrepend()
		{
			var log = string.Empty;
			var mapper = CreateMapper(() => log += "core;");

			mapper.PrependToMapping(nameof(IView.Background), (_, _) => log += "prepend;");
			mapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => log += "controls;");

			mapper.UpdateProperties(null, new Button());

			Assert.Equal("prepend;controls;", log);
		}

		[Fact]
		public void FrameworkReplacementPreservesEarlierModify()
		{
			var log = string.Empty;
			var mapper = CreateMapper(() => log += "core;");

			mapper.ModifyMapping(nameof(IView.Background), (handler, view, previous) =>
			{
				log += "before;";
				previous!(handler, view);
				log += "after;";
			});
			mapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => log += "controls;");

			mapper.UpdateProperties(null, new Button());

			Assert.Equal("before;controls;after;", log);
		}

		[Fact]
		public void FrameworkReplacementPreservesEarlierUserReplacement()
		{
			var log = string.Empty;
			var mapper = CreateMapper(() => log += "core;");

			mapper.ReplaceMapping<Label, IViewHandler>(nameof(IView.Background), (_, _) => log += "user;");
			mapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => log += "controls;");

			mapper.UpdateProperties(null, new Label());
			Assert.Equal("user;", log);

			log = string.Empty;
			mapper.UpdateProperties(null, new Button());
			Assert.Equal("controls;", log);
		}

		[Fact]
		public void FrameworkReplacementPreservesEarlierDirectReplacement()
		{
			var log = string.Empty;
			var mapper = CreateMapper(() => log += "core;");

			mapper[nameof(IView.Background)] = (_, _) => log += "user;";
			mapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => log += "controls;");

			mapper.UpdateProperties(null, new Button());

			Assert.Equal("user;", log);
		}

		[Fact]
		public void FrameworkReplacementPreservesEarlierDirectReplacementOfChainedMapping()
		{
			var log = string.Empty;
			var parentMapper = CreateMapper(() => log += "core;");
			var mapper = new PropertyMapper<IView, IViewHandler>(parentMapper).WithFrameworkMappingsSealed();

			mapper[nameof(IView.Background)] = (_, _) => log += "user;";
			mapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => log += "controls;");

			mapper.UpdateProperties(null, new Button());

			Assert.Equal("user;", log);
		}

		[Fact]
		public void FrameworkAppendRunsBeforeEarlierUserAppend()
		{
			var log = string.Empty;
			var mapper = CreateMapper(() => log += "core;");

			mapper.AppendToMapping(nameof(IView.Background), (_, _) => log += "user;");
			mapper.AppendToMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => log += "controls;");

			mapper.UpdateProperties(null, new Button());

			Assert.Equal("core;controls;user;", log);
		}

		[Fact]
		public void ParentFrameworkReplacementPreservesEarlierChildAppend()
		{
			var log = string.Empty;
			var parentMapper = CreateMapper(() => log += "core;");
			var childMapper = new PropertyMapper<IView, IViewHandler>(parentMapper);

			childMapper.AppendToMapping(nameof(IView.Background), (_, _) => log += "append;");
			parentMapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => log += "controls;");

			childMapper.UpdateProperties(null, new Button());

			Assert.Equal("controls;append;", log);
		}

		[Fact]
		public void CustomizationsUseOverriddenSetPropertyCore()
		{
			var mapper = new TrackingPropertyMapper
			{
				[nameof(IView.Background)] = (_, _) => { }
			};
			mapper.SetPropertyCallCount = 0;

			mapper.AppendToMapping(nameof(IView.Background), (_, _) => { });
			mapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => { });

			Assert.Equal(2, mapper.SetPropertyCallCount);
		}

		static PropertyMapper<IView, IViewHandler> CreateMapper(Action mapping) =>
			new()
			{
				[nameof(IView.Background)] = (_, _) => mapping()
			};

		sealed class TrackingPropertyMapper : PropertyMapper<IView, IViewHandler>
		{
			public int SetPropertyCallCount { get; set; }

			protected override void SetPropertyCore(string key, Action<IElementHandler, IElement> action)
			{
				SetPropertyCallCount++;
				base.SetPropertyCore(key, action);
			}
		}
	}
}
