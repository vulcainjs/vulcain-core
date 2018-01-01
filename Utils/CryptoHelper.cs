using System;
using System.IO;
using System.Security.Cryptography;
using Vulcain.Core.Configuration;

namespace Vulcain.Core.Utils
{
    internal class CryptoHelper
    {
        private static int IV_LENGTH = 16;
    private IDynamicProperty<string> secretKey;

        public CryptoHelper()
        {
            this.secretKey = DynamicConfiguration.getChainedConfigurationProperty(
                Conventions.Instance.VULCAIN_SECRET_KEY, Conventions.Instance.DefaultSecretKey);
        }

        public string Encrypt(string value)
        {
            if (String.IsNullOrEmpty(value)) return null;

            using (var aes = Aes.Create())
            {
                aes.Key = System.Text.Encoding.ASCII.GetBytes(secretKey.Value);

                byte[] iv = new byte[16];
                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(iv); 
                }
                aes.IV = iv;

                using (var ms = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (var es = new StreamWriter(cryptoStream))
                        {
                            es.Write(value);
                        }

                        var cryptedText = ms.ToArray();
                        return Convert.ToBase64String(iv) + ":" + Convert.ToBase64String(cryptedText);
                    }
                }
            }
        }

        public string Decrypt(string value)
        {
            if (String.IsNullOrEmpty(value)) return null;

            var pos = value.IndexOf(':');

            byte[] key = System.Text.Encoding.ASCII.GetBytes(secretKey.Value);
            byte[] iv = Convert.FromBase64String(value.Substring(0, pos));

            using (var aes = Aes.Create())
            {
                aes.IV = iv;
                aes.Key = key;
                using (var memoryStream = new MemoryStream(Convert.FromBase64String(value.Substring(pos + 1))))
                {
                    using (var cryptoStream = new CryptoStream(memoryStream,
                               aes.CreateDecryptor(key, iv),
                               CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(cryptoStream))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
