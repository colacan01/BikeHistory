using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BikeHistory.Server.Models
{
    /// <summary>
    /// ������ ���� �� ���� ������ �����ϴ� �� Ŭ����
    /// </summary>
    public class BikeServiceRecord
    {
        /// <summary>
        /// ���� ����� ���� �ĺ���
        /// </summary>
        [Key]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// ���� ���� ����� ��¥
        /// </summary>
        [Required]
        [JsonPropertyName("serviceDate")]
        public DateTime ServiceDate { get; set; }

        /// <summary>
        /// ���� ��� ������ �������� ID
        /// </summary>
        [Required]
        [JsonPropertyName("bikeFrameId")]
        public int BikeFrameId { get; set; }

        /// <summary>
        /// ���� ��� ������ ������
        /// </summary>
        [JsonPropertyName("bikeFrame")]
        public BikeFrame BikeFrame { get; set; } = null!;

        /// <summary>
        /// ���� ���� (����, �������� ��)
        /// </summary>
        [Required]
        [JsonPropertyName("serviceType")]
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// ���� ���� ����
        /// </summary>
        [Required]
        [JsonPropertyName("serviceDetails")]
        public string ServiceDetails { get; set; } = string.Empty;

        /// <summary>
        /// ���� ������ ����/���� ID
        /// </summary>
        [Required]
        [JsonPropertyName("serviceShopId")]
        public string ServiceShopId { get; set; } = string.Empty;

        /// <summary>
        /// ���� ������ ����/�� ����
        /// </summary>
        [JsonPropertyName("serviceShop")]
        public ApplicationUser ServiceShop { get; set; } = null!;

        /// <summary>
        /// ���� ���
        /// </summary>
        [JsonPropertyName("cost")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Cost { get; set; }

        /// <summary>
        /// ���� ��ǰ�� �� ����
        /// </summary>
        [JsonPropertyName("partDetails")]
        public string? PartDetails { get; set; }

        /// <summary>
        /// ���� ���� ����
        /// </summary>
        [JsonPropertyName("warrantyInfo")]
        public string? WarrantyInfo { get; set; }

        /// <summary>
        /// ���� ���� ����
        /// </summary>
        [JsonPropertyName("serviceStatus")]
        public ServiceStatus ServiceStatus { get; set; } = ServiceStatus.Completed;

        /// <summary>
        /// ���� ���� ������
        /// </summary>
        [JsonPropertyName("nextServiceDate")]
        public DateTime? NextServiceDate { get; set; }

        /// <summary>
        /// ���� ��� ������
        /// </summary>
        [Required]
        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ���� ��� ������ ������
        /// </summary>
        [JsonPropertyName("modifiedDate")]
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// ���� ���� �з�
    /// </summary>
    public enum ServiceType
    {
        /// <summary>
        /// ���� ��������
        /// </summary>
        Maintenance,
        
        /// <summary>
        /// ����
        /// </summary>
        Repair,
        
        /// <summary>
        /// ����
        /// </summary>
        Inspection,
        
        /// <summary>
        /// ���׷��̵�
        /// </summary>
        Upgrade,
        
        /// <summary>
        /// ��Ÿ ������ ����
        /// </summary>
        Custom
    }

    /// <summary>
    /// ���� ���� ����
    /// </summary>
    public enum ServiceStatus
    {
        /// <summary>
        /// �����
        /// </summary>
        Scheduled,
        
        /// <summary>
        /// ���� ��
        /// </summary>
        InProgress,
        
        /// <summary>
        /// �Ϸ��
        /// </summary>
        Completed,
        
        /// <summary>
        /// ��ҵ�
        /// </summary>
        Canceled
    }
}