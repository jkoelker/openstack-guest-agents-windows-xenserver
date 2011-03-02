using System;
using System.Security.Cryptography;
using System.Text;
using Rackspace.Cloud.Server.Agent.Utilities;

namespace Rackspace.Cloud.Server.Agent.Security {
    public class Encryption {
        public static string Decrypt(byte[] encryptedData, string sharedKey, byte[] salt) {
            var aesData = encryptedData;
            var password = Encoding.UTF8.GetBytes(sharedKey);
            var md5 = MD5.Create();

            var preKeyLength = password.Length + salt.Length;
            var preKey = new byte[preKeyLength];

            Buffer.BlockCopy(password, 0, preKey, 0, password.Length);
            Buffer.BlockCopy(salt, 0, preKey, password.Length, salt.Length);

            var key = md5.ComputeHash(preKey);
            var preIVLength = key.Length + preKeyLength;
            var preIV = new byte[preIVLength];

            Buffer.BlockCopy(key, 0, preIV, 0, key.Length);
            Buffer.BlockCopy(preKey, 0, preIV, key.Length, preKey.Length);

            var iv = md5.ComputeHash(preIV);
            md5.Clear();

            // Decrypt using AES
            var rijndael = new RijndaelManaged
                               {
                                   Mode = CipherMode.CBC,
                                   Padding = PaddingMode.None,
                                   KeySize = 128,
                                   BlockSize = 128,
                               };

            var rijndaelDecryptor = rijndael.CreateDecryptor(key, iv);
            var clearData = rijndaelDecryptor.TransformFinalBlock(aesData, 0, aesData.Length);
            var clearText = new ASCIIEncoding().GetString(clearData);

            ////When decrypted it comes back with a new line character at the end, whcih it gets rid of
            return clearText.SplitOnNewLine().First();
        }
    }
}