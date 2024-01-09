using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FattureWebAuxiliar
{
    public sealed class Settings
    {
        public required string UsuarioProducao { get; set; }
        public required string SenhaProducao { get; set; }
        public required string UsuarioTeste { get; set; }
        public required string SenhaTeste { get; set; }
    }
}
