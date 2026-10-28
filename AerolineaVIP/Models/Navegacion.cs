namespace AerolineaVIP.Models
{
    public static class Navegacion
    {
        public static int AnioActual { get { return System.DateTime.Now.Year; } }
        public static string ClasePestana(string actual, string pestana)
        {
            return actual == pestana ? "nav-link active" : "nav-link";
        }
    }
}
