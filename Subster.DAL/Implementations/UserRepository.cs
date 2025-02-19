using Subster.DAL.Interfaces;

namespace Subster.DAL.Implementations;

public class UserRepository : IUserRepository
{
    private readonly SubsterDbContext _dbContext;

    public UserRepository(SubsterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void GetAllUsers()
    {
        return;
    }
}