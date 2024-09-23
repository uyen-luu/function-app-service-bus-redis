using System.Net;
using System.Text.Json.Serialization;

namespace IPS.Grow.Func.Models;

public enum ApiResponseStatus
{
    Unknown = default,
    Successed,
    Failed
}
public class ApiResponse
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ApiResponseStatus Status { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public object? Data { get; set; }
    public string? Message { get; set; }
}
