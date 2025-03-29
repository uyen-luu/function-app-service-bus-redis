using IPS.Grow.Func.Models;
using static IPS.Grow.Shared.Utilities.MessageSerializer;
namespace IPS.Grow.Func.Convertors;

internal static class MessageConverter
{
    public static BrokerMessage<TData> ToBrokerMessage<TData>(this BinaryData input) where TData : class
       => input.ToObjectFromJson<BrokerMessage<TData>>(JsonSerializerOptions)!;
}