namespace AerolineaVIP.Models
{
    public static class RolesSistema
    {
        public const string SuperUsuario = "SuperUsuario";
        public const string Administrador = "Administrador";
        public const string EmpleadoCounter = "EmpleadoCounter";
        public const string Cliente = "Cliente";

        // Combinaciones para [Authorize(Roles = ...)]
        public const string AdminYSuper = Administrador + "," + SuperUsuario;
        public const string ClienteCounterYSuper = Cliente + "," + EmpleadoCounter + "," + SuperUsuario;

        public static readonly string[] Todos = { SuperUsuario, Administrador, EmpleadoCounter, Cliente };
    }
}
