using System;
using Microsoft.Maui.Authentication;
using Xunit;

namespace Tests
{
	public class Passkeys_Tests
	{
		[Theory]
		[InlineData(new byte[0], "")]
		[InlineData(new byte[] { 0x00 }, "AA")]
		[InlineData(new byte[] { 0x00, 0x01 }, "AAE")]
		[InlineData(new byte[] { 0x00, 0x01, 0x02 }, "AAEC")]
		public void Base64Url_Encode_Produces_Unpadded_UrlSafe(byte[] input, string expected) =>
			Assert.Equal(expected, Base64Url.Encode(input));

		[Fact]
		public void Base64Url_Uses_UrlSafe_Alphabet()
		{
			// 0xFB 0xFF -> standard base64 "+/8=", url-safe "-_8"
			var encoded = Base64Url.Encode(new byte[] { 0xFB, 0xFF });
			Assert.Equal("-_8", encoded);
			Assert.DoesNotContain('+', encoded);
			Assert.DoesNotContain('/', encoded);
			Assert.DoesNotContain('=', encoded);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		[InlineData(16)]
		[InlineData(32)]
		[InlineData(64)]
		[InlineData(65)]
		public void Base64Url_Roundtrips(int length)
		{
			var bytes = new byte[length];
			for (var i = 0; i < length; i++)
				bytes[i] = (byte)(i * 7 + 3);

			var roundtripped = Base64Url.Decode(Base64Url.Encode(bytes));
			Assert.Equal(bytes, roundtripped);
		}

		[Fact]
		public void Parser_Extracts_TopLevel_String()
		{
			const string json = "{\"id\":\"abc-_DEF\",\"rawId\":\"abc-_DEF\",\"type\":\"public-key\"}";
			Assert.Equal("abc-_DEF", PasskeyResponseParser.GetString(json, "id"));
		}

		[Fact]
		public void Parser_Extracts_Nested_String()
		{
			const string json = "{\"id\":\"X\",\"response\":{\"userHandle\":\"dXNlcg\",\"signature\":\"MEU\"}}";
			Assert.Equal("dXNlcg", PasskeyResponseParser.GetString(json, "userHandle"));
		}

		[Fact]
		public void Parser_Returns_Null_When_Missing()
		{
			const string json = "{\"id\":\"X\",\"response\":{\"signature\":\"MEU\"}}";
			Assert.Null(PasskeyResponseParser.GetString(json, "userHandle"));
		}

		[Fact]
		public void Parser_Returns_Null_For_Json_Null_Value()
		{
			const string json = "{\"id\":\"X\",\"response\":{\"userHandle\":null}}";
			Assert.Null(PasskeyResponseParser.GetString(json, "userHandle"));
		}

		[Fact]
		public void Parser_Tolerates_Whitespace()
		{
			const string json = "{ \"id\" : \"spaced\" , \"rawId\":\"x\" }";
			Assert.Equal("spaced", PasskeyResponseParser.GetString(json, "id"));
		}

		[Fact]
		public void Parser_Does_Not_Match_Key_Substring()
		{
			const string json = "{\"notid\":\"WRONG\",\"id\":\"RIGHT\"}";
			Assert.Equal("RIGHT", PasskeyResponseParser.GetString(json, "id"));
		}

		[Fact]
		public void Parser_Returns_Null_For_Unterminated_String()
		{
			// Missing closing quote must fail rather than returning a truncated value.
			const string json = "{\"id\":\"unterminated";
			Assert.Null(PasskeyResponseParser.GetString(json, "id"));
		}

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
			Assert.Throws<PasskeyException>(() => new PasskeyCreationResponse(json));
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
			Assert.Throws<PasskeyException>(() => new PasskeyAssertionResponse(json));
		}

		[Fact]
		public void Options_ToString_Returns_Raw_Json()
		{
			const string json = "{\"challenge\":\"abc\"}";
			Assert.Equal(json, new PasskeyCreationOptions(json).ToString());
			Assert.Equal(json, new PasskeyRequestOptions(json).ToString());
		}
	}
}
