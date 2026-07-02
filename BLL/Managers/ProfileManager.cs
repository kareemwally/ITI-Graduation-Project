using BLL.DTOs.Common;
using BLL.DTOs.Profile;
using BLL.Managers.CloudinaryManager;
using DAL.Models;
using DAL.Models.Enums;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BLL.Managers
{
    public class ProfileManager : IProfileManager
    {
        private readonly IUnitOfWork _uow;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly INotificationService _notificationService;

        public ProfileManager(
            IUnitOfWork uow,
            ICloudinaryService cloudinaryService,
            INotificationService notificationService)
        {
            _uow = uow;
            _cloudinaryService = cloudinaryService;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse<ProfileDto>> GetProfileAsync(int currentUserId)
        {
            var user = await _uow.Repository<User>().Query()
                .AsNoTracking()
                .Include(u => u.Factory)
                    .ThenInclude(f => f!.Documents)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (user == null)
                return BaseResponse<ProfileDto>.Failure("المستخدم غير موجود.");

            var dto = MapToProfileDto(user);
            return BaseResponse<ProfileDto>.Success(dto, "تم جلب الملف الشخصي.");
        }

        public async Task<BaseResponse<ProfileDto>> UpdateProfileAsync(int currentUserId, UpdateProfileDto dto)
        {
            var user = await _uow.Repository<User>().Query()
                .Include(u => u.Factory)
                    .ThenInclude(f => f!.Documents)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (user == null)
                return BaseResponse<ProfileDto>.Failure("المستخدم غير موجود.");

            // ─── Update simple fields (no re-verification needed) ───
            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            if (user.Factory != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.Address))
                    user.Factory.Address = dto.Address;

                if (!string.IsNullOrWhiteSpace(dto.Sector))
                    user.Factory.Sector = dto.Sector;

                if (dto.LogoFile != null)
                {
                    var logoUrl = await _cloudinaryService.UploadFileAsync(dto.LogoFile, "Logos");
                    if (!string.IsNullOrEmpty(logoUrl))
                        user.Factory.LogoUrl = logoUrl;
                }
            }

            // ─── Handle document re-uploads (triggers re-verification) ───
            var needsReVerification = false;
            var uploadedDocumentTypes = new List<string>();
            var uploadTasks = new List<Task<string>>();

            if (dto.CommercialRegistryFile != null)
            {
                uploadTasks.Add(_cloudinaryService.UploadFileAsync(dto.CommercialRegistryFile, "Documents"));
                uploadedDocumentTypes.Add("CommercialRegistry");
            }
            if (dto.TaxCardFile != null)
            {
                uploadTasks.Add(_cloudinaryService.UploadFileAsync(dto.TaxCardFile, "Documents"));
                uploadedDocumentTypes.Add("TaxCard");
            }
            if (dto.NationalIdFile != null)
            {
                uploadTasks.Add(_cloudinaryService.UploadFileAsync(dto.NationalIdFile, "Documents"));
                uploadedDocumentTypes.Add("NationalId");
            }
            if (dto.SelfieWithIdFile != null)
            {
                uploadTasks.Add(_cloudinaryService.UploadFileAsync(dto.SelfieWithIdFile, "Documents"));
                uploadedDocumentTypes.Add("SelfieWithId");
            }

            if (uploadTasks.Count > 0 && user.Factory != null)
            {
                needsReVerification = true;

                var uploadedUrls = await Task.WhenAll(uploadTasks);

                for (int i = 0; i < uploadedDocumentTypes.Count; i++)
                {
                    var docType = uploadedDocumentTypes[i];
                    var fileUrl = uploadedUrls[i];

                    if (string.IsNullOrEmpty(fileUrl))
                        continue;

                    var newDoc = new Document
                    {
                        FactoryId = user.Factory.Id,
                        DocumentType = docType,
                        FileUrl = fileUrl,
                        UploadedAt = DateTime.UtcNow
                    };
                    await _uow.Repository<Document>().AddAsync(newDoc);
                }
            }

            // ─── Reset verification status if documents changed ───
            if (needsReVerification)
            {
                user.VerificationStatus = VerificationStatus.Pending;
                if (user.Factory != null)
                    user.Factory.VerificationStatus = VerificationStatus.Pending;
            }

            _uow.Repository<User>().Update(user);
            if (user.Factory != null)
                _uow.Repository<Factory>().Update(user.Factory);

            await _uow.SaveChangesAsync();

            // ─── Notify admin about re-verification request ───
            if (needsReVerification)
            {
                await _notificationService.SendNotificationAsync(
                    1, // Admin user ID (default admin)
                    "طلب إعادة توثيق",
                    $"قام المستخدم {user.Name} بتحديث مستنداته. يرجى مراجعة التوثيق.",
                    "account_verified",
                    $"/admin/verification");
            }

            // Reload and return updated profile
            var updatedUser = await _uow.Repository<User>().Query()
                .AsNoTracking()
                .Include(u => u.Factory)
                    .ThenInclude(f => f!.Documents)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            var dtoResult = MapToProfileDto(updatedUser!);
            return BaseResponse<ProfileDto>.Success(dtoResult, needsReVerification
                ? "تم تحديث الملف الشخصي. المستندات قيد المراجعة من قبل الإدارة."
                : "تم تحديث الملف الشخصي بنجاح.");
        }

        // ───────────────────── Private ─────────────────────

        private static ProfileDto MapToProfileDto(User user)
        {
            var dto = new ProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                NationalId = user.NationalId,
                UserVerificationStatus = user.VerificationStatus,
                Documents = new List<DocumentDto>()
            };

            if (user.Factory != null)
            {
                dto.FactoryId = user.Factory.Id;
                dto.FactoryName = user.Factory.LegalName;
                dto.CommercialRegistryNo = user.Factory.CommercialRegistryNo;
                dto.TaxCardNo = user.Factory.TaxCardNo;
                dto.Address = user.Factory.Address;
                dto.Sector = user.Factory.Sector;
                dto.LogoUrl = user.Factory.LogoUrl;
                dto.FactoryVerificationStatus = user.Factory.VerificationStatus;

                dto.Documents = user.Factory.Documents
                    .Select(d => new DocumentDto
                    {
                        Id = d.Id,
                        DocumentType = d.DocumentType,
                        FileUrl = d.FileUrl,
                        UploadedAt = d.UploadedAt
                    })
                    .ToList();
            }

            return dto;
        }
    }
}
