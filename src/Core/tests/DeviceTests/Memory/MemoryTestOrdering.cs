using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;


namespace Microsoft.Maui.Handlers.Memory
{
	public class MemoryTestOrdering : ITestCaseOrderer
	{
		public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
				where TTestCase : ITestCase
		{
			var result = testCases.ToList();


			if (result.Count > 2)
				throw new InvalidOperationException("Add new test to sort if you want it to run");

			var method1 = result.FirstOrDefault(x => x.TestMethod.Method.Name == nameof(MemoryTests.Allocate));
			var method2 = result.FirstOrDefault(x => x.TestMethod.Method.Name == nameof(MemoryTests.CheckAllocation));

			if (method1 != null)
				yield return method1;

			if (method2 != null)
				yield return method2;
		}
	}
}