using IPS.Grow.Infra.Entities.Cosmos;
using IPS.Grow.Infra.Models;

namespace IPS.Grow.Infra.Convertors;

internal static class CosmosConverter
{
    public static UserViewModel Convert(this UserEntity user) => new(user.UserName,
                                                                         user.EmailAddress,
                                                                         user.FirstName,
                                                                         user.LastName,
                                                                         user.MobileNumber)
    {
        Roles = user.Roles
    };
}
