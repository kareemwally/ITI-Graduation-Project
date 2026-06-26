

namespace BLL.DTOs.AuthnticationDTOs
{
    public class FayedRegisterFactoryRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string NationalId { get; set; } = null!;
        public string FactoryName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Sector { get; set; } = null!;
        public IFormFile CommercialRegistryFile { get; set; } = null!;
        public IFormFile TaxCardFile { get; set; } = null!;
        public IFormFile NationalIdFile { get; set; } = null!;
        public IFormFile SelfieWithIdFile { get; set; } = null!;
        public string CommercialRegistryNo { get; set; } = null!;
        public string TaxCardNo { get; set; } = null!;
        
    }

    

   
}
