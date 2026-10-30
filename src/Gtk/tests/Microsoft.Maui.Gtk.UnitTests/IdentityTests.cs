using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Maui.Essentials.Gtk;
using Microsoft.Maui.Gtk.Hosting;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests;

/// <summary>The application identity the SDK emits (assembly metadata) and the icon files it copies next to the app.</summary>
public class IdentityTests
{
	static Assembly BuildAssembly(string name, params (string Key, string Value)[] metadata)
	{
		var builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Run);
		var ctor = typeof(AssemblyMetadataAttribute).GetConstructor(new[] { typeof(string), typeof(string) })!;
		foreach (var (key, value) in metadata)
			builder.SetCustomAttribute(new CustomAttributeBuilder(ctor, new object[] { key, value }));
		return builder;
	}

	[Theory]
	[InlineData("com.contoso.notes", true)]
	[InlineData("com.companyname.mauiapp", true)]
	[InlineData("org.maui.my-app_2", true)]
	[InlineData("single", false)]
	[InlineData("", false)]
	[InlineData(null, false)]
	[InlineData("com..notes", false)]
	[InlineData(".com.notes", false)]
	[InlineData("com.notes.", false)]
	[InlineData("com.1notes", false)]
	[InlineData("com.not es", false)]
	[InlineData("com.no\u00e9tes", false)]
	public void ApplicationIdValidity(string? id, bool valid) =>
		Assert.Equal(valid, GtkApplicationIdentity.IsValidApplicationId(id));

	[Fact]
	public void ApplicationIdTooLongIsInvalid() =>
		Assert.False(GtkApplicationIdentity.IsValidApplicationId("a." + new string('b', 254)));

	[Fact]
	public void MetadataIdIsUsedWhenValid()
	{
		var assembly = BuildAssembly("Notes.App", ("ApplicationId", "com.contoso.notes"));

		Assert.Equal("com.contoso.notes", GtkApplicationIdentity.ResolveApplicationId(assembly));
	}

	[Fact]
	public void InvalidMetadataIdFallsBackToTheAssemblyName()
	{
		var assembly = BuildAssembly("Notes.App", ("ApplicationId", "notes"));

		Assert.Equal("org.maui.notes_app", GtkApplicationIdentity.ResolveApplicationId(assembly));
	}

	[Fact]
	public void NoMetadataDerivesTheIdFromTheAssemblyName()
	{
		Assert.Equal("org.maui.notes_app", GtkApplicationIdentity.ResolveApplicationId(BuildAssembly("Notes.App")));
		Assert.Equal("org.maui.app1app", GtkApplicationIdentity.ResolveApplicationId(BuildAssembly("1App")));
		Assert.Equal("org.maui.maui", GtkApplicationIdentity.ResolveApplicationId(null));
	}

	[Fact]
	public void ApplicationTitleMetadataNamesTheApp()
	{
		var assembly = BuildAssembly("Notes.App", ("ApplicationTitle", "Contoso Notes"), ("ApplicationId", "com.contoso.notes"));

		var identity = AppIdentity.Resolve(assembly);

		Assert.Equal("Contoso Notes", identity.Name);
		Assert.Equal("com.contoso.notes", identity.ApplicationId);
	}

	[Fact]
	public void ApplicationTitleOptionBeatsMetadata()
	{
		var assembly = BuildAssembly("Notes.App", ("ApplicationTitle", "Contoso Notes"));

		Assert.Equal("Override", AppIdentity.Resolve(assembly, null, "Override").Name);
	}

	[Fact]
	public void IconNameIsThatOfTheLargestSize()
	{
		using var temp = new Microsoft.Maui.Gtk.UnitTests.Essentials.TempDir();
		foreach (var size in new[] { "48x48", "256x256", "128x128" })
		{
			var folder = Path.Combine(temp.Path, "Resources", "Icons", "hicolor", size, "apps");
			Directory.CreateDirectory(folder);
			File.WriteAllBytes(Path.Combine(folder, size == "256x256" ? "appicon.png" : "other.png"), new byte[] { 0 });
		}

		Assert.Equal("appicon", GtkApplicationIdentity.FindIconName(temp.Path));
	}

	[Fact]
	public void NoIconFilesNoIconName()
	{
		using var temp = new Microsoft.Maui.Gtk.UnitTests.Essentials.TempDir();

		Assert.Null(GtkApplicationIdentity.FindIconName(temp.Path));

		Directory.CreateDirectory(Path.Combine(temp.Path, "Resources", "Icons", "hicolor", "48x48", "apps"));
		Assert.Null(GtkApplicationIdentity.FindIconName(temp.Path));
	}

	[Fact]
	public void InstallingWithoutIconsIsANoOp()
	{
		using var temp = new Microsoft.Maui.Gtk.UnitTests.Essentials.TempDir();

		GtkApplicationIdentity.InstallIcon(temp.Path);
	}
}
