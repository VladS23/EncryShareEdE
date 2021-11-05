using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTools
{
    public class CryptoTools
    {
        public UnicodeEncoding ByteConverter = new UnicodeEncoding();
        public static RSACryptoServiceProvider RSAcp = new RSACryptoServiceProvider(2048);
        public static RSAParameters RSAParam = RSAcp.ExportParameters(false);
        
        public static Aes myAes = Aes.Create();

        public static byte[] GetRSAModulus()
        {
            return RSAParam.Modulus;
        }
        public static byte[] GetRSAExponent()
        {
            return RSAParam.Exponent;
        }
        public static void SetRSAOpenKeys(byte[] Modulus, byte[] Exponent)
        {
            
            RSAParam.Modulus = Modulus;
            RSAParam.Exponent = Exponent;
            RSAcp.ImportParameters(RSAParam); 
        }
        public static void SetAESKeys(byte [] EncryptedKey, byte [] EncryptedIV)
        {
            myAes.Key = RSAcp.Decrypt(EncryptedKey, true);
            myAes.IV = RSAcp.Decrypt(EncryptedIV, true);
            
        }
        public static byte[] EncryptRSA(byte[] DataToEncrypt)
        {
            return RSAcp.Encrypt(DataToEncrypt, true);
        } 
        public static byte [] DecryptRSA(byte [] DataToDecrypt)
        {
            return RSAcp.Decrypt(DataToDecrypt, true);
        }
        public static byte[] EncryptAESKey()
        {
            return RSAcp.Encrypt(myAes.Key, true);
        }
        public static byte[] EncryptAESIV()
        {
            return RSAcp.Encrypt(myAes.IV, true);
        }
        public static byte[] EncryptString(string textToEncrypt, byte[] AESKey, byte[] AESIV)
        {
            byte[] encrypted;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = AESKey;
                aesAlg.IV = AESIV;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(textToEncrypt);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }
        public static byte[] EncryptFileToByte(string FileDirectory, byte[] AESKey, byte[] AESIV)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = AESKey;
                aesAlg.IV = AESIV;
                byte[] encrypted=File.ReadAllBytes(FileDirectory);

                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                {
                    return PerformCryptography(encryptor, encrypted);
                }
            }
        }
        public static void EncryptFile(string FileDirectory, byte[] AESKey, byte[] AESIV)
        {
            using (Aes aesAlg = Aes.Create())
            {
                //Используем три потока для шифрования, один считывает данные из файла и передает их во второй для расшифровки, расшифрованные даннные передаются в третий, который передает их в первый, чтобы он записал их в файл
                aesAlg.Key = AESKey;
                aesAlg.IV = AESIV;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (FileStream inputStream = new FileStream(FileDirectory, FileMode.Open, FileAccess.ReadWrite))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(inputStream, encryptor, CryptoStreamMode.Write))
                        {
                            inputStream.CopyTo(memoryStream);
                            inputStream.SetLength(0);
                            inputStream.Position = 0;
                            memoryStream.WriteTo(cryptoStream);
                        }
                    }
                }
            }
        }
        public static string DecryptToString(byte[] DataToDecrypt, byte[] AESKey, byte[] AESIV)
        {
            string decryptedText;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = AESKey;
                aesAlg.IV = AESIV;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                //Создаем поток для расшифровки
                using (MemoryStream msDecrypt = new MemoryStream(DataToDecrypt))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            decryptedText = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return decryptedText;
        }
        public static void DecryptFile(string FileDirectory, byte[] AESKey, byte[] AESIV)
        {
            //Здесь для расшифровки используются три потока, первый считывает данные из файла и передает их во второй, который непосредственно расшифровывает и передает в третий, который снова передает их в первый, чтобы он записал в файл
            using (FileStream inputStream = new FileStream(FileDirectory, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = AESKey;
                    aesAlg.IV = AESIV;
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (CryptoStream cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            cryptoStream.CopyTo(memoryStream);
                            inputStream.SetLength(0);
                            memoryStream.Position = 0;
                            memoryStream.CopyTo(inputStream);
                        }
                    }
                }
            }
        }
        public static byte[] DecryptToByte(byte[] data, byte[] AESKey, byte[] AESIV)
        {
            using (Aes aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(AESKey, AESIV))
                {
                    return PerformCryptography(decryptor, data);
                }
            }
        }





        private static byte[] PerformCryptography(ICryptoTransform cryptoTransform, byte[] data)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
