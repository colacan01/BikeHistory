using BikeHistory.Server.Models;
using BikeHistory.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BikeHistory.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Add user to default role (User)
            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid email or password" });

            var token = await _jwtService.GenerateJwtToken(user);

            return Ok(new
            {
                token,
                userId = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName
            });
        }

        // 사용자 프로파일 조회
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            // 현재 인증된 사용자의 ID 가져오기
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // 사용자 정보 조회
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // 사용자의 역할 조회
            var roles = await _userManager.GetRolesAsync(user);

            // 프로파일 정보 반환
            return Ok(new ProfileDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            });
        }

        // 사용자 프로파일 수정
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 현재 인증된 사용자의 ID 가져오기
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // 사용자 정보 조회
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // 사용자 정보 업데이트
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            
            // 변경된 정보 저장
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // 비밀번호 변경이 요청된 경우
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                // 현재 비밀번호 확인
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    return BadRequest(new { message = "Current password is required" });
                }

                // 비밀번호 변경
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    return BadRequest(changePasswordResult.Errors);
                }
            }

            // 업데이트된 사용자의 새 토큰 생성
            var token = await _jwtService.GenerateJwtToken(user);

            // 업데이트된 프로파일 정보 반환
            return Ok(new
            {
                token,
                userId = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                message = "Profile updated successfully"
            });
        }

        // 사용자 목록 조회 (관리자 전용)
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            // 모든 사용자 조회
            var users = _userManager.Users.ToList();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                // 각 사용자의 역할 조회
                var roles = await _userManager.GetRolesAsync(user);

                userDtos.Add(new UserDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList(),
                    BikeCount = user.OwnedBikes.Count
                });
            }

            return Ok(userDtos);
        }

        // 특정 사용자 조회 (관리자 전용)
        [HttpGet("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUser(string id)
        {
            // 사용자 정보 조회
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // 사용자의 역할 조회
            var roles = await _userManager.GetRolesAsync(user);

            // 사용자 정보 반환
            return Ok(new UserDetailDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                BikeCount = user.OwnedBikes.Count,
                UserName = user.UserName,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd
            });
        }

        // 특정 사용자 정보 수정 (관리자 전용)
        [HttpPut("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 사용자 정보 조회
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // 사용자 정보 업데이트
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email; // 일반적으로 이메일을 사용자 이름으로 사용
            
            // 잠금 상태 업데이트
            if (model.LockoutEnabled.HasValue)
            {
                user.LockoutEnabled = model.LockoutEnabled.Value;
            }

            if (model.LockoutEnd.HasValue)
            {
                user.LockoutEnd = model.LockoutEnd.Value;
            }

            // 변경된 정보 저장
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // 역할 업데이트 (현재 역할 모두 제거 후 새로운 역할 할당)
            if (model.Roles != null && model.Roles.Any())
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRolesAsync(user, model.Roles);
            }

            // 비밀번호 변경이 요청된 경우
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                // 비밀번호 리셋 토큰 생성
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                // 비밀번호 변경
                var changePasswordResult = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    return BadRequest(changePasswordResult.Errors);
                }
            }

            return Ok(new { message = "User updated successfully" });
        }

        // 특정 사용자 삭제 (관리자 전용)
        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            // 사용자 정보 조회
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // 현재 로그인한 관리자 본인을 삭제하려는 경우 방지
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == currentUserId)
            {
                return BadRequest(new { message = "You cannot delete your own account" });
            }

            // 사용자 삭제
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "User deleted successfully" });
        }
    }

    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class ProfileDto
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;
        
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }
        
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }
        
        [JsonPropertyName("roles")]
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UpdateProfileDto
    {
        [Required]
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;

        // 비밀번호 변경을 위한 필드들(선택적)
        [JsonPropertyName("currentPassword")]
        public string? CurrentPassword { get; set; }

        [MinLength(6)]
        [JsonPropertyName("newPassword")]
        public string? NewPassword { get; set; }

        [Compare("NewPassword")]
        [JsonPropertyName("confirmNewPassword")]
        public string? ConfirmNewPassword { get; set; }
    }

    // 사용자 목록 조회용 DTO
    public class UserDto
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("roles")]
        public List<string> Roles { get; set; } = new List<string>();

        [JsonPropertyName("bikeCount")]
        public int BikeCount { get; set; }
    }

    // 사용자 상세 정보 조회용 DTO
    public class UserDetailDto : UserDto
    {
        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("emailConfirmed")]
        public bool EmailConfirmed { get; set; }
                
        [JsonPropertyName("lockoutEnabled")]
        public bool LockoutEnabled { get; set; }

        [JsonPropertyName("lockoutEnd")]
        public DateTimeOffset? LockoutEnd { get; set; }
    }

    // 사용자 정보 업데이트용 DTO
    public class UpdateUserDto
    {
        [Required]
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
                
        [JsonPropertyName("roles")]
        public List<string>? Roles { get; set; }

        [JsonPropertyName("lockoutEnabled")]
        public bool? LockoutEnabled { get; set; }

        [JsonPropertyName("lockoutEnd")]
        public DateTimeOffset? LockoutEnd { get; set; }

        [MinLength(6)]
        [JsonPropertyName("newPassword")]
        public string? NewPassword { get; set; }
    }
}