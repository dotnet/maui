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
		public void FrameworkMappingDoesNotReplacePostSealAssignmentOfAbsentKey()
		{
			var log = string.Empty;
			var mapper = CreateMapper(() => log += "core;").WithFrameworkMappingsSealed();

			mapper[nameof(IView.Opacity)] = (_, _) => log += "user;";
			mapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Opacity), (_, _) => log += "controls;");

			mapper.UpdateProperty(null, new Button(), nameof(IView.Opacity));

			Assert.Equal("user;", log);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ChildCustomizationOfInitiallyMissingParentKeyObservesLaterFrameworkMapping(bool sealChild)
		{
			var log = string.Empty;
			var parentMapper = CreateMapper(() => log += "core;").WithFrameworkMappingsSealed();
			var childMapper = new PropertyMapper<IView, IViewHandler>(parentMapper);
			if (sealChild)
				childMapper.WithFrameworkMappingsSealed();

			childMapper.AppendToMapping(nameof(IView.Opacity), (_, _) => log += "append;");
			parentMapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Opacity), (_, _) => log += "controls;");

			childMapper.UpdateProperty(null, new Button(), nameof(IView.Opacity));

			Assert.Equal("controls;append;", log);
		}

		[Fact]
		public void ChildCustomizationKeepsParentMappingFromWhenItWasRecorded()
		{
			var log = string.Empty;
			var parentMapper = CreateMapper(() => log += "A;");
			var childMapper = new PropertyMapper<IView, IViewHandler>(parentMapper);

			childMapper.AppendToMapping(nameof(IView.Background), (_, _) => log += "C;");
			parentMapper[nameof(IView.Background)] = (_, _) => log += "B;";
			parentMapper.AppendToMapping(nameof(IView.Background), (_, _) => log += "P;");

			childMapper.UpdateProperties(null, new Button());
			Assert.Equal("A;C;", log);

			log = string.Empty;
			parentMapper.UpdateProperties(null, new Button());
			Assert.Equal("B;P;", log);
		}

		[Fact]
		public void ChildCustomizationIncludesEarlierParentCustomizationAndLaterFrameworkRemap()
		{
			var log = string.Empty;
			var parentMapper = CreateMapper(() => log += "core;");
			var childMapper = new PropertyMapper<IView, IViewHandler>(parentMapper);

			parentMapper.AppendToMapping(nameof(IView.Background), (_, _) => log += "parent;");
			childMapper.AppendToMapping(nameof(IView.Background), (_, _) => log += "child;");
			parentMapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => log += "controls;");

			childMapper.UpdateProperties(null, new Button());

			Assert.Equal("controls;parent;child;", log);
		}

		[Fact]
		public void ChildFrameworkMappingIgnoresParentAppCustomizations()
		{
			var log = string.Empty;
			var parentMapper = CreateMapper(() => log += "core;").WithFrameworkMappingsSealed();
			var childMapper = new PropertyMapper<IView, IViewHandler>(parentMapper).WithFrameworkMappingsSealed();

			// A framework remap of the child would have run at startup, before any app customization of the parent.
			parentMapper.AppendToMapping(nameof(IView.Background), (_, _) => log += "parent;");
			childMapper.AppendToMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => log += "controls;");

			childMapper.UpdateProperties(null, new Button());

			Assert.Equal("core;controls;", log);
		}

		[Fact]
		public void CachedMappingObservesLaterFrameworkRemap()
		{
			var log = string.Empty;
			var mapper = CreateMapper(() => log += "core;").WithFrameworkMappingsSealed();

			mapper.AppendToMapping(nameof(IView.Background), (_, _) => log += "user;");
			var mapping = mapper.GetProperty(nameof(IView.Background));
			mapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => log += "controls;");

			mapping!(null!, new Button());

			Assert.Equal("controls;user;", log);
		}

		[Fact]
		public void ChildMergedMappingsObserveLaterParentFrameworkRemap()
		{
			var log = string.Empty;
			var parentMapper = CreateMapper(() => log += "core;").WithFrameworkMappingsSealed();
			var childMapper = new PropertyMapper<IView, IViewHandler>(parentMapper);

			childMapper.UpdateProperties(null, new Button());
			childMapper.UpdateProperty(null, new Button(), nameof(IView.Background));
			parentMapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => log += "controls;");

			log = string.Empty;
			childMapper.UpdateProperties(null, new Button());
			childMapper.UpdateProperty(null, new Button(), nameof(IView.Background));

			Assert.Equal("controls;controls;", log);
		}

		[Fact]
		public void LaterCustomizationOfEarlierChainedMapperDoesNotHideCapturedSiblingMapping()
		{
			var log = string.Empty;
			var firstParent = new PropertyMapper<IView, IViewHandler>();
			var secondParent = CreateMapper(() => log += "A;");
			var mapper = new PropertyMapper<IView, IViewHandler>(firstParent, secondParent);

			mapper.AppendToMapping(nameof(IView.Background), (_, _) => log += "C;");
			firstParent.AppendToMapping(nameof(IView.Background), (_, _) => log += "X;");

			mapper.UpdateProperty(null, new Button(), nameof(IView.Background));

			Assert.Equal("A;C;", log);
		}

		[Fact]
		public void ChildCustomizationUsesOverriddenParentGetProperty()
		{
			var log = string.Empty;
			var parentMapper = new OverriddenGetPropertyMapper((_, _) => log += "A;");
			var childMapper = new PropertyMapper<IView, IViewHandler>(parentMapper);

			childMapper.AppendToMapping(nameof(IView.Background), (_, _) => log += "C;");

			childMapper.UpdateProperty(null, new Button(), nameof(IView.Background));

			Assert.Equal("A;C;", log);
		}

		[Fact]
		public void ModifyMappingOfAbsentKeyReceivesNullPrevious()
		{
			bool? previousWasNull = null;
			var mapper = new PropertyMapper<IView, IViewHandler>().WithFrameworkMappingsSealed();

			mapper.ModifyMapping(nameof(IView.Opacity), (_, _, previous) => previousWasNull = previous is null);

			mapper.UpdateProperty(null, new Button(), nameof(IView.Opacity));

			Assert.True(previousWasNull);
		}

		[Fact]
		public void IndexerWrapperOfCustomizedKeyCallsPreviousMapping()
		{
			var log = string.Empty;
			var mapper = CreateMapper(() => log += "core;").WithFrameworkMappingsSealed();

			mapper.AppendToMapping(nameof(IView.Background), (_, _) => log += "append;");
			var previous = mapper[nameof(IView.Background)];
			mapper[nameof(IView.Background)] = (handler, view) =>
			{
				previous(handler, view);
				log += "wrapper;";
			};
			mapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => log += "controls;");

			mapper.UpdateProperty(null, new Button(), nameof(IView.Background));

			Assert.Equal("controls;append;wrapper;", log);
		}

		[Fact]
		public void IndexerWrapperOfFrameworkMappingIncludesLaterFrameworkRemap()
		{
			var log = string.Empty;
			var mapper = CreateMapper(() => log += "core;").WithFrameworkMappingsSealed();

			var previous = mapper[nameof(IView.Background)];
			mapper[nameof(IView.Background)] = (handler, view) =>
			{
				previous(handler, view);
				log += "wrapper;";
			};
			mapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => log += "controls;");

			mapper.UpdateProperty(null, new Button(), nameof(IView.Background));

			Assert.Equal("controls;wrapper;", log);
		}

		[Fact]
		public void GetPropertyWrapperOfFrameworkMappingIncludesLaterFrameworkRemap()
		{
			var log = string.Empty;
			var mapper = CreateMapper(() => log += "core;").WithFrameworkMappingsSealed();

			var previous = mapper.GetProperty(nameof(IView.Background));
			mapper[nameof(IView.Background)] = (handler, view) =>
			{
				previous!(handler, view);
				log += "wrapper;";
			};
			mapper.ReplaceMappingForControls<IView, IViewHandler>(nameof(IView.Background), (_, _) => log += "controls;");

			mapper.UpdateProperty(null, new Button(), nameof(IView.Background));

			Assert.Equal("controls;wrapper;", log);
		}

		[Fact]
		public void CustomizationOfOverriddenGetPropertyKeepsOverriddenMapping()
		{
			var log = string.Empty;
			var mapper = new OverriddenGetPropertyMapper((_, _) => log += "A;");

			mapper.AppendToMapping(nameof(IView.Background), (_, _) => log += "C;");

			mapper.GetProperty(nameof(IView.Background))!(null!, new Button());

			Assert.Equal("A;C;", log);
		}

		[Fact]
		public void CustomizationOfOverriddenLocalKeyStartsFromOverriddenMapping()
		{
			var log = string.Empty;
			var mapper = new ReplacingGetPropertyMapper((_, _) => log += "override;")
			{
				[nameof(IView.Background)] = (_, _) => log += "core;"
			};

			mapper.AppendToMapping(nameof(IView.Background), (_, _) => log += "append;");
			mapper.Overriding = false;

			mapper.UpdateProperty(null, new Button(), nameof(IView.Background));

			Assert.Equal("override;append;", log);
		}

		[Fact]
		public void TypedFrameworkMappingsSkipOtherVirtualViewTypes()
		{
			var log = string.Empty;
			// A mapper typed for an interface also serves other implementations of that interface.
			var mapper = new PropertyMapper<ILabel, IViewHandler>
			{
				[nameof(IView.Background)] = (_, _) => log += "core;"
			}.WithFrameworkMappingsSealed();

			mapper.AppendToMappingForControls<ILabel, IViewHandler>(nameof(IView.Background), (_, _) => log += "append;");
			mapper.PrependToMappingForControls<ILabel, IViewHandler>(nameof(IView.Background), (_, _) => log += "prepend;");
			mapper.ModifyMappingForControls<ILabel, IViewHandler>(nameof(IView.Background), (handler, view, previous) =>
			{
				log += "modify;";
				previous?.Invoke(handler, view);
			});

			// Button is not an ILabel: the framework layers must fall through without casting it.
			mapper.UpdateProperty(null, new Button(), nameof(IView.Background));
			Assert.Equal(string.Empty, log);

			log = string.Empty;
			mapper.UpdateProperty(null, new Label(), nameof(IView.Background));
			Assert.Equal("modify;prepend;core;append;", log);
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

		sealed class OverriddenGetPropertyMapper : PropertyMapper<IView, IViewHandler>
		{
			readonly Action<IElementHandler, IElement> _mapping;

			public OverriddenGetPropertyMapper(Action<IElementHandler, IElement> mapping) => _mapping = mapping;

			public override Action<IElementHandler, IElement> GetProperty(string key) =>
				base.GetProperty(key) ?? (key == nameof(IView.Background) ? _mapping : null);
		}

		sealed class ReplacingGetPropertyMapper : PropertyMapper<IView, IViewHandler>
		{
			readonly Action<IElementHandler, IElement> _mapping;

			public ReplacingGetPropertyMapper(Action<IElementHandler, IElement> mapping) => _mapping = mapping;

			public bool Overriding { get; set; } = true;

			public override Action<IElementHandler, IElement> GetProperty(string key) =>
				Overriding && key == nameof(IView.Background) ? _mapping : base.GetProperty(key);
		}

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
