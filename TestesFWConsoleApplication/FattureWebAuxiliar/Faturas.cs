//#pragma warning disable CS8602
using System.Diagnostics.Metrics;
using System.Text.Json;
using FattureWebAuxiliar;
using Microsoft.Extensions.Configuration;
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
                    SaldoEmMeses(dados);
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
        : JsonSerializer.Deserialize<Root>(contentResponseFaturas);
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
                            faturas += item.Id + ", ";
                            //break;
                        }
                    }
                }
            }
        }
    }

    Console.WriteLine(
        "Faturas com mUC: " + (string.IsNullOrEmpty(faturas) ? "NENHUMA FATURA" : faturas[..^2])
    );
}
static void modelosFaturas(List<Dado> dados)
{
    Console.WriteLine(" ************************************************* ");
    Console.WriteLine(" **************** MODELOS FATURAS **************** ");
    Console.WriteLine(" ************************************************* ");

    var faturasModelo1 = new List<(string, string)>();
    var faturasModelo2 = new List<(string, string)>();
    var faturasSemModelo = new List<(string, string)>();

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
                    if (
                        item.Conteudo.Fatura.Produtos.Any(
                            x =>
                                x.Descricao.Equals(
                                    "Consumo TE KWh",
                                    StringComparison.CurrentCultureIgnoreCase
                                )
                        )
                        && item.Conteudo.UnidadeConsumidora.CategoriaTensao == "BT"
                    )
                    {
                        faturasModelo1.Add(
                            (
                                item.Id.HasValue ? item.Id.Value.ToString() : "NONE",
                                item.Conteudo.UnidadeConsumidora.Nome
                            )
                        );
                    }
                    else if (
                        !item.Conteudo.Fatura.Produtos.Any(
                            x =>
                                x.Descricao.Equals(
                                    "Consumo TE KWh",
                                    StringComparison.CurrentCultureIgnoreCase
                                )
                        )
                        && item.Conteudo.UnidadeConsumidora.CategoriaTensao == "BT"
                    )
                    {
                        faturasModelo2.Add(
                            (
                                item.Id.HasValue ? item.Id.Value.ToString() : "NONE",
                                item.Conteudo.UnidadeConsumidora.Nome
                            )
                        );
                    }
                    else
                    {
                        faturasSemModelo.Add(
                            (
                                item.Id.HasValue ? item.Id.Value.ToString() : "NONE",
                                item.Conteudo.UnidadeConsumidora.Nome
                            )
                        );
                    }
                }
            }
        }
    }

    Console.WriteLine(
        "Produto IGUAL a 'Consumo TE KWh' e categoria tensao IGUAL a 'BT': " + faturasModelo1.Count
    );
    faturasModelo1.ForEach(x => Console.WriteLine($"Fatura {x.Item1} - UC: {x.Item2}"));
    Console.WriteLine(
        "Produto diferente de 'Consumo TE KWh' e categoria tensao IGUAL a 'BT': "
            + faturasModelo2.Count
    );
    faturasModelo2.ForEach(x => Console.WriteLine($"Fatura {x.Item1} - UC: {x.Item2}"));
    Console.WriteLine("Sem nenhuma das condições acima: " + faturasSemModelo.Count);

    faturasSemModelo.ForEach(x => Console.WriteLine($"Fatura {x.Item1} - UC: {x.Item2}"));
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

void SaldoEmMeses(List<Dado> dados)
{
    Console.WriteLine(" ************************************************************* ");
    Console.WriteLine(" *************** Saldo em Meses *************** ");
    Console.WriteLine(" ************************************************************* ");
    var faturasComMediaDeConsumoZerado = new List<(string, string, string, string, string)>();
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
                                saldoAcumulado.ToString(),
                                hist.ToString(),
                                (saldoAcumulado / hist).ToFixed(2).ToString() // SALDO EM MESES
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
            x => Console.WriteLine($"Fatura {x.Item1} | Saldo em meses: {x.Item5} | UC : {x.Item2}")
        );
}
static void pularLinha(int qtd = 1)
{
    for (int i = 0; i < qtd; i++)
        Console.WriteLine("\n");
}
#endregion
