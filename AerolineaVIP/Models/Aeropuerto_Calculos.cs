namespace AerolineaVIP.Models
{
    public partial class Aeropuerto
    {
        public string Descripcion
        {
            get { return Localidad + " (" + CodigoIATA + ")"; }
        }
    }
}
