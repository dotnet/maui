#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// Generates the freedesktop.org <c>.desktop</c> entry of a Linux (GTK 4) MAUI app next to the app: name, executable and icon
	/// name come from the project (<c>ApplicationTitle</c>, <c>AssemblyName</c>, <c>MauiIcon</c>). The file is named after the
	/// application id (<c>&lt;ApplicationId&gt;.desktop</c>), which is also the <c>GApplication</c> id of the running app, so
	/// Wayland and GNOME match the running window to the entry (and its icon).
	/// </summary>
	public class GenerateGtkDesktopFile : Task
	{
		[Required]
		public string IntermediateOutputPath { get; set; } = null!;

		/// <summary>The application id, e.g. <c>com.contoso.notes</c>. Without an id nothing is generated.</summary>
		public string? ApplicationId { get; set; }

		/// <summary>The display name (<c>Name=</c>). Falls back to the executable name.</summary>
		public string? ApplicationTitle { get; set; }

		/// <summary>The version shown by the entry (<c>X-AppVersion=</c>); optional.</summary>
		public string? ApplicationDisplayVersion { get; set; }

		/// <summary>The command that starts the app (<c>Exec=</c>), usually the apphost name. Installers make it absolute.</summary>
		[Required]
		public string Executable { get; set; } = null!;

		/// <summary>The <c>MauiIcon</c>/<c>IsAppIcon</c> items; the first one names the icon (<c>Icon=</c>).</summary>
		public ITaskItem[]? AppIcon { get; set; }

		/// <summary>The desktop categories (<c>Categories=</c>), <c>Utility;</c> when empty.</summary>
		public string? Categories { get; set; }

		/// <summary>The generated file, or nothing when there is no application id.</summary>
		[Output]
		public ITaskItem? GeneratedDesktopFile { get; set; }

		public override bool Execute()
		{
			try
			{
				if (string.IsNullOrWhiteSpace(ApplicationId))
				{
					Log.LogMessage(MessageImportance.Low, "No ApplicationId: the Linux desktop entry is not generated.");
					return true;
				}

				Directory.CreateDirectory(IntermediateOutputPath);

				var iconName = AppIcon is { Length: > 0 } icons ? GetIconName(icons[0]) : null;
				var content = Build(ApplicationId!, ApplicationTitle, ApplicationDisplayVersion, Executable, iconName, Categories);

				var path = Path.Combine(IntermediateOutputPath, ApplicationId + ".desktop");

				// Do not touch an unchanged file: it would re-copy it on every build.
				if (!File.Exists(path) || File.ReadAllText(path) != content)
					File.WriteAllText(path, content, new UTF8Encoding(false));

				GeneratedDesktopFile = new TaskItem(path);
				return true;
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex, true);
				return false;
			}
		}

		/// <summary>The icon name of a <c>MauiIcon</c> item: its <c>Link</c> (alias) or file name, without extension, like the generated icon files.</summary>
		internal static string GetIconName(ITaskItem icon)
		{
			var alias = icon.GetMetadata("Link");
			return Path.GetFileNameWithoutExtension(string.IsNullOrWhiteSpace(alias) ? icon.ItemSpec : alias);
		}

		/// <summary>Builds the desktop entry text (LF line ends, as the specification requires).</summary>
		internal static string Build(string applicationId, string? title, string? version, string executable, string? iconName, string? categories)
		{
			var builder = new StringBuilder();
			builder.Append("[Desktop Entry]\n");
			builder.Append("Type=Application\n");
			builder.Append("Version=1.0\n");
			builder.Append("Name=").Append(EscapeValue(string.IsNullOrWhiteSpace(title) ? executable : title!)).Append('\n');
			builder.Append("Exec=").Append(EscapeExec(executable)).Append('\n');

			if (!string.IsNullOrWhiteSpace(iconName))
				builder.Append("Icon=").Append(EscapeValue(iconName!)).Append('\n');

			builder.Append("Terminal=false\n");
			builder.Append("StartupNotify=true\n");
			builder.Append("StartupWMClass=").Append(EscapeValue(applicationId)).Append('\n');
			builder.Append("Categories=").Append(string.IsNullOrWhiteSpace(categories) ? "Utility;" : categories!.TrimEnd(';') + ";").Append('\n');

			if (!string.IsNullOrWhiteSpace(version))
				builder.Append("X-AppVersion=").Append(EscapeValue(version!)).Append('\n');

			return builder.ToString();
		}

		// Desktop entry string values: backslash and control characters are escaped.
		static string EscapeValue(string value) =>
			value.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");

		// Exec: a program with spaces or reserved characters is quoted, then the value is escaped once more.
		static string EscapeExec(string executable)
		{
			var needsQuotes = executable.Any(c => c == ' ' || c == '\t' || c == '"' || c == '\'' || c == '\\' || c == '`' || c == '$' || c == '<' || c == '>' || c == '~' || c == '|' || c == '&' || c == ';' || c == '*' || c == '?' || c == '#' || c == '(' || c == ')');
			if (!needsQuotes)
				return EscapeValue(executable);

			var quoted = "\"" + executable.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("`", "\\`").Replace("$", "\\$") + "\"";
			return EscapeValue(quoted);
		}
	}
}
