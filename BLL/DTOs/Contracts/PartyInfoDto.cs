namespace BLL.DTOs.Contracts
{
    public class PartyInfoDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string FactoryName { get; set; } = null!;
        public string FactoryCode { get; set; } = null!;
        public string? NationalId { get; set; }
        public string CommercialRegistryNo { get; set; } = null!;
        public string TaxCardNo { get; set; } = null!;
        public string? LogoUrl { get; set; }
    }
}
