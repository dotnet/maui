using System.Net;
using System.Net.Sockets;

namespace Bielik.Core;

public sealed record LocalEndpoint
{
    private LocalEndpoint(Uri address) => Address = address;

    public Uri Address { get; }

    public static LocalEndpoint Parse(string? value, bool allowInsecureHttp = true)
    {
        if (!Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var address) ||
            (address.Scheme != Uri.UriSchemeHttp && address.Scheme != Uri.UriSchemeHttps) ||
            address.UserInfo.Length != 0 ||
            address.Query.Length != 0 ||
            address.Fragment.Length != 0 ||
            address.AbsolutePath != "/")
        {
            throw new FormatException("Podaj adres serwera, np. http://127.0.0.1:11434, bez ścieżki, hasła ani parametrów.");
        }

        if (address.Scheme == Uri.UriSchemeHttp && !allowInsecureHttp)
        {
            throw new FormatException("Ta wersja wymaga HTTPS. HTTP jest dostępny tylko w lokalnym buildzie Debug.");
        }

        var host = address.Host.Trim('[', ']');
        if (!host.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
            (!IPAddress.TryParse(host, out var ip) || !IsLocalAddress(ip)))
        {
            throw new FormatException("Tylko lokalnie: użyj localhost lub prywatnego adresu IP. Serwery internetowe są wyłączone.");
        }

        return new LocalEndpoint(address);
    }

    private static bool IsLocalAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (IPAddress.IsLoopback(address))
        {
            return true;
        }

        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            return bytes[0] == 10 ||
                (bytes[0] == 172 && bytes[1] is >= 16 and <= 31) ||
                (bytes[0] == 192 && bytes[1] == 168);
        }

        return address.AddressFamily == AddressFamily.InterNetworkV6 && (bytes[0] & 0xfe) == 0xfc;
    }
}
