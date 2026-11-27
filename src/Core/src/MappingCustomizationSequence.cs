using System.Threading;

namespace Microsoft.Maui
{
	/// <summary>
	/// Orders app customizations of property and command mappers across all mappers.
	/// </summary>
	static class MappingCustomizationSequence
	{
		/// <summary>
		/// Framework mappings and remaps precede every app customization.
		/// </summary>
		public const long Framework = 0;

		static long s_current;

		public static long Next() => Interlocked.Increment(ref s_current);

		/// <summary>
		/// The sequence of the most recently recorded app customization.
		/// </summary>
		public static long Current => Volatile.Read(ref s_current);
	}
}
