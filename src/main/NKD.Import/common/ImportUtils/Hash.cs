using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace NKD.Import.ImportUtils
{
    static class Hash
    {
        private static byte[] ComputeHashBytes(this object o)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            var serializer = new DataContractJsonSerializer(o.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, o);
                return sha.ComputeHash(ms.ToArray());
            }
        }

        public static string ComputeHash(this object o)
        {
            return Encoding.UTF8.GetString(ComputeHashBytes(o));
        }

    }
}
