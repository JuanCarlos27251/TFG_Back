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
                    ExternalTransactionId = p.ExternalTransactionId,
                    ClientSecret = p.ClientSecret
                }).FirstOrDefaultAsync();
        }

        public async Task<PaymentDto?> GetByIdAsync(int id)
        {
            var p = await _context.Payments.FindAsync(id);
            if (p == null) return null;

            return new PaymentDto
            {
                Id = p.Id,
                ReservationId = p.ReservationId,
                Amount = p.Amount,
                Status = p.Status,
                Currency = p.Currency,
                PaymentDate = p.PaymentDate,
                ExternalTransactionId = p.ExternalTransactionId,
                ClientSecret = p.ClientSecret
            };
        }

        public async Task<PaymentDto> CreateAsync(PaymentDtin dtin)
        {
            var payment = new Payment
            {
                ReservationId = dtin.ReservationId,
                Amount = dtin.Amount,
                Status = dtin.Status != 0 ? dtin.Status : PaymentStatus.Pending, // Usar el del DTO si viene
                Currency = dtin.Currency,
                ExternalTransactionId = dtin.ExternalTransactionId,
                PaymentDate = DateTime.Now 
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            return new PaymentDto { Id = payment.Id, Amount = payment.Amount, Status = payment.Status };
        }


        public async Task<PaymentDto> CreateWithSecretAsync(PaymentDtin dtin, string clientSecret)
        {
            var payment = new Payment
            {
                ReservationId = dtin.ReservationId,
                Amount = dtin.Amount,
                Status = PaymentStatus.Pending,
                Currency = dtin.Currency,
                ClientSecret = clientSecret,
                ExternalTransactionId = dtin.ExternalTransactionId,
                // AHORA REGISTRA LA HORA DE ESPAÑA (del Sistema) EN VEZ DE LA UTC:
                PaymentDate = DateTime.Now 
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            return new PaymentDto
            {
                Id = payment.Id,
                ReservationId = payment.ReservationId,
                Amount = payment.Amount,
                Status = payment.Status,
                Currency = payment.Currency,
                ClientSecret = payment.ClientSecret,
                PaymentDate = payment.PaymentDate
            };
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