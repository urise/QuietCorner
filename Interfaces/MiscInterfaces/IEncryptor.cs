using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interfaces.MiscInterfaces
{
    public interface IEncryptor
    {
        string EncryptDecimal(decimal value);
        decimal DecryptDecimal(string str);
        string Decrypt(string str);
    }
}
