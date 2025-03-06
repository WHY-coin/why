using System;
using System.Runtime.InteropServices;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Why
{
#if !UNITY_5_3_OR_NEWER
    public sealed class WhyProgram
    {
        public static void Main()
        {
            IWhyMemoryAllocator allocator = new WhyMemoryAllocator();
            IWhyMemoryTransformer transformer = new WhyMemoryTransformer();
            IWhyLogger logger = new ConsoleWhyLogger();
        
            IWhyProcessor processor = new WhyProcessor(allocator, transformer, logger);

            processor.Process();
        }
    }
#else
    public sealed class Why : MonoBehaviour
    {
        private void Awake()
        {
            IWhyMemoryAllocator allocator = new WhyMemoryAllocator();
            IWhyMemoryTransformer transformer = new WhyMemoryTransformer();
            IWhyLogger logger = new UnityWhyLogger();
            
            IWhyProcessor processor = new WhyProcessor(allocator, transformer, logger);
            
            processor.Process();
        }
    }
#endif

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

    public unsafe interface IWhyMemoryTransformer
    {
        void Transform(byte* ptr);
    }

    public sealed unsafe class WhyMemoryTransformer : IWhyMemoryTransformer
    {
        public void Transform(byte* ptr)
        {
            *(ptr + 0) = '\u0057' ^ 0x00; // W
            *(ptr + 1) = '\u0068' ^ 0x00; // h
            *(ptr + 2) = '\u0079' ^ 0x00; // y
            *(ptr + 3) = '\u003f' ^ 0x00; // ?
        }
    }

    public interface IWhyLogger
    {
        void Log(char[] buffer);
    }

#if !UNITY_5_3_OR_NEWER
    public sealed class ConsoleWhyLogger : IWhyLogger
    {
        public void Log(char[] buffer) 
        {
            Console.WriteLine(buffer);
        }
    }
#else
    public sealed class UnityWhyLogger : IWhyLogger
    {
        public void Log(char[] buffer)
        {
            string result = new string(buffer);
            
            Debug.Log(result);
        }
    }
#endif

    public interface IWhyProcessor
    {
        void Process();
    }

    public sealed class WhyProcessor : IWhyProcessor
    {
        private readonly IWhyMemoryAllocator _allocator;
        private readonly IWhyMemoryTransformer _transformer;
        private readonly IWhyLogger _logger;

        public WhyProcessor(IWhyMemoryAllocator allocator, IWhyMemoryTransformer transformer, IWhyLogger logger) 
        {
            _allocator = allocator;
            _transformer = transformer;
            _logger = logger;
        }

        public unsafe void Process()
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

                _logger.Log(buffer);
            }
            finally
            {
                _allocator.Free(memory);
            }
        }
    }
}