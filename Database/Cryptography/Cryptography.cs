using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Database.Cryptography
{
    public class Cryptography
    {
        public static byte[] GenerateSalt()
        {
            int minSaltSize = 4;
            int maxSaltSize = 8;
            // Generate a random number for the size of the salt.
            Random random = new Random();
            int saltSize = random.Next(minSaltSize, maxSaltSize);

            // Allocate a byte array, which will hold the salt.
            byte[] saltBytes = new byte[saltSize];

            // Initialize a random number generator.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            // Fill the salt with cryptographically strong byte values.
            rng.GetNonZeroBytes(saltBytes);
            return saltBytes;
        }
        public static string ComputeHash(string plainText, byte[] saltBytes)
        {
            // Convert plain text into a byte array.
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            // Allocate array, which will hold plain text and salt.
            byte[] plainTextWithSaltBytes =
                    new byte[plainTextBytes.Length + saltBytes.Length];

            // Copy plain text bytes into resulting array.
            for (int i = 0; i < plainTextBytes.Length; i++)
                plainTextWithSaltBytes[i] = plainTextBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < saltBytes.Length; i++)
                plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];

            // Initialize appropriate hashing algorithm class.
            HashAlgorithm hash = new SHA256Managed();

            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

            // Create array which will hold hash and original salt bytes.
            byte[] hashWithSaltBytes = new byte[hashBytes.Length + saltBytes.Length];

            // Copy hash bytes into resulting array.
            for (int i = 0; i < hashBytes.Length; i++)
                hashWithSaltBytes[i] = hashBytes[i];

            // Append salt bytes to the result.
            for (int i = 0; i < saltBytes.Length; i++)
                hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];

            // Convert result into a base64-encoded string.
            string hashValue = Convert.ToBase64String(hashWithSaltBytes);

            // Return the result.
            return hashValue;
        }
        public static bool VerifyHash(string plainText, string hashValue)
        {
            // Convert base64-encoded hash value into a byte array.
            byte[] hashWithSaltBytes = Convert.FromBase64String(hashValue);

            // We must know size of hash (without salt).
            int hashSizeInBits, hashSizeInBytes;

            // Size of hash is based on the specified algorithm.
            hashSizeInBits = 256;
            // Convert size of hash from bits to bytes.
            hashSizeInBytes = hashSizeInBits / 8;

            // Make sure that the specified hash value is long enough.
            if (hashWithSaltBytes.Length < hashSizeInBytes)
                return false;

            // Allocate array to hold original salt bytes retrieved from hash.
            byte[] saltBytes = new byte[hashWithSaltBytes.Length - hashSizeInBytes];

            // Copy salt from the end of the hash to the new array.
            for (int i = 0; i < saltBytes.Length; i++)
                saltBytes[i] = hashWithSaltBytes[hashSizeInBytes + i];

            // Compute a new hash string.
            string expectedHashString = Cryptography.ComputeHash(plainText, saltBytes);

            // If the computed hash matches the specified hash,
            // the plain text value must be correct.
            return (hashValue == expectedHashString);
        }

        #region Decrypt String
        public static string Decrypt(string cipherText)
        {
            cipherText = cipherText.Replace(' ', '+');
            string password = "m1t!g@t3M@7@tA";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

            byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(16), pdb.GetBytes(16));
            return System.Text.Encoding.Unicode.GetString(decryptedData);
        }

        #endregion Decrypt String

        #region Decrypt Bytes
        public static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {
            // Create a MemoryStream that is going to accept the decrypted bytes 
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;

            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            // Write the data and make it do the decryption 
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();

            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }
        #endregion Decrypt Bytes

        #region Encrypt String

        public static string Encrypt(string clearText)
        {
            string password = "m1t!g@t3M@7@tA";
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(password,
            new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(16), pdb.GetBytes(16));
            return Convert.ToBase64String(encryptedData);
        }
        #endregion Encrypt String

        #region Encrypt Bytes
        public static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            // Create a MemoryStream that is going to accept the encrypted bytes 
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;

            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            // Write the data and make it do the encryption 
            cs.Write(clearData, 0, clearData.Length);
            cs.Close();

            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }
        #endregion Encrypt Bytes

        public static string encodeSTROnUrl(string thisEncode)
        {
            if (null == thisEncode)
                return string.Empty;

            return HttpUtility.UrlEncode(Encrypt(thisEncode));
        }
        public static string decodeSTROnUrl(string thisDecode)
        {
            return Decrypt(HttpUtility.UrlDecode(thisDecode));
        }

    }
}
