#if !UNITY_5_3_OR_NEWER
#define IS_CONSOLE_APP
#else
#define IS_UNITY
#endif

using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Numerics;
using System.Reflection;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif


namespace Why
{
    // ! ======== Entry Point ========
#if IS_CONSOLE_APP
    public sealed class WhyProgram
    {
        public static void Main(string[] args)
        {
            bool enableLogging = args.Contains("--log");
            bool fastMode = args.Contains("--fast");
            
            IWhyProcessor processor = WhyFactory.CreateProcessor(enableLogging, fastMode);
            
            processor.Process();
        }
    }
#endif

#if IS_UNITY
    public sealed class Why : MonoBehaviour
    {
        [SerializeField] private bool _enableLogging = true;
        [SerializeField] private bool _fastMode = false;

        private void Awake()
        {
            IWhyProcessor processor = WhyFactory.CreateProcessor(_enableLogging, _fastMode);

            processor.Process();
        }
    }
#endif
    
    public struct WhyObject
    {
        [JsonNode("w")]
        public char W;
        [JsonNode("h")]
        public char H;
        [JsonNode("y")]
        public char Y;
    }
    
    // ! ======== Constants ========
    
    public static class WhyConstants
    {
        public const int SIZE = 3;
        public const int MATRIX_SIZE = 4;
        public const int POLYNOMIAL_DEGREE = 7;
        public const int QUANTUM_ITERATIONS = 82;
    }
    
    // ! ======== Factory ========

    public static class WhyFactory
    {
        public static IWhyProcessor CreateProcessor(bool enableLogging, bool fastMode)
        {
            IWhyMemoryAllocator allocator = new WhyMemoryAllocator();
            IWhyMemoryTransformer transformer = new WhyMemoryTransformer();
            IWhyCrypto crypto = new WhyCrypto();
            IWhyQuantumSuperposition quantum = new WhyQuantumSuperposition();
            IWhyPolynomialExpander polynomial = new WhyPolynomialExpander();
            IWhyMatrixTransformer matrix = new WhyMatrixTransformer();
            
            IWhyLoggerBackend loggerBackend = 
#if IS_UNITY
                new UnityWhyLoggerBackend();
#else
                new ConsoleWhyLoggerBackend();
#endif
            
            IWhyLogger logger = new BasicWhyLogger(loggerBackend, enableLogging);
            
            return new WhyProcessor(allocator, transformer, crypto, quantum, polynomial, matrix, logger, fastMode);
        }
    }
    
    // ! ======== Memory ========

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
        private const string WhyJsonText = "{\n\"w\" : \"W\",\n\"h\" : \"h\",\n\"y\" : \"y\",\n}";
        
        public void Transform(byte* ptr)
        {
            var why = JsonDeserializer.Deserialize<WhyObject>(WhyJsonText);
            
            *(ptr + 0) = (byte)(why.W ^ 0x00);
            *(ptr + 1) = (byte)(why.H ^ 0x00);
            *(ptr + 2) = (byte)(why.Y ^ 0x00);
        }
    }
    
    // ! ======== Crypto ========

    public interface IWhyCrypto
    {
        byte[] Encrypt(byte[] data, byte[] key);
    }

    public sealed class WhyCrypto : IWhyCrypto
    {
        public byte[] Encrypt(byte[] data, byte[] key)
        {
            byte[] combined = new byte[data.Length + key.Length];
            Buffer.BlockCopy(data, 0, combined, 0, data.Length);
            Buffer.BlockCopy(key, 0, combined, data.Length, key.Length);
            
            byte[] intermediateHash = SHA256.HashData(combined);
            
            byte[] expandedKey = ExpandKey(key, 32);
            byte[] xorData = XorBytes(intermediateHash, expandedKey);
            
            byte[] permutation = PermuteBytes(xorData);
            byte[] substitution = SubstituteBytes(permutation);
            
            BigInteger bigInt = new BigInteger(substitution);
            bigInt = BigInteger.ModPow(bigInt, 65537, BigInteger.Parse("340282366920938463463374607431768211297"));
            
            byte[] bigIntBytes = bigInt.ToByteArray();
            byte[] feistelOutput = FeistelNetwork(bigIntBytes);
            
            byte[] aesKey = SHA256.HashData(expandedKey);
            byte[] encryptedData = AesEncrypt(feistelOutput, aesKey);
            
            return SHA256.HashData(encryptedData);
        }
        
        private static byte[] ExpandKey(byte[] key, int length)
        {
            byte[] expandedKey = new byte[length];
            
            for (int i = 0; i < length; i++)
            {
                expandedKey[i] = key[i % key.Length];
            }
            
            for (int i = 1; i < length; i++)
            {
                expandedKey[i] = (byte)((expandedKey[i] + expandedKey[i - 1]) % 256);
            }
            
            return expandedKey;
        }
        
        private static byte[] XorBytes(byte[] data, byte[] key)
        {
            byte[] result = new byte[data.Length];
            
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
            
            return result;
        }
        
        private static byte[] PermuteBytes(byte[] data)
        {
            byte[] result = new byte[data.Length];
            
            for (int i = 0; i < data.Length; i++)
            {
                int newPos = (i * 7 + 3) % data.Length;
                result[newPos] = data[i];
            }
            
            return result;
        }
        
        private static byte[] SubstituteBytes(byte[] data)
        {
            byte[] sbox = GenerateSBox();
            byte[] result = new byte[data.Length];
            
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = sbox[data[i]];
            }
            
            return result;
        }
        
        private static byte[] GenerateSBox()
        {
            byte[] sbox = new byte[256];
            
            for (int i = 0; i < 256; i++)
            {
                sbox[i] = (byte)i;
            }
            
            byte j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (byte)((j + sbox[i] + 42) % 256);
                
                (sbox[i], sbox[j]) = (sbox[j], sbox[i]);
            }
            
            return sbox;
        }
        
        private byte[] FeistelNetwork(byte[] data)
        {
            if (data.Length % 2 != 0)
            {
                byte[] padded = new byte[data.Length + 1];
                Buffer.BlockCopy(data, 0, padded, 0, data.Length);
                data = padded;
            }
            
            int halfSize = data.Length / 2;
            byte[] left = new byte[halfSize];
            byte[] right = new byte[halfSize];
            
            Buffer.BlockCopy(data, 0, left, 0, halfSize);
            Buffer.BlockCopy(data, halfSize, right, 0, halfSize);
            
            for (int round = 0; round < 16; round++)
            {
                byte[] temp = left;
                left = right;
                
                byte[] roundKey = GenerateRoundKey(round);
                byte[] feistelOutput = FeistelFunction(right, roundKey);
                
                for (int i = 0; i < halfSize; i++)
                {
                    right[i] = (byte)(temp[i] ^ feistelOutput[i % feistelOutput.Length]);
                }
            }
            
            byte[] result = new byte[data.Length];
            Buffer.BlockCopy(left, 0, result, 0, halfSize);
            Buffer.BlockCopy(right, 0, result, halfSize, halfSize);
            
            return result;
        }
        
        private byte[] GenerateRoundKey(int round)
        {
            byte[] key = new byte[8];
            
            for (int i = 0; i < 8; i++)
            {
                key[i] = (byte)((i + 1) * (round + 1) % 256);
            }
            
            return key;
        }
        
        private byte[] FeistelFunction(byte[] data, byte[] key)
        {
            byte[] expanded = new byte[data.Length * 2];
            
            for (int i = 0; i < data.Length; i++)
            {
                expanded[i * 2] = (byte)((data[i] << 4) & 0xF0);
                expanded[i * 2 + 1] = (byte)(data[i] & 0x0F);
            }
            
            for (int i = 0; i < expanded.Length; i++)
            {
                expanded[i] = (byte)(expanded[i] ^ key[i % key.Length]);
            }
            
            byte[] compressed = new byte[data.Length];
            
            for (int i = 0; i < data.Length; i++)
            {
                compressed[i] = (byte)((expanded[i * 2] & 0xF0) | (expanded[i * 2 + 1] & 0x0F));
            }
            
            return compressed;
        }
        
        private byte[] AesEncrypt(byte[] data, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = new byte[16];
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                
                using (var encryptor = aes.CreateEncryptor())
                {
                    return encryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }
    }
    
    // ! ======== Quantum Module ========
    
    public interface IWhyQuantumSuperposition
    {
        byte[] CollapseWaveFunction(byte[] input);
    }
    
    public sealed class WhyQuantumSuperposition : IWhyQuantumSuperposition
    {
        private readonly Random _random = new Random(42);
        
        public byte[] CollapseWaveFunction(byte[] input)
        {
            double[] probabilities = CalculateProbabilities(input);
            byte[] collapsed = new byte[input.Length];
            
            for (int i = 0; i < WhyConstants.QUANTUM_ITERATIONS; i++)
            {
                Simulate(probabilities);
            }
            
            for (int i = 0; i < input.Length; i++)
            {
                collapsed[i] = (byte)(input[i] ^ QuantumBitFlip(probabilities[i % probabilities.Length]));
            }
            
            return collapsed;
        }
        
        private static double[] CalculateProbabilities(byte[] input)
        {
            double[] probabilities = new double[input.Length];
            
            for (int i = 0; i < input.Length; i++)
            {
                probabilities[i] = (double)input[i] / 256.0;
            }
            
            return probabilities;
        }
        
        private void Simulate(double[] probabilities)
        {
            for (int i = 0; i < probabilities.Length; i++)
            {
                double p = probabilities[i];
                int bitCount = CountBits((byte)(p * 256));
                
                probabilities[i] = (p + Math.Sin(p * Math.PI)) / 2;
                
                for (int j = 0; j < bitCount; j++)
                {
                    HadamardTransform(probabilities, i);
                }
            }
        }
        
        private static void HadamardTransform(double[] probabilities, int index)
        {
            if (index + 1 < probabilities.Length)
            {
                double p1 = probabilities[index];
                double p2 = probabilities[index + 1];
                
                probabilities[index] = (p1 + p2) / Math.Sqrt(2);
                probabilities[index + 1] = (p1 - p2) / Math.Sqrt(2);
            }
        }
        
        private static int CountBits(byte value)
        {
            int count = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((value & (1 << i)) != 0)
                {
                    count++;
                }
            }
            return count;
        }
        
        private byte QuantumBitFlip(double probability)
        {
            byte result = 0;
            
            for (int i = 0; i < 8; i++)
            {
                if (_random.NextDouble() < probability)
                {
                    result |= (byte)(1 << i);
                }
            }
            
            return result;
        }
    }
    
    // ! ======== Polynomial Module ========
    
    public interface IWhyPolynomialExpander
    {
        byte[] ExpandPolynomial(byte[] input);
    }
    
    public sealed class WhyPolynomialExpander : IWhyPolynomialExpander
    {
        public byte[] ExpandPolynomial(byte[] input)
        {
            byte[] coefficients = GenerateCoefficients(input);
            byte[] expanded = new byte[input.Length];
            
            for (int i = 0; i < input.Length; i++)
            {
                expanded[i] = EvaluatePolynomial(coefficients, input[i]);
            }
            
            return expanded;
        }
        
        private byte[] GenerateCoefficients(byte[] input)
        {
            byte[] coefficients = new byte[WhyConstants.POLYNOMIAL_DEGREE + 1];
            
            for (int i = 0; i <= WhyConstants.POLYNOMIAL_DEGREE; i++)
            {
                int sum = 0;
                for (int j = 0; j < input.Length; j++)
                {
                    sum += (input[j] * (j + 1)) % 256;
                }
                
                coefficients[i] = (byte)((sum * (i + 1)) % 256);
            }
            
            return coefficients;
        }
        
        private byte EvaluatePolynomial(byte[] coefficients, byte x)
        {
            int result = 0;
            int power = 1;
            
            for (int i = 0; i <= WhyConstants.POLYNOMIAL_DEGREE; i++)
            {
                result = (result + coefficients[i] * power) % 256;
                power = (power * x) % 256;
            }
            
            return (byte)result;
        }
    }
    
    // ! ======== Matrix Module ========
    
    public interface IWhyMatrixTransformer
    {
        byte[] TransformVector(byte[] input);
    }
    
    public sealed class WhyMatrixTransformer : IWhyMatrixTransformer
    {
        public byte[] TransformVector(byte[] input)
        {
            int[,] matrix = GenerateMatrix(input);
            int[] vector = PadInputToVector(input);
            
            int[] result = MultiplyMatrixVector(matrix, vector);
            
            return ConvertVectorToBytes(result);
        }
        
        private int[,] GenerateMatrix(byte[] input)
        {
            int[,] matrix = new int[WhyConstants.MATRIX_SIZE, WhyConstants.MATRIX_SIZE];
            
            for (int i = 0; i < WhyConstants.MATRIX_SIZE; i++)
            {
                for (int j = 0; j < WhyConstants.MATRIX_SIZE; j++)
                {
                    matrix[i, j] = (input[(i + j) % input.Length] * (i + 1) * (j + 1)) % 256;
                }
            }
            
            return matrix;
        }
        
        private int[] PadInputToVector(byte[] input)
        {
            int[] vector = new int[WhyConstants.MATRIX_SIZE];
            
            for (int i = 0; i < WhyConstants.MATRIX_SIZE; i++)
            {
                vector[i] = i < input.Length ? input[i] : 0;
            }
            
            return vector;
        }
        
        private int[] MultiplyMatrixVector(int[,] matrix, int[] vector)
        {
            int[] result = new int[WhyConstants.MATRIX_SIZE];
            
            for (int i = 0; i < WhyConstants.MATRIX_SIZE; i++)
            {
                int sum = 0;
                for (int j = 0; j < WhyConstants.MATRIX_SIZE; j++)
                {
                    sum += (matrix[i, j] * vector[j]) % 256;
                }
                
                result[i] = sum % 256;
            }
            
            return result;
        }
        
        private byte[] ConvertVectorToBytes(int[] vector)
        {
            byte[] result = new byte[vector.Length];
            
            for (int i = 0; i < vector.Length; i++)
            {
                result[i] = (byte)vector[i];
            }
            
            return result;
        }
    }
    
    // ! ======== Logging ========
    
    public interface IWhyLogger
    {
        void Log(char[] buffer);
        void LogStep(string message, string category = "");
    }

    public interface IWhyLoggerBackend
    {
        void Log(char[] buffer);
    }
    
#if IS_CONSOLE_APP
    public sealed class ConsoleWhyLoggerBackend : IWhyLoggerBackend
    {
        public void Log(char[] buffer)
        {
            Console.WriteLine(buffer);
        }
    }
#endif
    
#if IS_UNITY
    public sealed class UnityWhyLoggerBackend : IWhyLoggerBackend
    {
        public void Log(char[] buffer)
        {
            var str = new string(buffer);
            
            Debug.Log(str);
        }
    }
#endif
    
    public sealed class BasicWhyLogger : IWhyLogger
    {
        private readonly IWhyLoggerBackend _loggerBackend;
        private readonly StringBuilder _sb;
        private readonly bool _enableLogStep;
        
        public BasicWhyLogger(IWhyLoggerBackend loggerBackend, bool enableLogStep = true)
        {
            _loggerBackend = loggerBackend;
            _enableLogStep = enableLogStep;
            
            _sb = new StringBuilder(64);
        }
        
        public void Log(char[] buffer) 
        {
            _loggerBackend.Log(buffer);
        }
        
        public void LogStep(string message, string category = "")
        {
            if (!_enableLogStep) return;
            
            if (!string.IsNullOrEmpty(category))
            {
                _sb.Append('[')
                    .Append(category)
                    .Append(']')
                    .Append(' ');
            }
            
            _sb.Append(message);
            
            _loggerBackend.Log(_sb.ToString().ToCharArray());
            
            _sb.Clear();
        }
    }

    // ! ======== Processor ========
    
    public interface IWhyProcessor
    {
        void Process();
    }

    public sealed unsafe class WhyProcessor : IWhyProcessor
    {
        private readonly IWhyMemoryAllocator _allocator;
        private readonly IWhyMemoryTransformer _transformer;
        private readonly IWhyCrypto _crypto;
        private readonly IWhyQuantumSuperposition _quantum;
        private readonly IWhyPolynomialExpander _polynomial;
        private readonly IWhyMatrixTransformer _matrix;
        private readonly IWhyLogger _logger;

        private readonly bool _fastMode;

        public WhyProcessor(
            IWhyMemoryAllocator allocator,
            IWhyMemoryTransformer transformer,
            IWhyCrypto crypto,
            IWhyQuantumSuperposition quantum,
            IWhyPolynomialExpander polynomial,
            IWhyMatrixTransformer matrix,
            IWhyLogger logger,
            bool fastMode = false)
        {
            _allocator = allocator;
            _transformer = transformer;
            _crypto = crypto;
            _quantum = quantum;
            _polynomial = polynomial;
            _matrix = matrix;
            _logger = logger;
            
            _fastMode = fastMode;
        }
        
        public void Process()
        {
            _logger.LogStep("Initiating Why protocol...", "SYSTEM");
            
            _logger.LogStep("Calculating memory requirements...", "MEMORY");
            _logger.LogStep("Allocating memory...", "MEMORY");
            
            IntPtr memory = _allocator.Allocate(WhyConstants.SIZE);
            try
            {
                byte* ptr = (byte*)memory;

                _logger.LogStep("Initializing memory values...", "MEMORY");
                for (int i = 0; i < WhyConstants.SIZE; i++)
                {
                    *(ptr + i) = 0;
                }
                
                _logger.LogStep("Generating prime sequences...", "MATH");
                _logger.LogStep("Calculating Fibonacci numbers...", "MATH");
                _logger.LogStep("Transforming memory with complex algorithms...", "MEMORY");
                _transformer.Transform(ptr);
                
                _logger.LogStep("Preparing vector space...", "VECTOR");
                _logger.LogStep("Initializing dimensional arrays...", "VECTOR");

                byte[] intermediateData = new byte[WhyConstants.SIZE];
                for (int i = 0; i < WhyConstants.SIZE; i++)
                {
                    intermediateData[i] = *(ptr + i);
                }
                
                _logger.LogStep("Applying quantum superposition...", "QUANTUM");
                byte[] quantumData = _quantum.CollapseWaveFunction(intermediateData);
                _logger.LogStep($"Quantum state collapsed to: {BitConverter.ToString(quantumData)}", "QUANTUM");
                
                _logger.LogStep("Expanding polynomial coefficients...", "POLYNOMIAL");
                byte[] polynomialData = _polynomial.ExpandPolynomial(quantumData);
                _logger.LogStep($"Polynomial expansion result: {BitConverter.ToString(polynomialData)}", "POLYNOMIAL");
                
                _logger.LogStep("Performing matrix transformations...", "MATRIX");
                byte[] matrixData = _matrix.TransformVector(polynomialData);
                _logger.LogStep($"Matrix transformation result: {BitConverter.ToString(matrixData)}", "MATRIX");
                
                _logger.LogStep("Performing vector space normalization...", "VECTOR");
                for (int i = 0; i < WhyConstants.SIZE; i++)
                {
                    ComplexCalculation(ref polynomialData[i % polynomialData.Length], ref matrixData[i % matrixData.Length]);

                    if (!_fastMode)
                    {
                        PerformCpuIntensiveWhyOperation();
                    }
                }

                char[] buffer = new char[WhyConstants.SIZE];
                
                _logger.LogStep("Converting memory to buffer...", "BUFFER");
                for (int i = 0; i < WhyConstants.SIZE; i++)
                {
                    buffer[i] = (char)*(ptr + i);
                    
                    if (!_fastMode)
                    {
                        PerformCpuIntensiveWhyOperation();
                    }
                }

                _logger.LogStep("Preparing cryptographic context...", "CRYPTO");
                _logger.LogStep("Generating encryption keys...", "CRYPTO");
                _logger.LogStep("Encrypting data...", "CRYPTO");
                
                byte[] encrypted = _crypto.Encrypt(Encoding.UTF8.GetBytes(buffer), new byte[]{ 42, 17, 99 });

                _logger.LogStep($"Encrypted hash: {BitConverter.ToString(encrypted)}", "CRYPTO");
                _logger.LogStep("Validation complete, producing final output...", "OUTPUT");

                _logger.Log(buffer);
            }
            finally
            {
                _logger.LogStep("Cleaning up resources...", "SYSTEM");
                _logger.LogStep("Free memory...", "MEMORY");
                
                _allocator.Free(memory);
                
                _logger.LogStep("Why protocol completed successfully.", "SYSTEM");
            }
        }
        
        private void ComplexCalculation(ref byte data1, ref byte data2)
        {
            for (int i = 0; i < 1000000; i++)
            {
                data1 = (byte)((data1 * data2 + i) % 256);
                data2 = (byte)((data2 * data1 + i) % 256);
                
                if (i % 100000 == 0)
                {
                    data1 = (byte)(data1 ^ data2);
                }
            }
            
            data1 = (byte)(data1 ^ 0x00);
            data2 = (byte)(data2 ^ 0x00);
        }
        
        // Why is this here? No one knows...
        private static void PerformCpuIntensiveWhyOperation()
        {
            double result = 0;
            for (int i = 0; i < 90000000; i++)
            {
                result += Math.Sin(i) * Math.Cos(i);
                if (i % 1000000 == 0)
                {
                    result = Math.Sqrt(result);
                }
            }
        }
    }
    
    // ! ======== Json ========
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class JsonNodeAttribute : Attribute
    {
        public readonly string Key;
        
        public JsonNodeAttribute()
        {
        }
        
        public JsonNodeAttribute(string key)
        {
            Key = key;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class JsonIgnoreAttribute : Attribute
    {
        public JsonIgnoreAttribute()
        {
        }
    }
    
    public static class JsonDeserializer
    {
        public static T Deserialize<T>(string json)
        {
            return (T)Deserialize(typeof(T), JsonTokenizer.Tokenize(json).GetEnumerator());
        } 
        
        private static object Deserialize(Type type, IEnumerator<JsonTokenizer.Token> tokens)
        {
            if (type.IsValueType)
            {
                if (type == typeof(int))        return int.Parse(tokens.Current.Value, CultureInfo.InvariantCulture);
                if (type == typeof(double))     return double.Parse(tokens.Current.Value, CultureInfo.InvariantCulture);
                if (type == typeof(float))      return float.Parse(tokens.Current.Value, CultureInfo.InvariantCulture);
                if (type == typeof(bool))       return bool.Parse(tokens.Current.Value);
                if (type == typeof(char))       return char.Parse(tokens.Current.Value);
                if (type == typeof(long))       return long.Parse(tokens.Current.Value, CultureInfo.InvariantCulture);
                if (type == typeof(short))      return short.Parse(tokens.Current.Value, CultureInfo.InvariantCulture);
                if (type == typeof(byte))       return byte.Parse(tokens.Current.Value, CultureInfo.InvariantCulture);
                if (type == typeof(decimal))    return decimal.Parse(tokens.Current.Value, CultureInfo.InvariantCulture);
                if (type == typeof(uint))       return uint.Parse(tokens.Current.Value, CultureInfo.InvariantCulture);
                if (type == typeof(ulong))      return ulong.Parse(tokens.Current.Value, CultureInfo.InvariantCulture);
                if (type == typeof(ushort))     return ushort.Parse(tokens.Current.Value, CultureInfo.InvariantCulture);
            }

            if (type == typeof(string))
            {
                return tokens.Current.Value;
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var list = new List<object>();
                
                while (tokens.MoveNext() && tokens.Current.Type != JsonTokenizer.TokenType.ArrayEnd)
                {
                    list.Add(Deserialize(elementType, tokens));
                }
                
                return list.ToArray();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = type.GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(elementType);
                var list = (IList)Activator.CreateInstance(listType);
                
                while (tokens.MoveNext() && tokens.Current.Type != JsonTokenizer.TokenType.ArrayEnd)
                {
                    list.Add(Deserialize(elementType, tokens));
                }
                
                return list;
            }

            if (type == typeof(IDictionary) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
            {
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];
                var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                var dict = (IDictionary)Activator.CreateInstance(dictType);
                
                var expectKey = true;
                object key = null;
                
                while (tokens.MoveNext() && tokens.Current.Type != JsonTokenizer.TokenType.ObjectEnd)
                {
                    if (expectKey && tokens.Current.Type == JsonTokenizer.TokenType.String)
                    {
                        key = Deserialize(keyType, tokens);
                        expectKey = false;
                    }
                    else if (!expectKey && tokens.Current.Type == JsonTokenizer.TokenType.Colon)
                    {
                        tokens.MoveNext();
                        var value = Deserialize(valueType, tokens);
                        dict.Add(key, value);
                        expectKey = true;
                    }
                }
                
                return dict;
            }

            if (type.IsClass || type.IsValueType)
            {
                var instance = Activator.CreateInstance(type);
                
                while (tokens.MoveNext() && tokens.Current.Type != JsonTokenizer.TokenType.ObjectEnd)
                {
                    if (tokens.Current.Type == JsonTokenizer.TokenType.String)
                    {
                        var propertyName = tokens.Current.Value;
                        tokens.MoveNext();
                        if (tokens.Current.Type == JsonTokenizer.TokenType.Colon)
                        {
                            tokens.MoveNext();

                            var member = type
                                .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                                .FirstOrDefault(m =>
                                    (m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property) &&
                                    m.GetCustomAttribute<JsonIgnoreAttribute>() == null &&
                                    m.GetCustomAttribute<JsonNodeAttribute>()?.Key == propertyName);

                            if (member != null)
                            {
                                object value;
                                
                                if (member is FieldInfo field)
                                {
                                    value = Deserialize(field.FieldType, tokens);
                                    field.SetValue(instance, value);
                                }
                                else if (member is PropertyInfo property)
                                {
                                    value = Deserialize(property.PropertyType, tokens);
                                    property.SetValue(instance, value);
                                }
                            }
                        }
                    }
                }

                return instance;
            }

            return null;
        }
    }
    
    public static class JsonTokenizer
    {
        public static IEnumerable<Token> Tokenize(string json, bool suppressError = true)
        {
            var i = 0;
            while (i < json.Length)
            {
                if (char.IsWhiteSpace(json[i]))
                {
                    i++;
                    continue;
                }
                
                switch (json[i])
                {
                    case '{':   yield return new Token(TokenType.ObjectStart, "{"); i++; break;
                    case '}':   yield return new Token(TokenType.ObjectEnd, "}"); i++; break;
                    case '[':   yield return new Token(TokenType.ArrayStart, "["); i++; break;
                    case ']':   yield return new Token(TokenType.ArrayEnd, "]"); i++; break;
                    case ':':   yield return new Token(TokenType.Colon, ":"); i++; break;
                    case ',':   yield return new Token(TokenType.Comma, ","); i++; break;
                    default:
                    {
                        if (i + 4 < json.Length && json.Substring(i, 4) == "true")
                        {
                            yield return new Token(TokenType.True, "true");
                            i += 4;
                        }
                        else if (i + 5 < json.Length && json.Substring(i, 5) == "false")
                        {
                            yield return new Token(TokenType.False, "false");
                            i += 5;
                        }
                        else if (i + 4 < json.Length && json.Substring(i, 4) == "null")
                        {
                            yield return new Token(TokenType.Null, "null");
                            i += 4;
                        }
                        else if (json[i] == '"')
                        {
                            var sb = new StringBuilder();
                            
                            i++;
                            while (i < json.Length)
                            {
                                if (json[i] == '\\')
                                {
                                    if (++i < json.Length)
                                    {
                                        switch (json[i])
                                        {
                                            case '"':   sb.Append('"');     break;
                                            case '\\':  sb.Append('\\');    break;
                                            case '/':   sb.Append('/');     break;
                                            case 'b':   sb.Append('\b');    break;
                                            case 'f':   sb.Append('\f');    break;
                                            case 'n':   sb.Append('\n');    break;
                                            case 'r':   sb.Append('\r');    break;
                                            case 't':   sb.Append('\t');    break;
                                            case 'u':
                                            {
                                                if (json.Length > i + 4 && int.TryParse(json.Substring(i + 1, 4),
                                                        NumberStyles.HexNumber, null, out var h))
                                                {
                                                    sb.Append((char)h);
                                                    i += 4;
                                                }
                                                break;
                                            }
                                            default:    sb.Append(json[i]); break;
                                        }
                                    }
                                }
                                else if (json[i] == '"')
                                {
                                    break;
                                }
                                else
                                {
                                    sb.Append(json[i]);
                                }
                                
                                i++;
                            }
                            
                            yield return new Token(TokenType.String, sb.ToString());
                            
                            i++;
                        }
                        else if (char.IsDigit(json[i]) || json[i] == '-')
                        {
                            var sb = new StringBuilder();

                            if (json[i] == '-')
                            {
                                AppendCurrentChar(sb, json, ref i);
                            }

                            AppendDigitSequence(ref i, json, sb);

                            if (i < json.Length && json[i] == '.')
                            {
                                AppendCurrentChar(sb, json, ref i);
                                AppendDigitSequence(ref i, json, sb);
                            }

                            if (i < json.Length && (json[i] == 'e' || json[i] == 'E'))
                            {
                                AppendCurrentChar(sb, json, ref i);

                                if (i < json.Length && (json[i] == '+' || json[i] == '-'))
                                {
                                    AppendCurrentChar(sb, json, ref i);
                                }

                                AppendDigitSequence(ref i, json, sb);
                            }

                            yield return new Token(TokenType.Number, sb.ToString());
                        }
                        else
                        {
                            if (!suppressError)
                            {
                                throw new Exception($"Unexpected character '{json[i]}' at position {i}");
                            }
                            
#if IS_UNITY
                            UnityEngine.Debug.LogError($"Unexpected character '{json[i]}' at position {i}");
#endif
                            
                            i++;
                        }

                        break;
                    }
                }
            }
        }

        private static void AppendDigitSequence(ref int i, string json, StringBuilder? sb)
        {
            while (i < json.Length && char.IsDigit(json[i]))
            {
                sb.Append(json[i]);
                i++;
            }
        }

        private static void AppendCurrentChar(StringBuilder? sb, string json, ref int i)
        {
            sb.Append(json[i]);
            i++;
        }

        public readonly struct Token
        {
            public readonly TokenType Type;
            public readonly string Value;
            
            public Token(TokenType type, string value)
            {
                Type = type;
                Value = value;
            }
            
            public override string ToString() => $"{Type.ToString()}: {Value}";
        }
        
        public enum TokenType
        {
            Null,
            True,
            False,
            String,
            Number,
            ObjectStart,
            ObjectEnd,
            ArrayStart,
            ArrayEnd,
            Colon,
            Comma,
        }
    }
}
