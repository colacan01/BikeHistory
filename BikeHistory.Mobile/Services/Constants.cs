namespace BikeHistory.Mobile.Services
{
    public static class Constants
    {
        // API 기본 URL - 실제 서버 주소로 변경 필요
        //public static string BaseApiUrl = DeviceInfo.Platform == DevicePlatform.Android
        //    ? "https://10.0.2.2:7213/api" // Android 에뮬레이터 접근용
        //    : "https://localhost:7213/api"; // 일반 로컬 접근용
            
        public static string BaseApiUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "https://localhost:5128/api" // Android 에뮬레이터 접근용
            : "https://localhost:5128/api"; // 일반 로컬 접근용

        // 토큰 저장 키
        public static string AuthTokenKey = "auth_token";
    }
}