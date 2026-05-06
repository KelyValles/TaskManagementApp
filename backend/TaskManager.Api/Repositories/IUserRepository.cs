using TaskManager.Api.Models;

namespace TaskManager.Api.Repositories;

public interface IUserRepository
{
    Task<User> CreateAsync(User user, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}
