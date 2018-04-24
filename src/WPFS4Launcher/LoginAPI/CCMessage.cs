using System;
using System.Text;

namespace TestServer_Launcher.LoginAPI
{
    public class ByteArray
    {
        private byte[] _buffer = new byte[0];

        public ByteArray()
        {
        }

        public ByteArray(ByteArray data)
        {
            _buffer = data._buffer;
        }

        public ByteArray(byte[] data)
        {
            _buffer = (byte[]) data.Clone();
        }

        public ByteArray(byte[] data, int length)
        {
            _buffer = new byte[length];
            Array.Copy(data, _buffer, length);
        }

        public int ReadOffset { get; private set; }

        public int WriteOffset { get; private set; }

        public byte[] GetBuffer()
        {
            return (byte[]) _buffer.Clone();
        }

        internal void Write(byte[] obj)
        {
            if (_buffer.Length <= WriteOffset + obj.Length)
                Array.Resize(ref _buffer, WriteOffset + obj.Length);
            Array.Copy(obj, 0, _buffer, WriteOffset, obj.Length);
            WriteOffset = WriteOffset + obj.Length;
        }

        internal void WriteScalar(byte obj)
        {
            WriteScalar((long) obj);
        }

        internal void WriteScalar(short obj)
        {
            WriteScalar((long) obj);
        }

        internal void WriteScalar(int obj)
        {
            WriteScalar((long) obj);
        }

        internal void WriteScalar(long obj)
        {
            if (obj <= sbyte.MaxValue)
            {
                Write((byte) 1);
                Write((byte) obj);
            }
            else if (obj <= short.MaxValue)
            {
                Write((byte) 2);
                Write((short) obj);
            }
            else if (obj <= int.MaxValue)
            {
                Write((byte) 4);
                Write((int) obj);
            }
            else
            {
                Write((byte) 8);
                Write(obj);
            }
        }

        internal void Write(bool obj)
        {
            if (obj)
                Write((byte) 1);
            else
                Write((byte) 0);
        }

        internal void Write(byte obj)
        {
            var data = new byte[1];
            data[0] = obj;
            Write(data);
        }

        internal void Write(short obj)
        {
            Write(BitConverter.GetBytes(obj));
        }

        internal void Write(int obj)
        {
            Write(BitConverter.GetBytes(obj));
        }

        internal void Write(long obj)
        {
            Write(BitConverter.GetBytes(obj));
        }

        internal void Write(ByteArray obj)
        {
            WriteScalar(obj.WriteOffset);
            Write(obj._buffer);
        }

        internal bool Read(ref ByteArray obj)
        {
            long length = 0;
            if (ReadScalar(ref length))
            {
                var data = new byte[length];
                if (Read(ref data, data.Length))
                {
                    obj = new ByteArray(data);
                    return true;
                }
            }

            return false;
        }

        internal bool Read(ref byte[] obj, int length)
        {
            if (_buffer.Length < ReadOffset + length) return false;
            var data = new byte[length];
            Array.Copy(_buffer, ReadOffset, data, 0, length);
            obj = data;
            ReadOffset = ReadOffset + length;
            return true;
        }

        internal bool Read(ref bool obj)
        {
            byte a = 0;
            var retval = Read(ref a);
            obj = a == 1;
            return retval;
        }

        internal bool Read(ref byte obj)
        {
            if (_buffer.Length >= ReadOffset)
            {
                obj = _buffer[ReadOffset];
                ReadOffset = ReadOffset + 1;
                return true;
            }

            return false;
        }

        internal bool Read(ref short obj)
        {
            var data = new byte[2];
            if (Read(ref data[0])
                && Read(ref data[1]))
            {
                obj = BitConverter.ToInt16(data, 0);
                return true;
            }

            return false;
        }

        internal bool Read(ref int obj)
        {
            var data = new byte[4];
            if (Read(ref data[0])
                && Read(ref data[1])
                && Read(ref data[2])
                && Read(ref data[3]))
            {
                obj = BitConverter.ToInt32(data, 0);
                return true;
            }

            return false;
        }

        internal bool Read(ref long obj)
        {
            var data = new byte[8];
            if (Read(ref data[0])
                && Read(ref data[1])
                && Read(ref data[2])
                && Read(ref data[3])
                && Read(ref data[4])
                && Read(ref data[5])
                && Read(ref data[6])
                && Read(ref data[7]))
            {
                obj = BitConverter.ToInt64(data, 0);
                return true;
            }

            return false;
        }

        internal bool ReadScalar(ref long obj)
        {
            byte a = 0;
            short b = 0;
            var c = 0;
            long d = 0;

            byte padding = 0;
            if (!Read(ref padding))
                return false;
            switch (padding)
            {
                case 8:
                    if (!Read(ref d))
                        return false;
                    obj = d;
                    break;
                case 4:
                    if (!Read(ref c))
                        return false;
                    obj = c;
                    break;
                case 2:
                    if (!Read(ref b))
                        return false;
                    obj = b;
                    break;
                case 1:
                    if (!Read(ref a))
                        return false;
                    obj = a;
                    break;
                default:
                    return false;
            }

            return true;
        }
    }

    public class CCMessage : ByteArray
    {
        public CCMessage()
        {
        }

        public CCMessage(ByteArray packet) : base(packet)
        {
        }

        public CCMessage(byte[] data, int length) : base(data, length)
        {
        }

        public byte[] Buffer => GetBuffer();
        public int Length => Buffer.Length;


        internal void Write(MessageType obj)
        {
            Write((byte) obj);
        }

        internal bool Read(ref MessageType obj)
        {
            byte a = 0;
            if (!Read(ref a))
                return false;
            obj = (MessageType) a;
            return true;
        }

        internal void Write(CCMessage obj)
        {
            Write(obj.Buffer);
        }

        internal void Write(string obj)
        {
            Write((byte) 1);
            WriteScalar(obj.Length);
            Write(Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(Encoding.UTF8.GetBytes(obj))));
        }

        internal bool Read(ref string obj)
        {
            long length = 0;
            byte type = 0;
            if (Read(ref type)
                && ReadScalar(ref length))
            {
                var binarytext = new byte[length];

                if (Read(ref binarytext, (int) length))
                    switch (type)
                    {
                        case 1:
                            obj = Encoding.ASCII.GetString(binarytext);
                            return true;
                        case 2:
                            obj = Encoding.Unicode.GetString(binarytext);
                            return true;
                        default:
                            return false;
                    }
            }

            return false;
        }

        internal enum MessageType : byte
        {
            Ignore,
            Rmi,
            Encrypted,
            Notify
        }
    }
}
