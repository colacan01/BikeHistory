namespace BikeHistory.Mobile.Models
{
    public class BikeFrame
    {
        public int Id { get; set; }
        public required string FrameNumber { get; set; }
        public Manufacturer? Manufacturer { get; set; }
        public Brand? Brand { get; set; }
        public BikeType? BikeType { get; set; }
        public string? Model { get; set; }
        public int? ManufactureYear { get; set; }
        public string? Color { get; set; }
        public required string CurrentOwnerId { get; set; }
        public required User CurrentOwner { get; set; }
        public required DateTime RegisteredDate { get; set; }
    }

    public class BikeFrameRegisterRequest
    {
        public required string FrameNumber { get; set; }
        public int? ManufacturerId { get; set; }
        public int? BrandId { get; set; }
        public int? BikeTypeId { get; set; }
        public string? Model { get; set; }
        public int? ManufactureYear { get; set; }
        public string? Color { get; set; }
    }

    public class OwnershipTransferRequest
    {
        public required string NewOwnerId { get; set; }
        public string? Notes { get; set; }
    }

    public class OwnershipRecord
    {
        public required int Id { get; set; }
        public required int BikeFrameId { get; set; }
        public required string PreviousOwnerId { get; set; }
        public required User PreviousOwnerName { get; set; }
        public required string NewOwnerId { get; set; }
        public required User NewOwnerName { get; set; }
        public required DateTime TransferDate { get; set; }
        public string? Notes { get; set; }
    }
}