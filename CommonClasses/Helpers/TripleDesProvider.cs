using System;
using System.Text;
using System.Security.Cryptography;

namespace CommonClasses.Helpers
{
    /// <summary>
    /// TripleDES Class to create a more user-friendly assembly.
    /// <see cref="System.Security.Cryptography.TripleDESCryptoServiceProvider"/>
    /// </summary>
    public class TripleDesProvider
    {
        #region Variables
        /// <summary>
        /// Global variable to hold the Triple DES Crypto Provider.
        /// </summary>
        private TripleDESCryptoServiceProvider des;
        /// <summary>
        /// Byte array that stores the key.
        /// </summary>
        private byte[] key;
        /// <summary>
        /// Byte array that stores the Initialization Vector.
        /// </summary>
        private byte[] iv;
        #endregion Variables

        #region Properties
        /// <summary>
        /// Property to set the value of the key.
        /// </summary>
        public byte[] Key
        {
            get { return key; }
            set { key = value; }
        }
        /// <summary>
        /// Property to set the value of the Initialization Vector.
        /// </summary>
        public byte[] IV
        {
            get { return iv; }
            set { iv = value; }
        }

        /// <summary>
        /// SetKeys will create a 192 bit key and 64 bit IV based on 		
        /// </summary>
        public string SetKeys
        {
            set
            {
                // create the byte arrays needed to create the key and iv.
                byte[] md5key;
                byte[] hashedkey;

                md5key = CryptHelper.MD5Encryption(value);
                hashedkey = CryptHelper.MD5SaltedHashEncryption(value);

                // loop to transfer the keys.
                for (int i = 0; i < hashedkey.Length; i++)
                {
                    key[i] = hashedkey[i];
                }

                // create the start and mid portion of the hashed key
                int startcount = hashedkey.Length; /* always 128 */
                int midcount = md5key.Length / 2; /* always 64 */

                // loop to fill in the rest of the key, and create 
                // the IV with the remaining.
                for (int i = midcount; i < md5key.Length; i++)
                {
                    key[startcount + (i - midcount)] = md5key[i];
                    iv[i - midcount] = md5key[i - midcount];
                }
            }
        }

        /// <summary>
        /// Gets the current CipherMode for the TripleDes Provider.
        /// <see cref="System.Security.Cryptography.CipherMode"/>
        /// </summary>
        public System.Security.Cryptography.CipherMode GetCipherMode
        {
            get { return des.Mode; }
        }
        /// <summary>
        /// Sets the CipherMode by translating the enum found in this class
        /// and sets the appropriate CipherMode enum value to the Mode of 
        /// the TripleDes Provider.
        /// <see cref="CipherMode"/>
        /// <see cref="System.Security.Cryptography.CipherMode"/>
        /// </summary>
        public TripleDesProvider.CipherMode SetCipherMode
        {
            set { des.Mode = this.TranslateCipherMode(value); }
        }

        #endregion Properties

        #region Cosntructor
        /// <summary>
        /// Creates an instance of the necessary objects.
        /// </summary>
        public TripleDesProvider(string password)
        {
            des = new TripleDESCryptoServiceProvider();
            iv = new byte[8];
            key = new byte[24];
            SetKeys = password;
        }

        #endregion Cosntructor

        #region Methods
        /// <summary>
        /// Encrypt will create an instance of the ICryptoTransform, and 
        /// Return/Execute the Transform method.
        /// </summary>
        /// <param name="inputvar">Byte array holding the value to be encrypted.</param>
        /// <returns>Byte array storing the holding value.</returns>
        public byte[] Encrypt(byte[] inputvar)
        {
            return Transform(inputvar, des.CreateEncryptor(key, iv));
        }

        /// <summary>
        /// Encrypt will create a byte array, and call it's overload that 
        /// will create the ICryptoTransform to return the needed output.
        /// </summary>
        /// <param name="inputvar">String that needs to be encrypted.</param>
        /// <returns>The encrypted value in string format.</returns>
        public string Encrypt(string inputvar)
        {
            byte[] inputbytes = Encoding.UTF8.GetBytes(inputvar);
            return CryptHelper.ByteArrayToString(Encrypt(inputbytes));
        }

        /// <summary>
        /// Decrypt will create an instance of the ICryptoTransform, and 
        /// Return/Execute the Transform method.
        /// </summary>
        /// <param name="inputvar">Byte array holding the value to be decrypted.</param>
        /// <returns>Byte array holding the decrypted value.</returns>
        public byte[] Decrypt(byte[] inputvar)
        {
            return Transform(inputvar, des.CreateDecryptor(key, iv));
        }

        /// <summary>
        /// Decrypt will create a byte array, and call it's overload that 
        /// will create the ICryptoTransform to return the needed output.
        /// </summary>
        /// <param name="inputvar">String that needs to be decrypted.</param>
        /// <returns>The decrypted value in string format.</returns>
        public string Decrypt(string inputvar)
        {

            byte[] inputbytes = CryptHelper.StringToByteArray(inputvar);
            return Encoding.UTF8.GetString(Decrypt(inputbytes));
        }

        /// <summary>
        /// Transform will accept the input byte array, and transform it based on 
        /// the type of ICryptTransform created.
        /// 
        /// Special note on the way the streams are managed and created.
        /// <see cref="System.Security.Cryptography.TripleDESCryptoServiceProvider"/>
        /// </summary>
        /// <param name="inputvar">Byte array holding the value to be transformed.</param>
        /// <param name="transform">This will be the outcome of System.</param>
        /// <returns>Byte array holding the transformed value.</returns>
        private byte[] Transform(byte[] inputvar, ICryptoTransform transform)
        {
            // Declare the output variable.
            byte[] returnvar;
            // MemoryStream to hold the bytes of the output.
            System.IO.MemoryStream stream = new System.IO.MemoryStream(2048);
            // CryptoStream that will be used to actually write the transformation.
            CryptoStream encryptstream = new CryptoStream(stream, transform, CryptoStreamMode.Write);

            // Write the input array values into the crypto stream, and transform.
            encryptstream.Write(inputvar, 0, (int)inputvar.Length);
            encryptstream.FlushFinalBlock();

            // Get the output to return.
            stream.Position = 0;
            returnvar = new byte[(int)stream.Length];
            stream.Read(returnvar, 0, (int)returnvar.Length);

            // close streams and destroy objects.
            encryptstream.Close();
            stream.Close();
            encryptstream = null;
            stream = null;

            // return the output.
            return returnvar;
        }

        #endregion Methods

        #region Enum & Method
        /// <summary>
        /// These modes are the only ones available for DES based encryption.  
        /// It's identical to the ones found in System.Security.Cryptography.CipherMode 
        /// but it is completely written out rather than abbreviated.
        /// <see cref="System.Security.Cryptography.CipherMode"/>
        /// </summary>
        public enum CipherMode
        {
            /// <summary>
            /// System.Security.Cryptography.CipherMode.CBC
            /// </summary>
            CipherBlockChaining = 1,
            /// <summary>
            /// System.Security.Cryptography.CipherMode.ECB
            /// </summary>
            ElectronicCodebook = 2,
            /// <summary>
            /// System.Security.Cryptography.CipherMode.OFB
            /// </summary>
            OutputFeedback = 3,
            /// <summary>
            /// System.Security.Cryptography.CipherMode.CFB
            /// </summary>
            CipherFeedback = 4
        }

        /// <summary>
        /// Could have been done better with mapping, but decided to go with this route so you 
        /// can get a more text like selection in code.
        /// </summary>
        /// <param name="ciphermode">TripleDES.CipherMode value mapped to the corresponding 
        /// System.Security.Cryptography.CipherMode</param>
        /// <returns>System.Security.Cryptography.CipherMode that matches the selected 
        /// TripleDES.CipherMode</returns>
        private System.Security.Cryptography.CipherMode TranslateCipherMode(TripleDesProvider.CipherMode ciphermode)
        {
            return (System.Security.Cryptography.CipherMode)Convert.ToInt32(ciphermode);
        }
        #endregion Enum & Method
    }
}
