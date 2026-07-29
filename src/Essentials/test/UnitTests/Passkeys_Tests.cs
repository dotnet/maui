using System;
using System.Text.Json;
using Microsoft.Maui.Authentication;
using Xunit;

namespace Tests;

public class Passkeys_Tests
{
	[Fact]
	public void CreationResponse_Exposes_Id()
	{
		const string json = "{\"id\":\"cred-1\",\"rawId\":\"cred-1\",\"type\":\"public-key\"}";
		var response = new PasskeyCreationResponse(json);
		Assert.Equal("cred-1", response.Id);
		Assert.Equal(json, response.ToString());
	}

	[Fact]
	public void CreationResponse_Throws_When_Id_Missing()
	{
		const string json = "{\"rawId\":\"cred-1\",\"type\":\"public-key\"}";
		Assert.Throws<InvalidOperationException>(() => new PasskeyCreationResponse(json));
	}

	[Fact]
	public void AssertionResponse_Exposes_Id_And_UserHandle()
	{
		const string json = "{\"id\":\"cred-1\",\"response\":{\"userHandle\":\"dXNlcg\"}}";
		var response = new PasskeyAssertionResponse(json);
		Assert.Equal("cred-1", response.Id);
		Assert.Equal("dXNlcg", response.UserHandle);
	}

	[Fact]
	public void AssertionResponse_UserHandle_Null_When_Absent()
	{
		const string json = "{\"id\":\"cred-1\",\"response\":{\"signature\":\"MEU\"}}";
		var response = new PasskeyAssertionResponse(json);
		Assert.Null(response.UserHandle);
	}

	[Fact]
	public void AssertionResponse_Throws_When_Id_Missing()
	{
		const string json = "{\"response\":{\"signature\":\"MEU\"}}";
		Assert.Throws<InvalidOperationException>(() => new PasskeyAssertionResponse(json));
	}

	[Fact]
	public void Options_ToString_Returns_Raw_Json()
	{
		const string json = "{\"challenge\":\"abc\"}";
		Assert.Equal(json, new PasskeyCreationOptions(json).ToString());
		Assert.Equal(json, new PasskeyRequestOptions(json).ToString());
	}

	[Theory]
	[InlineData("required", null, true)]
	[InlineData("", true, true)]
	[InlineData("preferred", true, false)]
	[InlineData("preferred", false, false)]
	[InlineData("discouraged", true, false)]
	[InlineData("required", false, true)]
	public void RequiresResidentKey_Uses_Modern_And_Legacy_Options(string residentKey, bool? requireResidentKey, bool expected)
	{
		var selection = new WebAuthn.AuthenticatorSelection
		{
			ResidentKey = string.IsNullOrEmpty(residentKey) ? null : residentKey,
			RequireResidentKey = requireResidentKey,
		};

		Assert.Equal(expected, WebAuthn.RequiresResidentKey(selection));
	}

	[Fact]
	public void GetTimeout_Rejects_Negative_Values()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => WebAuthn.GetTimeout(-1));
		Assert.Equal(60000u, WebAuthn.GetTimeout(null));
	}

	[Fact]
	public void DecodeRequired_Rejects_Invalid_Base64Url_As_ArgumentException()
	{
		var exception = Assert.Throws<ArgumentException>(() => WebAuthn.DecodeRequired("***", "challenge"));
		Assert.IsType<FormatException>(exception.InnerException);
	}

	[Fact]
	public void Extensions_RoundTrip_As_Json()
	{
		using var extensions = JsonDocument.Parse("""{"credProps":true}""");
		using var empty = JsonDocument.Parse("{}");

		Assert.Equal("""{"credProps":true}""", System.Text.Encoding.UTF8.GetString(WebAuthn.GetExtensionsJson(extensions.RootElement)));
		Assert.Empty(WebAuthn.GetExtensionsJson(empty.RootElement));
		Assert.Empty(WebAuthn.GetExtensionsJson(null));

		var outputs = WebAuthn.ReadExtensionOutputs("""{"credProps":{"rk":true}}"""u8.ToArray());
		Assert.True(outputs.Values!["credProps"].GetProperty("rk").GetBoolean());
	}
}
