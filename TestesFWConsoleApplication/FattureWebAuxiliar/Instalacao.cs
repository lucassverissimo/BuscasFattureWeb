using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FattureWebAuxiliar
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public record DadoInstalacao(
        [property: JsonProperty("id")] int? Id,
        [property: JsonProperty("distribuidora_id")] int? DistribuidoraId,
        [property: JsonProperty("cliente_id")] int? ClienteId,
        [property: JsonProperty("entidade_id")] int? EntidadeId,
        [property: JsonProperty("status_webcrawlers_id")] int? StatusWebcrawlersId,
        [property: JsonProperty("data_criacao")] DateTime? DataCriacao,
        [property: JsonProperty("data_atualizacao")] DateTime? DataAtualizacao,
        [property: JsonProperty("data_inicio_leitura")] DateTime? DataInicioLeitura
    );

    public record RootInstalacao(
        [property: JsonProperty("status")] string Status,
        [property: JsonProperty("mensagem")] string Mensagem,
        [property: JsonProperty("dados")] IReadOnlyList<DadoInstalacao> Dados
    );
}
