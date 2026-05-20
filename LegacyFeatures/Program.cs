using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

Console.WriteLine("=== Legacy Features Demo — APIs removed/changed in .NET 9 ===\n");

RunBinaryFormatterDemo();
RunUriEscapeDemo();
RunThreadAbortDemo();

// -----------------------------------------------------------------------
// Rule 1: BinaryFormatter removed in .NET 9
// Still available in .NET 8 via EnableUnsafeBinaryFormatterSerialization
// In .NET 9: the type no longer exists — compile error.
// Replacement: System.Text.Json, XmlSerializer, or MessagePack.
// -----------------------------------------------------------------------
static void RunBinaryFormatterDemo()
{
    Console.WriteLine("[Rule 1] BinaryFormatter — removed in .NET 9");

    var payload = new LegacyPayload { Id = 42, Label = "legacy" };

    using var stream = new MemoryStream();

#pragma warning disable SYSLIB0011
    var formatter = new BinaryFormatter();
    formatter.Serialize(stream, payload);
    stream.Seek(0, SeekOrigin.Begin);
    var result = (LegacyPayload)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011

    Console.WriteLine($"  Serialized/deserialized: Id={result.Id}, Label={result.Label}");
    Console.WriteLine("  .NET 9 fix: replace with System.Text.Json.JsonSerializer\n");
}

// -----------------------------------------------------------------------
// Rule 2: Uri.EscapeUriString deprecated — use Uri.EscapeDataString
// Marked [Obsolete] in .NET 7, produces CS0618 warning in .NET 8.
// The method still works in .NET 8 but signals intent to remove it.
// Replacement: Uri.EscapeDataString for query values.
// -----------------------------------------------------------------------
static void RunUriEscapeDemo()
{
    Console.WriteLine("[Rule 2] Uri.EscapeUriString — deprecated, use EscapeDataString");

    string raw = "hello world & C# rocks!";

#pragma warning disable CS0618
    string old = Uri.EscapeUriString(raw);
#pragma warning restore CS0618

    string modern = Uri.EscapeDataString(raw);

    Console.WriteLine($"  EscapeUriString  (old):    {old}");
    Console.WriteLine($"  EscapeDataString (correct): {modern}");
    Console.WriteLine("  Note: EscapeUriString leaves '&' unescaped — EscapeDataString encodes it properly.\n");
}

// -----------------------------------------------------------------------
// Rule 3: Thread.Abort() throws PlatformNotSupportedException in .NET 5+
// Was valid on .NET Framework; removed from .NET Core entirely.
// In .NET 8 the overload still compiles but always throws at runtime.
// Replacement: CancellationToken cooperative cancellation.
// -----------------------------------------------------------------------
static void RunThreadAbortDemo()
{
    Console.WriteLine("[Rule 3] Thread.Abort() — throws PlatformNotSupportedException in .NET 5+");

    using var cts = new CancellationTokenSource();

    var thread = new Thread(() =>
    {
        Console.WriteLine("  Thread started — running until cancelled...");
        while (!cts.Token.IsCancellationRequested)
            Thread.Sleep(50);
        Console.WriteLine("  Thread stopped via CancellationToken (modern way).");
    });

    thread.Start();
    Thread.Sleep(100);

    try
    {
        thread.Abort();
    }
    catch (PlatformNotSupportedException ex)
    {
        Console.WriteLine($"  Thread.Abort() threw: {ex.GetType().Name} — {ex.Message}");
        Console.WriteLine("  Falling back to CancellationToken...");
        cts.Cancel();
    }

    thread.Join();
    Console.WriteLine("  .NET 9 fix: design with CancellationToken from the start.\n");
}

[Serializable]
public sealed class LegacyPayload
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
}
