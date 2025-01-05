public interface ISecretProvider
{
    Task<string> GetConnectionString(string key);
    Task<T> GetSecret<T>(string path) where T : class;
}

public class JsonSecretProvider : ISecretProvider
{
    private readonly IConfiguration _configuration;

    public JsonSecretProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<string> GetConnectionString(string key)
    {
        return Task.FromResult(_configuration.GetConnectionString(key));
    }

    public Task<T> GetSecret<T>(string path) where T : class
    {
        var section = _configuration.GetSection(path);
        return Task.FromResult(section.Get<T>());
    }
}

public class VaultSecretProvider : ISecretProvider
{
    private readonly VaultService _vaultService;

    public VaultSecretProvider(VaultService vaultService)
    {
        _vaultService = vaultService;
    }

    public async Task<string> GetConnectionString(string key)
    {
        var secret = await _vaultService.GetConnectionString(key);
        return secret;
    }

    public async Task<T> GetSecret<T>(string path) where T : class
    {
        var secret = await _vaultService.GetSecret(path);
        return JsonSerializer.Deserialize<T>(secret);
    }
}

public static class SecretProviderFactory
{
    public static ISecretProvider Create(IServiceProvider services, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            return new JsonSecretProvider(
                services.GetRequiredService<IConfiguration>());
        }
        
        return new VaultSecretProvider(
            services.GetRequiredService<VaultService>());
    }
}

public class SecretsDemoService
{
    private readonly ISecretProvider _secretProvider;

    public SecretsDemoService(ISecretProvider secretProvider)
    {
        _secretProvider = secretProvider;
    }

    public async Task<ApiSettings> GetApiSettings()
    {
        return await _secretProvider.GetSecret<ApiSettings>("MySecrets");
    }

    public async Task<string> GetDbConnection()
    {
        return await _secretProvider.GetConnectionString("DefaultConnection");
    }
}