using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class InvalidationTriggerTests : BaseTestFixture
	{
		[Theory]
		[InlineData(InvalidationTrigger.MeasureChanged)]
		[InlineData(InvalidationTrigger.HorizontalOptionsChanged)]
		[InlineData(InvalidationTrigger.VerticalOptionsChanged)]
		[InlineData(InvalidationTrigger.SizeRequestChanged)]
		[InlineData(InvalidationTrigger.RendererReady)]
		[InlineData(InvalidationTrigger.MarginChanged)]
		[InlineData(InvalidationTrigger.Undefined)]
		public void InvalidationTriggerConvertsToFlagsAndBack(InvalidationTrigger trigger)
		{
			var flags = trigger.ToInvalidationTriggerFlags();
			var result = flags.ToInvalidationTrigger();

			Assert.Equal(trigger, result);
		}

		[Fact]
		public void InvalidationTriggerFlagsConvertsToHigherInvalidationTrigger()
		{
			foreach (var (flags, expected) in GetInvalidationTriggerFlagsConversionExpectations())
			{
				var result = ((InvalidationTriggerFlags)flags).ToInvalidationTrigger();
				Assert.Equal(expected, result);
			}
		}
		
		public static IEnumerable<(ushort Flags, InvalidationTrigger Trigger)> GetInvalidationTriggerFlagsConversionExpectations()
		{
			var flagsByImportance = new[] {
				InvalidationTriggerFlags.WillTriggerUndefined,
				InvalidationTriggerFlags.WillTriggerRendererReady,
				InvalidationTriggerFlags.WillTriggerMeasureChanged,
				InvalidationTriggerFlags.WillTriggerSizeRequestChanged,
				InvalidationTriggerFlags.WillTriggerMarginChanged,
				InvalidationTriggerFlags.WillTriggerVerticalOptionsChanged,
				InvalidationTriggerFlags.WillTriggerHorizontalOptionsChanged,
				InvalidationTriggerFlags.WillNotifyParentMeasureInvalidated,
				InvalidationTriggerFlags.ApplyingBindingContext
			};

			var invalidationTriggerExpected = new []
			{
				InvalidationTrigger.Undefined,
				InvalidationTrigger.RendererReady,
				InvalidationTrigger.MeasureChanged,
				InvalidationTrigger.SizeRequestChanged,
				InvalidationTrigger.MarginChanged,
				InvalidationTrigger.VerticalOptionsChanged,
				InvalidationTrigger.HorizontalOptionsChanged,
				InvalidationTrigger.Undefined,
				InvalidationTrigger.Undefined
			};

			for (int i = 0; i < flagsByImportance.Length; i++)
			{
				var baseFlag = flagsByImportance[i];
				var expected = invalidationTriggerExpected[i];
				var remainingFlags = flagsByImportance.Skip(i + 1).ToArray();
				foreach (InvalidationTriggerFlags combination in Combinations(remainingFlags))
				{
					yield return ((ushort)(baseFlag | combination), expected);
				}
			}
		}

		private static IEnumerable<InvalidationTriggerFlags> Combinations(InvalidationTriggerFlags[] data) {
			// https://stackoverflow.com/a/57058345
			return Enumerable
				.Range(0, 1 << data.Length)
				.Select(index => data
					.Where((v, i) => (index & (1 << i)) != 0)
					.DefaultIfEmpty()
					.Aggregate((a, b) => a | b));
		}
	}
}