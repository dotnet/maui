using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	static class TestDataHelpers
	{
		static IEnumerable<IEnumerable<T>> CartesianProduct<T>(
			this IEnumerable<IEnumerable<T>> sequences)
		{
			IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
			return sequences.Aggregate(
			  emptyProduct,
			  (accumulator, sequence) =>
				from accseq in accumulator
				from item in sequence
				select accseq.Concat(new[] { item }));
		}

		public static IEnumerable<object[]> Combinations<T>(IEnumerable<IEnumerable<T>> inputs)
		{
			foreach (var set in CartesianProduct(inputs))
			{
				yield return set.Cast<object>().ToArray();
			}
		}

		public static IEnumerable<object[]> Combinations<T>(IEnumerable<T> inputs)
		{
			var all = new List<IEnumerable<T>>(inputs.Count());
			foreach (var _ in inputs)
			{
				all.Add(new List<T>(inputs));
			}

			return Combinations(all);
		}

		public static IEnumerable<object[]> Range(int start, int end)
		{
			return Range(start, end, 2);
		}

		public static IEnumerable<object[]> Range(int start, int end, int parameterCount)
		{
			var range = Enumerable.Range(start, end - start + 1);

			var paramSets = (from x in range select new List<object> { x }.AsEnumerable());

			while (parameterCount > 1)
			{
				paramSets = from e1 in paramSets
							from e2 in range
							select e1.Concat(new List<object> { e2 });

				parameterCount -= 1;
			}

			foreach (var paramSet in paramSets)
			{
				yield return paramSet.ToArray();
			}
		}

		public static IEnumerable<object[]> TrueFalse()
		{
			return Combinations(new List<bool>() { true, false });
		}
	}
}
