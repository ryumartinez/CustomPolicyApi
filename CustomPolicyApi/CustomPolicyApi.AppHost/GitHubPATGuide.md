
# 📘 Guide: Creating and Using a GitHub Personal Access Token (PAT)

This guide explains how to create a long-lived GitHub Personal Access Token (PAT) for testing and use it in a .NET controller to access GitHub APIs or simulate OAuth-protected requests.

---

## ✅ 1. Create a GitHub Personal Access Token (PAT)

### Steps:

1. Go to [https://github.com/settings/tokens](https://github.com/settings/tokens)
2. Choose **"Generate new token (classic)"**
3. Fill in:
   - **Note**: e.g., `PAT for test automation`
   - **Expiration**: `No expiration`
   - **Scopes**: select at minimum:
     - `read:user`
     - `user:email`
4. Click **Generate token**
5. Copy and **securely store** the token (you won't see it again).

---

## ✅ 2. Store the PAT in Configuration

### `appsettings.Development.json`:

```json
"OAuth": {
  "GitHub": {
    "PatToken": "ghp_XXXXXXXXXXXXXXXXXXXXXXXXXXXX"
  }
}
```

---

## ✅ 3. Create a .NET Controller to Use the PAT

```csharp
public class GitHubOptions
{
    public string PatToken { get; set; } = "";
}

[ApiController]
[Route("api/github-token")]
public class GitHubTokenController : ControllerBase
{
    private readonly GitHubOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public GitHubTokenController(IOptions<GitHubOptions> options, IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserProfile()
    {
        if (string.IsNullOrWhiteSpace(_options.PatToken))
            return BadRequest("GitHub PAT token is not configured.");

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("token", _options.PatToken);

        var response = await client.GetAsync("https://api.github.com/user");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return Ok(JsonDocument.Parse(json).RootElement);
    }
}
```

---

## ✅ 4. Register GitHubOptions in Program.cs

```csharp
builder.Services.Configure<GitHubOptions>(builder.Configuration.GetSection("OAuth:GitHub"));
builder.Services.AddHttpClient();
```

---

## ✅ 5. Test It

Run your app and call:

```
GET http://localhost:<port>/api/github-token
```

Expected response:

```json
{
  "login": "your-github-username",
  "email": "your-email@example.com",
  ...
}
```

---

## 🔐 Notes

- This method is ideal for **automated testing**, **CI pipelines**, or local dev tools.
- Always **store tokens securely** and avoid committing them to source control.
- PATs work like OAuth access tokens and bypass interactive login.

---

## ✅ Summary

| Step | What You Did |
|------|---------------|
| 1    | Created a PAT with read scopes |
| 2    | Stored it in config |
| 3    | Built a .NET controller to use it |
| 4    | Called GitHub API using the token |

You're ready to use GitHub authentication in automated .NET tests!
