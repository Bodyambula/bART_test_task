using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TicketSystem.Domain.Common;

[JsonConverter(typeof(TicketIdJsonConverter))]
public readonly record struct TicketId(Guid Value) : IParsable<TicketId>
{
    public static TicketId New() => new(Guid.NewGuid());
    public static readonly TicketId Empty = new(Guid.Empty);
    public override string ToString() => Value.ToString();

    public static TicketId Parse(string s, IFormatProvider? provider)
    {
        if (Guid.TryParse(s, out var guid)) return new TicketId(guid);
        throw new FormatException("Invalid TicketId format.");
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out TicketId result)
    {
        if (Guid.TryParse(s, out var guid))
        {
            result = new TicketId(guid);
            return true;
        }
        result = Empty;
        return false;
    }
}

[JsonConverter(typeof(NotificationIdJsonConverter))]
public readonly record struct NotificationId(Guid Value) : IParsable<NotificationId>
{
    public static NotificationId New() => new(Guid.NewGuid());
    public static readonly NotificationId Empty = new(Guid.Empty);
    public override string ToString() => Value.ToString();

    public static NotificationId Parse(string s, IFormatProvider? provider)
    {
        if (Guid.TryParse(s, out var guid)) return new NotificationId(guid);
        throw new FormatException("Invalid NotificationId format.");
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out NotificationId result)
    {
        if (Guid.TryParse(s, out var guid))
        {
            result = new NotificationId(guid);
            return true;
        }
        result = Empty;
        return false;
    }
}

public class TicketIdJsonConverter : JsonConverter<TicketId>
{
    public override TicketId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetGuid());

    public override void Write(Utf8JsonWriter writer, TicketId value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}

public class NotificationIdJsonConverter : JsonConverter<NotificationId>
{
    public override NotificationId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetGuid());

    public override void Write(Utf8JsonWriter writer, NotificationId value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
