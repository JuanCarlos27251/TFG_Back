using PARKit.Backend.Data;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.CompanyDtin;
using PARKit.Backend.Repositories;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Services
{
    public class CompanyService : ICompanyService
    {
       private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync()
        {
            return await _companyRepository.GetAllAsync();
        }

        public async Task<CompanyDto?> GetCompanyByIdAsync(int id)
        {
            return await _companyRepository.GetByIdAsync(id);
        }

        public async Task<CompanyDto?> GetCompanyByCifAsync(string cif)
        {
            if (string.IsNullOrWhiteSpace(cif))
            {
                throw new ArgumentException("El CIF proporcionado no es válido.");
            }

            // Asumiendo que el repositorio tiene GetByCifAsync
            return await _companyRepository.GetByCifAsync(cif);
        }

        public async Task<CompanyDto?> GetCompanyByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("El email proporcionado no es válido.");
            }

            return await _companyRepository.GetByEmailAsync(email);
        }

        public async Task<CompanyDto> CreateCompanyAsync(CompanyDtin dtin)
        {
            if (string.IsNullOrWhiteSpace(dtin.NameCompany))
            {
                throw new ArgumentException("El nombre de la empresa es obligatorio.");
            }
            if (string.IsNullOrWhiteSpace(dtin.CIF))
            {
                throw new ArgumentException("El CIF es obligatorio.");
            }
            if (string.IsNullOrWhiteSpace(dtin.Email))
            {
                throw new ArgumentException("El email es obligatorio.");
            }
            if (string.IsNullOrWhiteSpace(dtin.Password))
            {
                throw new ArgumentException("La contraseña es obligatoria.");
            }

            var existingCif = await _companyRepository.GetByCifAsync(dtin.CIF);
            if (existingCif != null)
            {
                throw new InvalidOperationException("Ya existe una empresa registrada con ese CIF.");
            }

            var existingEmail = await _companyRepository.GetByEmailAsync(dtin.Email);
            if (existingEmail != null)
            {
                throw new InvalidOperationException("Ya existe una empresa registrada con ese Email.");
            }

            return await _companyRepository.AddAsync(dtin);
        }

        public async Task<bool> UpdateCompanyAsync(int id, CompanyDtin dtin)
        {
            if (string.IsNullOrWhiteSpace(dtin.NameCompany))
            {
                throw new ArgumentException("El nombre de la empresa no puede estar vacío.");
            }
            if (string.IsNullOrWhiteSpace(dtin.CIF))
            {
                throw new ArgumentException("El CIF no puede estar vacío.");
            }

            return await _companyRepository.UpdateAsync(id, dtin);
        }

        public async Task<bool> DeleteCompanyAsync(int id)
        {
            return await _companyRepository.DeleteAsync(id);
        }
    }
}