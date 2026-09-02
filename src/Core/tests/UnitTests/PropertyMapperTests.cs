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

		// Even if a mapper throws partway through a bulk pass, the pass must be considered over - so any
		// pass-scoped state a mapper implementation recorded against that pass's id (e.g. a "skip the next
		// redundant redirected call" mark) can never again be matched once the pass has ended, whether it
		// ended normally or via exception.
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
