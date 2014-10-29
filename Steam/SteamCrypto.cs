/*
	This project is licensed under the GPL 2.0 license. Please respect that.

	Initial author: (https://github.com/)Convery
	Started: 2014-10-29
	Notes:
        Straight port of the C++ part, should work.
*/

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using softwareunion;

namespace SteamServer
{
    class SteamCrypto
    {
        // InitializationVector for 3DES.
        public static Byte[] CalculateIV(UInt32 Seed)
        {
            HashAlgorithm hash = new Tiger();

            return hash.ComputeHash(BitConverter.GetBytes(Seed));
        }

        // Hash for integrity checking.
        public static UInt64 fnv1_hash(Byte[] Data)
        {
            UInt64 h = 14695981039346656037;

            for (Int32 i = 0; i < Data.Length; i++)
                h = (h * 1099511628211) ^ Data[i];

            return h;
        }

        // 3DES Crypto.
        public static Byte[] Encrypt(Byte[] Data, Byte[] Key, Byte[] IV)
        {
            TripleDES des3 = new TripleDESCryptoServiceProvider();

            des3.Mode = CipherMode.CBC;
            des3.Padding = PaddingMode.Zeros;
            des3.Key = Key;
            des3.IV = IV;

            ICryptoTransform CryptoTransform = des3.CreateEncryptor();
            return CryptoTransform.TransformFinalBlock(Data, 0, Data.Length);
        }
        public static Byte[] Decrypt(Byte[] Data, Byte[] Key, Byte[] IV)
        {
            TripleDES des3 = new TripleDESCryptoServiceProvider();

            des3.Mode = CipherMode.CBC;
            des3.Padding = PaddingMode.Zeros;
            des3.Key = Key;
            des3.IV = IV;

            ICryptoTransform CryptoTransform = des3.CreateDecryptor();
            return CryptoTransform.TransformFinalBlock(Data, 0, Data.Length);
        }
    }
}
