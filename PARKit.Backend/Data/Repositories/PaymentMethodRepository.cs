using Microsoft.EntityFrameworkCore;
using PARKit.Backend.Data;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.PaymentMethodDtin;
using PARKit.Backend.Models;

namespace PARKit.Backend.Repositories
{
  public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly AppDbContext _context;

        public PaymentMethodRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaymentMethodDto>> GetByUserIdAsync(int userId)
        {
            return await _context.PaymentMethods
                .Where(p => p.UserId == userId)
                .Select(p => new PaymentMethodDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    CadType = p.CadType,
                    LastFourDigits = p.LastFourDigits,
                    HolderName = p.HolderName,
                    ExpiryDate = p.ExpiryDate
                }).ToListAsync();
        }

        public async Task<PaymentMethodDto?> GetByIdAsync(int id)
        {
            var p = await _context.PaymentMethods.FindAsync(id);
            if (p == null) return null;

            return new PaymentMethodDto
            {
                Id = p.Id,
                UserId = p.UserId,
                CadType = p.CadType,
                LastFourDigits = p.LastFourDigits,
                HolderName = p.HolderName,
                ExpiryDate = p.ExpiryDate
            };
        }

        public async Task<PaymentMethodDto> AddAsync(PaymentMethodDtin dtin)
        {
            var entity = new PaymentMethod
            {
                UserId = dtin.UserId,
                CadType = dtin.CadType,
                LastFourDigits = dtin.LastFourDigits,
                HolderName = dtin.HolderName,
                ExpiryDate = dtin.ExpiryDate
            };

            await _context.PaymentMethods.AddAsync(entity);
            await _context.SaveChangesAsync();

            return new PaymentMethodDto 
            { 
                Id = entity.Id, 
                UserId = entity.UserId,
                LastFourDigits = entity.LastFourDigits 
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.PaymentMethods.FindAsync(id);
            if (entity == null) return false;

            _context.PaymentMethods.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}