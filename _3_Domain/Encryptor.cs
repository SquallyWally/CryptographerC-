using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cryptographer._3_Domain
{
    internal class Encryptor
    {
        private readonly string _encrFolder;

        public Encryptor(string EncrtFolder)
        {
            _encrFolder = EncrtFolder;
        }

        public void EncryptFile(string inFile, RSACryptoServiceProvider rsa)
        {
            //1 Create AES symm algo to encrypt the content
            Aes aes = Aes.Create();
            ICryptoTransform transform = aes.CreateEncryptor();
            //2 Creates RSA Crypto object
            byte[] keyEncrypted = rsa.Encrypt(aes.Key, false);

            //3 determines the lengths of the key and IV, and creates byte arrays of their lenght values
            byte[] LenKey = new byte[4];
            byte[] LenIV = new byte[4];

            int lKey = keyEncrypted.Length;
            LenKey = BitConverter.GetBytes(lKey);
            int lIV = aes.IV.Length;
            LenIV = BitConverter.GetBytes(lIV);
            //4 use Stream object to read and encrypt the filestream of the surce file in blocks of bytes

            int startFileName = inFile.LastIndexOf("\\") + 1;
            //Change the file's extension to ".enc"
            string outFile = _encrFolder + inFile.Substring(startFileName, inFile.LastIndexOf(".") - startFileName) + ".enc";

            using (FileStream outFilestream = new FileStream(outFile, FileMode.Create))
            {
                //5 writes the key, IV and their lenght values to the ecnrypted package
                outFilestream.Write(LenKey, 0, 4);
                outFilestream.Write(LenIV, 0, 4);
                outFilestream.Write(keyEncrypted, 0, lKey);
                outFilestream.Write(aes.IV, 0, lIV);

                //writes cipher text using a cryptostream for encrypting
                using (CryptoStream outStreamEncrypted = new CryptoStream(outFilestream, transform, CryptoStreamMode.Write))
                {
                    //Encrypt a chunk at a time for memory savings
                    int count, offset = 0;

                    int blockSizeBytes = aes.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];
                    int bytesRead = 0;

                    using (FileStream inFilestream = new FileStream(inFile, FileMode.Open))
                    {
                        do
                        {
                            count = inFilestream.Read(data, 0, blockSizeBytes);
                            offset += count;
                            outStreamEncrypted.Write(data, 0, count);
                            bytesRead += blockSizeBytes;
                        }

                        while (count > 0);
                        inFilestream.Close();
                    }
                    outStreamEncrypted.FlushFinalBlock();
                    outStreamEncrypted.Close();
                }

                outFilestream.Close();
            }
        }
    }
}