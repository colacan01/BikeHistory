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

        // ����� �������� ��ȸ
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            // ���� ������ ������� ID ��������
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // ����� ���� ��ȸ
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // ������� ���� ��ȸ
            var roles = await _userManager.GetRolesAsync(user);

            // �������� ���� ��ȯ
            return Ok(new ProfileDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            });
        }

        // ����� �������� ����
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ���� ������ ������� ID ��������
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // ����� ���� ��ȸ
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // ����� ���� ������Ʈ
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            
            // ����� ���� ����
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // ��й�ȣ ������ ��û�� ���
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                // ���� ��й�ȣ Ȯ��
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    return BadRequest(new { message = "Current password is required" });
                }

                // ��й�ȣ ����
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    return BadRequest(changePasswordResult.Errors);
                }
            }

            // ������Ʈ�� ������� �� ��ū ����
            var token = await _jwtService.GenerateJwtToken(user);

            // ������Ʈ�� �������� ���� ��ȯ
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

        // ����� ��� ��ȸ (������ ����)
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            // ��� ����� ��ȸ
            var users = _userManager.Users.ToList();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                // �� ������� ���� ��ȸ
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

        // Ư�� ����� ��ȸ (������ ����)
        [HttpGet("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUser(string id)
        {
            // ����� ���� ��ȸ
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // ������� ���� ��ȸ
            var roles = await _userManager.GetRolesAsync(user);

            // ����� ���� ��ȯ
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

        // Ư�� ����� ���� ���� (������ ����)
        [HttpPut("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ����� ���� ��ȸ
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // ����� ���� ������Ʈ
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email; // �Ϲ������� �̸����� ����� �̸����� ���
            
            // ��� ���� ������Ʈ
            if (model.LockoutEnabled.HasValue)
            {
                user.LockoutEnabled = model.LockoutEnabled.Value;
            }

            if (model.LockoutEnd.HasValue)
            {
                user.LockoutEnd = model.LockoutEnd.Value;
            }

            // ����� ���� ����
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // ���� ������Ʈ (���� ���� ��� ���� �� ���ο� ���� �Ҵ�)
            if (model.Roles != null && model.Roles.Any())
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRolesAsync(user, model.Roles);
            }

            // ��й�ȣ ������ ��û�� ���
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                // ��й�ȣ ���� ��ū ����
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                // ��й�ȣ ����
                var changePasswordResult = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    return BadRequest(changePasswordResult.Errors);
                }
            }

            return Ok(new { message = "User updated successfully" });
        }

        // Ư�� ����� ���� (������ ����)
        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            // ����� ���� ��ȸ
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // ���� �α����� ������ ������ �����Ϸ��� ��� ����
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == currentUserId)
            {
                return BadRequest(new { message = "You cannot delete your own account" });
            }

            // ����� ����
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

        // ��й�ȣ ������ ���� �ʵ��(������)
        [JsonPropertyName("currentPassword")]
        public string? CurrentPassword { get; set; }

        [MinLength(6)]
        [JsonPropertyName("newPassword")]
        public string? NewPassword { get; set; }

        [Compare("NewPassword")]
        [JsonPropertyName("confirmNewPassword")]
        public string? ConfirmNewPassword { get; set; }
    }

    // ����� ��� ��ȸ�� DTO
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

    // ����� �� ���� ��ȸ�� DTO
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

    // ����� ���� ������Ʈ�� DTO
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