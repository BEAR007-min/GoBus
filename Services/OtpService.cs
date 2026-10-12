using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;

namespace GoBus.Services;

public class OtpService
{
    private readonly IMemoryCache _cache;

    private const int ExpirationMinutes = 5;
    private const int MaxAttempts = 5;

    public OtpService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public string GenerateOtp(int userId)
    {
        string otp = RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();

        string otpHash = Convert.ToHexString(
            SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(otp)));

        var data = new OtpData
        {
            OtpHash = otpHash,
            Attempts = 0
        };

        string cacheKey = GetCacheKey(userId);

        _cache.Set(
            cacheKey,
            data,
            TimeSpan.FromMinutes(ExpirationMinutes));

        return otp;
    }

    public bool VerifyOtp(int userId, string inputOtp)
    {
        string cacheKey = GetCacheKey(userId);

        if (!_cache.TryGetValue(cacheKey, out OtpData? data))
        {
            return false;
        }

        if (data == null)
        {
            return false;
        }

        if (data.Attempts >= MaxAttempts)
        {
            _cache.Remove(cacheKey);
            return false;
        }

        data.Attempts++;

        string inputHash = Convert.ToHexString(
            SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(inputOtp)));

        if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(data.OtpHash),
                Convert.FromHexString(inputHash)))
        {
            return false;
        }

        // OTP đúng → chỉ được dùng một lần.
        _cache.Remove(cacheKey);

        return true;
    }

    public void RemoveOtp(int userId)
    {
        _cache.Remove(GetCacheKey(userId));
    }

    private static string GetCacheKey(int userId)
    {
        return $"ForgotPasswordOtp:{userId}";
    }

    private class OtpData
    {
        public string OtpHash { get; set; } = string.Empty;
        public int Attempts { get; set; }
    }
}