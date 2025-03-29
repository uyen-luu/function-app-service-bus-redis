using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace IPS.Grow.Infra.Models;

/// <summary>
/// Api error response type - for 500, 400 and so on
/// </summary>
/// <param name="Title"> Api Error Message </param>
/// <param name="StatusCode"></param>
/// <param name="RequestId"> Api Request Id </param>
[DisplayName("ApiErrorResponse")]
[DataContract(Name = "apiErrorResponse")]
public sealed record ApiErrorResponse([property: Required, DataMember(Name = "title")] string Title,
                                      [property: DataMember(Name = "statusCode")] int StatusCode,
                                      [property: Required, DataMember(Name = "requestId")] string RequestId)
{
    public string[]? Errors { get; set; }
    public string? Detail { get; set; }
};