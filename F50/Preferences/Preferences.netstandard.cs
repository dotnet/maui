using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.F50
{
    public partial class Preferences
    {
		public bool ContainsKey(string key) =>
			throw new NotImplentedInReferenceAssembly();

		public bool Remove(string key) =>
			throw new NotImplentedInReferenceAssembly();

		public bool Clear() =>
			throw new NotImplentedInReferenceAssembly();

		bool Set<T>(string key, T value) =>
			throw new NotImplentedInReferenceAssembly();

		T Get<T>(string key, T defaultValue) =>
			throw new NotImplentedInReferenceAssembly();
		
		void Dispose(bool disposing)
		{
			throw new NotImplentedInReferenceAssembly();
		}
	}
}
