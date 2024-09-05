//#pragma warning disable CS8602
#region usings
using FattureWebAuxiliar;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion
#region pré-inicialização
IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();


var settings = config.GetRequiredSection("Settings").Get<Settings>();
if (settings is null)
{
    escreverLog("Não foi possível carregar as variáveis de ambiente!", false, true);
    return;
}
escreverLog("Realizar login!");
var token = await realizarLoginAsync();
if (string.IsNullOrEmpty(token))
{
    escreverLog("Token não encontrado!!");
    return;
}
escreverLog($"Login realizado com sucesso! Token: {token}");
escreverLog($"Obter Faturas!");
var objeto = await GetFaturasPaginadoAsync();
var dados = objeto?.Dados.ToList();

if (dados == null)
{
    escreverLog("Faturas não encontradas.");
    return;
}
#endregion
#region programa


bool continuar = true;

while (continuar)
{
    Console.Clear();
    try
    {
        escreverLog(
            @$" 
BASE FattureWeb SELECIONADA: {settings.TipoConta.GetEnumDescription()} -> {settings.getUsuario()}                
Quantidade Faturas: {dados.Count}
0 - SAIR
R - Recarregar Lista
{getMenuBases()}          

1 - Criar csv das faturas
        ", false
        );
        string entrada = Console.ReadLine();

        if (entrada == "0")
        {
            continuar = false;
            break;
        }

        if (string.IsNullOrEmpty(entrada))
        {
            continue;
        }

        if (entrada == "R")
        {
            await carregarFaturas();
            continue;
        }

        if (entrada.Equals("P", StringComparison.OrdinalIgnoreCase))
        {
            if (settings.TipoConta == TipoContaEnum.Prod)
            {
                continue;
            }
            settings.TipoConta = TipoContaEnum.Prod;
            await trocarBase();
            continue;
        }

        if (entrada.Equals("D", StringComparison.OrdinalIgnoreCase))
        {
            if (settings.TipoConta == TipoContaEnum.Dev)
            {
                continue;
            }
            settings.TipoConta = TipoContaEnum.Dev;
            await trocarBase();
            continue;
        }

        if (entrada.Equals("Q", StringComparison.OrdinalIgnoreCase))
        {
            if (settings.TipoConta == TipoContaEnum.Qa)
            {
                continue;
            }
            settings.TipoConta = TipoContaEnum.Qa;
            await trocarBase();
            continue;
        }

        foreach (var opcao in entrada.Split(','))
        {
            pularLinha();
            escreverLog($" >>> OPÇÃO {opcao} <<< ", false);
            switch (opcao)
            {
                case "1":
                    criarArquivoCsvFaturas();
                    break;
                default:
                    escreverLog(" >>> OPÇÃO INVÁLIDA <<< ", false);

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
}
#endregion
#region Métodos
void criarArquivoCsvFaturas()
{
    string outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");

    if (!Directory.Exists(outputDir))
    {
        Directory.CreateDirectory(outputDir);
    }

    string timestamp = DateTime.Now.ToString("ddMMyyyyHHmmss");

    string fileName = $"faturas{settings.TipoConta.GetEnumDescription()}_{timestamp}.csv";
    string filePath = Path.Combine(outputDir, fileName);

    using (StreamWriter writer = new StreamWriter(filePath))
    {
        writer.WriteLine($"\"dado.Conteudo.FaturaId\",\"dado.Conteudo.UnidadeConsumidora.Instalacao\",\"dado.Conteudo.Fatura.MesReferencia\",\"dado.Conteudo.Distribuidora\",\"idInstalacao\",\"consumo_total\"");
        foreach (var dado in dados)
        {
            DateTime dataMesRef = DateTime.Parse(dado.Conteudo.Fatura.MesReferencia);
            var consumoTotal = dado.Conteudo.Fatura.HistoricoFaturamento != null ? dado.Conteudo.Fatura.HistoricoFaturamento.FirstOrDefault().EnergiaAtiva : 0;
            writer.WriteLine($"\"{dado.Conteudo.FaturaId}\",\"{dado.Conteudo.UnidadeConsumidora.Instalacao}\",\"{dataMesRef.ToString("MMyyyy")}\",\"{dado.Conteudo.Distribuidora}\",\"{dado.InstalacaoId}\",\"{consumoTotal}\"");
        }
    }

    escreverLog($"Dados foram escritos no arquivo: {filePath}", true, false);
}
async Task trocarBase()
{
    escreverLog(" AGUARDE... TROCANDO DE BASE... ", false);
    token = await realizarLoginAsync();
    await carregarFaturas();
}
async Task carregarFaturas()
{
    objeto = await GetFaturasPaginadoAsync();
    dados = objeto?.Dados.ToList();
    escreverLog("", false, true);
}
async Task<string?> realizarLoginAsync()
{
    var clientToken = new HttpClient();
    var requestToken = new HttpRequestMessage(HttpMethod.Post, Constants.URLFW_LOGIN);
    var jsonRequestToken = new JObject
    {
        { "email", settings.getUsuario() },
        { "senha", settings.getSenha() }
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
async Task<Root?> GetFaturasPaginadoAsync()
{
    Root retorno = new Root("", "", new List<Dado>());

    var clientFaturas = new HttpClient();
    clientFaturas.Timeout = Timeout.InfiniteTimeSpan;
    const int tamanhoMaximoPagina = 1000;
    int limit = tamanhoMaximoPagina;
    int skip = 0;
    bool hasMoreData = true;

    while (hasMoreData)
    {
        try
        {
            escreverLog($"Buscando faturas: {(limit + skip)}");
            string url = $"https://api.fattureweb.com.br/faturas?limit={limit}&skip={skip}";
            var requestFaturas = new HttpRequestMessage(HttpMethod.Get, url);
            requestFaturas.Headers.Add("Fatture-AuthToken", token);
            requestFaturas.Headers.Add(
                "Fatture-SearchFields",
                "id, instalacao_id, arquivo_id, status_fatura_id, status, data_criacao, data_atualizacao, processamento_id, usuario_id, email_fatura_id, email_fatura_assunto, data_processamento, erro_processamento, mes_referencia, data_vencimento, valor_total, conteudo"
            );

            var responseFaturas = await clientFaturas.SendAsync(requestFaturas);
            responseFaturas.EnsureSuccessStatusCode();

            var contentResponseFaturas = await responseFaturas.Content.ReadAsStringAsync();
            var retornoFw = JsonConvert.DeserializeObject<Root>(contentResponseFaturas);

            if (retornoFw != null && retornoFw.Dados.Any())
            {
                retorno.Dados.AddRange(retornoFw.Dados);
                skip += limit;
                if (retornoFw.Dados.Count < tamanhoMaximoPagina)
                {
                    hasMoreData = false;
                }
            }
            else
            {
                hasMoreData = false;
            }
        }
        catch
        {
            hasMoreData = false;
        }
    }

    return retorno;
}
string getMenuBases()
{
    var options = new Dictionary<TipoContaEnum, string>
    {
        { TipoContaEnum.Prod, "P - Mudar para PRODUÇÃO" },
        { TipoContaEnum.Dev, "D - Mudar para DEV" },
        { TipoContaEnum.Qa, "Q - Mudar para QA" }
    };

    return string.Join("\n", options
        .Where(opt => settings.TipoConta != opt.Key)
        .Select(opt => opt.Value));
}
void pularLinha(int qtd = 1)
{
    for (int i = 0; i < qtd; i++)
        escreverLog("\n", false);
}
void escreverLog(string msg, bool withDate = true, bool withReadKey = false)
{
    if (withDate)
    {
        Console.WriteLine($"{DateTime.Now}: {msg}");
    }
    else
    {
        Console.WriteLine($"{msg}");
    }

    if (withReadKey)
    {
        Console.WriteLine(" Pressione Qualquer Tecla ");
        Console.ReadKey(false);
    }
}
#endregion