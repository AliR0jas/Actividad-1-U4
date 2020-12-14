using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Actividad_1_U4.Helpers
{
    public class Helper
    {
        public static int GetCodigo()
        {
            Random r = new Random();
            int codigo = r.Next(1000, 9999);
            int codigo2 = r.Next(1000, 9999);

            return (codigo + codigo2);
        }
    }
}
