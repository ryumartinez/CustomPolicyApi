
# 📘 Guide: Using LinkedIn OAuth 2.0 Access Tokens for Automated Testing

LinkedIn does **not** support personal access tokens (PATs) like GitHub. However, you can use a **refresh token** if you're an approved partner or use a **manual authorization flow** to retrieve an access token for testing.

This guide walks you through how to get an access token and implement a .NET controller to automate the process for testing purposes.

---

## ✅ 1. Register a LinkedIn App

1. Visit [LinkedIn Developer Portal](https://www.linkedin.com/developers/)
2. Create a new app
3. Set:
   - OAuth 2.0 Redirect URL: e.g., `http://localhost:8080`
   - Required permissions: `r_liteprofile`, `r_emailaddress`
4. Save the **Client ID** and **Client Secret**

---

## ✅ 2. Get Authorization Code

Paste this in your browser:

```
https://www.linkedin.com/oauth/v2/authorization?
  response_type=code&
  client_id=YOUR_CLIENT_ID&
  redirect_uri=http://localhost:8080&
  scope=r_liteprofile%20r_emailaddress
```

- Approve the app
- You'll be redirected to:  
  `http://localhost:8080/?code=AUTH_CODE`
- Copy the code

---

## ✅ 3. Exchange Code for Access Token

Use `curl`:

```bash
curl -X POST https://www.linkedin.com/oauth/v2/accessToken \
  -d grant_type=authorization_code \
  -d code=AUTH_CODE \
  -d redirect_uri=http://localhost:8080 \
  -d client_id=YOUR_CLIENT_ID \
  -d client_secret=YOUR_CLIENT_SECRET
```

Response:

```json
{
  "access_token": "AQX...",
  "expires_in": 5184000
}
```

✅ Save the `access_token` — it lasts for 60 days.

---

## ✅ 4. Store the Access Token in Configuration

```json
"OAuth": {
  "LinkedIn": {
    "AccessToken": "AQX_LONG_TOKEN"
  }
}
```

---

## ✅ 5. Create a .NET Controller

```csharp
public class LinkedInOptions
{
    public string AccessToken { get; set; } = "";
}

[ApiController]
[Route("api/linkedin-token")]
public class LinkedInTokenController : ControllerBase
{
    private readonly LinkedInOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public LinkedInTokenController(IOptions<LinkedInOptions> options, IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserProfile()
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.AccessToken);
        client.DefaultRequestHeaders.Add("X-Restli-Protocol-Version", "2.0.0");

        var response = await client.GetAsync("https://api.linkedin.com/v2/me");
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return BadRequest(new { error = "LinkedIn API error", details = content });
        }

        return Ok(JsonDocument.Parse(content).RootElement);
    }
}
```

---

## ✅ 6. Register LinkedIn Options

```csharp
builder.Services.Configure<LinkedInOptions>(builder.Configuration.GetSection("OAuth:LinkedIn"));
builder.Services.AddHttpClient();
```

---

## ✅ Summary

| Step | What You Did |
|------|---------------|
| 1    | Created LinkedIn App |
| 2    | Got an auth code manually |
| 3    | Exchanged code for access token |
| 4    | Stored token in config |
| 5    | Built a controller to test login |

---

**Note**: LinkedIn does not support refresh tokens unless you're an approved partner. You'll need to reauthorize manually after 60 days.
