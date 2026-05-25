using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.CompanyDtin;

namespace PARKit.Backend.Repositories
{
    public interface ICompanyRepository
    {
        Task<IEnumerable<CompanyDto>> GetAllAsync();
        Task<CompanyDto?> GetByIdAsync(int id);
        Task<CompanyDto?> GetByCifAsync(string cif);
        Task<CompanyDto?> GetByEmailAsync(string email);
        Task<CompanyDto> AddAsync(CompanyDtin dtin);
        Task<bool> UpdateAsync(int id, CompanyDtin dtin);
        Task<bool> DeleteAsync(int id);
    }
}