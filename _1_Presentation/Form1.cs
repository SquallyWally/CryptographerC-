using Cryptographer._3_Domain;
using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Cryptographer

{
    public partial class Crypto_Form : Form
    {
        //Declare CSPParameters and rsaCrpytoServiceProvider
        private CspParameters cspp = new CspParameters();

        private RSACryptoServiceProvider rsa;

        //Paths
        private const string PATH = @"c:\Cryptographer\encrypt\rsaPublicKey.txt";

        private const string EncrtFolder = @"c:\Cryptographer\Encrypt\";

        private const string DecrFolder = @"c:\Cryptographer\Decrypt\";

        private const string SrcFolder = @"c:\Cryptographer\docs\";

        //Pub key
        private const string PubKeyFile = PATH;

        //Private/public key value pair
        private const string KeyName = "Key01";

        private Decryptor decrypt = new Decryptor(DecrFolder, EncrtFolder);
        private Encryptor encrypt = new Encryptor(EncrtFolder);

        public Crypto_Form()
        {
            InitializeComponent();
        }

        private void ButtonCreateAsmKeys_Click(object sender, EventArgs e)
        {
            //stores a key pair in the key container
            cspp.KeyContainerName = KeyName;
            rsa = new RSACryptoServiceProvider(cspp);
            rsa.PersistKeyInCsp = true;
            string labeltxt = (rsa.PublicOnly) ? " - Public Only" : " - Full Key Pair";
            label1.Text = "Key: " + cspp.KeyContainerName + labeltxt;
        }

        private void ButtonEncryptFile_Click(object sender, EventArgs e)
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
                        encrypt.EncryptFile(name, rsa);
                    }
                }
            }
        }

        private void ButtonDecryptFile_Click(object sender, EventArgs e)
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
                        decrypt.DecryptFile(name, rsa);
                    }
                }
            }
        }

        private void ButtonExportPublicKey_Click(object sender, EventArgs e)
        {
            //saves a RSA public key
            Directory.CreateDirectory(EncrtFolder);
            StreamWriter streamwriter = new StreamWriter(PubKeyFile, false);
            streamwriter.Write(rsa.ToXmlString(false));
            streamwriter.Close();
        }

        private void ButtonImportPublicKey_Click(object sender, EventArgs e)
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

        private void ButtonGetPrivateKey_Click(object sender, EventArgs e)
        {
            cspp.KeyContainerName = KeyName;
            rsa = new RSACryptoServiceProvider(cspp);
            rsa.PersistKeyInCsp = true;
            string labeltxt = (rsa.PublicOnly) ? " - Public Only" : " - Full Key Pair";
            label1.Text = "Key: " + cspp.KeyContainerName + labeltxt;
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
    }
}