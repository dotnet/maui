using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.DeviceTests.Stubs;


namespace Microsoft.Maui.Handlers.Memory
{
	public class MemoryTestTypes : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
#if !ANDROID
			yield return new object[] { (typeof(DatePickerStub), typeof(DatePickerHandler)) };
#endif
			yield return new object[] { (typeof(EditorStub), typeof(EditorHandler)) };
			yield return new object[] { (typeof(EntryStub), typeof(EntryHandler)) };
			yield return new object[] { (typeof(SearchBarStub), typeof(SearchBarHandler)) };
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}