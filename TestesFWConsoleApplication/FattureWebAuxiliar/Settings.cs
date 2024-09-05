namespace FattureWebAuxiliar
{
    public sealed class Settings
    {
        public required string UsuarioProducao { get; set; }
        public required string SenhaProducao { get; set; }
        public required string UsuarioDev { get; set; }
        public required string SenhaDev { get; set; }
        public required string UsuarioQa { get; set; }
        public required string SenhaQa { get; set; }
        public TipoContaEnum TipoConta { get; set; }

        public string getUsuario()
        {
            switch (TipoConta)
            {
                case TipoContaEnum.Prod:
                    return UsuarioProducao;
                case TipoContaEnum.Dev:
                    return UsuarioDev;
                case TipoContaEnum.Qa:
                    return UsuarioQa;
                default:
                    return UsuarioDev;

            }
        }

        public string getSenha()
        {
            switch (TipoConta)
            {
                case TipoContaEnum.Prod:
                    return SenhaProducao;
                case TipoContaEnum.Dev:
                    return SenhaDev;
                case TipoContaEnum.Qa:
                    return SenhaQa;
                default:
                    return SenhaDev;

            }
        }
    }
}
