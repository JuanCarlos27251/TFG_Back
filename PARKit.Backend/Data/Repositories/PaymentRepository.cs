using Microsoft.EntityFrameworkCore;
using PARKit.Backend.Data;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.PaymentDtin;
using PARKit.Backend.Enums;
using PARKit.Backend.Models;

namespace PARKit.Backend.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentDto?> GetByReservationIdAsync(int reservationId)
        {
            return await _context.Payments
                .Where(p => p.ReservationId == reservationId)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    ReservationId = p.ReservationId,
                    Amount = p.Amount,
                    Status = p.Status,
                    Currency = p.Currency,
                    PaymentDate = p.PaymentDate,
                    ExternalTransactionId = p.ExternalTransactionId
                }).FirstOrDefaultAsync();
        }

        public async Task<PaymentDto> CreateAsync(PaymentDtin dtin)
        {
            var payment = new Payment
            {
                ReservationId = dtin.ReservationId,
                Amount = dtin.Amount,
                Status = dtin.Status,
                Currency = dtin.Currency,
                ClientSecret = dtin.ClientSecret,
                ExternalTransactionId = dtin.ExternalTransactionId,
                PaymentDate = DateTime.UtcNow
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            return new PaymentDto { Id = payment.Id, Amount = payment.Amount, Status = payment.Status };
        }

        public async Task<bool> UpdateStatusAsync(int id, PaymentStatus status, string? externalId = null)
        {
            var p = await _context.Payments.FindAsync(id);
            if (p == null) return false;

            p.Status = status;
            if (!string.IsNullOrEmpty(externalId)) 
                p.ExternalTransactionId = externalId;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}