using System;
using System.IO;
using System.Security.Cryptography;
using System.Web;

internal class AesEncryption
{
    private byte[] key; 

    public AesEncryption(byte[] key)
    { 
        this.key = key;
    }

    public static byte[] GenerateKey()
    {
        Aes aesAlg = Aes.Create();
        aesAlg.KeySize = 256;
        aesAlg.GenerateKey();
        return aesAlg.Key;
    }
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            throw new ArgumentNullException(nameof(plainText), "Input cannot be empty.");

        using (Aes aesAlg = Aes.Create())
        {

            aesAlg.Key = key;
            aesAlg.IV = new byte[16]; // IV (Vector d'initialisation) doit être unique pour chaque opération

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                }
                byte[] encryptedData = msEncrypt.ToArray();
                return HttpUtility.UrlEncode(Convert.ToBase64String(encryptedData));
            }
        }
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrWhiteSpace(encryptedText))
            throw new ArgumentNullException(nameof(encryptedText), "Input cannot be empty.");

        byte[] encryptedData = Convert.FromBase64String(HttpUtility.UrlDecode(encryptedText));

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = new byte[16]; // IV (Vector d'initialisation) doit être le même que lors du chiffrement

            using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
            {
                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
