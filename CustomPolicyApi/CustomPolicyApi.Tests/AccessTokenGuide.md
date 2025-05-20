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

## ✅ Step 1: Create OAuth Credentials in Google Cloud Console

1. Go to: [https://console.cloud.google.com/](https://console.cloud.google.com/)
2. Create or select an existing project.
3. Enable required APIs:
   - Go to **API Library** → Enable:
      - `People API` or `Google+ API`
4. Create OAuth credentials:
   - Go to **Credentials** tab
   - Click **“+ CREATE CREDENTIALS” → OAuth client ID**
   - Choose:
      - **Application Type**: Web application
      - **Name**: e.g., Test Client
   - Under **Authorized redirect URIs**, add:
     ```
     https://developers.google.com/oauthplayground
     ```
   - Click **Create**
5. Copy the **Client ID** and **Client Secret**

---

## 🧪 Step 2: Use OAuth 2.0 Playground to Get the Token

1. Go to: [OAuth 2.0 Playground](https://developers.google.com/oauthplayground)
2. Click the ⚙️ icon and:
   - ✅ Enable “Use your own OAuth credentials”
   - Paste your **Client ID** and **Client Secret**
3. Under scopes, enter:
   ```
   https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile
   ```
4. Click **Authorize APIs**
5. Sign in with the test Google account (must be listed in the consent screen)
6. Click **Exchange authorization code for tokens**
7. Copy the **Access Token**

---

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