using System.Net;
using System.Text.Json.Serialization;

namespace IPS.Grow.Domain.Models;

public enum ResourceStatusType
{
    Added,
    Updated,
    Unmodified
}
public struct IpsResource(int identifier, ResourceStatusType status)
{
    public int Identifier { get; private set; } = identifier;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ResourceStatusType Status { get; private set; } = status;
};

public class ApiResponse
{
    public int StatusCode { get; set; }
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public bool IsError { get; set; }
    public IpsResource? Resource { get; set; }
    public static ApiResponse NewError() => new() { IsError = true };
    public static ApiResponse Error(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => Error(message, (int)statusCode);
    public static ApiResponse NotFound<T>() => Error($"{typeof(T).Name} not found");

    public static ApiResponse Error(string message, int statusCode)
        => new()
        {
            IsError = true,
            Title = message,
            StatusCode = statusCode
        };

    public static ApiResponse Success(string message = "Success", IpsResource? resource = null)
        => new()
        {
            IsError = false,
            Title = message,
            StatusCode = (int)HttpStatusCode.OK,
            Resource = resource
        };
}