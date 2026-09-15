using System;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.CommandMapping)]
	public class CommandMapperExtensionTests
	{
		[Fact]
		public void AddAfterMapping()
		{
			string log = string.Empty;

			var msg1 = "original mapping should have run";
			var msg2 = "and also this one";

			var mapper1 = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (r, v, a) => log += msg1
			};

			mapper1.AppendToMapping(nameof(IView.Focus), (h, v, a) => log += msg2);

			mapper1.Invoke(null, new Button(), nameof(IView.Focus), null);

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

			var mapper1 = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (r, v, a) => log += msg1
			};

			mapper1.PrependToMapping(nameof(IView.Focus), (h, v, a) => log += msg2);

			mapper1.Invoke(null, new Button(), nameof(IView.Focus), null);

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

			var mapper1 = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (r, v, a) => log += msg1
			};

			mapper1.ModifyMapping(nameof(IView.Focus), (h, v, a, b) => log += msg2);

			mapper1.Invoke(null, new Button(), nameof(IView.Focus), null);

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

			var mapper1 = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (r, v, a) => log += msg1
			};

			mapper1.AppendToMapping<Label, IViewHandler>(nameof(Label.Focus), (h, v, a) => log += msg2);

			mapper1.Invoke(null, (IView)Activator.CreateInstance(controlType), nameof(IView.Focus), null);

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

			var mapper1 = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (r, v, a) => log += msg1
			};

			mapper1.PrependToMapping<Label, IViewHandler>(nameof(Label.Focus), (h, v, a) => log += msg2);

			mapper1.Invoke(null, (IView)Activator.CreateInstance(controlType), nameof(IView.Focus), null);

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

			var mapper1 = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (r, v, a) => log += msg1
			};

			mapper1.ReplaceMapping<Label, IViewHandler>(nameof(IView.Focus), (h, v, a) => log += msg2);

			mapper1.Invoke(null, (IView)Activator.CreateInstance(controlType), nameof(IView.Focus), null);

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

			var mapper1 = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (r, v, a) => log += msg1
			};

			mapper1.ModifyMapping<Label, IViewHandler>(nameof(IView.Focus), (h, v, a, b) => log += msg2);

			mapper1.Invoke(null, (IView)Activator.CreateInstance(controlType), nameof(IView.Focus), null);

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
		public void FrameworkPrependRunsBeforeEarlierUserAppend()
		{
			var log = string.Empty;
			var mapper = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (_, _, _) => log += "core;"
			};

			mapper.AppendToMapping(nameof(IView.Focus), (_, _, _) => log += "user;");
			mapper.PrependToMappingForControls<IView, IViewHandler>(nameof(IView.Focus), (_, _, _) => log += "controls;");

			mapper.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.Equal("controls;core;user;", log);
		}

		[Fact]
		public void FrameworkModificationPreservesEarlierUserModification()
		{
			var log = string.Empty;
			var mapper = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (_, _, _) => log += "core;"
			};

			mapper.ModifyMapping(nameof(IView.Focus), (handler, view, args, previous) =>
			{
				log += "user-before;";
				previous!(handler, view, args);
				log += "user-after;";
			});
			mapper.ModifyMappingForControls<IView, IViewHandler>(nameof(IView.Focus), (_, _, _, _) => log += "controls;");

			mapper.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.Equal("user-before;controls;user-after;", log);
		}

		[Fact]
		public void FrameworkModificationPreservesEarlierDirectReplacement()
		{
			var log = string.Empty;
			var mapper = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (_, _, _) => log += "core;"
			};

			mapper[nameof(IView.Focus)] = (_, _, _) => log += "user;";
			mapper.ModifyMappingForControls<IView, IViewHandler>(nameof(IView.Focus), (_, _, _, _) => log += "controls;");

			mapper.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.Equal("user;", log);
		}

		[Fact]
		public void FrameworkModificationPreservesEarlierDirectReplacementOfChainedCommand()
		{
			var log = string.Empty;
			var parentMapper = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (_, _, _) => log += "core;"
			};
			var mapper = new CommandMapper<IView, IViewHandler>(parentMapper).WithFrameworkMappingsSealed();

			mapper[nameof(IView.Focus)] = (_, _, _) => log += "user;";
			mapper.ModifyMappingForControls<IView, IViewHandler>(nameof(IView.Focus), (_, _, _, _) => log += "controls;");

			mapper.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.Equal("user;", log);
		}
	}
}
