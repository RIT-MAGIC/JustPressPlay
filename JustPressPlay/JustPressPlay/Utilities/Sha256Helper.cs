using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace JustPressPlay.Utilities
{
    public class Sha256Helper
    {
        private static byte[] GenerateSalt()
        {
            const int saltSize = 6; // TODO: randomize?

            byte[] saltData = new byte[saltSize];

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetNonZeroBytes(saltData);

            return saltData;
        }

        /// <summary>
        /// Salt and hash a string using SHA256
        /// </summary>
        /// <param name="value">The string to hash</param>
        /// <param name="hashedValue">The salted hash of the input string</param>
        /// <param name="salt">The salt used to generate the hash</param>
        public static void HashString(string value, out string hashedValue, out string salt)
        {
            byte[] stringData = Encoding.UTF8.GetBytes(value);
            byte[] generatedSalt = GenerateSalt();

            byte[] saltedString = stringData.Concat(generatedSalt).ToArray();

            SHA256Managed generator = new SHA256Managed();
            byte[] hashData = generator.ComputeHash(saltedString);

            hashedValue = Convert.ToBase64String(hashData);
            salt = Convert.ToBase64String(generatedSalt);
        }
    }
}