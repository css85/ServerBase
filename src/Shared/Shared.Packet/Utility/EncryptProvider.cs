using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Shared.Utility
{
    public static class EncryptProvider
    {
        private static readonly int _keySize = 256;
        private static readonly int _BlockSize = 128;
        private static readonly int _keyLength = 32;
        private static readonly int _ivLength = 16;

        public static string CreateKey()
        {
            var createKey = StringUtility.RandomString(_keyLength);
            return createKey;
        }

        public static byte[] EncryptAes256(string encryptKey, byte[] packetStream)
        {
            if (packetStream == null || packetStream.Length <= 0)
            {
                return packetStream;
            }

            try
            {
                string key = encryptKey;
                string iv = encryptKey.Substring(0, _ivLength);

                var rijndael = Rijndael.Create();
                rijndael.KeySize = _keySize;
                rijndael.BlockSize = _BlockSize;
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;
                rijndael.Key = Encoding.UTF8.GetBytes(key);
                rijndael.IV = Encoding.UTF8.GetBytes(iv);
                var encryptor = rijndael.CreateEncryptor(rijndael.Key, rijndael.IV);
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(packetStream, 0, packetStream.Length);
                        csEncrypt.FlushFinalBlock();

                        return msEncrypt.ToArray();
                    }
                }
            }
            catch (Exception)
            {
                throw new NotImplementedException("Crypto Encrypt fail");
            }
        }

        public static byte[] DecryptAes256(string encryptKey, byte[] packetStream)
        {
            if (packetStream == null || packetStream.Length <= 0)
            {
                return packetStream;
            }

            try
            {
                string key = encryptKey;
                string iv = encryptKey.Substring(0, _ivLength);

                var rijndael = Rijndael.Create();
                rijndael.KeySize = _keySize;
                rijndael.BlockSize = _BlockSize;
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;
                rijndael.Key = Encoding.UTF8.GetBytes(key);
                rijndael.IV = Encoding.UTF8.GetBytes(iv);
                
                var decryptor = rijndael.CreateDecryptor();
                using (var msDecrypt = new MemoryStream(packetStream))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var memory = new MemoryStream())
                        {
                            var buffer = new byte[packetStream.Length];
                            var readBytes = 0;
                            while ((readBytes = csDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                memory.Write(buffer, 0, readBytes);
                            }
                            return  memory.ToArray();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new NotImplementedException("Crypto Decrypt fail");
            }
        }
    }
}