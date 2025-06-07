using System.Text.Json.Serialization;

namespace BikeHistory.Server.Models.Enums
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