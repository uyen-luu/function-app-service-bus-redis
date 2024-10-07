//using IPS.Grow.Domain.Models;
//using IPS.Grow.Domain.Monads;
//using IPS.Grow.OWASP.AccessControl;
//using IPS.Grow.OWASP.Entities.Cosmos;
//using IPS.Grow.OWASP.Models;
//using System.Text;

//namespace IPS.Grow.OWASP.Services;

//public interface IUserService
//{
//    Task<Maybe<UserViewModel>> LoginAsync(LoginModel user, string ipAddress, CancellationToken ct = default);
//    Task<ApiResponse> RevokeTokenAsync(string token, string ipAddress, CancellationToken ct = default);
//    Task<Maybe<UserViewModel>> RefreshTokenAsync(string token, string ipAddress, CancellationToken ct = default);
//    Task<ApiResponse> UpdateProfileAsync(int userNo, UserProfileModel userProfile, CancellationToken ct = default);
//    Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordModel request, CancellationToken ct = default);
//    Task<ApiResponse> ForgotPasswordAsync(Uri domain, string email, CancellationToken ct = default);
//}

//internal class UserService(IUnitOfWork unitOfWork, TokenFactory tokenFactory, IFluentEmail fluentEmail) : IUserService
//{
//    public async Task<Maybe<UserViewModel>> LoginAsync(LoginModel userModel, string ipAddress, CancellationToken ct = default)
//    {
//        var maybeUser = await unitOfWork.Repository<Employee>().LoginAsync(userModel.Username, userModel.Password, ct);
//        return await maybeUser.SelectAsync(async employee =>
//        {
//            var result = employee.Convert() with
//            {
//                Roles = await unitOfWork.Repository<Employee>().GetRolesAsync(employee.EmployeeNo, ct),
//                DefaultJobDocumentsPath = await unitOfWork.Repository<SystemParameter>().GetDefaultJobDocumentsPathAsync(ct),
//                RememberMe = userModel.RememberMe
//            };
//            //
//            var refreshToken = tokenFactory.BuildRefreshToken(ipAddress, employee.EmployeeNo, userModel.RememberMe);
//            //
//            await unitOfWork.Repository<RefreshToken>().InsertAsync(refreshToken);
//            //
//            return result with
//            {
//                JwtToken = tokenFactory.WriteToken(result.GetClaims()),
//                RefreshToken = refreshToken.Token
//            };
//        });
//    }

//    public async Task<ApiResponse> RevokeTokenAsync(string token, string ipAddress, CancellationToken ct = default)
//    {
//        var refreshToken = await unitOfWork.Repository<RefreshToken>().FindAsync(token);
//        if (refreshToken == null)
//            return ApiResponse.NotFound<RefreshToken>();

//        // Return false if token is not active
//        if (!refreshToken.IsActive) return ApiResponse.Error("TokenExpired");

//        // Revoke token and save
//        refreshToken.RevokedDate = DateTime.UtcNow;
//        refreshToken.RevokedByIp = ipAddress;

//        await unitOfWork.CommitAsync();

//        return ApiResponse.Success();
//    }

//    public async Task<Maybe<UserViewModel>> RefreshTokenAsync(string token, string ipAddress, CancellationToken ct = default)
//    {
//        var refreshToken = await unitOfWork.Repository<RefreshToken>().FindAsync(token);
//        if (refreshToken == null || !refreshToken.IsActive)
//            return Maybe<UserViewModel>.None;
//        //
//        var maybeUser = await GetUserByIdAsync(refreshToken.UserNo, ct);
//        return await maybeUser.SelectAsync(async authenticatedUser =>
//        {
//            var validRefreshToken = refreshToken.Token;
//            if (refreshToken.IsExpired)
//            {
//                // Replace old refresh token with a new one and save
//                var newRefreshToken = tokenFactory.RenewRefreshToken(refreshToken);
//                //
//                // Update old refresh token
//                refreshToken.RevokedDate = DateTime.UtcNow;
//                refreshToken.RevokedByIp = ipAddress;
//                refreshToken.ReplacedByToken = newRefreshToken.Token;
//                // Create new refresh token
//                await unitOfWork.Repository<RefreshToken>().InsertAsync(newRefreshToken);

//                validRefreshToken = newRefreshToken.Token;
//            }
//            authenticatedUser.Roles = await unitOfWork.Repository<Employee>().GetRolesAsync(authenticatedUser.UserNo, ct);
//            authenticatedUser.DefaultJobDocumentsPath = await unitOfWork.Repository<SystemParameter>().GetDefaultJobDocumentsPathAsync(ct);
//            return authenticatedUser with
//            {
//                RefreshToken = validRefreshToken,
//                JwtToken = tokenFactory.WriteToken(authenticatedUser.GetClaims()),
//            };
//        });
//    }

//    public async Task<ApiResponse> UpdateProfileAsync(int userNo, UserProfileModel userProfile, CancellationToken ct = default)
//    {
//        return await unitOfWork.ExecuteTransaction(async () =>
//        {
//            var user = await unitOfWork.Repository<Employee>().FindAsync(userNo);
//            if (user == null) return ApiResponse.NotFound<Employee>();
//            //
//            user.EmployeeEmailSignature = userProfile.EmployeeEmailSignature;

//            return ApiResponse.Success();
//        });
//    }

//    public async Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordModel request, CancellationToken ct = default)
//    {
//        //await unitOfWork.Repository<Employee>().ChangePasswordAsync(userId, request.NewPassword);
//        return ApiResponse.Success();
//    }

//    public async Task<ApiResponse> ForgotPasswordAsync(Uri domain, string email, CancellationToken ct = default)
//    {
//        var maybe = await unitOfWork.Repository<Employee>().GetUserByEmailAsync(email, ct);
//        return await maybe.MatchResultAsync(ApiResponse.Error("The entered email address is not associated with any account."), async employee =>
//        {
//            return await unitOfWork.ExecuteTransaction(async () =>
//            {
//                var randomPassword = PasswordHash.CreateRandomPassword();
//                await unitOfWork.Repository<Employee>().ChangePasswordAsync(employee.UserId ?? employee.EmailAddress, randomPassword);
//                var fullName = string.Join(" ", employee.EmployeeFirstName, employee.EmployeeLastName).Trim();
//                var res = await SendForgotPasswordEmailAsync(domain, fullName, randomPassword, employee.EmailAddress, ct);
//                return res.Successful
//                ? ApiResponse.Success("Your password has been reset")
//                : ApiResponse.Error(string.Join(Environment.NewLine, res.ErrorMessages), StatusCodes.Status400BadRequest);
//            });
//        });
//    }

//    #region Privates
//    private async Task<Maybe<UserViewModel>> GetUserByIdAsync(int userNo, CancellationToken ct = default)
//    {
//        var user = await unitOfWork.Repository<Employee>().FindAsync(userNo, ct);
//        if (user == null)
//            return Maybe<UserViewModel>.None;
//        //
//        return Maybe<UserViewModel>.Some(user.Convert());
//    }

//    private async Task<SendResponse> SendForgotPasswordEmailAsync(Uri domain, string employeeName, string password, string emailAddress, CancellationToken ct = default)
//    {
//        var emailBody = File.ReadAllText(Environment.CurrentDirectory + "\\EmailTemplates\\ResetPassword.html");
//        //
//        var codes = new Dictionary<string, string>
//        {
//            {"[[EmployeeName]]", employeeName  },
//            {"[[Password]]", password },
//            {"[[AppUrl]]", domain.OriginalString }
//        };

//#if DEBUG
//        codes["[[AppUrl]]"] = "https://dev.goalmaker.com.au:1114";
//#endif

//        codes.ForEach(code => emailBody = emailBody.Replace(code.Key, code.Value));

//        return await fluentEmail.To(emailAddress, employeeName)
//             .Subject("Your password has been reset")
//             .Body(emailBody, true)
//             .SendAsync(ct)
//             .ConfigureAwait(false);
//    }
//    #endregion
//}