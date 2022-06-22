using Project.DTOs.UserAuth;

namespace Project.Services.UserAuth
{
    public interface IAuthenticationServices
    {
        Task RegisterAsync(UserDTO userDTO);
        Task<AccountResponseDTO> LoginAsync(UserDTO userDTO);
        Task<AccountResponseDTO> RefreshAsync(string refreshToken);
    }
}
