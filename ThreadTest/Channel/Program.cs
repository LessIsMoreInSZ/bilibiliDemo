using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

Channel<Wrapper> channel = Channel.CreateUnbounded<Wrapper>();

Task.Run(async () =>
{
    await Task.WhenAll(Sender1(channel), Sender2(channel));
});

JsonSerializerOptions options = new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

await foreach (var item in channel.Reader.ReadAllAsync())
{
    //string json = item.MessageType switch
    //{
    //    MessageType.MyMessage1 => JsonSerializer.Serialize((MyMessage1)item.Message, options),
    //    MessageType.MyMessage2 => JsonSerializer.Serialize((MyMessage2)item.Message, options),
    //    _ => throw new InvalidOperationException("Unknown message type")
    //};
    string json = JsonSerializer.Serialize(item, options);
    Console.WriteLine(json);
}

//PLC1 发消息
async Task Sender1(Channel<Wrapper> channel)
{
    int id = 0;
    while (true)
    {
        await Task.Delay(100);
        Random random = new();
        var a = random.Next(1, 1000);
        var message1 = new MyMessage1
        {
            Id = id++,
            Message = a,
            Client = "sender1",
            DateTime = DateTime.Now
        };
        await channel.Writer.WriteAsync(new Wrapper
        {
            MessageType = MessageType.MyMessage1,
            Message = message1
        });
    }
}

//PLC2 发消息
async Task Sender2(Channel<Wrapper> channel)
{
    int id = 0;
    while (true)
    {
        await Task.Delay(100);
        Random random = new();
        var a = random.Next(-1000, 0);
        var message2 = new MyMessage2
        {
            Id = id++,
            Message = a,
            Client = "sender2",
            DateTime = DateTime.Now
        };
        await channel.Writer.WriteAsync(new Wrapper
        {
            MessageType = MessageType.MyMessage2,
            Message = message2
        });
    }
}

[Flags]
public enum MessageType
{
    MyMessage1,
    MyMessage2
}

public class Wrapper
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageType MessageType { get; set; }
    public BaseMessage Message { get; set; }
}

public abstract class BaseMessage
{
    public int Id { get; set; }
    public string? Client { get; set; }
    public DateTime DateTime { get; set; }
}

public class MyMessage1 : BaseMessage
{
    public float Message { get; set; }
}

public class MyMessage2 : BaseMessage
{
    public float Message { get; set; }
}

