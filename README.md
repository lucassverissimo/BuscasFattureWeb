> 1 - criar o arquivo appsettings dentro do diretório "TestesFWConsoleApplication\FattureWebAuxiliar" com informação de login e senha do fattureweb.
```json
{
  "Settings": {
    "TipoConta": "",
    "UsuarioProducao": "",
    "SenhaProducao": "",
    "UsuarioDev": "",
    "SenhaDev": "",
    "UsuarioQa": "",
    "SenhaQa": ""
  }
}
```

> 2 - Nas propriedades do arquivo appsettings setar o atributo "Copy to Output Directory" para "Copy always"
