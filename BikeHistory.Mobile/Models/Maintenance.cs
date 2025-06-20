using BikeHistory.Mobile.Models.Enums;
using System.Text.Json.Serialization;

namespace BikeHistory.Mobile.Models
{
    public class Maintenance
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("maintenanceDate")]
        public DateTime MaintenanceDate { get; set; }

        [JsonPropertyName("maintenanceType")]
        public MaintenanceType MaintenanceType { get; set; }

        [JsonPropertyName("storeId")]
        public string StoreId { get; set; } = string.Empty;

        [JsonPropertyName("store")]
        public User? Store { get; set; }

        [JsonPropertyName("ownerId")]
        public string OwnerId { get; set; } = string.Empty;

        [JsonPropertyName("owner")]
        public User? Owner { get; set; }

        [JsonPropertyName("bikeFrameId")]
        public int BikeFrameId { get; set; }

        [JsonPropertyName("bikeFrame")]
        public BikeFrame? BikeFrame { get; set; }

        [JsonPropertyName("paymentMethod")]
        public PaymentMethod PaymentMethod { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("maintenanceDetails")]
        public List<MaintenanceDetail>? MaintenanceDetails { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount => MaintenanceDetails?.Sum(d => d.LaborCost + d.PartPrice) ?? 0;

        // UI ǥ�ÿ� ������Ƽ
        [JsonIgnore]
        public string MaintenanceTypeName => GetMaintenanceTypeName(MaintenanceType);

        [JsonIgnore]
        public string PaymentMethodName => GetPaymentMethodName(PaymentMethod);

        [JsonIgnore]
        public string FormattedDate => MaintenanceDate.ToString("yyyy-MM-dd");

        [JsonIgnore]
        public string FormattedTotalAmount => TotalAmount.ToString("C0");

        [JsonIgnore]
        public string StoreName => Store != null ? $"{Store.FirstName} {Store.LastName}" : string.Empty;

        private string GetMaintenanceTypeName(MaintenanceType type)
        {
            return type switch
            {
                MaintenanceType.Maintenance => "����",
                MaintenanceType.Repair => "����",
                MaintenanceType.Custom => "Ŀ����",
                MaintenanceType.Self => "�ڰ� ����",
                _ => type.ToString()
            };
        }

        private string GetPaymentMethodName(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "����",
                PaymentMethod.LocalCurrency => "����ȭ��",
                PaymentMethod.CreditCard => "�ſ�ī��",
                PaymentMethod.BankTransfer => "������ü",
                PaymentMethod.Other => "��Ÿ",
                _ => method.ToString()
            };
        }
    }
}