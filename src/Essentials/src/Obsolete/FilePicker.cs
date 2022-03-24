#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/FilePicker.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FilePicker']/Docs" />
	public static partial class FilePicker
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePicker.xml" path="//Member[@MemberName='PickAsync']/Docs" />
		public static Task<FileResult?> PickAsync(PickOptions? options = null) =>
			Current.PickAsync(options);

		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePicker.xml" path="//Member[@MemberName='PickMultipleAsync']/Docs" />
		public static Task<IEnumerable<FileResult>> PickMultipleAsync(PickOptions? options = null) =>
			Current.PickMultipleAsync(options);
	
		static IFilePicker Current => Storage.FilePicker.Default;
	}
}
