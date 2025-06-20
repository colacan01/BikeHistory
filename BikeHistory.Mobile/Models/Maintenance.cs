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

        // UI 표시용 프로퍼티
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
                MaintenanceType.Maintenance => "정비",
                MaintenanceType.Repair => "수리",
                MaintenanceType.Custom => "커스텀",
                MaintenanceType.Self => "자가 수리",
                _ => type.ToString()
            };
        }

        private string GetPaymentMethodName(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "현금",
                PaymentMethod.LocalCurrency => "지역화폐",
                PaymentMethod.CreditCard => "신용카드",
                PaymentMethod.BankTransfer => "계좌이체",
                PaymentMethod.Other => "기타",
                _ => method.ToString()
            };
        }
    }
}