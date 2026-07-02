using BLL.DTOs.Authentication;
using BLL.DTOs.Common;
using BLL.Managers.Authentication;
using BLL.Managers.CloudinaryManager;
using BLL.Managers.EmailService;
using BLL.Settings;
using DAL.Models;
using DAL.Models.Enums;
using DAL.Models.ExceptionModels;
using DAL.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BLL.Managers.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IEmailService _emailService;
        private readonly JwtSettings _jwtSettings;
        private readonly GeneralSettings _generalSettings;

        public AuthService(
            UserManager<User> userManager,
            IUnitOfWork unitOfWork,
            ICloudinaryService cloudinaryService,
            IEmailService emailService,
            IOptions<JwtSettings> jwtSettings,
            IOptions<GeneralSettings> generalSettings)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _emailService = emailService;
            _jwtSettings = jwtSettings.Value;
            _generalSettings = generalSettings.Value;
        }

        public async Task<BaseResponse> RegisterFactoryAsync(FayedRegisterFactoryRequest request)
        {
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
                throw new EmailAlreadyExistsException();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var user = new User
                {
                    UserName = request.Email.Contains('@') ? request.Email.Split('@')[0] : request.Email,
                    Email = request.Email,
                    Name = request.Name,
                    NationalId = request.NationalId,
                    VerificationStatus = VerificationStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errorMsg = result.Errors.Select(e => e.Description).ToList();
                    throw new IdentityCreationFailedException(errorMsg);
                }

                var roleResult = await _userManager.AddToRoleAsync(user, "Factory");
                if (!roleResult.Succeeded)
                {
                    var errorMsg = roleResult.Errors.Select(e => e.Description).ToList();
                    throw new IdentityCreationFailedException(errorMsg);
                }

                var factory = new Factory
                {
                    UserId = user.Id,
                    LegalName = request.FactoryName,
                    Address = request.Address,
                    Sector = request.Sector,
                    CommercialRegistryNo = request.CommercialRegistryNo,
                    TaxCardNo = request.TaxCardNo,
                    VerificationStatus = VerificationStatus.Pending
                };

                await _unitOfWork.Repository<Factory>().AddAsync(factory);
                await _unitOfWork.SaveChangesAsync();

                var commercialRegistryTask = _cloudinaryService.UploadFileAsync(request.CommercialRegistryFile, "Documents");
                var taxCardTask = _cloudinaryService.UploadFileAsync(request.TaxCardFile, "Documents");
                var nationalIdTask = _cloudinaryService.UploadFileAsync(request.NationalIdFile, "Documents");
                var selfieWithIdTask = _cloudinaryService.UploadFileAsync(request.SelfieWithIdFile, "Documents");

                await Task.WhenAll(commercialRegistryTask, taxCardTask, nationalIdTask, selfieWithIdTask);

                var documents = new List<Document>
                {
                    new Document { FactoryId = factory.Id, DocumentType = "CommercialRegistry", FileUrl = await commercialRegistryTask },
                    new Document { FactoryId = factory.Id, DocumentType = "TaxCard", FileUrl = await taxCardTask },
                    new Document { FactoryId = factory.Id, DocumentType = "NationalId", FileUrl = await nationalIdTask },
                    new Document { FactoryId = factory.Id, DocumentType = "SelfieWithId", FileUrl = await selfieWithIdTask }
                };

                await _unitOfWork.Repository<Document>().AddRangeAsync(documents);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                await SendConfirmationEmailAsync(request, user);

                return BaseResponse.Success("تم تسجيل طلب المصنع بنجاح، يرجى مراجعة بريدك الإلكتروني لتفعيل الحساب.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (ex is BaseBusinessException)
                    throw;

                throw new Exception("حدث خطأ غير متوقع أثناء التسجيل", ex);
            }
        }

        public async Task<BaseResponse> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new UserNotFoundException();

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                throw new InvalidTokenException();

            return BaseResponse.Success("تم تأكيد بريدك الإلكتروني بنجاح! يمكنك الآن تسجيل الدخول.");
        }

        public async Task<BaseResponse<LoginResponseDto>> LoginAsync(FayedLoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            // Return the same error for "unknown email" and "wrong password" to avoid
            // leaking which emails are registered (user enumeration).
            if (user == null)
                throw new InvalidLoginException();

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                throw new InvalidLoginException();

            if (!user.EmailConfirmed)
                throw new EmailIsNotConfirmedException();
            var userRoles = await _userManager.GetRolesAsync(user);

            var factory = await _unitOfWork.Repository<Factory>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.UserId == user.Id);

            var token = GenerateJwtToken(user, userRoles, factory);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var response = new LoginResponseDto
            {
                Token = tokenString,
                ExpiresOn = token.ValidTo,
                UserName = user.Name,
                Email = user.Email!
            };

            return BaseResponse<LoginResponseDto>.Success(response, "تم تسجيل الدخول بنجاح.");
        }

        public async Task<BaseResponse> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new UserNotFoundException();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetPasswordLink = $"{_generalSettings.FrontendUrl}/reset-password?userEmail={user.Email}&token={Uri.EscapeDataString(token)}";

            string subject = "تعيين كلمة مرور جديدة - منصة فايد";
            await SendResetPasswordEmailAsync(user, resetPasswordLink, subject);

            return BaseResponse.Success("تم إرسال رابط إعادة تعيين كلمة المرور إلى بريدك الإلكتروني بنجاح.");
        }

        public async Task<BaseResponse> ResetPasswordAsync(FayedResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new UserNotFoundException();

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                var firstError = result.Errors.Select(e => e.Description).FirstOrDefault();
                return BaseResponse.Failure($"فشل إعادة تعيين كلمة المرور", new List<string> { firstError! });
            }

            return BaseResponse.Success("تمت إعادة تعيين كلمة المرور بنجاح. يمكنك تسجيل الدخول الآن.");
        }

        #region Helper Methods (Private)

        private JwtSecurityToken GenerateJwtToken(User user, IList<string> roles, Factory? factory = null)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("FullName", user.Name),
                new Claim("FactoryId", factory?.Id.ToString() ?? string.Empty),
                new Claim("LogoUrl", factory?.LogoUrl ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: DateTime.UtcNow.AddDays(_jwtSettings.DurationInDays),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        private async Task SendConfirmationEmailAsync(FayedRegisterFactoryRequest request, User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{_generalSettings.FrontendUrl}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            var emailBody = $@"
                <div style='direction: rtl; font-family: Arial, sans-serif; text-align: right; padding: 20px;'>
                    <h2 style='color: #2b6cb0;'>مرحباً بك في منصة فايد، {user.Name}!</h2>
                    <p>يسعدنا انضمام مصنعك <strong>({request.FactoryName})</strong> إلينا.</p>
                    <p>يرجى الضغط على الزر أدناه لتأكيد بريدك الإلكتروني وتفعيل الحساب وسيقوم الإدمن بمراجعة أوراقك في أقرب وقت:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{confirmationLink}' style='background-color: #3182ce; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold;'>تأكيد الحساب</a>
                    </div>
                    <p>إذا لم تكن أنت من قام بهذا الطلب، يرجى إهمال هذا الإيميل.</p>
                    <hr style='border: 0; border-top: 1px solid #e2e8f0; margin-top: 40px;'>
                    <p style='font-size: 12px; color: #718096;'>منصة فايد - خطوتك نحو الاستقلالية والنمو.</p>
                </div>";

            await _emailService.SendEmailAsync(user.Email!, "تأكيد الحساب - منصة فايد", emailBody);
        }

        private async Task SendResetPasswordEmailAsync(User user, string resetPasswordLink, string subject)
        {
            var htmlMessage = $@"
            <div style='direction: rtl; font-family: tahoma; padding: 20px; border: 1px solid #eee;'>
                <h2>أهلاً بك في منصة فايد،</h2>
                <p>لقد تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بحسابك.</p>
                <p>يرجى الضغط على الرابط التالي لإتمام العملية (هذا الرابط صالحة لفترة محدودة):</p>
                <div style='margin: 30px 0;'>
                    <a href='{resetPasswordLink}' style='background-color: #28a745; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>إعادة تعيين كلمة المرور</a>
                </div>
                <p>إذا لم تكن أنت من طلب هذا، يمكنك تجاهل هذا الإيميل بأمان.</p>
            </div>";

            await _emailService.SendEmailAsync(user.Email!, subject, htmlMessage);
        }

        #endregion
    }
}
