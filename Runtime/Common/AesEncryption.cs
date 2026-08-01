using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace CLabs.Utility {
    /// <summary>
    /// Symmetric encrypt/decrypt of a UTF-8 payload under a caller-supplied passphrase. Synchronous by
    /// design so it stays engine- and async-runtime-agnostic; the work is CPU-bound over in-memory buffers.
    /// </summary>
    public interface IEncryptionAlgorithm {
        byte[] Encrypt(string data, string key);
        string Decrypt(byte[] data, string key);
    }

    /// <summary>
    /// AES-256 in CBC with a per-call random salt + IV, a PBKDF2-derived key (100k iterations), and an
    /// appended HMAC-SHA256 for tamper detection. Layout: [salt(16)][iv(16)][ciphertext][hmac(32)].
    /// </summary>
    public sealed class AesEncryption : IEncryptionAlgorithm {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int IvSize = 16;
        private const int HmacSize = 32;
        private const int Iterations = 100000;

        public byte[] Encrypt(string data, string key) {
            var salt = new byte[SaltSize];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            var keyBytes = new Rfc2898DeriveBytes(key, salt, Iterations).GetBytes(KeySize);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = keyBytes;
            aes.GenerateIV();

            using var ms = new MemoryStream();
            ms.Write(salt, 0, salt.Length);
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var encryptor = aes.CreateEncryptor())
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs)) {
                sw.Write(data);
            }

            var encryptedData = ms.ToArray();

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(encryptedData);

            return encryptedData.Concat(hash).ToArray();
        }

        public string Decrypt(byte[] cipherText, string key) {
            if (cipherText.Length < SaltSize + IvSize + HmacSize) {
                throw new CryptographicException("Invalid data structure.");
            }

            var encryptedData = cipherText.Take(cipherText.Length - HmacSize).ToArray();
            var storedHmac = cipherText.Skip(cipherText.Length - HmacSize).ToArray();

            using var ms = new MemoryStream(encryptedData);
            var salt = new byte[SaltSize];
            var iv = new byte[IvSize];
            ms.Read(salt, 0, salt.Length);
            ms.Read(iv, 0, iv.Length);

            var keyBytes = new Rfc2898DeriveBytes(key, salt, Iterations).GetBytes(KeySize);

            using var hmac = new HMACSHA256(keyBytes);
            var calculatedHmac = hmac.ComputeHash(encryptedData);

            if (false == storedHmac.SequenceEqual(calculatedHmac)) {
                throw new CryptographicException("Data integrity check failed — data may be tampered with.");
            }

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = keyBytes;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }
}
