namespace CustomPolicyApi.ApiService.Models;

public class OAuthOptions
{
    public ProviderOptions GitHub { get; set; } = new();
    public ProviderOptions Google { get; set; } = new();
    public ProviderOptions LinkedIn { get; set; } = new();
    public MicrosoftGraphOptions MicrosoftGraph { get; set; } = new();
    public Auth0Options Auth0 { get; set; } = new();
}

public class ProviderOptions
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
}

public class MicrosoftGraphOptions : ProviderOptions
{
    public string TenantId { get; set; } = "";
}

public class Auth0Options : ProviderOptions
{
    public string Domain { get; set; } = "";
}