using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.Handlers.Memory
{
	public class MemoryTestTypes : IEnumerable<object[]>
	{
		public static readonly IReadOnlyDictionary<Type, Type> Types = new Dictionary<Type, Type>
		{
			[typeof(DatePickerStub)] = typeof(DatePickerHandler),
			[typeof(EditorStub)] = typeof(EditorHandler),
			[typeof(EntryStub)] = typeof(EntryHandler),
			[typeof(SearchBarStub)] = typeof(SearchBarHandler),
		};

		public IEnumerator<object[]> GetEnumerator()
		{
			foreach (var pair in Types)
				yield return new object[] { (pair.Key, pair.Value) };
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
