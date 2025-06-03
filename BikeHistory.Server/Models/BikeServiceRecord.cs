using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BikeHistory.Server.Models
{
    /// <summary>
    /// 자전거 정비 및 수리 내역을 관리하는 모델 클래스
    /// </summary>
    public class BikeServiceRecord
    {
        /// <summary>
        /// 정비 기록의 고유 식별자
        /// </summary>
        [Key]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// 실제 정비가 진행된 날짜
        /// </summary>
        [Required]
        [JsonPropertyName("serviceDate")]
        public DateTime ServiceDate { get; set; }

        /// <summary>
        /// 정비 대상 자전거 프레임의 ID
        /// </summary>
        [Required]
        [JsonPropertyName("bikeFrameId")]
        public int BikeFrameId { get; set; }

        /// <summary>
        /// 정비 대상 자전거 프레임
        /// </summary>
        [JsonPropertyName("bikeFrame")]
        public BikeFrame BikeFrame { get; set; } = null!;

        /// <summary>
        /// 정비 유형 (수리, 정기점검 등)
        /// </summary>
        [Required]
        [JsonPropertyName("serviceType")]
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// 정비 세부 내용
        /// </summary>
        [Required]
        [JsonPropertyName("serviceDetails")]
        public string ServiceDetails { get; set; } = string.Empty;

        /// <summary>
        /// 정비를 수행한 매장/샵의 ID
        /// </summary>
        [Required]
        [JsonPropertyName("serviceShopId")]
        public string ServiceShopId { get; set; } = string.Empty;

        /// <summary>
        /// 정비를 수행한 매장/샵 정보
        /// </summary>
        [JsonPropertyName("serviceShop")]
        public ApplicationUser ServiceShop { get; set; } = null!;

        /// <summary>
        /// 정비 비용
        /// </summary>
        [JsonPropertyName("cost")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Cost { get; set; }

        /// <summary>
        /// 사용된 부품의 상세 정보
        /// </summary>
        [JsonPropertyName("partDetails")]
        public string? PartDetails { get; set; }

        /// <summary>
        /// 보증 관련 정보
        /// </summary>
        [JsonPropertyName("warrantyInfo")]
        public string? WarrantyInfo { get; set; }

        /// <summary>
        /// 정비 진행 상태
        /// </summary>
        [JsonPropertyName("serviceStatus")]
        public ServiceStatus ServiceStatus { get; set; } = ServiceStatus.Completed;

        /// <summary>
        /// 다음 정비 권장일
        /// </summary>
        [JsonPropertyName("nextServiceDate")]
        public DateTime? NextServiceDate { get; set; }

        /// <summary>
        /// 정비 기록 생성일
        /// </summary>
        [Required]
        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 정비 기록 마지막 수정일
        /// </summary>
        [JsonPropertyName("modifiedDate")]
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// 정비 유형 분류
    /// </summary>
    public enum ServiceType
    {
        /// <summary>
        /// 정기 유지보수
        /// </summary>
        Maintenance,
        
        /// <summary>
        /// 수리
        /// </summary>
        Repair,
        
        /// <summary>
        /// 점검
        /// </summary>
        Inspection,
        
        /// <summary>
        /// 업그레이드
        /// </summary>
        Upgrade,
        
        /// <summary>
        /// 기타 맞춤형 서비스
        /// </summary>
        Custom
    }

    /// <summary>
    /// 정비 진행 상태
    /// </summary>
    public enum ServiceStatus
    {
        /// <summary>
        /// 예약됨
        /// </summary>
        Scheduled,
        
        /// <summary>
        /// 진행 중
        /// </summary>
        InProgress,
        
        /// <summary>
        /// 완료됨
        /// </summary>
        Completed,
        
        /// <summary>
        /// 취소됨
        /// </summary>
        Canceled
    }
}