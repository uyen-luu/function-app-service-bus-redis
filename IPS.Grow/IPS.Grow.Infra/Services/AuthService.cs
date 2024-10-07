using IPS.Grow.Infra.AccessControl;
using IPS.Grow.Infra.Convertors;
using IPS.Grow.Infra.Cryptographic;
using IPS.Grow.Infra.Entities.Cosmos;
using IPS.Grow.Infra.Models;
using IPS.Grow.Shared.Configs;
using IPS.Grow.Shared.Extensions;
using IPS.Grow.Shared.Monads;
using IPS.Grow.Shared.Services;
using Microsoft.Azure.Cosmos;
using System.Text;

namespace IPS.Grow.Infra.Services;

public interface IAuthService
{
    Task<Maybe<UserViewModel>> LoginAsync(LoginModel user, string ipAddress, CancellationToken ct = default);
    Task<ApiResponse> RevokeTokenAsync(string token, string username, string ipAddress, CancellationToken ct = default);
    Task<Maybe<UserViewModel>> RefreshTokenAsync(string token, string username, string ipAddress, CancellationToken ct = default);
}

internal class AuthService(ICosmosService cosmosService, TokenFactory tokenFactory) : IAuthService
{
    private readonly Task<Container> _container = cosmosService.GetContainerAsync(nameof(ContainerNames.User));
    private readonly Task<Container> _tokenContainer = cosmosService.GetContainerAsync(nameof(ContainerNames.RefreshToken));
    public async Task<Maybe<UserViewModel>> LoginAsync(LoginModel login, string ipAddress, CancellationToken ct = default)
    {
        var maybeUser = await LoginAsync(login.Username, login.Password, ct);
        return await maybeUser.SelectAsync(async user =>
        {
            var result = user.Convert() with
            {
                //Roles = await unitOfWork.Repository<Employee>().GetRolesAsync(employee.EmployeeNo, ct),
                RememberMe = login.RememberMe
            };
            //
            var refreshToken = tokenFactory.BuildRefreshToken(ipAddress, user.UserName, login.RememberMe);
            //
            var tokenContainer = await _tokenContainer;
            await tokenContainer.CreateItemAsync(refreshToken, new PartitionKey(user.UserName), cancellationToken: ct);
            //
            return result with
            {
                JwtToken = tokenFactory.WriteToken(result.GetClaims()),
                RefreshToken = refreshToken.Id
            };
        });
    }

    public async Task<ApiResponse> RevokeTokenAsync(string token, string username, string ipAddress, CancellationToken ct = default)
    {
        var tokenContainer = await _tokenContainer;
        var refreshToken = await tokenContainer.FindAsync<RefreshTokenEntity>(token, username, ct);
        if (refreshToken == null)
            return ApiResponse.NotFound<RefreshTokenEntity>();

        // Return false if token is not active
        if (refreshToken.RevokedDate != null || refreshToken.ExpiryDate < DateTime.UtcNow) return ApiResponse.Error("TokenExpired");

        // Revoke token and save
        refreshToken.RevokedDate = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        await tokenContainer.UpsertItemAsync(refreshToken, cancellationToken: ct);

        return ApiResponse.Success();
    }

    public async Task<Maybe<UserViewModel>> RefreshTokenAsync(string token, string username, string ipAddress, CancellationToken ct = default)
    {
        var tokenContainer = await _tokenContainer;
        var refreshToken = await tokenContainer.FindAsync<RefreshTokenEntity>(token, username, ct);
        var isExpired = refreshToken?.ExpiryDate < DateTime.UtcNow;
        if (refreshToken == null || refreshToken.RevokedDate != null || isExpired)
            return Maybe<UserViewModel>.None;
        //
        var maybeUser = await GetUserByIdAsync(refreshToken.Id, ct);
        return await maybeUser.SelectAsync(async authenticatedUser =>
        {
            var validRefreshToken = refreshToken.Id;
            if (isExpired)
            {
                // Replace old refresh token with a new one and save
                var newRefreshToken = tokenFactory.RenewRefreshToken(refreshToken);
                //
                // Update old refresh token
                refreshToken.RevokedDate = DateTime.UtcNow;
                refreshToken.RevokedByIp = ipAddress;
                refreshToken.ReplacedByToken = newRefreshToken.Id;
                // Create new refresh token
                await tokenContainer.CreateItemAsync(newRefreshToken, cancellationToken: ct);

                validRefreshToken = newRefreshToken.Id;
            }
            //authenticatedUser.Roles = await unitOfWork.Repository<Employee>().GetRolesAsync(authenticatedUser.UserNo, ct);
            return authenticatedUser with
            {
                RefreshToken = validRefreshToken,
                JwtToken = tokenFactory.WriteToken(authenticatedUser.GetClaims()),
            };
        });
    }

    #region Privates
    private async Task<Maybe<UserViewModel>> GetUserByIdAsync(string username, CancellationToken ct = default)
    {
        var container = await _container;
        var key = username.ToLowerInvariant();
        var user = await container.FindAsync<UserEntity>(key, key, ct);
        if (user == null)
            return Maybe<UserViewModel>.None;
        //
        return Maybe<UserViewModel>.Some(user.Convert());
    }

    private async Task<Maybe<UserEntity>> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var container = await _container;
        var key = username.ToLowerInvariant();
        var user = await container.FindAsync<UserEntity>(key, key, ct);
        if (user == null)
            return Maybe<UserEntity>.None;

        var userSecret = string.Concat(password, user.Salt);
        var hashed = EncodingFactory.GetHashString(userSecret, HashAlgorithmType.SHA256);
        return user.Password == hashed ? Maybe<UserEntity>.Some(user) : Maybe<UserEntity>.None;
    }

    #endregion
}