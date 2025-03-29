using System.Text.Json.Serialization;

namespace IPS.Grow.Infra.AccessControl;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRoleType
{
    Admin,
    Member
}