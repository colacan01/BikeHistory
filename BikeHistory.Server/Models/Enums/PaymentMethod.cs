using System.Text.Json.Serialization;

namespace BikeHistory.Server.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PaymentMethod
    {
        Cash,           // 현금
        LocalCurrency,  // 지역화폐
        CreditCard,     // 신용카드
        BankTransfer,   // 계좌이체
        Other           // 기타
    }
}