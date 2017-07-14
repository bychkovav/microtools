using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Platform.Utils.Events.ScriptEngine;

namespace Platform.Utils.Owin.Authorization
{
    public class PasswordHasher : IAppProxyExtension
    {
        #region Fields

        public const string DefaultSalt = "ohGPRtQpvRYcKscrFntAFBXQ9xIhiLtHlyRg81Hg2sUe4IcbV74zo3dNuuCzZdtIzWiBPakP9QR6hP+OHV/DTw==";

        private readonly HashAlgorithm hashAlgorithm = new SHA512Managed();

        private readonly byte[] cryptoVector;

        private readonly byte[] cryptoKey;

        #endregion

        public PasswordHasher(string key, string vector)
        {
            this.cryptoKey = Convert.FromBase64String(key);
            this.cryptoVector = Convert.FromBase64String(vector);
        }

        public bool VerifyPasswordHash(string inputValue, string hash, string salt)
        {
            if (string.IsNullOrEmpty(inputValue))
            {
                return false;
            }
            string inputHash = this.HashPassword(inputValue.Trim(), (salt ?? string.Empty).Trim());

            return string.Compare(inputHash, hash, StringComparison.InvariantCulture) == 0;
        }

        public string HashPassword(string inputValue, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(inputValue);

            var plainTextWithSaltBytes = new byte[plainTextBytes.Length + saltBytes.Length];

            plainTextBytes.CopyTo(plainTextWithSaltBytes, 0);
            saltBytes.CopyTo(plainTextWithSaltBytes, plainTextBytes.Length);

            byte[] hashBytes = this.hashAlgorithm.ComputeHash(plainTextWithSaltBytes);

            return Convert.ToBase64String(hashBytes);
        }

        public void AddToServiceContainer(dynamic container)
        {
            container.GetHash = new Func<string, string>(x => HashPassword(x, DefaultSalt));
        }
    }
}
