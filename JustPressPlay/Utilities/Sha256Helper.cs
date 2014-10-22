/*
 * Copyright 2014 Rochester Institute of Technology
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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

        /// <summary>
        /// Hash a string with a set salt using SHA256
        /// </summary>
        /// <param name="value">The string to hash</param>
        /// <param name="salt">The salt to hash with</param>
        /// <returns>The salted hash of the input string</returns>
        public static string HashStringWithSalt(string value, string salt)
        {
            byte[] stringData = Encoding.UTF8.GetBytes(value);
            byte[] saltData = Encoding.UTF8.GetBytes(salt);

            byte[] saltedString = stringData.Concat(saltData).ToArray();

            SHA256Managed generator = new SHA256Managed();
            byte[] hashData = generator.ComputeHash(saltedString);

            return Convert.ToBase64String(hashData);
        }
    }
}