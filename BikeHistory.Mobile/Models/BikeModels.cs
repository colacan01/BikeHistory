namespace BikeHistory.Mobile.Models
{
    public class BikeFrame
    {
        public int Id { get; set; }
        public string FrameNumber { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public Brand Brand { get; set; }
        public BikeType BikeType { get; set; }
        public string Model { get; set; }
        public int? ManufactureYear { get; set; }
        public string Color { get; set; }
        public string CurrentOwnerId { get; set; }
        public User CurrentOwner { get; set; }
        public DateTime RegisteredDate { get; set; }
    }

    public class BikeFrameRegisterRequest
    {
        public string FrameNumber { get; set; }
        public int ManufacturerId { get; set; }
        public int BrandId { get; set; }
        public int BikeTypeId { get; set; }
        public string Model { get; set; }
        public int? ManufactureYear { get; set; }
        public string Color { get; set; }
    }

    public class OwnershipTransferRequest
    {
        public string NewOwnerId { get; set; }
        public string Notes { get; set; }
    }

    public class OwnershipRecord
    {
        public int Id { get; set; }
        public int BikeFrameId { get; set; }
        public string PreviousOwnerId { get; set; }
        public User PreviousOwnerName { get; set; }
        public string NewOwnerId { get; set; }
        public User NewOwnerName { get; set; }
        public DateTime TransferDate { get; set; }
        public string Notes { get; set; }
    }
}