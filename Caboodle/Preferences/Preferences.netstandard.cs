using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Caboodle
{
	public partial class Preferences
	{
		public bool ContainsKey(string key) =>
			throw new NotImplentedInReferenceAssembly();

		public void Remove(string key) =>
			throw new NotImplentedInReferenceAssembly();

		public void Clear() =>
			throw new NotImplentedInReferenceAssembly();

		void Set<T>(string key, T value) =>
			throw new NotImplentedInReferenceAssembly();

		T Get<T>(string key, T defaultValue) =>
			throw new NotImplentedInReferenceAssembly();
	}
}
