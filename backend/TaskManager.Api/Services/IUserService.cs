using TaskManager.Api.Dtos;

namespace TaskManager.Api.Services;

public interface IUserService
{
    Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default);
}
