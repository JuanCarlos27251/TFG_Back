using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.TarifDtin;
using PARKit.Backend.Repositories;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Services
{
    public class TarifService : ITarifService
    {
        private readonly ITarifRepository _tarifRepository;

        public TarifService(ITarifRepository tarifRepository)
        {
            _tarifRepository = tarifRepository;
        }

        public async Task<IEnumerable<TarifDto>> GetTarifsByParkingIdAsync(int parkingId)
        {
            return await _tarifRepository.GetByParkingIdAsync(parkingId);
        }

        public async Task<TarifDto?> GetTarifByIdAsync(int id)
        {
            return await _tarifRepository.GetByIdAsync(id);
        }

        public async Task<TarifDto> AddTarifAsync(TarifDtin dtin)
        {
            if (dtin.PricePerHour < 0)
            {
                throw new ArgumentException("El precio por hora no puede ser un valor negativo.");
            }
            
            return await _tarifRepository.AddAsync(dtin);
        }

        public async Task<bool> UpdateTarifAsync(int id, TarifDtin dtin)
        {
            if (dtin.PricePerHour < 0)
            {
                throw new ArgumentException("El precio por hora no puede ser un valor negativo.");
            }

            return await _tarifRepository.UpdateAsync(id, dtin);
        }

        public async Task<bool> DeleteTarifAsync(int id)
        {
            return await _tarifRepository.DeleteAsync(id);
        }
    }
}