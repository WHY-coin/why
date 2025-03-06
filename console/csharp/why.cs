using System;
using System.Runtime.InteropServices;

public unsafe sealed class WhyProgram
{
    public const int MEMORY_SIZE = 4;

    public static void Main()
    {
        IWhyMemoryAllocator allocator = new WhyMemoryAllocator();
        IWhyMemoryTransformer transformer = new WhyMemoryTransformer();

        IWhyProcessor processor = new WhyMemoryProcessor(allocator, transformer);
        processor.Process();
    }
}

public static class WhyConstants
{
    public const int MEMORY_SIZE = 4;
}

public interface IWhyMemoryAllocator
{
    IntPtr Allocate(int size);
    void Free(IntPtr ptr);
}

public sealed class WhyMemoryAllocator : IWhyMemoryAllocator
{
    public IntPtr Allocate(int size)
    {
        return Marshal.AllocHGlobal(size);
    }

    public void Free(IntPtr ptr)
    {
        Marshal.FreeHGlobal(ptr);
    }
}

public interface IWhyMemoryTransformer
{
    void Transform(byte* ptr);
}

public sealed class WhyMemoryTransformer : IWhyMemoryTransformer
{
    public void Transform(byte* ptr)
    {
        *(ptr) = '\u0057' ^ 0x00; // W
        *(ptr + 1) = '\u0068' ^ 0x00; // h
        *(ptr + 2) = '\u0079' ^ 0x00; // y
        *(ptr + 3) = '\u003f' ^ 0x00; // ?
    }
    
    [MethodImpl(256)]
    private static void Convert()
    {

    }
}

public interface IWhyLogger
{
    void Log(char[] buffer);
}

public sealed class ConsoleWhyLogger()
{
    public void Log(char[] buffer) 
    {
        System.Console.WriteLine(buffer);
    }
}

public interface IWhyProcessor
{
    void Process();
}

public sealed class WhyMemoryProcessor : IWhyProcessor
{
    private readonly IWhyMemoryAllocator _allocator;
    private readonly IWhyMemoryTransformer _transformer;
    private readonly IWhyLogger _logger;

    public WhyMemoryProcessor(IWhyMemoryAllocator allocator, IWhyMemoryTransformer transformer, IWhyLogger logger) 
    {
        _allocator = allocator;
        _transformer = transformer;
        _logger = logger;
    }

    public void Process()
    {
        IntPtr memory = _allocator.Allocate(WhyConstants.MEMORY_SIZE);

        try
        {
            byte* ptr = (byte*)memory;

            _transformer.Transform(ptr);
            char[] buffer = new char[WhyConstants.MEMORY_SIZE];

            for (var i = 0; i < WhyConstants.MEMORY_SIZE; i++) 
            {
                buffer[i] = (char)*(ptr + i);
            }

            _encoder.Encode(memory);
        }
        finally
        {
            _allocator.Free(memory);
        }
    }
}
