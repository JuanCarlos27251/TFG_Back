using Microsoft.EntityFrameworkCore;
using PARKit.Backend.Data;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.CompanyDtin;
using PARKit.Backend.Models;

namespace PARKit.Backend.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly AppDbContext _context;

        public CompanyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CompanyDto>> GetAllAsync()
        {
            return await _context.Companies
                .Select(c => new CompanyDto
                {
                    Id = c.Id,
                    NameCompany = c.NameCompany,
                    CIF = c.CIF,
                    Email = c.Email,
                    Phone = c.Phone,
                    Address = c.Address,
                    IsActive = c.IsActive
                }).ToListAsync();
        }

        public async Task<CompanyDto?> GetByIdAsync(int id)
        {
            var c = await _context.Companies.FindAsync(id);
            if (c == null) return null;

            return new CompanyDto
            {
                Id = c.Id,
                NameCompany = c.NameCompany,
                CIF = c.CIF,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                IsActive = c.IsActive
            };
        }

        public async Task<CompanyDto?> GetByCifAsync(string cif)
        {
            var c = await _context.Companies.FirstOrDefaultAsync(x => x.CIF == cif);
            if (c == null) return null;

            return new CompanyDto
            {
                Id = c.Id,
                NameCompany = c.NameCompany,
                CIF = c.CIF,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                IsActive = c.IsActive
            };
        }

        public async Task<CompanyDto?> GetByEmailAsync(string email)
        {
            var c = await _context.Companies.FirstOrDefaultAsync(x => x.Email == email);
            if (c == null) return null;

            return new CompanyDto
            {
                Id = c.Id,
                NameCompany = c.NameCompany,
                CIF = c.CIF,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                IsActive = c.IsActive
            };
        }

        public async Task<CompanyDto> AddAsync(CompanyDtin dtin)
        {
            var company = new Company
            {
                NameCompany = dtin.NameCompany,
                CIF = dtin.CIF,
                Email = dtin.Email,
                Phone = dtin.Phone,
                Address = dtin.Address,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dtin.Password), // Hasheamos la password
                Role = "Manager", // Rol por defecto
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Companies.AddAsync(company);
            await _context.SaveChangesAsync();

            return new CompanyDto
            {
                Id = company.Id,
                NameCompany = company.NameCompany,
                CIF = company.CIF,
                Email = company.Email,
                Phone = company.Phone,
                Address = company.Address,
                IsActive = company.IsActive
            };
        }

        public async Task<bool> UpdateAsync(int id, CompanyDtin dtin)
        {
            var c = await _context.Companies.FindAsync(id);
            if (c == null) return false;

            c.NameCompany = dtin.NameCompany;
            c.CIF = dtin.CIF;
            c.Email = dtin.Email;
            c.Phone = dtin.Phone;
            c.Address = dtin.Address;

            if (!string.IsNullOrWhiteSpace(dtin.Password))
            {
                c.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dtin.Password);
            }

            _context.Companies.Update(c);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var c = await _context.Companies.FindAsync(id);
            if (c == null) return false;

            _context.Companies.Remove(c);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}