using System.Security.Cryptography;

namespace KupidonServer.Services;

// random preko RNGCryptoServiceProvider (zadatak tako trazi).
// klasa je deprecated od .net 6 (SYSLIB0023) pa pragma; zvanicna zamena bi bio RandomNumberGenerator
public sealed class SecureRng : IDisposable
{
#pragma warning disable SYSLIB0023
    private readonly RNGCryptoServiceProvider _rng = new();
#pragma warning restore SYSLIB0023
    private readonly object _lock = new();

    // random u [min, max)
    public int Next(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive)
            throw new ArgumentOutOfRangeException(nameof(maxExclusive));

        uint range = (uint)((long)maxExclusive - minInclusive);

        // rejection sampling da nema modulo bias-a (ako range deli 2^32, reject = 0)
        uint reject = (uint)(0x1_0000_0000UL % range);

        var bytes = new byte[4];
        uint value;
        do
        {
            lock (_lock) { _rng.GetBytes(bytes); }
            value = BitConverter.ToUInt32(bytes, 0);
        } while (value < reject);

        return (int)(minInclusive + (value % range));
    }

    public void Dispose() => _rng.Dispose();
}
