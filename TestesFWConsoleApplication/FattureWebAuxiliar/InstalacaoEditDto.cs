using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FattureWebAuxiliar
{
    public class InstalacaoEditDto
    {
        [JsonIgnore]
        public string Id { get; set; }

        [JsonProperty("data_inicio_leitura")]
        public string? DataInicioLeitura { get; set; }
    }
}
