using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cryptographer

{
    public partial class Crypto_Form : Form
    {
        //Declare CSPParameters and rsaCrpytoServiceProvider
        private CspParameters cspp = new CspParameters();

        private RSACryptoServiceProvider rsa;

        //Paths
        private const string EncrtFolder = @"c:\Cryptographer\Encrypt\";

        private const string DecrFolder = @"c:\Cryptographer\Decrypt\";

        private const string SrcFolder = @"c:\Cryptographer\docs\";

        //Pub key
        private const string PubKeyFile = @"c:\Cryptographer\encrypt\rsaPublicKey.txt";

        //Private/public key value pair
        private const string KeyName = "Key01";

        public Crypto_Form()
        {
            InitializeComponent();
        }

        private void buttonCreateAsmKeys_Click(object sender, EventArgs e)
        {
            //stores a key pair in the key container
            cspp.KeyContainerName = KeyName;
            rsa = new RSACryptoServiceProvider(cspp);
            rsa.PersistKeyInCsp = true;
            string labeltxt = (rsa.PublicOnly) ? " - Public Only" : " - Full Key Pair";
            label1.Text = "Key: " + cspp.KeyContainerName + labeltxt;
        }

        private void EncryptFile(string inFile)
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
            string outFile = EncrtFolder + inFile.Substring(startFileName, inFile.LastIndexOf(".") - startFileName) + ".enc";

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

        private void buttonEncryptFile_Click(object sender, EventArgs e)
        {
            if (rsa == null)
            {
                MessageBox.Show("Key not set.");
            }
            else
            {
                //Display a dialog box to select a file to encrypt

                openFileDialog1.InitialDirectory = SrcFolder;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string fname = openFileDialog1.FileName;
                    if (fname != null)
                    {
                        FileInfo finfo = new FileInfo(fname);
                        //Pass the file name without the path
                        string name = finfo.FullName;
                        EncryptFile(name);
                    }
                }
            }
        }

        private void buttonDecryptFile_Click(object sender, EventArgs e)
        {
            if (rsa == null)
            {
                MessageBox.Show("Key not set.");
            }
            else
            {
                //Display a dialog box to select a file to encrypt

                openFileDialog2.InitialDirectory = EncrtFolder;
                if (openFileDialog2.ShowDialog() == DialogResult.OK)
                {
                    string fname = openFileDialog2.FileName;
                    if (fname != null)
                    {
                        FileInfo finfo = new FileInfo(fname);
                        //Pass the file name without the path
                        string name = finfo.Name;
                        DecryptFile(name);
                    }
                }
            }
        }

        private void DecryptFile(string inFile)
        {
            Aes aes = Aes.Create();

            byte[] LenKey = new byte[4];
            byte[] LenIV = new byte[4];
            Console.WriteLine(inFile);

            string outFile = DecrFolder + inFile.Substring(0, inFile.LastIndexOf(".")) + ".txt";

            using (FileStream inFilestream = new FileStream(EncrtFolder + inFile, FileMode.Open)) //combines two strings
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

                Directory.CreateDirectory(DecrFolder);

                byte[] KeyDecrypted = rsa.Decrypt(KeyEncrypted, false);

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

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            // Method intentionally left empty.
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            // Method intentionally left empty.
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Method intentionally left empty.
        }

        private void buttonExportPublicKey_Click(object sender, EventArgs e)
        {
            //saves a RSA public key
            Directory.CreateDirectory(EncrtFolder);
            StreamWriter streamwriter = new StreamWriter(PubKeyFile, false);
            streamwriter.Write(rsa.ToXmlString(false));
            streamwriter.Close();
        }

        private void buttonImportPublicKey_Click(object sender, EventArgs e)
        {
            //loads key
            StreamReader streamReader = new StreamReader(PubKeyFile);
            cspp.KeyContainerName = KeyName;
            rsa = new RSACryptoServiceProvider(cspp);
            string keytxt = streamReader.ReadToEnd();
            rsa.FromXmlString(keytxt);
            rsa.PersistKeyInCsp = true;

            string labeltxt = (rsa.PublicOnly) ? " - Public Only" : " - Full Key Pair";
            label1.Text = "Key: " + cspp.KeyContainerName + labeltxt;

            streamReader.Close();
        }

        private void buttonGetPrivateKey_Click(object sender, EventArgs e)
        {
            cspp.KeyContainerName = KeyName;
            rsa = new RSACryptoServiceProvider(cspp);
            rsa.PersistKeyInCsp = true;
            string labeltxt = (rsa.PublicOnly) ? " - Public Only" : " - Full Key Pair";
            label1.Text = "Key: " + cspp.KeyContainerName + labeltxt;
        }
    }
}