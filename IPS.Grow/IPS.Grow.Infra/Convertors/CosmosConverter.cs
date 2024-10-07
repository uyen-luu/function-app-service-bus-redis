using IPS.Grow.Infra.Entities.Cosmos;
using IPS.Grow.Infra.Models;

namespace IPS.Grow.Infra.Convertors;

internal static class CosmosConverter
{
    public static UserViewModel Convert(this UserEntity employee) => new(employee.UserName,
                                                                         employee.EmailAddress,
                                                                         employee.FirstName,
                                                                         employee.LastName,
                                                                         employee.MobileNumber);
}
