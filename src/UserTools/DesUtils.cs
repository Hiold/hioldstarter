using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HioldMod.src.UserTools
{
    class DesUtils
    {
        public static string Encrypt(string stringToEncrypt, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.GetEncoding("UTF-8").GetBytes(stringToEncrypt);
            des.Key = ASCIIEncoding.UTF8.GetBytes(sKey);
            des.IV = ASCIIEncoding.UTF8.GetBytes(sKey);
            des.Padding = PaddingMode.PKCS7;
            des.Mode = CipherMode.ECB;
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            ret.ToString();
            return ret.ToString();
        }


        public static string Decrypt(string stringToDecrypt, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Padding = PaddingMode.PKCS7;
            des.Mode = CipherMode.ECB;
            byte[] inputByteArray = new byte[stringToDecrypt.Length / 2];
            for (int x = 0; x < stringToDecrypt.Length / 2; x++)
            {
                int i = (Convert.ToInt32(stringToDecrypt.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }
            des.Key = ASCIIEncoding.UTF8.GetBytes(sKey);
            des.IV = ASCIIEncoding.UTF8.GetBytes(sKey);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            return System.Text.Encoding.GetEncoding("UTF-8").GetString(ms.ToArray());
        }
    }
}
