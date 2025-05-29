namespace BikeHistory.Mobile.Models
{
    public class Manufacturer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CountryOfOrigin { get; set; }
        public string Website { get; set; }
    }

    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ManufacturerId { get; set; }
        public Manufacturer Manufacturer { get; set; }
    }

    public class BikeType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}