using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.PropertyMapping)]
	public class PropertyMapperTests
	{
		[Fact]
		public void MapperRetainsInsertionOrder()
		{
			// This test ensures the internal Dictionary implementation is not changed.

			// Thanks to the way Dictionary is internally implemented,
			// when looping through the dictionary entries, and considering _mapper is an append-only dictionary,
			// the order is guaranteed to be the same as the order mappers were added.

			var mapper = new PropertyMapper<IElement>();
			var letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			var insertedProperties = new List<string>();
			var updatedProperties = new List<string>();
			for (int i = 0; i < 1000; i++)
			{
				var randomPropertyName = string.Join(string.Empty, Enumerable.Range(0, Random.Shared.Next(8, 60))
					.Select(chars => string.Join(string.Empty,
						Enumerable.Range(0, chars).Select(_ => letters[Random.Shared.Next(0, letters.Length)]))));

				insertedProperties.Add(randomPropertyName);
				mapper.Add(randomPropertyName, (h, v) => updatedProperties.Add(randomPropertyName));
			}

			var keys = mapper.GetKeys().ToList();
			mapper.UpdateProperties(null!, new Button());
			Assert.Equal(insertedProperties, keys);
			Assert.Equal(insertedProperties, updatedProperties);
		}

		// Regression test for PropertyMapperPassScope, the pass/generation-scoped tracking used by
		// VisualElement's BackgroundColor/BackgroundImageSource/SemanticProperties mapper redirects to
		// distinguish a genuine step of *this* bulk UpdateProperties pass from an unrelated single-key
		// UpdateProperty call that merely happens to touch the same handler at a different time.
		[Fact]
		public void UpdatePropertiesPassScopeIsActiveDuringBulkPassAndClearedAfterward()
		{
			var handler = new HandlerStub();
			int? passIdDuringA = null;
			int? passIdDuringB = null;

			var mapper = new PropertyMapper<IView, IViewHandler>
			{
				["A"] = (h, v) => passIdDuringA = PropertyMapperPassScope.GetCurrentPassId(h),
				["B"] = (h, v) => passIdDuringB = PropertyMapperPassScope.GetCurrentPassId(h),
			};

			Assert.Equal(0, PropertyMapperPassScope.GetCurrentPassId(handler));

			mapper.UpdateProperties(handler, new Button());

			// Both mappers ran as part of the very same bulk pass, so they must have observed the same,
			// non-zero pass id.
			Assert.NotNull(passIdDuringA);
			Assert.NotEqual(0, passIdDuringA);
			Assert.Equal(passIdDuringA, passIdDuringB);

			// The pass must be considered over once UpdateProperties has returned.
			Assert.Equal(0, PropertyMapperPassScope.GetCurrentPassId(handler));

			// A later, standalone single-key update is not part of a bulk pass, so it must report no
			// active pass at all - and must get a fresh id from a subsequent bulk pass, never reusing
			// (or being confused with) the earlier one.
			mapper.UpdateProperty(handler, new Button(), "A");
			Assert.Equal(0, PropertyMapperPassScope.GetCurrentPassId(handler));

			int? passIdDuringSecondPass = null;
			var mapper2 = new PropertyMapper<IView, IViewHandler>
			{
				["A"] = (h, v) => passIdDuringSecondPass = PropertyMapperPassScope.GetCurrentPassId(h),
			};
			mapper2.UpdateProperties(handler, new Button());
			Assert.NotNull(passIdDuringSecondPass);
			Assert.NotEqual(passIdDuringA, passIdDuringSecondPass);
		}

		// Even if a mapper throws partway through a bulk pass, the pass must be popped on the way out, so
		// nothing that ran within it can ever again be mistaken for a step of a still-running pass.
		[Fact]
		public void UpdatePropertiesPassScopeIsClearedInFinallyWhenAMapperThrows()
		{
			var handler = new HandlerStub();
			int? passIdBeforeThrow = null;

			var mapper = new PropertyMapper<IView, IViewHandler>
			{
				["A"] = (h, v) => passIdBeforeThrow = PropertyMapperPassScope.GetCurrentPassId(h),
				["Throws"] = (h, v) => throw new InvalidOperationException("Simulated mapper failure"),
				["C"] = (h, v) => Assert.Fail("Should not run - the pass was aborted by an earlier mapper throwing"),
			};

			Assert.Throws<InvalidOperationException>(() => mapper.UpdateProperties(handler, new Button()));

			Assert.NotNull(passIdBeforeThrow);
			Assert.NotEqual(0, passIdBeforeThrow);

			// The aborted pass must not be left "active" - a later caller must see no active pass.
			Assert.Equal(0, PropertyMapperPassScope.GetCurrentPassId(handler));
		}

		// The pass scope is a stack: a nested UpdateProperties pass - on the same handler or on another
		// one - must restore the pass that enclosed it when it ends, so the outer pass keeps running with
		// its own identity and its own "current key" position.
		[Fact]
		public void NestedPassOnTheSameHandlerRestoresTheOuterPassWhenItEnds()
		{
			var handler = new HandlerStub();
			var view = new Button();

			int outerPassId = 0;
			int innerPassId = 0;
			int outerPassIdAfterNesting = 0;

			var innerMapper = new PropertyMapper<IView, IViewHandler>
			{
				["Inner"] = (h, v) => innerPassId = PropertyMapperPassScope.GetCurrentPassId(h),
			};

			var outerMapper = new PropertyMapper<IView, IViewHandler>
			{
				["BeforeNesting"] = (h, v) => outerPassId = PropertyMapperPassScope.GetCurrentPassId(h),
				["Nest"] = (h, v) => innerMapper.UpdateProperties(h, v),
				["AfterNesting"] = (h, v) => outerPassIdAfterNesting = PropertyMapperPassScope.GetCurrentPassId(h),
			};

			outerMapper.UpdateProperties(handler, view);

			Assert.NotEqual(0, outerPassId);
			Assert.NotEqual(0, innerPassId);
			Assert.NotEqual(outerPassId, innerPassId);

			// The outer pass must be exactly the same pass it was before the nested one ran.
			Assert.Equal(outerPassId, outerPassIdAfterNesting);
			Assert.Equal(0, PropertyMapperPassScope.GetCurrentPassId(handler));
		}

		// A nested pass on a *different* handler must not make the outer handler's pass look current while
		// it runs, and must restore it once it is done.
		[Fact]
		public void NestedPassOnAnotherHandlerDoesNotLeakOrClobberTheOuterHandlersPass()
		{
			var outerHandler = new HandlerStub();
			var innerHandler = new HandlerStub();
			var view = new Button();

			int outerPassId = 0;
			int outerPassIdSeenFromInnerPass = -1;
			int outerPassIdAfterNesting = 0;

			var innerMapper = new PropertyMapper<IView, IViewHandler>
			{
				["Inner"] = (h, v) => outerPassIdSeenFromInnerPass = PropertyMapperPassScope.GetCurrentPassId(outerHandler),
			};

			var outerMapper = new PropertyMapper<IView, IViewHandler>
			{
				["BeforeNesting"] = (h, v) => outerPassId = PropertyMapperPassScope.GetCurrentPassId(h),
				["Nest"] = (h, v) => innerMapper.UpdateProperties(innerHandler, v),
				["AfterNesting"] = (h, v) => outerPassIdAfterNesting = PropertyMapperPassScope.GetCurrentPassId(h),
			};

			outerMapper.UpdateProperties(outerHandler, view);

			Assert.NotEqual(0, outerPassId);
			Assert.Equal(0, outerPassIdSeenFromInnerPass);
			Assert.Equal(outerPassId, outerPassIdAfterNesting);
		}

		// A nested pass aborted by an exception must still restore the enclosing pass on the way out.
		[Fact]
		public void NestedPassThatThrowsStillRestoresTheOuterPass()
		{
			var handler = new HandlerStub();
			var view = new Button();

			int outerPassId = 0;
			int outerPassIdAfterNesting = 0;

			var innerMapper = new PropertyMapper<IView, IViewHandler>
			{
				["Throws"] = (h, v) => throw new InvalidOperationException("Simulated nested mapper failure"),
			};

			var outerMapper = new PropertyMapper<IView, IViewHandler>
			{
				["BeforeNesting"] = (h, v) => outerPassId = PropertyMapperPassScope.GetCurrentPassId(h),
				["Nest"] = (h, v) =>
				{
					Assert.Throws<InvalidOperationException>(() => innerMapper.UpdateProperties(h, v));
				},
				["AfterNesting"] = (h, v) => outerPassIdAfterNesting = PropertyMapperPassScope.GetCurrentPassId(h),
			};

			outerMapper.UpdateProperties(handler, view);

			Assert.NotEqual(0, outerPassId);
			Assert.Equal(outerPassId, outerPassIdAfterNesting);
			Assert.Equal(0, PropertyMapperPassScope.GetCurrentPassId(handler));
		}

		// A bulk pass must report the key it is currently mapping, which is what lets a redirect mapping
		// recognize its own - provably redundant - turn within the pass.
		[Fact]
		public void BulkPassReportsTheKeyCurrentlyBeingMapped()
		{
			var handler = new HandlerStub();
			var view = new Button();
			var observed = new List<string>();

			var mapper = new PropertyMapper<IView, IViewHandler>();
			foreach (var key in new[] { "A", "B", "C" })
			{
				mapper.Add(key, (h, v) =>
				{
					var pass = PropertyMapperPassScope.Current;
					observed.Add(pass!.Keys![pass.CurrentKeyIndex]);
				});
			}

			mapper.UpdateProperties(handler, view);

			Assert.Equal(new[] { "A", "B", "C" }, observed);
			Assert.Null(PropertyMapperPassScope.Current);
		}

		// IsRedundantBulkPassRedirect is the primitive the Controls-side redirects are built on: it must be
		// true *only* on the redirect key's own step of a bulk pass for that same handler, and only when
		// the canonical key already had its turn earlier in the very same pass.
		[Fact]
		public void IsRedundantBulkPassRedirectIsTrueOnlyOnTheRedirectKeysOwnStepAfterTheCanonicalKey()
		{
			var handler = new HandlerStub();
			var otherHandler = new HandlerStub();
			var view = new Button();

			var results = new List<bool>();
			bool onRedirectStepForAnotherHandler = true;
			bool onALaterUnrelatedStep = true;

			PropertyMapper<IView, IViewHandler> mapper = null;
			mapper = new PropertyMapper<IView, IViewHandler>
			{
				["Canonical"] = (h, v) => results.Add(PropertyMapperPassScope.IsRedundantBulkPassRedirect(h, "Redirect", "Canonical")),
				["Redirect"] = (h, v) =>
				{
					results.Add(PropertyMapperPassScope.IsRedundantBulkPassRedirect(h, "Redirect", "Canonical"));
					onRedirectStepForAnotherHandler = PropertyMapperPassScope.IsRedundantBulkPassRedirect(otherHandler, "Redirect", "Canonical");
				},
				["Later"] = (h, v) =>
				{
					onALaterUnrelatedStep = PropertyMapperPassScope.IsRedundantBulkPassRedirect(h, "Redirect", "Canonical");

					// A re-entrant single-key update raised by a mapper running later in the same pass is
					// not the redirect key's own pass step, so it must be honored.
					mapper.UpdateProperty(h, v, "Redirect");
				},
			};

			mapper.UpdateProperties(handler, view);

			// Canonical's own step, Redirect's own step (redundant), and Redirect re-entered from Later.
			Assert.Equal(new[] { false, true, false }, results);
			Assert.False(onRedirectStepForAnotherHandler);
			Assert.False(onALaterUnrelatedStep);

			// Outside of any bulk pass there is nothing redundant to skip.
			Assert.False(PropertyMapperPassScope.IsRedundantBulkPassRedirect(handler, "Redirect", "Canonical"));
			Assert.False(PropertyMapperPassScope.IsRedundantBulkPassRedirect(null, "Redirect", "Canonical"));
		}

		// If the canonical key runs *after* the redirect key (or is missing entirely), the redirect is not
		// redundant and must be applied - the de-duplication must never be able to drop the only chance a
		// value had to reach the platform.
		[Fact]
		public void IsRedundantBulkPassRedirectIsFalseWhenTheCanonicalKeyDoesNotPrecedeTheRedirectKey()
		{
			var handler = new HandlerStub();
			var view = new Button();

			bool canonicalAfterRedirect = true;
			bool canonicalMissing = true;

			var reorderedMapper = new PropertyMapper<IView, IViewHandler>
			{
				["Redirect"] = (h, v) => canonicalAfterRedirect = PropertyMapperPassScope.IsRedundantBulkPassRedirect(h, "Redirect", "Canonical"),
				["Canonical"] = (h, v) => { },
			};
			reorderedMapper.UpdateProperties(handler, view);

			var mapperWithoutCanonical = new PropertyMapper<IView, IViewHandler>
			{
				["Redirect"] = (h, v) => canonicalMissing = PropertyMapperPassScope.IsRedundantBulkPassRedirect(h, "Redirect", "Canonical"),
			};
			mapperWithoutCanonical.UpdateProperties(handler, view);

			Assert.False(canonicalAfterRedirect);
			Assert.False(canonicalMissing);
		}

		// Pass tracking is per-thread, so concurrent bulk passes - even for the very same handler - can
		// never corrupt each other's state or throw. (The previous ConditionalWeakTable Remove/Add
		// implementation could throw when two passes for one handler raced.)
		[Fact]
		public void ConcurrentBulkPassesForTheSameHandlerAreIndependentAndDoNotThrow()
		{
			var handler = new HandlerStub();
			const int threadCount = 8;
			const int iterations = 200;

			var observedPassIds = new System.Collections.Concurrent.ConcurrentBag<int>();
			var failures = new System.Collections.Concurrent.ConcurrentBag<Exception>();

			var mapper = new PropertyMapper<IView, IViewHandler>
			{
				["A"] = (h, v) =>
				{
					var passId = PropertyMapperPassScope.GetCurrentPassId(h);
					if (passId == 0)
					{
						throw new InvalidOperationException("A bulk pass must always be current while its own mappers run.");
					}

					observedPassIds.Add(passId);
				},
				["B"] = (h, v) => Assert.NotEqual(0, PropertyMapperPassScope.GetCurrentPassId(h)),
			};

			var threads = new System.Threading.Thread[threadCount];
			for (int t = 0; t < threadCount; t++)
			{
				threads[t] = new System.Threading.Thread(() =>
				{
					try
					{
						// Each thread uses its own virtual view; only the handler (and therefore the pass
						// bookkeeping that used to be keyed on it) is shared.
						var view = new Button();
						for (int i = 0; i < iterations; i++)
						{
							mapper.UpdateProperties(handler, view);
						}
					}
					catch (Exception ex)
					{
						failures.Add(ex);
					}
				});
				threads[t].Start();
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}

			Assert.Empty(failures);

			// Every pass got its own id, and none of them is left current on this thread.
			Assert.Equal(threadCount * iterations, observedPassIds.Count);
			Assert.Equal(threadCount * iterations, observedPassIds.Distinct().Count());
			Assert.Equal(0, PropertyMapperPassScope.GetCurrentPassId(handler));
		}

		[Fact]
		public void MapperExecutesChainedKeysFirst()
		{
			int counter = 0;

			int background3 = 0;
			int scale1 = 0;
			int scale3 = 0;
			int zindex2 = 0;
			int zindex3 = 0;

			var mapper1 = new PropertyMapper<IView>
			{
				[nameof(IView.Scale)] = (r, v) => scale1 = ++counter
			};

			var mapper2 = new PropertyMapper<IView>
			{
				[nameof(IView.ZIndex)] = (r, v) => zindex2 = ++counter
			};

			var mapper3 = new PropertyMapper<IButton>(mapper2, mapper1)
			{
				[nameof(IView.Background)] = (r, v) => background3 = ++counter,
				[nameof(IView.Scale)] = (r, v) => scale3 = ++counter,
				[nameof(IView.ZIndex)] = (r, v) => zindex3 = ++counter,
			};

			mapper3.UpdateProperties(null!, new Button());

			Assert.Equal(0, scale1);
			Assert.Equal(0, zindex2);
			Assert.Equal(1, scale3);
			Assert.Equal(2, zindex3);
			Assert.Equal(3, background3);
		}

		[Fact]
		public void MapperCanExecuteSkippedMappers()
		{
			int counter = 0;

			int scale1 = 0;
			int scale2 = 0;
			int zindex1 = 0;

			var mapper1 = new SkippingPropertyMapper<IView>()
			{
				[nameof(IView.Scale)] = (r, v) => scale1 = ++counter,
				[nameof(IView.ZIndex)] = (r, v) => zindex1 = ++counter
			};

			var mapper2 = new PropertyMapper<IButton>(mapper1)
			{
				[nameof(IView.Scale)] = (r, v) => scale2 = ++counter,
			};

			mapper2.UpdateProperties(null!, new Button());

			// ZIndex is skipped, so it should not be updated
			Assert.Equal(0, zindex1);
			// Scale is skipped in the first mapper, but not in the second
			Assert.Equal(0, scale1);
			Assert.Equal(1, scale2);

			mapper2.UpdateProperty(null!, new Button(), nameof(IView.ZIndex));

			// When updating a single property, the skipped mapper should be executed
			Assert.Equal(2, zindex1);
		}

		[Fact]
		public void ChainingMappersOverrideBase()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			var mapper1 = new PropertyMapper<IView>
			{
				[nameof(IView.Background)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<IButton>(mapper1)
			{
				[nameof(IView.Background)] = (r, v) => wasMapper2Called = true
			};

			mapper2.UpdateProperties(null, new Button());

			Assert.False(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}

		[Fact]
		public void ChainingMappersWorks()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			var mapper1 = new PropertyMapper<IView>
			{
				[nameof(IView.Background)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<ITextButton>(mapper1)
			{
				[nameof(ITextButton.TextColor)] = (r, v) => wasMapper2Called = true
			};

			mapper2.UpdateProperties(null, new Button());

			Assert.True(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}


		[Fact]
		public void ConstructorChainingMappersWorks()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			var mapper1 = new PropertyMapper<IView>
			{
				[nameof(IView.Background)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<ITextButton>()
			{
				[nameof(ITextButton.TextColor)] = (r, v) => wasMapper2Called = true
			};


			new PropertyMapper<ITextButton>(mapper2, mapper1)
				.UpdateProperties(null, new Button());

			Assert.True(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}

		[Fact]
		public void ConstructorChainingMappersOverrideBase()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			var mapper1 = new PropertyMapper<IView>
			{
				[nameof(IView.Background)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<IButton>()
			{
				[nameof(IView.Background)] = (r, v) => wasMapper2Called = true
			};

			new PropertyMapper<ITextButton>(mapper2, mapper1)
				.UpdateProperties(null, new Button());

			Assert.False(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}


		[Fact]
		public void ChainingMappersStillAllowReplacingChainedRoot()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			bool wasMapper3Called = false;
			var mapper1 = new PropertyMapper<IView>
			{
				[nameof(IView.Background)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<ITextButton>(mapper1)
			{
				[nameof(ITextButton.TextColor)] = (r, v) => wasMapper2Called = true
			};

			mapper1[nameof(IView.Background)] = (r, v) => wasMapper3Called = true;

			mapper2.UpdateProperties(null, new Button());

			Assert.False(wasMapper1Called, "Mapper 1 was called");
			Assert.True(wasMapper2Called, "Mapper 2 was called");
			Assert.True(wasMapper3Called, "Mapper 3 was called");
		}

		[Fact]
		public void GenericMappersWorks()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			var mapper1 = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<IButton, ButtonHandler>(mapper1)
			{
				[nameof(ITextStyle.TextColor)] = (r, v) => wasMapper2Called = true
			};

			mapper2.UpdateProperties(null, new Button());

			Assert.True(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}

		class SkippingPropertyMapper<T> : PropertyMapper<T>
			where T : IElement
		{
			public override IEnumerable<string> GetKeys()
			{
				return Enumerable.Empty<string>();
			}
		}
	}
}
