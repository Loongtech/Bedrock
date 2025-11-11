using System;

namespace Net.LoongTech.Bedrock.Algorithms.Compression
{
    /// <summary>
    /// 提供基于 LZW 算法的数据压缩和解压功能。
    /// 此实现源于一个特定版本的LZW算法，使用13位编码空间。
    /// </summary>
    public class LzwCompressor
    {
        #region --- LZW 算法核心常量 ---

        private const int BITS = 13;
        private const int MAX_CODE = (1 << BITS) - 1;
        private const int TABLE_SIZE = 9973; // 一个用于哈希的质数
        private const int END_OF_STREAM = 256;
        private const int BUMP_CODE = 257;
        private const int FIRST_CODE = 258;
        private const int UNUSED = -1;

        #endregion

        #region --- 内部状态字段 ---

        private readonly short[] _dictionaryCode;
        private readonly short[] _dictionaryParent;
        private readonly byte[] _dictionaryChar;
        private readonly byte[] _decodeStack;

        #endregion

        /// <summary>
        /// 初始化 LzwCompressor 的新实例。
        /// </summary>
        public LzwCompressor()
        {
            _dictionaryCode = new short[TABLE_SIZE];
            _dictionaryParent = new short[TABLE_SIZE];
            _dictionaryChar = new byte[TABLE_SIZE];
            _decodeStack = new byte[TABLE_SIZE];
        }

        #region --- 公共 API ---

        /// <summary>
        /// 使用 LZW 算法压缩字节数组。
        /// </summary>
        /// <param name="input">要压缩的原始数据。</param>
        /// <returns>压缩后的数据。如果压缩后的数据没有变小，则会抛出 LzwCompressionException。</returns>
        /// <exception cref="ArgumentNullException">当输入为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">当输入数据过大时抛出。</exception>
        /// <exception cref="LzwCompressionException">当压缩失败或压缩结果更大时抛出。</exception>
        public byte[] Compress(byte[] input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (input.Length == 0) return Array.Empty<byte>();
            if (input.Length > 8192) throw new ArgumentException("输入数据大小不能超过 8192 字节。", nameof(input));

            // 输出缓冲区最大可能和输入一样大
            var output = new byte[input.Length];
            int compressedSize = CompressInternal(input, output);

            // 返回实际大小的压缩数据
            var result = new byte[compressedSize];
            Array.Copy(output, result, compressedSize);
            return result;
        }

        /// <summary>
        /// 使用 LZW 算法解压字节数组。
        /// </summary>
        /// <param name="input">要解压的压缩数据。</param>
        /// <param name="maxOutputSize">预期的最大输出大小，用于防止解压炸弹。</param>
        /// <returns>解压后的原始数据。</returns>
        /// <exception cref="ArgumentNullException">当输入为 null 时抛出。</exception>
        /// <exception cref="LzwCompressionException">当解压过程中发生错误时抛出。</exception>
        public byte[] Decompress(byte[] input, int maxOutputSize = 8192)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (input.Length == 0) return Array.Empty<byte>();

            var output = new byte[maxOutputSize];
            int decompressedSize = DecompressInternal(input, output);

            var result = new byte[decompressedSize];
            Array.Copy(output, result, decompressedSize);
            return result;
        }

        #endregion

        #region --- 核心压缩/解压实现 (私有) ---

        private void InitializeDictionary()
        {
            Array.Fill(_dictionaryCode, (short)UNUSED);
        }

        private int CompressInternal(byte[] input, byte[] output)
        {
            InitializeDictionary();
            var writer = new BitWriter(output);

            int currentCode = input[0];
            int nextCodeValue = FIRST_CODE;
            int currentBitSize = 9;

            for (int i = 1; i < input.Length; i++)
            {
                int nextChar = input[i];
                int index = FindChildNode(currentCode, nextChar);

                if (_dictionaryCode[index] != UNUSED)
                {
                    currentCode = _dictionaryCode[index];
                }
                else
                {
                    writer.WriteBits(currentCode, currentBitSize);

                    if (nextCodeValue <= MAX_CODE)
                    {
                        _dictionaryCode[index] = (short)nextCodeValue;
                        _dictionaryParent[index] = (short)currentCode;
                        _dictionaryChar[index] = (byte)nextChar;
                        nextCodeValue++;
                    }

                    currentCode = nextChar;

                    if (nextCodeValue >= (1 << currentBitSize) && currentBitSize < BITS)
                    {
                        writer.WriteBits(BUMP_CODE, currentBitSize);
                        currentBitSize++;
                    }
                }
            }

            writer.WriteBits(currentCode, currentBitSize);
            writer.WriteBits(END_OF_STREAM, currentBitSize);

            int finalByteCount = writer.Flush();

            if (finalByteCount >= input.Length)
            {
                throw new LzwCompressionException("压缩失败：压缩后的数据尺寸不小于原始数据。");
            }

            return finalByteCount;
        }

        private int DecompressInternal(byte[] input, byte[] output)
        {
            InitializeDictionary();
            var reader = new BitReader(input);

            int outputPtr = 0;
            int currentBitSize = 9;
            int nextCodeValue = FIRST_CODE;

            int oldCode = reader.ReadBits(currentBitSize);
            if (oldCode < 0) throw new LzwCompressionException("解压失败：无法读取初始代码。");

            byte character = (byte)oldCode;
            output[outputPtr++] = character;

            while (reader.CanRead())
            {
                int newCode = reader.ReadBits(currentBitSize);
                if (newCode < 0) break; // 正常结束

                if (newCode == END_OF_STREAM)
                {
                    return outputPtr; // 解压成功结束
                }

                if (newCode == BUMP_CODE)
                {
                    if (currentBitSize < BITS)
                    {
                        currentBitSize++;
                    }
                    continue;
                }

                int codeToDecode;
                if (newCode >= nextCodeValue)
                {
                    codeToDecode = oldCode;
                    _decodeStack[0] = character;
                }
                else
                {
                    codeToDecode = newCode;
                }

                int decodeCount = 1;
                while (codeToDecode > 255)
                {
                    if (decodeCount >= _decodeStack.Length || codeToDecode >= TABLE_SIZE)
                        throw new LzwCompressionException("解压失败：解码栈溢出或代码无效。");

                    _decodeStack[decodeCount++] = _dictionaryChar[codeToDecode];
                    codeToDecode = _dictionaryParent[codeToDecode];
                }
                _decodeStack[decodeCount++] = (byte)codeToDecode;
                character = _decodeStack[decodeCount - 1];

                // 将解码栈中的字符写入输出缓冲区
                for (int i = decodeCount - 1; i >= 0; i--)
                {
                    if (outputPtr >= output.Length)
                        throw new LzwCompressionException("解压失败：输出缓冲区已满。");
                    output[outputPtr++] = _decodeStack[i];
                }

                if (nextCodeValue <= MAX_CODE)
                {
                    _dictionaryParent[nextCodeValue] = (short)oldCode;
                    _dictionaryChar[nextCodeValue] = character;
                    nextCodeValue++;
                }

                oldCode = newCode;
            }

            return outputPtr;
        }

        private int FindChildNode(int parentCode, int childChar)
        {
            int index = (childChar << (BITS - 8)) ^ parentCode;
            int offset = (index == 0) ? 1 : TABLE_SIZE - index;

            for (int i = 0; i < TABLE_SIZE; ++i)
            {
                if (_dictionaryCode[index] == UNUSED ||
                   (_dictionaryParent[index] == parentCode && _dictionaryChar[index] == childChar))
                {
                    return index;
                }
                index = (index >= offset) ? index - offset : index + TABLE_SIZE - offset;
            }
            // 理论上不应该到达这里，除非哈希表完全满了且没有找到空位
            throw new LzwCompressionException("压缩失败：哈希表冲突，无法找到节点。");
        }

        #endregion

        #region --- 内部位操作辅助类 ---

        // BitWriter 和 BitReader 是为了取代不安全的 PutBitToBuf32/GetBitFromBuf32
        // 它们提供了更安全、更面向对象的位流读写方式

        private class BitWriter
        {
            private readonly byte[] _buffer;
            private int _bitPosition;

            public BitWriter(byte[] buffer)
            {
                _buffer = buffer;
                _bitPosition = 0;
            }

            public void WriteBits(int value, int bitCount)
            {
                for (int i = bitCount - 1; i >= 0; i--)
                {
                    int byteIndex = _bitPosition / 8;
                    int bitIndex = _bitPosition % 8;

                    if (byteIndex >= _buffer.Length)
                        throw new LzwCompressionException("压缩失败：输出缓冲区已满。");

                    if (((value >> i) & 1) == 1)
                    {
                        _buffer[byteIndex] |= (byte)(1 << (7 - bitIndex));
                    }
                    _bitPosition++;
                }
            }

            public int Flush()
            {
                return (_bitPosition + 7) / 8; // 返回占用的总字节数
            }
        }

        private class BitReader
        {
            private readonly byte[] _buffer;
            private int _bitPosition;
            private readonly int _totalBits;

            public BitReader(byte[] buffer)
            {
                _buffer = buffer;
                _bitPosition = 0;
                _totalBits = buffer.Length * 8;
            }

            public bool CanRead() => _bitPosition < _totalBits;

            public int ReadBits(int bitCount)
            {
                if (_bitPosition + bitCount > _totalBits)
                    return -1; // 表示数据不足

                int value = 0;
                for (int i = 0; i < bitCount; i++)
                {
                    int byteIndex = _bitPosition / 8;
                    int bitIndex = _bitPosition % 8;

                    value <<= 1;
                    if ((_buffer[byteIndex] & (1 << (7 - bitIndex))) != 0)
                    {
                        value |= 1;
                    }
                    _bitPosition++;
                }
                return value;
            }
        }

        #endregion
    }

    /// <summary>
    /// 表示在 LZW 压缩或解压过程中发生的特定错误。
    /// </summary>
    public class LzwCompressionException : Exception
    {
        public LzwCompressionException(string message) : base(message) { }
        public LzwCompressionException(string message, Exception innerException) : base(message, innerException) { }
    }
}