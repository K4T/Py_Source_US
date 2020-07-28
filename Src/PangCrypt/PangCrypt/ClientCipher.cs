using System;

namespace PangCrypt
{
    /// <summary>
    ///     Cipher used for packets sent by the client. PangYa clients would
    ///     encrypt outgoing packets using this cipher and PangYa servers would
    ///     decrypt incoming packets using this cipher.
    /// </summary>
    public static class ClientCipher
    {
        /// <summary>
        ///     Decrypts data from client-side packets (sent from clients to servers.)
        /// </summary>
        /// <param name="source">The encrypted packet data.</param>
        /// <param name="key">Key to decrypt with.</param>
        /// <returns>The decrypted packet data.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if the key is invalid or the packet data is too short.
        /// </exception>
        public static byte[] Decrypt(byte[] source, byte key)
        {
            if (key >= 0x10) throw new ArgumentOutOfRangeException(nameof(key), $"Key too large ({key} >= 0x10)");

            if (source.Length < 5)
                throw new ArgumentOutOfRangeException(nameof(source), $"Packet too small ({source.Length} < 5)");

            var buffer = (byte[]) source.Clone();

            buffer[4] = CryptoOracle.CryptTable2[(key << 8) + source[0]];

            for (var i = 8; i < buffer.Length; i++) buffer[i] ^= buffer[i - 4];

            var output = new byte[buffer.Length - 5];

            Array.Copy(buffer, 5, output, 0, buffer.Length - 5);

            return output;
        }

        /// <summary>
        ///     Encrypts data for client-side packets (sent from clients to servers.)
        /// </summary>
        /// <param name="source">The decrypted packet data.</param>
        /// <param name="key">Key to encrypt with.</param>
        /// <param name="salt">Random salt value to encrypt with.</param>
        /// <returns>The encrypted packet data.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if an invalid key is specified.
        /// </exception>
        public static byte[] Encrypt(byte[] source, byte key, byte salt)
        {
            if (key >= 0x10) throw new ArgumentOutOfRangeException(nameof(key), $"Key too large ({key} >= 0x10)");

            var oracleIndex = (key << 8) + salt;
            var buffer = new byte[source.Length + 5];
            var pLen = buffer.Length - 4;

            buffer[0] = salt;
            buffer[1] = (byte) ((pLen >> 0) & 0xFF);
            buffer[2] = (byte) ((pLen >> 8) & 0xFF);
            buffer[4] = CryptoOracle.CryptTable2[oracleIndex];

            Array.Copy(source, 0, buffer, 5, source.Length);

            for (var i = buffer.Length - 1; i >= 8; i--) buffer[i] ^= buffer[i - 4];

            buffer[4] ^= CryptoOracle.CryptTable1[oracleIndex];
            return buffer;
        }
    }
}