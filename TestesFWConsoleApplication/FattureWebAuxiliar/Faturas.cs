﻿//#pragma warning disable CS8602
using System.ComponentModel.Design;
using System.Diagnostics.Metrics;
using System.Text.Json;
using FattureWebAuxiliar;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

const bool S = true;
const bool N = false;

bool _isprod = N;

// Build a config object, using env vars and JSON providers.
IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

// Get values from the config given their key and their target type.
var settings = config.GetRequiredSection("Settings").Get<Settings>();

var _LOGINPROD = settings?.UsuarioProducao;
var _SENHAPROD = settings?.SenhaProducao;
var _LOGINTESTE = settings?.UsuarioTeste;
var _SENHATESTE = settings?.SenhaTeste;

#region programa


var token = await realizarLoginAsync();
if (string.IsNullOrEmpty(token))
{
    Console.WriteLine("Token não encontrado!!");
    return;
}
var objeto = await getFaturasAsync(token);
var dados = objeto?.Dados.ToList();

if (dados == null)
{
    Console.WriteLine("Faturas não encontradas.");
    return;
}

bool continuar = true;

while (continuar)
{
    try
    {
        Console.WriteLine(
            @" 
            BASE FattureWeb SELECIONADA: "
                + (_isprod ? "PRODUÇÃO" : "TESTE")
                + @"
            0 - SAIR
            R - Recarregar Lista
            M - Mudar Base para "
                + (_isprod ? "TESTE" : "PRODUÇÃO")
                + @"
            
            1 - Mostrar Faturas MUC
            2 - Mostrar Todos os Produtos
            3 - Mostrar Modelos Faturas
            4 - Mostrar Saldo Acumulado Maior Que Zero
            5 - Mostrar Tributos negativos
            6 - Mostrar Saldos Em Meses
            7 - Alterar Instalação
        "
        );
        string entrada = Console.ReadLine();

        if (entrada == "0")
        {
            continuar = false;
            break;
        }

        if (string.IsNullOrEmpty(entrada))
        {
            Console.Clear();
            continue;
        }

        if (entrada == "R")
        {
            Console.WriteLine(" AGUARDE... Buscando faturas... ");
            objeto = await getFaturasAsync(token);
            dados = objeto?.Dados.ToList();
            Console.WriteLine(" Pressione Qualquer Tecla ");
            Console.ReadKey(false);
            Console.Clear();
            continue;
        }
        if (entrada == "M")
        {
            Console.WriteLine(" AGUARDE... TROCANDO DE BASE... ");
            _isprod = !_isprod;
            token = await realizarLoginAsync();
            objeto = await getFaturasAsync(token);
            dados = objeto?.Dados.ToList();
            Console.WriteLine(" Pressione Qualquer Tecla ");
            Console.ReadKey(false);
            Console.Clear();
            continue;
        }

        foreach (var opcao in entrada.Split(','))
        {
            pularLinha();
            Console.WriteLine($" >>> OPÇÃO {opcao} <<< ");
            switch (opcao)
            {
                case "1":
                    faturasMuc(dados);
                    break;
                case "2":
                    todosProdutos(dados);
                    break;
                case "3":
                    modelosFaturas(dados);
                    break;
                case "4":
                    SaldosAcumulados(dados);
                    break;
                case "5":
                    TributosNegativos(dados);
                    break;
                case "6":
                    await SaldoEmMeses(dados, verificaComPRod: true);
                    break;
                case "7":
                    await EditarInstalacao(dados);
                    break;
                default:
                    Console.WriteLine(" >>> OPÇÃO INVÁLIDA <<< ");

                    break;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERRO: {ex.Message}");
        break;
    }
    pularLinha(2);
    Console.WriteLine(" >>> Pressione Qualquer Tecla <<< ");
    Console.ReadKey(false);
    Console.Clear();
}

#endregion

#region Métodos
async Task<string?> realizarLoginAsync()
{
    var clientToken = new HttpClient();
    var requestToken = new HttpRequestMessage(HttpMethod.Post, Constants.URLFW_LOGIN);
    var jsonRequestToken = new JObject
    {
        { "email", _isprod ? _LOGINPROD : _LOGINTESTE },
        { "senha", _isprod ? _SENHAPROD : _SENHATESTE }
    };
    var contentToken = new StringContent(jsonRequestToken.ToString(), null, "application/json");
    requestToken.Content = contentToken;
    var response = await clientToken.SendAsync(requestToken);
    response.EnsureSuccessStatusCode();
    var responseBody = await response.Content.ReadAsStringAsync();
    var jsonResponse = JObject.Parse(responseBody);

    if (!jsonResponse.HasValues)
    {
        return default;
    }
    else
    {
        var dados = jsonResponse["dados"];
        if (dados != null && dados.Any())
        {
            var primeiroDado = dados.FirstOrDefault();
            if (primeiroDado != null)
            {
                var token = primeiroDado["token"];
                if (token != null)
                    return token.ToString();
            }
        }
        return default;
    }
}
async Task editarInstalacao(string token, InstalacaoEditDto instalacaoEdit)
{
    var client = new HttpClient();
    string urlEdit = $"https://api.fattureweb.com.br/instalacoes/{instalacaoEdit.Id}";
    var request = new HttpRequestMessage(HttpMethod.Put, urlEdit);
    request.Headers.Add("Fatture-AuthToken", token);
    var json = JsonConvert.SerializeObject(instalacaoEdit);
    var content = new StringContent(json, null, "application/json");
    request.Content = content;
    var response = await client.SendAsync(request);
    Console.WriteLine($"INSTALAÇÃO {instalacaoEdit.Id}");
    Console.WriteLine(await response.Content.ReadAsStringAsync());
    pularLinha();
}

static async Task<Root?> getFaturasAsync(string token)
{
    var clientFaturas = new HttpClient();
    var requestFaturas = new HttpRequestMessage(HttpMethod.Get, Constants.URLFW_FATURAS);
    requestFaturas.Headers.Add("Fatture-AuthToken", token);
    requestFaturas.Headers.Add(
        "Fatture-SearchFields",
        "id, instalacao_id, arquivo_id, status_fatura_id, status, data_criacao, data_atualizacao, processamento_id, usuario_id, email_fatura_id, email_fatura_assunto, data_processamento, erro_processamento, mes_referencia, data_vencimento, valor_total, conteudo"
    );
    requestFaturas.Headers.Add("sort", "nome");
    requestFaturas.Headers.Add("order", "asc");
    requestFaturas.Headers.Add("limit", "100");
    requestFaturas.Headers.Add("skip", "0");

    var contentFaturas = new StringContent("", null, "application/json");
    requestFaturas.Content = contentFaturas;
    var responseFaturas = await clientFaturas.SendAsync(requestFaturas);
    responseFaturas.EnsureSuccessStatusCode();

    //Console.WriteLine(await response.Content.ReadAsStringAsync());

    var contentResponseFaturas = await responseFaturas.Content.ReadAsStringAsync();

    return string.IsNullOrEmpty(contentResponseFaturas)
        ? null
        : JsonConvert.DeserializeObject<Root>(contentResponseFaturas); //JsonSerializer.Deserialize<Root>(contentResponseFaturas);
}

static async Task<RootInstalacao?> GetInstalacoesAsync(string token)
{
    var clientInstalacoes = new HttpClient();
    var requestInstalacoes = new HttpRequestMessage(
        HttpMethod.Get,
        "https://api.fattureweb.com.br/instalacoes?sort=distribuidora_sigla&order=asc&limit=9999"
    );
    requestInstalacoes.Headers.Add("Fatture-AuthToken", token);
    requestInstalacoes.Headers.Add(
        "Fatture-SearchFields",
        "id, distribuidora_id, cliente_id, entidade_id, instalacao_id, status_webcrawlers_id, erro_suporte, data_criacao, data_atualizacao, data_agendamento, data_inicio, data_fim, num_faturas_encontradas, num_faturas_baixadas,data_inicio_leitura"
    );
    var content = new StringContent("", null, "application/json");
    requestInstalacoes.Content = content;
    var responseInstalacoes = await clientInstalacoes.SendAsync(requestInstalacoes);
    responseInstalacoes.EnsureSuccessStatusCode();

    var contentResponseInstalacoes = await responseInstalacoes.Content.ReadAsStringAsync();
    if (!string.IsNullOrEmpty(contentResponseInstalacoes))
    {
        var dadosInstalacoes = JsonConvert.DeserializeObject<RootInstalacao>(
            contentResponseInstalacoes
        );

        return dadosInstalacoes;
    }
    return null;
}

static void todosProdutos(List<Dado> dados)
{
    Console.WriteLine(" ************************************************* ");
    Console.WriteLine(" *************** TODOS OS PRODUTOS *************** ");
    Console.WriteLine(" ************************************************* ");

    string produtos = string.Empty;
    foreach (var item in dados)
    {
        if (item.Conteudo != null)
        {
            if (item.Conteudo.Fatura != null)
            {
                if (
                    item.Conteudo.Fatura.Produtos != null
                    && item.Conteudo.Fatura.Produtos.Count > 0
                )
                {
                    foreach (var produto in item.Conteudo.Fatura.Produtos)
                    {
                        if (!string.IsNullOrEmpty(produto.Descricao))
                        {
                            if (!produtos.Contains(produto.Descricao))
                            {
                                Console.WriteLine($"case \"{produto.Descricao}\": ");
                                produtos += produto.Descricao;
                            }
                        }
                    }
                }
            }
        }
    }
}
static void faturasMuc(List<Dado> dados)
{
    Console.WriteLine(" ************************************************* ");
    Console.WriteLine(" **************** FATURAS COM MUC **************** ");
    Console.WriteLine(" ************************************************* ");

    string faturas = string.Empty;
    foreach (var item in dados)
    {
        if (item.Conteudo != null && item.Conteudo.UnidadeConsumidora.CategoriaTensao == "BT")
        {
            if (item.Conteudo.Fatura != null)
            {
                if (
                    item.Conteudo.Fatura.Produtos != null
                    && item.Conteudo.Fatura.Produtos.Count > 0
                )
                {
                    List<(string, string, string)> faturasComMuc =
                        new List<(string, string, string)>();
                    foreach (var produto in item.Conteudo.Fatura.Produtos)
                    {
                        if (
                            (
                                produto.Descricao.Equals(
                                    "Energia Injetada kWh",
                                    StringComparison.CurrentCultureIgnoreCase
                                )
                                || produto.Descricao.Equals(
                                    "Energia Injetada TE kWh",
                                    StringComparison.CurrentCultureIgnoreCase
                                )
                            ) && produto.DescricoesOriginais.ToList().Any(x => x.Contains("mUC"))
                        )
                        {
                            string descricaoMuc = produto
                                .DescricoesOriginais.ToList()
                                .Where(x => x.Contains("mUC"))
                                .FirstOrDefault();
                            faturasComMuc.Add(
                                (item.Id.ToString(), item.InstalacaoId.ToString(), descricaoMuc)
                            );
                        }
                    }

                    if (faturasComMuc.Count > 0)
                    {
                        faturasComMuc.ForEach(
                            x => Console.WriteLine($"Fatura: {x.Item1} | Descricao: {x.Item3}")
                        );
                    }
                }
            }
        }
    }
}
static void modelosFaturas(List<Dado> dados)
{
    Dado dadoComErro;
    try
    {
        Console.WriteLine(" ************************************************* ");
        Console.WriteLine(" **************** MODELOS FATURAS **************** ");
        Console.WriteLine(" ************************************************* ");

        var faturasModelo1 = new List<(string, string, string, string)>();
        var faturasModelo2 = new List<(string, string, string, string)>();
        var faturasModelo3 = new List<(string, string, string, string)>();

        foreach (var item in dados)
        {
            dadoComErro = item;
            if (item.Conteudo != null)
            {
                if (item.Conteudo.Fatura != null && item.Conteudo.Fatura.Produtos != null)
                {
                    var TemConsumoTeKwh = item.Conteudo.Fatura.Produtos.Any(
                        x =>
                            x.Descricao.Equals(
                                "Consumo TE KWh",
                                System.StringComparison.OrdinalIgnoreCase
                            )
                    );

                    var TemConsumoKwhComValorSemImpostos = item.Conteudo.Fatura.Produtos.Any(
                        x =>
                            x.Descricao.Equals("Consumo kWh", StringComparison.OrdinalIgnoreCase)
                            && x.ValorSemImpostos > 0
                    );

                    if (TemConsumoTeKwh)
                    {
                        faturasModelo1.Add(
                            (
                                item.Id.HasValue ? item.Id.Value.ToString() : "NONE",
                                item.Conteudo.UnidadeConsumidora.Instalacao,
                                item.Conteudo.UnidadeConsumidora.Nome,
                                item.Conteudo.Fatura.MesReferencia
                            )
                        );
                    }
                    else
                    {
                        if (TemConsumoKwhComValorSemImpostos)
                        {
                            faturasModelo2.Add(
                                (
                                    item.Id.HasValue ? item.Id.Value.ToString() : "NONE",
                                    item.Conteudo.UnidadeConsumidora.Instalacao,
                                    item.Conteudo.UnidadeConsumidora.Nome,
                                    item.Conteudo.Fatura.MesReferencia
                                )
                            );
                        }
                        else
                        {
                            faturasModelo3.Add(
                                (
                                    item.Id.HasValue ? item.Id.Value.ToString() : "NONE",
                                    item.Conteudo.UnidadeConsumidora.Instalacao,
                                    item.Conteudo.UnidadeConsumidora.Nome,
                                    item.Conteudo.Fatura.MesReferencia
                                )
                            );
                        }
                    }
                }
            }
        }

        Console.WriteLine(" ************************************************* ");
        Console.WriteLine("Modelo 1: " + faturasModelo1.Count);
        Console.WriteLine("Modelo 2: " + faturasModelo2.Count);
        Console.WriteLine("Modelo 3: " + faturasModelo3.Count);
        Console.WriteLine(" ************************************************* ");
        pularLinha();

        Console.WriteLine(" Quer exibir as faturas? S/N");
        string entrada = Console.ReadLine();

        if (
            !string.IsNullOrEmpty(entrada)
            && entrada.Equals("S", StringComparison.OrdinalIgnoreCase)
        )
        {
            Console.WriteLine(" ************************************************* ");
            Console.WriteLine(" *********************MODELO 1******************** ");
            Console.WriteLine(" ************************************************* ");
            faturasModelo1.ForEach(
                x =>
                    Console.WriteLine(
                        $"Fatura {x.Item1} instalação {x.Item2} mesRef: {x.Item4} - UC: {x.Item3}"
                    )
            );
            pularLinha();
            Console.WriteLine(" ************************************************* ");
            Console.WriteLine(" *********************MODELO 2******************** ");
            Console.WriteLine(" ************************************************* ");
            faturasModelo2.ForEach(
                x =>
                    Console.WriteLine(
                        $"Fatura {x.Item1} instalação {x.Item2} mesRef: {x.Item4} - UC: {x.Item3}"
                    )
            );
            pularLinha();
            Console.WriteLine(" ************************************************* ");
            Console.WriteLine(" *********************MODELO 3******************** ");
            Console.WriteLine(" ************************************************* ");
            faturasModelo3.ForEach(
                x =>
                    Console.WriteLine(
                        $"Fatura {x.Item1} instalação {x.Item2} mesRef: {x.Item4} - UC: {x.Item3}"
                    )
            );
            pularLinha();
        }
    }
    catch (Exception e)
    {
        Console.WriteLine($"Erro Fatal: {e.Message}");
    }
}

void SaldosAcumulados(List<Dado> dados)
{
    Console.WriteLine(" ************************************************* ");
    Console.WriteLine(" *************** SALDOS ACUMULADOS *************** ");
    Console.WriteLine(" ************************************************* ");
    var faturasComSaldo = new List<(string, string, string, string)>();
    foreach (var item in dados)
    {
        if (item.Conteudo != null)
        {
            if (
                item.Conteudo.Fatura != null
                && item.Conteudo.UnidadeConsumidora.CategoriaTensao == "BT"
            )
            {
                if (
                    item.Conteudo.Fatura.DevolucaoGeracao != null
                    && item.Conteudo.Fatura.DevolucaoGeracao.SaldosGeracao?.Count > 0
                    && !item.Conteudo.Fatura.Produtos.Any(
                        x =>
                            x.Descricao.Contains("PONTA", StringComparison.CurrentCultureIgnoreCase)
                    )
                )
                {
                    var saldos = item.Conteudo.Fatura.DevolucaoGeracao.SaldosGeracao;
                    var primeiroValor = saldos.First();
                    faturasComSaldo.Add(
                        (
                            (item.Id.HasValue ? item.Id.Value.ToString() : "NONE"),
                            (
                                primeiroValor.Valor.HasValue ? primeiroValor.Valor.Value : 0
                            ).ToString(),
                            saldos.Sum(x => (x.Valor ?? 0)).ToString(),
                            item.Conteudo.UnidadeConsumidora.Nome
                        )
                    );
                }
            }
        }
    }
    faturasComSaldo
        .Where(x => decimal.Parse(x.Item2) > 0)
        .ToList()
        .ForEach(
            x =>
                Console.WriteLine(
                    $"Fatura {x.Item1}: {x.Item4} - primeiro valor: {x.Item2} - count: {x.Item3}"
                )
        );
}

void TributosNegativos(List<Dado> dados)
{
    Console.WriteLine(" ************************************************* ");
    Console.WriteLine(" *************** TRIBUTOS NEGATIVOS *************** ");
    Console.WriteLine(" ************************************************* ");
    var faturasComTributosNegativos = new List<(string, string)>();
    foreach (var item in dados)
    {
        if (item.Conteudo != null)
        {
            if (
                item.Conteudo.Fatura != null
                && item.Conteudo.UnidadeConsumidora.CategoriaTensao == "BT"
            )
            {
                var tributos = item.Conteudo.Fatura.Tributos;
                if (
                    tributos != null
                    && tributos.Count > 0
                    && tributos.Where(x => x.Valor < 0).Any()
                )
                {
                    faturasComTributosNegativos.Add(
                        (
                            (item.Id.HasValue ? item.Id.Value.ToString() : "NONE"),
                            item.Conteudo.UnidadeConsumidora.Nome
                        )
                    );
                }
            }
        }
    }
    faturasComTributosNegativos
        .ToList()
        .ForEach(x => Console.WriteLine($"Fatura {x.Item1}: {x.Item2}"));
}

async Task SaldoEmMeses(List<Dado> dados, bool verificaComPRod)
{
    Console.WriteLine(" ************************************************************* ");
    Console.WriteLine(" *************** Saldo em Meses *************** ");
    Console.WriteLine(" ************************************************************* ");
    var faturasComMediaDeConsumoZerado =
        new List<(string, string, string, string, string, string, string, string)>();
    foreach (var item in dados)
    {
        if (item.Conteudo != null)
        {
            if (
                item.Conteudo.Fatura != null
                && item.Conteudo.UnidadeConsumidora.CategoriaTensao == "BT"
            )
            {
                var historico = item.Conteudo.Fatura.HistoricoFaturamento;
                if (
                    historico != null
                    && item.Conteudo.Fatura.DevolucaoGeracao != null
                    && item.Conteudo.Fatura.DevolucaoGeracao.SaldosGeracao != null
                )
                {
                    int mesesDisponíveis = historico.Count >= 12 ? 12 : historico.Count;

                    var historicoComEnergiaAtiva = historico
                        .ToList()
                        .GetRange(0, mesesDisponíveis)
                        .Where(x => x.EnergiaAtiva > 0)
                        .ToList();

                    var saldoAcumulado = (decimal)
                        item.Conteudo.Fatura.DevolucaoGeracao.SaldosGeracao.Sum(x => x.Valor ?? 0);

                    if (historicoComEnergiaAtiva.Count > 0 && saldoAcumulado > 0)
                    {
                        var hist = historicoComEnergiaAtiva.Average(x => x.EnergiaAtiva.Value);
                        faturasComMediaDeConsumoZerado.Add(
                            (
                                (item.Id.HasValue ? item.Id.Value.ToString() : "NONE"),
                                item.Conteudo.UnidadeConsumidora.Nome,
                                item.Conteudo.UnidadeConsumidora.Instalacao,
                                saldoAcumulado.ToString(),
                                hist.ToString(),
                                (saldoAcumulado / hist).ToFixed(2).ToString(), // SALDO EM MESES
                                item.Conteudo.Fatura.MesReferencia,
                                item.InstalacaoId.Value.ToString()
                            )
                        );
                    }
                }
            }
        }
    }
    faturasComMediaDeConsumoZerado
        .OrderBy(x => x.Item5)
        .ToList()
        .ForEach(
            x =>
                Console.WriteLine(
                    $"Fatura {x.Item1} | Instalação: {x.Item3} | MesReferencia: {x.Item7}| Saldo em meses: {x.Item5} | UC : {x.Item2}"
                )
        );

    if (verificaComPRod)
    {
        bool isProdAnterior = _isprod;
        _isprod = !_isprod;
        var tokenOutraBase = await realizarLoginAsync();
        var objetoOutraBase = await getFaturasAsync(tokenOutraBase);
        var dadosOutraBase = objetoOutraBase?.Dados.ToList();

        if (dadosOutraBase.Count > 0)
        {
            var ddd = faturasComMediaDeConsumoZerado
                .Where(x => !dadosOutraBase.Any(l => l.InstalacaoId.Value.ToString() == x.Item3))
                .ToList();
            if (ddd.Any())
            {
                ddd.OrderBy(x => x.Item3)
                    .ToList()
                    .ForEach(
                        x =>
                            Console.WriteLine(
                                $"Fatura: {x.Item1} | Instalacao : {x.Item3.PadRight(10, ' ')} | SALDO: {x.Item6} | UC : {x.Item2} "
                            )
                    );
            }
        }
        _isprod = isProdAnterior;
    }
}

async Task EditarInstalacao(List<Dado>? dados)
{
    Console.WriteLine(" ************************************************************* ");
    Console.WriteLine(" *************** Editar Instalação *************** ");
    Console.WriteLine(" ************************************************************* ");
    pularLinha();

    var instalacoes = await GetInstalacoesAsync(token);
    var da = instalacoes.Dados?.ToList();
    foreach (var inst in da)
    {
        if (!string.IsNullOrEmpty(inst.Id.ToString()))
        {
            var instalacao = new InstalacaoEditDto
            {
                Id = inst.Id.ToString(),
                DataInicioLeitura = "2023-11-03T00:00:00-03:00"
            };
            await editarInstalacao(token, instalacao);
        }
    }
}

static void pularLinha(int qtd = 1)
{
    for (int i = 0; i < qtd; i++)
        Console.WriteLine("\n");
}
#endregion
