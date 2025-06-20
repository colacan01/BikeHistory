using System.Text.Json.Serialization;

namespace BikeHistory.Mobile.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PaymentMethod
    {
        Cash,           // ����
        LocalCurrency,  // ����ȭ��
        CreditCard,     // �ſ�ī��
        BankTransfer,   // ������ü
        Other           // ��Ÿ
    }
}