using Cryptographer._4_Domain;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Cryptographer._3_Domain
{
    internal class Decryptor : IDecryptor
    {
        private readonly string _decrFolder;
        private readonly string _encrFolder;

        public Decryptor(string DecrFolder, string EncrtFolder)
        {
            _decrFolder = DecrFolder;
            _encrFolder = EncrtFolder;
        }

        public void DecryptFile(string inFile, RSACryptoServiceProvider _rsa)
        {
            Aes aes = Aes.Create();

            byte[] LenKey = new byte[4];
            byte[] LenIV = new byte[4];
            Console.WriteLine(inFile);

            string outFile = _decrFolder + inFile.Substring(0, inFile.LastIndexOf(".")) + ".txt";

            using (FileStream inFilestream = new FileStream(_encrFolder + inFile, FileMode.Open)) //combines two strings
            {
                inFilestream.Seek(0, SeekOrigin.Begin);
                inFilestream.Seek(0, SeekOrigin.Begin);
                inFilestream.Read(LenKey, 0, 3);
                inFilestream.Seek(4, SeekOrigin.Begin);
                inFilestream.Read(LenIV, 0, 3);

                //convert the len to ints
                int intLenKey = BitConverter.ToInt32(LenKey, 0);
                int intLenIV = BitConverter.ToInt32(LenIV, 0);

                //Determine the start pos of the cipher text
                int startCipher = intLenKey + intLenIV + 8;
                int lenCipher = (int)inFilestream.Length - startCipher;

                byte[] KeyEncrypted = new byte[intLenKey];
                byte[] IV = new byte[intLenIV];

                //EXTRAT THE KEy and IV starting from index 8 after the length values
                inFilestream.Seek(8, SeekOrigin.Begin);
                inFilestream.Read(KeyEncrypted, 0, intLenKey);
                inFilestream.Seek(8 + intLenKey, SeekOrigin.Begin);
                inFilestream.Read(IV, 0, intLenIV);

                Directory.CreateDirectory(_decrFolder);

                byte[] KeyDecrypted = _rsa.Decrypt(KeyEncrypted, false);

                ICryptoTransform transform = aes.CreateDecryptor(KeyDecrypted, IV);
                Console.WriteLine(outFile);
                //Decrypt the cipher text from the filestream
                using (FileStream outfileStream = new FileStream(outFile, FileMode.Create))
                {
                    int count, offset = 0;

                    int blockSizeBytes = aes.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];

                    inFilestream.Seek(startCipher, SeekOrigin.Begin);
                    using (CryptoStream outStreamDecrypted = new CryptoStream(outfileStream, transform, CryptoStreamMode.Write))
                    {
                        do
                        {
                            count = inFilestream.Read(data, 0, blockSizeBytes);
                            offset += count;
                            outStreamDecrypted.Write(data, 0, count);
                        }

                        while (count > 0);
                        outStreamDecrypted.FlushFinalBlock();
                        outStreamDecrypted.Close();
                    }
                    outfileStream.Close();
                }
                inFilestream.Close();
            }
        }
    }
}