using System;
using Microsoft.Maui.Controls;
using NSubstitute;
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

		[Fact]
		public void FrameworkMappingDoesNotReplacePostSealAssignmentOfAbsentKey()
		{
			var log = string.Empty;
			var mapper = new CommandMapper<IView, IViewHandler>().WithFrameworkMappingsSealed();

			mapper[nameof(IView.Focus)] = (_, _, _) => log += "user;";
			mapper.PrependToMappingForControls<IView, IViewHandler>(nameof(IView.Focus), (_, _, _) => log += "controls;");

			mapper.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.Equal("user;", log);
		}

		[Fact]
		public void ChildCustomizationOfInitiallyMissingParentKeyObservesLaterFrameworkMapping()
		{
			var log = string.Empty;
			var parentMapper = new CommandMapper<IView, IViewHandler>().WithFrameworkMappingsSealed();
			var childMapper = new CommandMapper<IView, IViewHandler>(parentMapper);

			childMapper.AppendToMapping(nameof(IView.Focus), (_, _, _) => log += "append;");
			parentMapper.PrependToMappingForControls<IView, IViewHandler>(nameof(IView.Focus), (_, _, _) => log += "controls;");

			childMapper.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.Equal("controls;append;", log);
		}

		[Fact]
		public void ChildCustomizationKeepsParentMappingFromWhenItWasRecorded()
		{
			var log = string.Empty;
			var parentMapper = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (_, _, _) => log += "A;"
			};
			var childMapper = new CommandMapper<IView, IViewHandler>(parentMapper);

			childMapper.AppendToMapping(nameof(IView.Focus), (_, _, _) => log += "C;");
			parentMapper[nameof(IView.Focus)] = (_, _, _) => log += "B;";

			childMapper.Invoke(null, new Button(), nameof(IView.Focus), null);
			Assert.Equal("A;C;", log);

			log = string.Empty;
			parentMapper.Invoke(null, new Button(), nameof(IView.Focus), null);
			Assert.Equal("B;", log);
		}

		[Fact]
		public void IndexerWrapperOfCustomizedKeyCallsPreviousMapping()
		{
			var log = string.Empty;
			var mapper = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (_, _, _) => log += "core;"
			}.WithFrameworkMappingsSealed();

			mapper.AppendToMapping(nameof(IView.Focus), (_, _, _) => log += "append;");
			var previous = mapper[nameof(IView.Focus)];
			mapper[nameof(IView.Focus)] = (handler, view, args) =>
			{
				previous(handler, view, args);
				log += "wrapper;";
			};
			mapper.PrependToMappingForControls<IView, IViewHandler>(nameof(IView.Focus), (_, _, _) => log += "controls;");

			mapper.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.Equal("controls;core;append;wrapper;", log);
		}

		[Fact]
		public void GetCommandWrapperOfFrameworkMappingIncludesLaterFrameworkRemap()
		{
			var log = string.Empty;
			var mapper = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (_, _, _) => log += "core;"
			}.WithFrameworkMappingsSealed();

			var previous = mapper.GetCommand(nameof(IView.Focus));
			mapper[nameof(IView.Focus)] = (handler, view, args) =>
			{
				previous!(handler, view, args);
				log += "wrapper;";
			};
			mapper.PrependToMappingForControls<IView, IViewHandler>(nameof(IView.Focus), (_, _, _) => log += "controls;");

			mapper.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.Equal("controls;core;wrapper;", log);
		}

		[Fact]
		public void CustomizationOfOverriddenGetCommandKeepsOverriddenCommand()
		{
			var log = string.Empty;
			var mapper = new OverriddenGetCommandMapper((_, _, _) => log += "A;");

			mapper.AppendToMapping(nameof(IView.Focus), (_, _, _) => log += "C;");

			mapper.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.Equal("A;C;", log);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void WrapperOfInheritedCommandCallsParentCommand(bool useIndexer)
		{
			var log = string.Empty;
			var parentMapper = CreateMapper(() => log += "core;");
			var childMapper = new CommandMapper<IView, IViewHandler>(parentMapper);

			var previous = GetPrevious(childMapper, useIndexer);
			childMapper[nameof(IView.Focus)] = (handler, view, args) =>
			{
				previous(handler, view, args);
				log += "wrapper;";
			};

			childMapper.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.Equal("core;wrapper;", log);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void WrapperOfInheritedCommandOfSealedMapperIncludesLaterFrameworkRemap(bool useIndexer)
		{
			var log = string.Empty;
			var parentMapper = CreateMapper(() => log += "core;").WithFrameworkMappingsSealed();
			var childMapper = new CommandMapper<IView, IViewHandler>(parentMapper).WithFrameworkMappingsSealed();

			var previous = GetPrevious(childMapper, useIndexer);
			childMapper[nameof(IView.Focus)] = (handler, view, args) =>
			{
				previous(handler, view, args);
				log += "wrapper;";
			};
			childMapper.PrependToMappingForControls<IView, IViewHandler>(nameof(IView.Focus), (_, _, _) => log += "controls;");

			childMapper.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.Equal("controls;core;wrapper;", log);
		}

		[Fact]
		public void GetCommandOfCustomizedOrInheritedKeyReturnsSameCommandUntilCustomizedAgain()
		{
			var parentMapper = CreateMapper(() => { }).WithFrameworkMappingsSealed();
			var childMapper = new CommandMapper<IView, IViewHandler>(parentMapper).WithFrameworkMappingsSealed();
			parentMapper.AppendToMapping(nameof(IView.Focus), (_, _, _) => { });

			var customized = parentMapper.GetCommand(nameof(IView.Focus));
			var inherited = childMapper.GetCommand(nameof(IView.Focus));

			Assert.Same(customized, parentMapper.GetCommand(nameof(IView.Focus)));
			Assert.Same(inherited, childMapper.GetCommand(nameof(IView.Focus)));

			var allocated = GC.GetAllocatedBytesForCurrentThread();
			for (var i = 0; i < 100; i++)
			{
				parentMapper.GetCommand(nameof(IView.Focus));
				childMapper.GetCommand(nameof(IView.Focus));
			}
			Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);

			parentMapper.AppendToMapping(nameof(IView.Focus), (_, _, _) => { });
			Assert.NotSame(customized, parentMapper.GetCommand(nameof(IView.Focus)));
		}

		[Fact]
		public void ChildFrameworkCommandIncludesEarlierParentAppCustomizations()
		{
			var log = string.Empty;
			var parentMapper = CreateMapper(() => log += "core;").WithFrameworkMappingsSealed();
			var childMapper = new CommandMapper<IView, IViewHandler>(parentMapper).WithFrameworkMappingsSealed();

			parentMapper.AppendToMapping(nameof(IView.Focus), (_, _, _) => log += "parent;");
			childMapper.PrependToMappingForControls<IView, IViewHandler>(nameof(IView.Focus), (_, _, _) => log += "controls;");
			parentMapper.AppendToMapping(nameof(IView.Focus), (_, _, _) => log += "later;");

			childMapper.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.Equal("controls;core;parent;", log);
		}

		[Fact]
		public void ReplacingGrandparentRefreshesChildCustomization()
		{
			var log = string.Empty;
			var firstGrandparent = CreateMapper(() => log += "A;");
			firstGrandparent[nameof(IView.Frame)] = (_, _, _) => { };
			var secondGrandparent = CreateMapper(() => log += "B;");
			var parentMapper = new CommandMapper<IView, IViewHandler>(firstGrandparent);
			var childMapper = new CommandMapper<IView, IViewHandler>(parentMapper);

			childMapper.AppendToMapping(nameof(IView.Focus), (_, _, _) => log += "C;");
			childMapper.Invoke(null, new Button(), nameof(IView.Focus), null);
			parentMapper.Chained = secondGrandparent;
			log = string.Empty;

			childMapper.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.Equal("B;C;", log);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void CustomizationKeepsChainedMapperAfterChainIsReplacedOrCleared(bool clear)
		{
			var log = string.Empty;
			var firstParent = CreateMapper(() => log += "A;");
			var secondParent = CreateMapper(() => log += "B;");
			var childMapper = new CommandMapper<IView, IViewHandler>(firstParent);

			childMapper.AppendToMapping(nameof(IView.Focus), (_, _, _) => log += "C;");
			childMapper.Chained = clear ? null : secondParent;

			childMapper.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.Equal("A;C;", log);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public void TypedFrameworkCommandFallsThroughForOtherVirtualViewOrIncompatibleHandler(bool prepend, bool incompatibleHandler)
		{
			var log = string.Empty;
			var args = new object();
			var handler = incompatibleHandler ? Substitute.For<IViewHandler>() : null;
			IView view = incompatibleHandler ? new Label() : new Button();
			var parentMapper = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (h, v, a) => log += $"previous:{ReferenceEquals(h, handler)}:{ReferenceEquals(v, view)}:{ReferenceEquals(a, args)};"
			};
			var mapper = new CommandMapper<IView, IViewHandler>(parentMapper).WithFrameworkMappingsSealed();

			if (prepend)
				mapper.PrependToMappingForControls<ILabel, Microsoft.Maui.Handlers.ILabelHandler>(nameof(IView.Focus), (_, _, _) => log += "controls;");
			else
				mapper.ModifyMappingForControls<ILabel, Microsoft.Maui.Handlers.ILabelHandler>(nameof(IView.Focus), (_, _, _, _) => log += "controls;");

			mapper.Invoke(handler!, view, nameof(IView.Focus), args);

			Assert.Equal("previous:True:True:True;", log);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void TypedPublicModifyMappingFallsThroughForIncompatibleHandlerOrView(bool incompatibleHandler)
		{
			var log = string.Empty;
			var handler = incompatibleHandler ? Substitute.For<IViewHandler>() : null;
			IView view = incompatibleHandler ? new Label() : new Button();
			ICommandMapper<IElement, IElementHandler> mapper = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (_, _, _) => log += "previous;"
			};

			mapper.ModifyMapping<ILabel, Microsoft.Maui.Handlers.ILabelHandler>(nameof(IView.Focus), (_, _, _, _) => log += "user;");
			mapper.Invoke(handler!, view, nameof(IView.Focus), null);

			Assert.Equal("previous;", log);
		}

		static CommandMapper<IView, IViewHandler> CreateMapper(Action command) =>
			new()
			{
				[nameof(IView.Focus)] = (_, _, _) => command()
			};

		static Action<IElementHandler, IElement, object> GetPrevious(CommandMapper<IView, IViewHandler> mapper, bool useIndexer)
		{
			if (!useIndexer)
				return mapper.GetCommand(nameof(IView.Focus))!;

			var indexed = mapper[nameof(IView.Focus)];
			return (handler, view, args) => indexed((IViewHandler)handler, (IView)view, args);
		}

		sealed class OverriddenGetCommandMapper : CommandMapper<IView, IViewHandler>
		{
			readonly Action<IElementHandler, IElement, object> _command;

			public OverriddenGetCommandMapper(Action<IElementHandler, IElement, object> command) => _command = command;

			public override Action<IElementHandler, IElement, object> GetCommand(string key) =>
				base.GetCommand(key) ?? (key == nameof(IView.Focus) ? _command : null);
		}
	}
}
