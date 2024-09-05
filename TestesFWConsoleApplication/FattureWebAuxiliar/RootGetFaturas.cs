using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace FattureWebAuxiliar;

public record BandeirasTarifaria(
    [property: JsonProperty("dias")][property: JsonPropertyName("dias")] double? Dias,
    [property: JsonProperty("nome")][property: JsonPropertyName("nome")] string Nome
);

public record Composicao(
    [property: JsonProperty("energia")][property: JsonPropertyName("energia")] double? Energia,
    [property: JsonProperty("encargos")][property: JsonPropertyName("encargos")] double? Encargos,
    [property: JsonProperty("tributos")][property: JsonPropertyName("tributos")] double? Tributos,
    [property: JsonProperty("transmissao")]
    [property: JsonPropertyName("transmissao")]
        double? Transmissao,
    [property: JsonProperty("distribuicao")]
    [property: JsonPropertyName("distribuicao")]
        double? Distribuicao
);

public record Conteudo(
    [property: JsonProperty("fatura")][property: JsonPropertyName("fatura")] Fatura Fatura,
    [property: JsonProperty("outros")][property: JsonPropertyName("outros")] Outros Outros,
    [property: JsonProperty("fatura_id")][property: JsonPropertyName("fatura_id")] int? FaturaId,
    [property: JsonProperty("data_insercao")]
    [property: JsonPropertyName("data_insercao")]
        string DataInsercao,
    [property: JsonProperty("distribuidora")]
    [property: JsonPropertyName("distribuidora")]
        string Distribuidora,
    [property: JsonProperty("modelo_fatura")]
    [property: JsonPropertyName("modelo_fatura")]
        string ModeloFatura,
    [property: JsonProperty("distribuidora_cnpj")]
    [property: JsonPropertyName("distribuidora_cnpj")]
        string DistribuidoraCnpj,
    [property: JsonProperty("unidade_consumidora")]
    [property: JsonPropertyName("unidade_consumidora")]
        UnidadeConsumidora UnidadeConsumidora
);

public record Dado(
    [property: JsonProperty("id")][property: JsonPropertyName("id")] int? Id,
    [property: JsonProperty("instalacao_id")]
    [property: JsonPropertyName("instalacao_id")]
        int? InstalacaoId,
    [property: JsonProperty("arquivo_id")]
    [property: JsonPropertyName("arquivo_id")]
        int? ArquivoId,
    [property: JsonProperty("status_fatura_id")]
    [property: JsonPropertyName("status_fatura_id")]
        int? StatusFaturaId,
    [property: JsonProperty("status")][property: JsonPropertyName("status")] bool? Status,
    [property: JsonProperty("data_criacao")]
    [property: JsonPropertyName("data_criacao")]
        DateTime? DataCriacao,
    [property: JsonProperty("data_atualizacao")]
    [property: JsonPropertyName("data_atualizacao")]
        DateTime? DataAtualizacao,
    [property: JsonProperty("processamento_id")]
    [property: JsonPropertyName("processamento_id")]
        object ProcessamentoId,
    [property: JsonProperty("usuario_id")]
    [property: JsonPropertyName("usuario_id")]
        object UsuarioId,
    [property: JsonProperty("email_fatura_id")]
    [property: JsonPropertyName("email_fatura_id")]
        object EmailFaturaId,
    [property: JsonProperty("email_fatura_assunto")]
    [property: JsonPropertyName("email_fatura_assunto")]
        object EmailFaturaAssunto,
    [property: JsonProperty("data_processamento")]
    [property: JsonPropertyName("data_processamento")]
        DateTime? DataProcessamento,
    [property: JsonProperty("erro_processamento")]
    [property: JsonPropertyName("erro_processamento")]
        string ErroProcessamento,
    [property: JsonProperty("mes_referencia")]
    [property: JsonPropertyName("mes_referencia")]
        DateTime? MesReferencia,
    [property: JsonProperty("data_vencimento")]
    [property: JsonPropertyName("data_vencimento")]
        DateTime? DataVencimento,
    [property: JsonProperty("valor_total")]
    [property: JsonPropertyName("valor_total")]
        double? ValorTotal,
    [property: JsonProperty("conteudo")][property: JsonPropertyName("conteudo")] Conteudo Conteudo
);

public record DevolucaoGeracao(
    [property: JsonProperty("saldos_geracao")]
    [property: JsonPropertyName("saldos_geracao")]
        IReadOnlyList<SaldosGeracao> SaldosGeracao,
    [property: JsonProperty("participacao")]
    [property: JsonPropertyName("participacao")]
        double? Participacao,
    [property: JsonProperty("cod_unidade_geradora")]
    [property: JsonPropertyName("cod_unidade_geradora")]
        object CodUnidadeGeradora
);

public record Dic(
    [property: JsonProperty("meta_mensal")]
    [property: JsonPropertyName("meta_mensal")]
        double? MetaMensal,
    [property: JsonProperty("apurado_mensal")]
    [property: JsonPropertyName("apurado_mensal")]
        double? ApuradoMensal
);

public record Dicri(
    [property: JsonProperty("meta_mensal")]
    [property: JsonPropertyName("meta_mensal")]
        double? MetaMensal
);

public record Dmic(
    [property: JsonProperty("meta_mensal")]
    [property: JsonPropertyName("meta_mensal")]
        double? MetaMensal,
    [property: JsonProperty("apurado_mensal")]
    [property: JsonPropertyName("apurado_mensal")]
        double? ApuradoMensal
);

public record Eusd(
    [property: JsonProperty("apurado_mensal")]
    [property: JsonPropertyName("apurado_mensal")]
        double? ApuradoMensal
);

public record Fatura(
    [property: JsonProperty("leitura")][property: JsonPropertyName("leitura")] Leitura Leitura,
    [property: JsonProperty("produtos")]
    [property: JsonPropertyName("produtos")]
        IReadOnlyList<Produto> Produtos,
    [property: JsonProperty("tributos")]
    [property: JsonPropertyName("tributos")]
        IReadOnlyList<Tributo> Tributos,
    [property: JsonProperty("indicadores")]
    [property: JsonPropertyName("indicadores")]
        Indicadores Indicadores,
    [property: JsonProperty("total_pagar")]
    [property: JsonPropertyName("total_pagar")]
        double? TotalPagar,
    [property: JsonProperty("data_emissao")]
    [property: JsonPropertyName("data_emissao")]
        string DataEmissao,
    [property: JsonProperty("total_fatura")]
    [property: JsonPropertyName("total_fatura")]
        double? TotalFatura,
    [property: JsonProperty("numero_fatura")]
    [property: JsonPropertyName("numero_fatura")]
        string NumeroFatura,
    [property: JsonProperty("mes_referencia")]
    [property: JsonPropertyName("mes_referencia")]
        string MesReferencia,
    [property: JsonProperty("data_vencimento")]
    [property: JsonPropertyName("data_vencimento")]
        string DataVencimento,
    [property: JsonProperty("data_apresentacao")]
    [property: JsonPropertyName("data_apresentacao")]
        string DataApresentacao,
    [property: JsonProperty("bandeiras_tarifarias")]
    [property: JsonPropertyName("bandeiras_tarifarias")]
        IReadOnlyList<BandeirasTarifaria> BandeirasTarifarias,
    [property: JsonProperty("historico_faturamento")]
    [property: JsonPropertyName("historico_faturamento")]
        IReadOnlyList<HistoricoFaturamento> HistoricoFaturamento,
    [property: JsonProperty("devolucao_geracao")]
    [property: JsonPropertyName("devolucao_geracao")]
        DevolucaoGeracao DevolucaoGeracao,
    [property: JsonProperty("composicao")]
    [property: JsonPropertyName("composicao")]
        Composicao Composicao
);

public record Fic(
    [property: JsonProperty("meta_mensal")]
    [property: JsonPropertyName("meta_mensal")]
        double? MetaMensal,
    [property: JsonProperty("apurado_mensal")]
    [property: JsonPropertyName("apurado_mensal")]
        double? ApuradoMensal
);

public record HistoricoFaturamento(
    [property: JsonProperty("data")][property: JsonPropertyName("data")] string Data,
    [property: JsonProperty("periodo_dias")]
    [property: JsonPropertyName("periodo_dias")]
        int? PeriodoDias,
    [property: JsonProperty("energia_ativa")]
    [property: JsonPropertyName("energia_ativa")]
        decimal? EnergiaAtiva,
    [property: JsonProperty("valor_total")]
    [property: JsonPropertyName("valor_total")]
        double? ValorTotal
);

public record Indicadores(
    [property: JsonProperty("dic")][property: JsonPropertyName("dic")] Dic Dic,
    [property: JsonProperty("fic")][property: JsonPropertyName("fic")] Fic Fic,
    [property: JsonProperty("dmic")][property: JsonPropertyName("dmic")] Dmic Dmic,
    [property: JsonProperty("eusd")][property: JsonPropertyName("eusd")] Eusd Eusd,
    [property: JsonProperty("dicri")][property: JsonPropertyName("dicri")] Dicri Dicri,
    [property: JsonProperty("mes_referencia")]
    [property: JsonPropertyName("mes_referencia")]
        string MesReferencia
);

public record Leitura(
    [property: JsonProperty("medidores")]
    [property: JsonPropertyName("medidores")]
        IReadOnlyList<Medidore> Medidores,
    [property: JsonProperty("data_atual")]
    [property: JsonPropertyName("data_atual")]
        string DataAtual,
    [property: JsonProperty("data_proxima")]
    [property: JsonPropertyName("data_proxima")]
        string DataProxima,
    [property: JsonProperty("periodo_dias")]
    [property: JsonPropertyName("periodo_dias")]
        int? PeriodoDias,
    [property: JsonProperty("data_anterior")]
    [property: JsonPropertyName("data_anterior")]
        string DataAnterior,
    [property: JsonProperty("fator_potencia")]
    [property: JsonPropertyName("fator_potencia")]
        object FatorPotencia
);

public record Leitura2(
    [property: JsonProperty("posto")][property: JsonPropertyName("posto")] string Posto,
    [property: JsonProperty("valor_atual")]
    [property: JsonPropertyName("valor_atual")]
        double? ValorAtual,
    [property: JsonProperty("valor_leitura")]
    [property: JsonPropertyName("valor_leitura")]
        double? ValorLeitura,
    [property: JsonProperty("valor_anterior")]
    [property: JsonPropertyName("valor_anterior")]
        double? ValorAnterior,
    [property: JsonProperty("ativa_ou_reativa")]
    [property: JsonPropertyName("ativa_ou_reativa")]
        string AtivaOuReativa,
    [property: JsonProperty("criterio_medicao")]
    [property: JsonPropertyName("criterio_medicao")]
        string CriterioMedicao,
    [property: JsonProperty("consumo_ou_geracao")]
    [property: JsonPropertyName("consumo_ou_geracao")]
        string ConsumoOuGeracao,
    [property: JsonProperty("energia_ou_demanda")]
    [property: JsonPropertyName("energia_ou_demanda")]
        string EnergiaOuDemanda,
    [property: JsonProperty("fator_multiplicador")]
    [property: JsonPropertyName("fator_multiplicador")]
        double? FatorMultiplicador
);

public record Medidore(
    [property: JsonProperty("leituras")]
    [property: JsonPropertyName("leituras")]
        IReadOnlyList<Leitura> Leituras,
    [property: JsonProperty("numero_medidor")]
    [property: JsonPropertyName("numero_medidor")]
        string NumeroMedidor,
    [property: JsonProperty("taxa_perda_transformacao")]
    [property: JsonPropertyName("taxa_perda_transformacao")]
        double? TaxaPerdaTransformacao
);

public record Outros(
    [property: JsonProperty("fisco")][property: JsonPropertyName("fisco")] string Fisco,
    [property: JsonProperty("aviso_corte")]
    [property: JsonPropertyName("aviso_corte")]
        bool? AvisoCorte,
    [property: JsonProperty("nota_fiscal")]
    [property: JsonPropertyName("nota_fiscal")]
        string NotaFiscal,
    [property: JsonProperty("codigo_barras")]
    [property: JsonPropertyName("codigo_barras")]
        string CodigoBarras,
    [property: JsonProperty("classe_consumo")]
    [property: JsonPropertyName("classe_consumo")]
        string ClasseConsumo,
    [property: JsonProperty("numero_cliente")]
    [property: JsonPropertyName("numero_cliente")]
        string NumeroCliente,
    [property: JsonProperty("possui_debitos")]
    [property: JsonPropertyName("possui_debitos")]
        bool? PossuiDebitos,
    [property: JsonProperty("debito_automatico")]
    [property: JsonPropertyName("debito_automatico")]
        bool? DebitoAutomatico,
    [property: JsonProperty("serie_nota_fiscal")]
    [property: JsonPropertyName("serie_nota_fiscal")]
        string SerieNotaFiscal,
    [property: JsonProperty("cfop")][property: JsonPropertyName("cfop")] string Cfop,
    [property: JsonProperty("roteiro_leitura")]
    [property: JsonPropertyName("roteiro_leitura")]
        string RoteiroLeitura
);

public record Produto(
    [property: JsonProperty("tributos")]
    [property: JsonPropertyName("tributos")]
        IReadOnlyList<Tributo> Tributos,
    [property: JsonProperty("descricao")]
    [property: JsonPropertyName("descricao")]
        string Descricao,
    [property: JsonProperty("quantidade")]
    [property: JsonPropertyName("quantidade")]
        double? Quantidade,
    [property: JsonProperty("valor_total")]
    [property: JsonPropertyName("valor_total")]
        double? ValorTotal,
    [property: JsonProperty("tarifa_com_impostos")]
    [property: JsonPropertyName("tarifa_com_impostos")]
        double? TarifaComImpostos,
    [property: JsonProperty("tarifa_sem_impostos")]
    [property: JsonPropertyName("tarifa_sem_impostos")]
        double? TarifaSemImpostos,
    [property: JsonProperty("descricoes_originais")]
    [property: JsonPropertyName("descricoes_originais")]
        IReadOnlyList<string> DescricoesOriginais,
    [property: JsonProperty("valor_sem_impostos")]
    [property: JsonPropertyName("valor_sem_impostos")]
        double? ValorSemImpostos,
    [property: JsonProperty("taxa_desconto")]
    [property: JsonPropertyName("taxa_desconto")]
        object TaxaDesconto
);

public record Root(
    [property: JsonProperty("status")][property: JsonPropertyName("status")] string Status,
    [property: JsonProperty("mensagem")][property: JsonPropertyName("mensagem")] string Mensagem,
    [property: JsonProperty("dados")]
    [property: JsonPropertyName("dados")]
        List<Dado> Dados
)
{ }

public record SaldosGeracao(
    [property: JsonProperty("posto")][property: JsonPropertyName("posto")] string Posto,
    [property: JsonProperty("valor")][property: JsonPropertyName("valor")] double? Valor,
    [property: JsonProperty("saldo_recebido")]
    [property: JsonPropertyName("saldo_recebido")]
        double? SaldoRecebido,
    [property: JsonProperty("saldo_utilizado")]
    [property: JsonPropertyName("saldo_utilizado")]
        object SaldoUtilizado
);

public record Tributo(
    [property: JsonProperty("nome")][property: JsonPropertyName("nome")] string Nome,
    [property: JsonProperty("valor")][property: JsonPropertyName("valor")] double? Valor,
    [property: JsonProperty("taxa")][property: JsonPropertyName("taxa")] double? Taxa,
    [property: JsonProperty("base_calculo")]
    [property: JsonPropertyName("base_calculo")]
        double? BaseCalculo
);

public record UnidadeConsumidora(
    [property: JsonProperty("nome")][property: JsonPropertyName("nome")] string Nome,
    [property: JsonProperty("cpf_cnpj")][property: JsonPropertyName("cpf_cnpj")] string CpfCnpj,
    [property: JsonProperty("endereco")][property: JsonPropertyName("endereco")] string Endereco,
    [property: JsonProperty("subgrupo")][property: JsonPropertyName("subgrupo")] string Subgrupo,
    [property: JsonProperty("instalacao")]
    [property: JsonPropertyName("instalacao")]
        string Instalacao,
    [property: JsonProperty("tipo_ligacao")]
    [property: JsonPropertyName("tipo_ligacao")]
        string TipoLigacao,
    [property: JsonProperty("limite_tensao")]
    [property: JsonPropertyName("limite_tensao")]
        string LimiteTensao,
    [property: JsonProperty("tipo_contrato")]
    [property: JsonPropertyName("tipo_contrato")]
        string TipoContrato,
    [property: JsonProperty("tensao_nominal")]
    [property: JsonPropertyName("tensao_nominal")]
        string TensaoNominal,
    [property: JsonProperty("categoria_tensao")]
    [property: JsonPropertyName("categoria_tensao")]
        string CategoriaTensao,
    [property: JsonProperty("modalidade_tarifaria")]
    [property: JsonPropertyName("modalidade_tarifaria")]
        string ModalidadeTarifaria,
    [property: JsonProperty("inscricao_estadual")]
    [property: JsonPropertyName("inscricao_estadual")]
        string InscricaoEstadual
);
