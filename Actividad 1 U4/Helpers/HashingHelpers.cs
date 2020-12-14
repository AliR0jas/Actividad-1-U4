using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Actividad_1_U4.Helpers
{
    public static class HashingHelpers
    {
        public static string GetHash(string cadena)
        {
            var a = SHA256.Create();
            byte[] codificar = System.Text.Encoding.UTF8.GetBytes(cadena);
            byte[] hash = a.ComputeHash(codificar);

            string res = "";
            foreach (var b in hash)
            {
                res += b.ToString("X2");
            }
            return res;
        }
    }
}
