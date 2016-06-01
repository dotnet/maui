using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;

namespace Xamarin.Forms.Build.Tasks
{
	public class FixedCreateCSharpManifestResourceName : CreateCSharpManifestResourceName
	{
		[Output]
#pragma warning disable 0108
		public ITaskItem[] ResourceFilesWithManifestResourceNames { get; set; }
#pragma warning restore 0108

		public override bool Execute()
		{
			var ret = base.Execute();

			ResourceFilesWithManifestResourceNames = new TaskItem[ResourceFiles.Length];

			for (var i = 0; i < ResourceFiles.Length; i++)
			{
				var copy = new TaskItem(ResourceFiles[i]);
				copy.SetMetadata("ManifestResourceName", ManifestResourceNames[i].ItemSpec);
				ResourceFilesWithManifestResourceNames[i] = copy;
			}
			return ret;
		}
	}
}