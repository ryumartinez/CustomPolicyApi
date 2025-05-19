# 🔐 How to Obtain Access Tokens for Testing

This guide explains how to manually obtain OAuth access tokens for **GitHub**, **Google**, and **LinkedIn**, which are required to run the external identity provider tests.

---

## 📋 Table of Contents

- [GitHub Access Token](#github-access-token)
- [Google Access Token](#google-access-token)
- [LinkedIn Access Token](#linkedin-access-token)
- [How to Use Tokens in Tests](#how-to-use-tokens-in-tests)

---

## 🐙 GitHub Access Token

1. Go to: [https://github.com/settings/tokens](https://github.com/settings/tokens)
2. Click **"Generate new token (classic)"**
3. Set an expiration date (e.g., 7 days)
4. Select **scopes**:
   - `read:user`
   - `user:email`
5. Click **"Generate token"**
6. Copy the token and store it in your environment:

   ```bash
   # PowerShell
   $env:GITHUB_TEST_TOKEN = "ghp_..."

   # Linux/macOS
   export GITHUB_TEST_TOKEN="ghp_..."
   ```

---

## 🔍 Google Access Token

### ⚠️ Google OAuth does **not support** username/password authentication. You must use OAuth tools.

1. Go to: [OAuth 2.0 Playground](https://developers.google.com/oauthplayground/)
2. Click the gear icon ⚙️ and enable:
   - ✅ Use your own OAuth credentials
3. Go to [Google Cloud Console](https://console.cloud.google.com/)
   - Create a project and OAuth 2.0 client
   - Copy your **Client ID** and **Client Secret**
4. Back in the Playground:
   - Enter your credentials
   - Select scope: `https://www.googleapis.com/auth/userinfo.email`
5. Authorize and click **"Exchange authorization code for tokens"**
6. Copy the access token and store it:

   ```bash
   $env:GOOGLE_TEST_TOKEN = "ya29..."
   ```

---

## 💼 LinkedIn Access Token

1. Go to: [LinkedIn Developer Portal](https://www.linkedin.com/developers/)
2. Create an app and note the **Client ID** and **Client Secret**
3. Generate an access token using the **OAuth 2.0 Authorization Code Flow**:
   ```
   https://www.linkedin.com/oauth/v2/authorization?response_type=code
     &client_id=YOUR_CLIENT_ID
     &redirect_uri=http://localhost:3000
     &scope=r_emailaddress+r_liteprofile
   ```

4. After user consent, you'll get a `code` in the redirect URL.
5. Exchange it for a token:
   ```bash
   curl -X POST https://www.linkedin.com/oauth/v2/accessToken \
     -d grant_type=authorization_code \
     -d code=AUTHORIZATION_CODE \
     -d redirect_uri=http://localhost:3000 \
     -d client_id=YOUR_CLIENT_ID \
     -d client_secret=YOUR_CLIENT_SECRET
   ```

6. Save the resulting token:

   ```bash
   $env:LINKEDIN_TEST_TOKEN = "AQX..."
   ```

---

## 🧪 How to Use Tokens in Tests

Your test method looks like this:

```csharp
[TestCase("github", "GITHUB_TEST_TOKEN")]
[TestCase("google", "GOOGLE_TEST_TOKEN")]
[TestCase("linkedin", "LINKEDIN_TEST_TOKEN")]
public async Task GetExternalUserDataWithTokenReturnsOkStatusCode(string provider, string envVar)
{
    var token = Environment.GetEnvironmentVariable(envVar);
    Assume.That(!string.IsNullOrWhiteSpace(token), $"Missing environment variable: {envVar}");
    ...
}
```

✅ Set environment variables before running tests:

```bash
# PowerShell
$env:GITHUB_TEST_TOKEN = "ghp_..."
$env:GOOGLE_TEST_TOKEN = "ya29..."
$env:LINKEDIN_TEST_TOKEN = "AQX..."

dotnet test
```

---

## 📎 Notes

- 🔐 Keep tokens private — **do not commit them to source control**
- ♻️ Tokens expire — regenerate them periodically
- 🧑‍🔬 You can replace this system with a test secrets manager in CI later