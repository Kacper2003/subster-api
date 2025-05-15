using Subster.API.Services.Interfaces;
using Subster.DAL.Interfaces;
using Subster.DAL.Entities;
using Subster.Models.Dtos;
using Subster.Models.InputModels;
using Subster.Models.UpdateModels;
using Subster.API.Exceptions; 

namespace Subster.API.Services.Implementations
{
    public class TrainerService(ITrainerRepository trainerRepository) : ITrainerService
    {
        private readonly ITrainerRepository _trainerRepository = trainerRepository;

		public Task<IEnumerable<TrainerDto>> GetAllTrainersAsync()
            => _trainerRepository.GetAllTrainersAsync();

        public Task<TrainerDto?> GetTrainerByIdAsync(Guid trainerId)
            => _trainerRepository.GetTrainerByIdAsync(trainerId);

        public async Task<TrainerDetailsDto> GetTrainerDetailsBySsnAsync(string trainerSsn)
        {
			TrainerDto trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
                ?? throw new UnauthorizedException("Invalid trainer credentials.");

			TrainerDetailsDto trainerDetails = await _trainerRepository.GetTrainerDetailsByIdAsync(trainer.Id)
                ?? throw new NotFoundException("Trainer not found.");

            return trainerDetails;
        }

        public async Task<TrainerDetailsDto> UpdateTrainerDetailsAsync(string trainerSsn, TrainerUpdateModel updateModel)
        {
			TrainerDto trainer = await _trainerRepository.GetTrainerBySsnAsync(trainerSsn)
                ?? throw new UnauthorizedException("Invalid trainer credentials.");

			TrainerDetailsDto updatedTrainer = await _trainerRepository.UpdateTrainerDetailsAsync(trainer.Id, updateModel)
                ?? throw new NotFoundException("Trainer not found.");

            return updatedTrainer;
        }

        public Task<bool> ExistsBySsnAsync(string trainerSsn)
            => _trainerRepository.ExistsBySsnAsync(trainerSsn);

        public async Task<bool> CreateTrainerIfNotExistsAsync(UserInputModel inputModel)
        {
            if (!await _trainerRepository.ExistsBySsnAsync(inputModel.Ssn))
            {
                await _trainerRepository.CreateTrainerAsync(inputModel);
                return true;
            }
            return false;
        }

        public Task<Trainer?> GetTrainerEntityBySsnAsync(string trainerSsn)
            => _trainerRepository.FindTrainerEntityBySsnAsync(trainerSsn);

        // Just has to check if the fields are not null, as they are already validated when uploaded
        public async Task<bool> HasPaydayCredentialsAsync(string trainerSsn)
        {
			Trainer trainer = await _trainerRepository.FindTrainerEntityBySsnAsync(trainerSsn)
                ?? throw new UnauthorizedException("Invalid trainer credentials.");

            return !string.IsNullOrEmpty(trainer.PaydayClientId) && !string.IsNullOrEmpty(trainer.PaydayClientSecret);
        }

        public async Task<bool> UpdatePaydayCredentialsAsync(string trainerSsn, string clientId, string clientSecret)
        {
			Trainer? trainer = await _trainerRepository.FindTrainerEntityBySsnAsync(trainerSsn);
            if (trainer == null)
                return false;

            await _trainerRepository.UpdatePaydayCredentialsAsync(
                trainer.Id, clientId, clientSecret);
            return true;
        }

        public async Task<bool> DeletePaydayCredentialsAsync(string trainerSsn)
        {
			Trainer? trainer = await _trainerRepository.FindTrainerEntityBySsnAsync(trainerSsn);
            if (trainer == null)
                return false;

            await _trainerRepository.UpdatePaydayCredentialsAsync(
                trainer.Id, null, null);
            return true;
        }

        public async Task<bool> DeleteTrainerAsync(string trainerSsn)
        {
			Trainer? trainer = await _trainerRepository.FindTrainerEntityBySsnAsync(trainerSsn);
            if (trainer == null)
                return false;

            await _trainerRepository.DeleteTrainerAsync(trainer.Id);
            return true;
        }
    }
}
