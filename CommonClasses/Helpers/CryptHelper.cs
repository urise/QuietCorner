using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Security.Cryptography;

namespace CommonClasses.Helpers
{
    public static class CryptHelper
    {
        public static string GetSha512Base64Hash(string text)
        {
            byte[] result = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(text));
            return Convert.ToBase64String(result).Replace("/", "-").Replace("+","_");
        }

        public static string GetMd5Hash(string text)
        {
            return Encoding.UTF8.GetString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(text)));
        }

        public static string GetSaltedMd5Hash(string text)
        {
            return GetMd5Hash(text + GetMd5Hash(text));
        }

        #region MD5
        /// <summary>
        /// Encrypts the string to a byte array using the MD5 Encryption Algorithm.
        /// <see cref="System.Security.Cryptography.MD5CryptoServiceProvider"/>
        /// </summary>
        /// <param name="ToEncrypt">System.String.  Usually a password.</param>
        /// <returns>System.Byte[]</returns>
        public static byte[] MD5Encryption(string ToEncrypt)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hashedbytes;
            UTF8Encoding textencoder = new UTF8Encoding();
            hashedbytes = md5.ComputeHash(textencoder.GetBytes(ToEncrypt));
            md5.Clear();
            return hashedbytes;
        }


        /// <summary>
        /// Encrypts the string to a byte array using the MD5 Encryption 
        /// Algorithm with an additional Salted Hash.
        /// <see cref="System.Security.Cryptography.MD5CryptoServiceProvider"/>
        /// </summary>
        /// <param name="ToEncrypt">System.String.  Usually a password.</param>
        /// <returns>System.Byte[]</returns>
        public static byte[] MD5SaltedHashEncryption(string ToEncrypt)
        {

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hashedbytes;
            byte[] saltedhash;
            UTF8Encoding textencoder = new UTF8Encoding();
            hashedbytes = md5.ComputeHash(textencoder.GetBytes(ToEncrypt));
            ToEncrypt += textencoder.GetString(hashedbytes);
            saltedhash = md5.ComputeHash(textencoder.GetBytes(ToEncrypt));
            md5.Clear();
            return saltedhash;
        }
        #endregion

        public static string ByteArrayToString(byte[] ba)
        {
            SoapHexBinary Hex = new SoapHexBinary(ba);
            return Hex.ToString();
        }

        public static byte[] StringToByteArray(string ba)
        {
            SoapHexBinary Hex = SoapHexBinary.Parse(ba);
            return Hex.Value;
        }

        public static bool IsHashEqualsWithStored(string salt, string storedHash, string transferredHash)
        {
            var hash = GetSha512Base64Hash(salt + storedHash);
            return hash.Equals(transferredHash);
        }
    }
}
