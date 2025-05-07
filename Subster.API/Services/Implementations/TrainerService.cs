using Subster.API.Services.Interfaces;
using Subster.DAL.Interfaces;
using Subster.DAL.Entities;
using Subster.Models.Dtos;
using Subster.Models.InputModels;

namespace Subster.API.Services.Implementations
{
    public class TrainerService : ITrainerService
    {
        private readonly ITrainerRepository _trainerRepository;

        public TrainerService(ITrainerRepository trainerRepository)
        {
            _trainerRepository = trainerRepository;
        }

        /// <summary>
        /// Retrieves a list of all trainers.
        /// </summary>
        public Task<IEnumerable<TrainerDto>> GetAllTrainersAsync()
            => _trainerRepository.GetAllTrainersAsync();

        /// <summary>
        /// Retrieves a trainer by their internal ID.
        /// Returns null if not found.
        /// </summary>
        public Task<TrainerDto?> GetTrainerByIdAsync(int id)
            => _trainerRepository.GetTrainerByIdAsync(id);

        /// <summary>
        /// Checks whether a trainer with the given SSN already exists.
        /// </summary>
        public Task<bool> ExistsBySsnAsync(string ssn)
            => _trainerRepository.ExistsBySsnAsync(ssn);

        /// <summary>
        /// Creates a trainer if none exists with the same SSN.
        /// Returns true if created, false if one already existed.
        /// </summary>
        public async Task<bool> CreateTrainerIfNotExistsAsync(UserInputModel inputModel)
        {
            if (!await _trainerRepository.ExistsBySsnAsync(inputModel.Ssn))
            {
                await _trainerRepository.CreateTrainerAsync(inputModel);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves the full Trainer entity (including decrypted Payday credentials).
        /// </summary>
        public Task<Trainer?> GetTrainerEntityBySsnAsync(string ssn)
            => _trainerRepository.FindTrainerEntityBySsnAsync(ssn);

        /// <summary>
        /// Updates the trainer's Payday client ID and secret.
        /// Returns false if the trainer is not found.
        /// </summary>
        public async Task<bool> UpdatePaydayCredentialsAsync(string ssn, string clientId, string clientSecret)
        {
            var trainer = await _trainerRepository.FindTrainerEntityBySsnAsync(ssn);
            if (trainer == null)
                return false;

            await _trainerRepository.UpdatePaydayCredentialsAsync(
                trainer.Id, clientId, clientSecret);
            return true;
        }

        /// <summary>
        /// Deletes the trainer's Payday credentials.
        /// Returns false if the trainer is not found.
        /// </summary>
        public async Task<bool> DeletePaydayCredentialsAsync(string ssn)
        {
            var trainer = await _trainerRepository.FindTrainerEntityBySsnAsync(ssn);
            if (trainer == null)
                return false;

            await _trainerRepository.UpdatePaydayCredentialsAsync(
                trainer.Id, null, null);
            return true;
        }

        /// <summary>
        /// Deletes (or soft-deletes) the trainer record.
        /// Returns false if the trainer is not found.
        /// </summary>
        public async Task<bool> DeleteTrainerAsync(string ssn)
        {
            var trainer = await _trainerRepository.FindTrainerEntityBySsnAsync(ssn);
            if (trainer == null)
                return false;

            await _trainerRepository.DeleteTrainerAsync(trainer.Id);
            return true;
        }
    }
}
