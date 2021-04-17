using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cryptographer._4_Domain
{
    internal interface IDecryptor
    {
        void DecryptFile(string inFile, RSACryptoServiceProvider _rsa);
    }
}