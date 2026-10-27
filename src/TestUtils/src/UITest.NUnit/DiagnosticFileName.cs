using System.Security.Cryptography;
using System.Text;

namespace UITest.Appium.NUnit;

internal static class DiagnosticFileName
{
	internal const int ArtifactPathLimit = 399;

	internal static int GetMaximumLength(string artifactName, string relativeResultsDirectory, string userName, string machineName)
	{
		ArgumentException.ThrowIfNullOrEmpty(artifactName);
		ArgumentException.ThrowIfNullOrEmpty(machineName);
		if (artifactName.IndexOfAny(['/', '\\']) >= 0)
			throw new ArgumentException("The artifact name must be a single path component.", nameof(artifactName));
		var relativeDirectory = relativeResultsDirectory == "." ? "" : relativeResultsDirectory.Replace('\\', '/');
		if (Path.IsPathRooted(relativeDirectory) || relativeDirectory.Split('/').Any(part => part is "." or ".."))
			throw new ArgumentException("Test results must be inside the published artifact directory.", nameof(relativeResultsDirectory));

		// TRX copies attachments into <user>@<machine> <timestamp>/In/<execution GUID>/<machine>.
		var deploymentRootLength = userName.Length + machineName.Length + 2 + "yyyy-MM-dd HH:mm:ss".Length;
		var directoryLength = artifactName.Length + 1 + (relativeDirectory.Length == 0 ? 0 : relativeDirectory.Length + 1) +
			deploymentRootLength + 1 + "In".Length + 1 + Guid.Empty.ToString("D").Length + 1 + machineName.Length + 1;
		return Math.Min(255, ArtifactPathLimit - directoryLength);
	}

	internal static string Create(string testName, string device, string? note, string fileName, Guid id, int maximumLength)
	{
		ArgumentException.ThrowIfNullOrEmpty(testName);
		ArgumentException.ThrowIfNullOrEmpty(fileName);
		if (fileName.IndexOfAny(['/', '\\', '<', '>', ':', '"', '|', '?', '*']) >= 0 || fileName.Any(char.IsControl))
			throw new ArgumentException("The diagnostic type must be a file name, not a path.", nameof(fileName));

		var stage = string.IsNullOrEmpty(note) ? "-" : $"-{note}-";
		var identity = $"{testName}-{device}{stage}";
		var type = Path.GetFileNameWithoutExtension(fileName);
		if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(Path.GetExtension(fileName)))
			throw new ArgumentException("The diagnostic file name must include a type and extension.", nameof(fileName));
		var extension = Path.GetExtension(fileName);
		var original = $"{identity}{type}-{id:N}{extension}";
		var limit = Math.Min(255, maximumLength);
		if (original.Length <= limit && Encoding.UTF8.GetByteCount(original) <= 255 &&
			original.IndexOfAny(['/', '\\', '<', '>', ':', '"', '|', '?', '*']) < 0 && !original.Any(char.IsControl))
			return original;

		var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))[..12].ToLowerInvariant();
		var suffix = $"-{hash}-{type}-{id:N}{extension}";
		var prefixLength = Math.Min(testName.Length, limit - suffix.Length);
		if (prefixLength <= 0)
			throw new InvalidOperationException($"The diagnostic path budget ({maximumLength}) cannot retain the test identity, type and full GUID.");

		var safeName = new string(testName.Select(c => char.IsControl(c) || "/\\<>:\"|?*".Contains(c, StringComparison.Ordinal) ? '_' : c).ToArray());
		while (prefixLength > 0)
		{
			if (char.IsHighSurrogate(safeName[prefixLength - 1]))
				prefixLength--;
			var compact = safeName[..prefixLength] + suffix;
			if (prefixLength > 0 && Encoding.UTF8.GetByteCount(compact) <= 255)
				return compact;
			prefixLength--;
		}

		throw new InvalidOperationException("The diagnostic file name cannot fit the filesystem's byte limit.");
	}
}
