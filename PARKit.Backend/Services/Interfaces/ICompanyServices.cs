using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.CompanyDtin;

namespace PARKit.Backend.Services.Interfaces
{
    public interface ICompanyService
    {
       Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync();
        Task<CompanyDto?> GetCompanyByIdAsync(int id);
        Task<CompanyDto?> GetCompanyByCifAsync(string cif);
        Task<CompanyDto?> GetCompanyByEmailAsync(string email);
        Task<CompanyDto> CreateCompanyAsync(CompanyDtin dtin);
        Task<bool> UpdateCompanyAsync(int id, CompanyDtin dtin);
        Task<bool> DeleteCompanyAsync(int id);
    }
}