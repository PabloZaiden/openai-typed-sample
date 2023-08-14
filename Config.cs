namespace OpenAITypedSample;

public static class Config
{
    static Config()
    {
        LoadDotEnv();
    }

    private static void LoadDotEnv()
    {
        var envFile = Path.Join(Directory.GetCurrentDirectory(), ".env");
        if (!File.Exists(envFile))
        {
            return;
        }

        var lines = File.ReadAllLines(envFile);
        foreach (var line in lines)
        {
            if (line.StartsWith("#"))
            {
                continue;
            }
            var parts = line.Split('=');
            if (parts.Length < 2)
            {
                continue;
            }

            var key = parts[0];
            var val = String.Join("", parts.Skip(1));
            if (val.StartsWith("") && val.EndsWith(""))
            {
                val = val[1..^1];
            }

            Environment.SetEnvironmentVariable(key, val);
        }
    }

    public static string Get(string key)
    {
        return GetEnv(key);
    }

    public static bool GetFlag(string key)
    {
        if (!Has(key))
        {
            return false;
        }

        var val = GetEnv(key).ToLowerInvariant();
        return val == "1" || val == "true";
    }

    public static bool Has(string key)
    {
        return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key));
    }

    private static string GetEnv(string key)
    {
        var val = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrEmpty(val))
        {
            throw new Exception($"Environment variable {key} is not set");
        }

        return val;
    }
}