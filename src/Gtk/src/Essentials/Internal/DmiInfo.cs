using System;
using System.IO;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>Hardware identity from <c>/sys/devices/virtual/dmi/id</c>.</summary>
sealed class DmiInfo
{
	const string DefaultRoot = "/sys/devices/virtual/dmi/id";

	static readonly string[] VirtualMarkers =
	{
		"qemu", "kvm", "vmware", "virtualbox", "innotek", "xen", "parallels", "bochs", "bhyve", "hyper-v", "virtual machine", "amazon ec2", "google compute",
	};

	internal DmiInfo(string? productName, string? vendor)
	{
		ProductName = productName;
		Vendor = vendor;
	}

	/// <summary><c>product_name</c> (the device model).</summary>
	internal string? ProductName { get; }

	/// <summary><c>sys_vendor</c> (the manufacturer).</summary>
	internal string? Vendor { get; }

	/// <summary>Heuristic: the vendor or product names a hypervisor.</summary>
	internal bool IsVirtual
	{
		get
		{
			foreach (var marker in VirtualMarkers)
			{
				if (Contains(Vendor, marker) || Contains(ProductName, marker))
					return true;
			}

			return false;
		}
	}

	internal static DmiInfo Read(string? root = null)
	{
		root ??= DefaultRoot;
		return new DmiInfo(ReadValue(Path.Combine(root, "product_name")), ReadValue(Path.Combine(root, "sys_vendor")));
	}

	static bool Contains(string? text, string marker) =>
		text is not null && text.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0;

	static string? ReadValue(string path)
	{
		try
		{
			if (!File.Exists(path))
				return null;

			var text = File.ReadAllText(path).Trim();
			return text.Length == 0 ? null : text;
		}
		catch (IOException)
		{
			return null;
		}
		catch (UnauthorizedAccessException)
		{
			return null;
		}
	}
}
