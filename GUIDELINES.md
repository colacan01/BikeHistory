# 자전거 소유권 증명 앱 개발 지침

1. 프로젝트 개요
본 프로젝트는 자전거의 소유권을 증명하고 관리하는 웹 애플리케이션으로, 다음과 같은 특징을 가집니다:
•	백엔드: .NET 8 WebAPI
•	프론트엔드: Angular
•	데이터 교환: JSON 형식
2. 아키텍처 개요
2.1. 백엔드 (.NET 8 WebAPI)
•	모델(Models): 데이터 엔티티 정의
•	컨트롤러(Controllers): API 엔드포인트 구현
•	서비스(Services): 비즈니스 로직 처리
•	인증 및 권한 관리: JWT 기반 인증
2.2. 프론트엔드 (Angular)
•	컴포넌트(Components): UI 요소 구현
•	서비스(Services): HTTP 요청 및 데이터 관리
•	모델(Models): 데이터 인터페이스 정의
•	라우팅(Routing): 페이지 내비게이션
3. 명명 규칙
3.1. 공통 규칙
•	백엔드와 프론트엔드 사이의 JSON 속성명은 동일하게 유지
•	예: frameNumber, currentOwnerId
3.2. 백엔드 (.NET)
•	클래스: PascalCase 사용
•	예: BikeFrame, OwnershipRecord
•	프로퍼티: PascalCase 사용
•	예: FrameNumber, CurrentOwnerId
•	메서드: PascalCase 사용
•	예: GetBikeFrameById, TransferOwnership
•	JSON 직렬화: JsonPropertyName 어트리뷰트를 사용하여 camelCase로 변환
•	예: [JsonPropertyName("frameNumber")]
3.3. 프론트엔드 (TypeScript/Angular)
•	인터페이스: PascalCase 사용, 'I' 접두사 없음
•	예: BikeFrame, OwnershipRecord
•	변수/프로퍼티: camelCase 사용
•	예: frameNumber, currentOwnerId
•	메서드: camelCase 사용
•	예: getBikeFrameById(), transferOwnership()
•	컴포넌트: kebab-case 파일명, PascalCase 클래스명
•	예: bike-detail.component.ts → BikeDetailComponent
4. 데이터 모델 구조
4.1. 주요 모델
BikeFrame (자전거 프레임)
- id: 자전거 식별자
- frameNumber: 자전거 프레임 번호
- manufacturerId/manufacturer: 제조사
- brandId/brand: 브랜드
- bikeTypeId/bikeType: 자전거 유형
- model: 모델명
- manufactureYear: 제조연도
- color: 색상
- currentOwnerId/currentOwner: 현재 소유자
- registeredDate: 등록일
ownershipRecords: 소유권 기록 목록
- id: 기록 식별자
- bikeFrameId/bikeFrame: 자전거 정보
- previousOwnerId/previousOwner: 이전 소유자
- newOwnerId/newOwner: 새 소유자
- transferDate: 이전 일자
- notes: 비고
ApplicationUser (사용자)
- id: 사용자 식별자
- email: 이메일
- firstName: 이름
- lastName: 성
- roles: 역할(User/Admin/Store) 
5. API 엔드포인트 가이드라인
5.1. 명명 규칙
•	리소스 이름은 복수형 사용
•	예: /api/BikeFrames, /api/OwnershipRecords
•	HTTP 메서드로 작업 유형 표현
•	GET: 조회
•	POST: 생성
•	PUT: 전체 업데이트
•	PATCH: 부분 업데이트
•	DELETE: 삭제
5.2. 주요 엔드포인트
자전거 관리
•	GET /api/BikeFrames: 자전거 목록 조회
•	GET /api/BikeFrames/{id}: 특정 자전거 조회
•	POST /api/BikeFrames: 새 자전거 등록
•	PUT /api/BikeFrames/{id}: 자전거 정보 업데이트
소유권 관리
•	GET /api/BikeFrames/{id}/OwnershipRecords: 소유권 이력 조회
•	POST /api/BikeFrames/{id}/Transfer: 소유권 이전
6. 권한 관리
6.1. 사용자 역할
•	User: 일반 사용자 (자전거 소유자)
•	Store: 정비소 (자전거 정비 기록 관리)
•	Admin: 관리자 (모든 권한)
6.2. 접근 제어
•	자전거 정보는 소유자와 관리자만 접근 가능
•	정비 기록은 소유자, 정비소, 관리자가 접근 가능
•	소유권 이전은 현재 소유자와 관리자만 가능
7. 코드 작성 가이드라인
7.1. 백엔드
•	컨트롤러:
•	경량화된 컨트롤러 지향 (비즈니스 로직은 서비스에 위임)
•	적절한 HTTP 상태 코드 반환
•	모델 유효성 검사 활용
•	서비스:
•	의존성 주입 활용
•	비동기 방식(async/await) 사용
•	예외 처리 철저히 구현
7.2. 프론트엔드
•	컴포넌트:
•	단일 책임 원칙 준수
•	재사용 가능한 컴포넌트 설계
•	반응형 디자인 적용
•	서비스:
•	HTTP 요청 캡슐화
•	에러 처리 통합
•	Observable 패턴 활용
8. 에러 처리 및 로깅
8.1. 백엔드
•	구조화된 에러 응답 제공
•	적절한 로깅 레벨 사용
•	민감한 정보 노출 방지
8.2. 프론트엔드
•	사용자 친화적 에러 메시지 표시
•	개발자용 상세 로그는 콘솔에만 출력
•	ActivityLoggerService 활용하여 중요 사용자 활동 기록
9. 성능 최적화
•	불필요한 데이터 로딩 지양
•	페이지네이션 적용
•	적절한 인덱싱 사용
•	필요한 경우 데이터 캐싱 고려
10. 보안 고려사항
•	JWT 토큰 안전하게 관리
•	사용자 입력 데이터 검증
•	HTTPS 사용 강제
•	비밀번호 해싱 적용
•	CORS 정책 적절히 설정