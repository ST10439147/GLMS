namespace ST10439147_PRO7311_POE.Models
{
    public enum ClientRegion
    {
        EMEA,
        APAC,
        NorthAmerica,
        LatinAmerica,
        MiddleEast,
        SubSaharanAfrica,
        NorthAfrica,
        CentralAsia,
        SouthAsia,
        EastAsia,
        SoutheastAsia
    }

    public class Clients
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactDetails { get; set; } = string.Empty;
        public ClientRegion Region { get; set; }
    }
}
