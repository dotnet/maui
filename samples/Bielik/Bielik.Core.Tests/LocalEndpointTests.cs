using Bielik.Core;
using Xunit;

namespace Bielik.Core.Tests;

public class LocalEndpointTests
{
    [Fact]
    public void ReleasePolicyRejectsCleartextEvenOnLoopback()
    {
        Assert.Throws<FormatException>(() => LocalEndpoint.Parse(ModelInfo.DefaultEndpoint, allowInsecureHttp: false));
        Assert.NotNull(LocalEndpoint.Parse("https://127.0.0.1:11434", allowInsecureHttp: false));
    }

    [Theory]
    [InlineData("http://127.0.0.1:11434")]
    [InlineData("http://localhost:11434")]
    [InlineData("https://192.168.1.20:11434")]
    [InlineData("http://10.0.0.4:11434/")]
    [InlineData("https://172.16.0.1:443")]
    [InlineData("https://172.31.255.254")]
    [InlineData("http://[::1]:11434")]
    [InlineData("https://[fd12::4]:11434")]
    [InlineData("http://[::ffff:127.0.0.1]:11434")]
    public void AcceptsOnlyLoopbackAndPrivateAddresses(string value)
    {
        Assert.NotNull(LocalEndpoint.Parse(value));
    }

    [Theory]
    [InlineData("https://chat.bielik.ai")]
    [InlineData("https://ollama.com")]
    [InlineData("https://8.8.8.8")]
    [InlineData("http://172.15.0.1")]
    [InlineData("http://172.32.0.1")]
    [InlineData("http://169.254.169.254")]
    [InlineData("http://0.0.0.0")]
    [InlineData("http://[2001:4860:4860::8888]")]
    [InlineData("http://[::ffff:8.8.8.8]")]
    [InlineData("http://localhost.example.com")]
    [InlineData("http://localhost@evil.example")]
    [InlineData("http://user:password@localhost:11434")]
    [InlineData("http://localhost:11434/api/chat")]
    [InlineData("http://localhost:11434/?host=example.com")]
    [InlineData("http://localhost:11434/#fragment")]
    [InlineData("ftp://localhost")]
    [InlineData("not an address")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void RejectsInternetEndpointsAndAmbiguousUrls(string? value)
    {
        Assert.Throws<FormatException>(() => LocalEndpoint.Parse(value));
    }
}
