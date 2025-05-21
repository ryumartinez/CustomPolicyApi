
# 📘 Guide: Obtaining a Long-Lived Google Refresh Token

## 1. Set Up OAuth 2.0 Credentials

- Go to the [Google Cloud Console](https://console.cloud.google.com/)
- Navigate to: **APIs & Services > Credentials**
- Click **Create Credentials > OAuth client ID**
- Choose **Web application** as the application type
- Add a redirect URI like `http://localhost:8080`
- Save the **Client ID** and **Client Secret**

## 2. Configure OAuth Consent Screen

- Set up the consent screen with necessary scopes: `openid`, `email`, `profile`
- Set publishing status to **In Production** to prevent token expiration after 7 days

## 3. Authorize Using access_type=offline

Open this URL in a browser (replace values):

```
https://accounts.google.com/o/oauth2/v2/auth?
  client_id=YOUR_CLIENT_ID&
  redirect_uri=http://localhost:8080&
  response_type=code&
  scope=openid%20email%20profile&
  access_type=offline&
  prompt=consent
```

After login, copy the `code` from the redirected URL.

## 4. Exchange Code for Tokens

Use `curl` or Postman:

```bash
curl -X POST https://oauth2.googleapis.com/token \
  -d client_id=YOUR_CLIENT_ID \
  -d client_secret=YOUR_CLIENT_SECRET \
  -d code=AUTH_CODE \
  -d grant_type=authorization_code \
  -d redirect_uri=http://localhost:8080
```

The response will include a `refresh_token`. Save it securely.

## 5. Use the Refresh Token to Get Access Tokens

```http
POST https://oauth2.googleapis.com/token

grant_type=refresh_token
client_id=YOUR_CLIENT_ID
client_secret=YOUR_CLIENT_SECRET
refresh_token=YOUR_REFRESH_TOKEN
```

## 6. .NET Controller Example

```csharp
[ApiController]
[Route("api/google-token")]
public class GoogleTokenController : ControllerBase
{
    private readonly GoogleOAuthOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public GoogleTokenController(IOptions<GoogleOAuthOptions> options, IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetAccessToken()
    {
        var parameters = new Dictionary<string, string>
        {
            { "client_id", _options.ClientId },
            { "client_secret", _options.ClientSecret },
            { "refresh_token", _options.RefreshToken },
            { "grant_type", "refresh_token" }
        };

        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(parameters));
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return BadRequest(new { error = "Failed to retrieve access token", details = content });
        }

        var json = JsonDocument.Parse(content);
        var accessToken = json.RootElement.GetProperty("access_token").GetString();
        var expiresIn = json.RootElement.GetProperty("expires_in").GetInt32();

        return Ok(new { access_token = accessToken, expires_in = expiresIn });
    }
}
```

## 7. Configuration in appsettings.Development.json

```json
"OAuth": {
  "Google": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "RefreshToken": "your-long-lived-refresh-token"
  }
}
```

## ✅ Done!

Use `GET /api/google-token` to fetch a fresh access token for your tests.
