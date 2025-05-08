using Microsoft.EntityFrameworkCore;
using Subster.DAL.Interfaces;
using Subster.DAL.Entities;
using Subster.DAL.Utilities;
using Subster.Models.Dtos;
using Subster.Models.InputModels;


namespace Subster.DAL.Implementations;

public class ClientRepository : IClientRepository
{
    private readonly SubsterDbContext _dbContext;

    public ClientRepository(SubsterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ClientDto>> GetAllClientsAsync()
    {
        return await _dbContext.Clients
            .Select(u => new ClientDto
            {
                Id = u.Id,
                Name = u.Name,
                Ssn = u.Ssn,
            })
            .ToListAsync();
    }

    public async Task<ClientDto?> GetClientBySsnAsync(string ssn)
    {
        return await _dbContext.Clients
            .Where(u => u.Ssn == ssn)
            .Select(u => new ClientDto
            {
                Id = u.Id,
                Name = u.Name,
                Ssn = u.Ssn,
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Guid> CreateClientAsync(UserInputModel inputModel)
    {
        // Check if client already exists
        var existingClient = _dbContext.Clients
            .FirstOrDefault(u => u.Ssn == inputModel.Ssn);

        if (existingClient == null)
        {
            var client = new Client
            {
                Name = inputModel.Name,
                Ssn = inputModel.Ssn,
                // CreatedAt = DateTime.UtcNow
            };

            // Save new client to database
            _dbContext.Clients.Add(client);
            await _dbContext.SaveChangesAsync();
            return client.Id;
        } else {
            // Update client name if it has changed
            existingClient.Name = inputModel.Name;
            await _dbContext.SaveChangesAsync();
            return existingClient.Id;
        }
    }
}
