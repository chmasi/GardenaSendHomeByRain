using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ms
{
    class cProtection
    {
        readonly byte[] _key = { 3, 4, 19, 64, 27, 6, 64, 6, 6, 90, 28, 4, 93, 5, 1, 1, 17, 18, 19, 20, 21, 22, 23, 24 };
        readonly byte[] _iv = { 18, 1, 6, 11, 5, 3, 2, 11 };

        // define the triple des provider
        private readonly TripleDESCryptoServiceProvider _mDes = new TripleDESCryptoServiceProvider();

        // define the string handler
        private readonly UTF8Encoding _mUtf8 = new UTF8Encoding();

        // define the local property arrays
        private readonly byte[] _mKey;
        private readonly byte[] _mIv;

        /// <summary>
        /// Default constructor
        /// </summary>
        public cProtection()
        {
            _mKey = _key;
            _mIv = _iv;
        }

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        public cProtection(byte[] key, byte[] iv)
        {
            _mKey = key;
            _mIv = iv;
        }

        /// <summary>
        /// Encrypts the given byte array input
        /// </summary>
        /// <param name="input">Input value</param>
        /// <returns>Encrypted result</returns>
        public byte[] Encrypt(byte[] input)
        {
            return Transform(input, _mDes.CreateEncryptor(_mKey, _mIv));
        }

        /// <summary>
        /// Decrypts the given encrypted byte array input
        /// </summary>
        /// <param name="input">Encrypted byte array input</param>
        /// <returns>Decrypted result</returns>
        public byte[] Decrypt(byte[] input)
        {
            return Transform(input, _mDes.CreateDecryptor(_mKey, _mIv));
        }

        /// <summary>
        /// Encrypts the given string input
        /// </summary>
        /// <param name="text">Input value</param>
        /// <returns>Encrypted result</returns>
        public string Encrypt(string text)
        {
            byte[] input = _mUtf8.GetBytes(text);
            byte[] output = Transform(input, _mDes.CreateEncryptor(_mKey, _mIv));
            return Convert.ToBase64String(output);
        }

        /// <summary>
        /// Decrypts the given encrypted string input
        /// </summary>
        /// <param name="text">Encrypted string input</param>
        /// <returns>Decrypted result</returns>
        public string Decrypt(string text)
        {
            byte[] input = Convert.FromBase64String(text);
            byte[] output = Transform(input, _mDes.CreateDecryptor(_mKey, _mIv));
            return _mUtf8.GetString(output);
        }

        private static byte[] Transform(byte[] input, ICryptoTransform cryptoTransform)
        {
            // create the necessary streams
            using (MemoryStream memStream = new MemoryStream())
            {
                using (CryptoStream cryptStream = new CryptoStream(memStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    // transform the bytes as requested
                    cryptStream.Write(input, 0, input.Length);
                    cryptStream.FlushFinalBlock();
                    // Read the memory stream andconvert it back into byte array
                    memStream.Position = 0;
                    byte[] result = memStream.ToArray();
                    // close and release the streams
                    memStream.Close();
                    cryptStream.Close();
                    // hand back the encrypted buffer
                    return result;
                }
            }
        }
    }
}
