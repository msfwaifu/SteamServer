/*
	All files containing this header is released under the GPL 3.0 license.

	Initial author: (https://github.com/)Convery
	Started: 2014-10-28
	Notes:
        This buffer is based on DemonWares bdByteBuffer.
        Should really be rewritten with unsafe code.
*/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace SteamServer
{
    // Data types that are currently supported.
    enum DataTypes : byte
    {
        BOOL = 1,
        INT8 = 2,
        UINT8 = 3,
        INT16 = 5,
        UINT16 = 6,
        INT32 = 7,
        UINT32 = 8,
        INT64 = 9,
        UINT64 = 10,
        FLOAT = 13,
        STRING = 16,
        BLOB = 19,
    }

    class redBuffer
    {
        List<Byte> Buffer;
        UInt32 Position;

        // Constructors.
        public redBuffer(Byte[] Data)
        {
            Buffer = new List<Byte>();

            for (UInt32 i = 0; i < Data.Length; i++)
                Buffer.Add(Data[i]);

            Position = 0;
        }
        public redBuffer()
        {
            Buffer = new List<Byte>();
            Position = 0;
        }

        // Utility methods.
        public Byte[] GetBuffer()
        {
            return Buffer.ToArray();
        }
        public UInt32 CurrentPosition()
        {
            return Position;
        }
        public UInt32 Length()
        {
            return (UInt32)Buffer.Count();
        }
        public Int32 Find(Byte[] Data)
        {
            Byte[] Temp = new Byte[Data.Length];

            // I wish we had a memcmp.
            for (Int32 i = 0; i < Buffer.Count - Data.Length; i++)
            {
                for (Int32 c = 0; c < Data.Length; i++)
                    Temp[c] = Data[c];

                if (StructuralComparisons.StructuralEqualityComparer.Equals(Temp, Data))
                    return i;
                else
                    Temp = new Byte[Data.Length];
            }

            return -1;
        }
        public Boolean SetPosition(UInt32 Pos)
        {
            if (Pos > Buffer.Count)
                return false;

            Position = Pos;
            return true;
        }
        public void Rewind()
        {
            Position = 0;
        }

        // Base functions.
        public Boolean Read(UInt32 Count, ref Byte[] Output)
        {
            if (Position + Count > Buffer.Count)
                return false;

            if (Output != null)
            {
                Output = new Byte[Count];

                for (UInt32 i = 0; i < Count; i++)
                    Output[i] = Buffer[(int)(Position + i)];
            }

            Position += Count;
            return true;
        }
        public Boolean Write(UInt32 Count, Byte[] Input)
        {
            if (Input != null)
            {
                if (Position == Buffer.Count)
                {
                    for (UInt32 i = 0; i < Count; i++)
                        Buffer.Add(Input[i]);
                }
                else
                {
                    if (Position + Count > Buffer.Count)
                    {
                        for (UInt32 i = 0; i < Buffer.Count - Position; i++)
                            Buffer[(int)(Position + i)] = Input[i];

                        for (UInt32 i = (UInt32)(Buffer.Count - Position); i < Count; i++)
                            Buffer.Add(Input[i]);
                    }
                    else
                    {
                        for (UInt32 i = 0; i < Count; i++)
                            Buffer[(int)(Position + i)] = Input[i];
                    }
                }
            }

            Position += Count;
            return true;
        }
        public Boolean ReadDataType(Byte Type)
        {
            Byte[] StoredType = new Byte[1];

            Read(1, ref StoredType);
            if (StoredType[0] != Type)
                return false;
            else
                return true;
        }
        public Boolean WriteDataType(Byte Type)
        {
            return Write(1, new Byte[1] {Type});
        }

        // Read.
        public Boolean ReadBoolean(ref Boolean Output)
        {
            Byte[] Temp = new Byte[1];
            Boolean Result = false;

            if (ReadDataType((Byte)DataTypes.BOOL))
            {
                Result = Read(1, ref Temp);
                Output = BitConverter.ToBoolean(Temp, 0);
            }

            return Result;
        }
        public Boolean ReadInt8(ref SByte Output)
        {
            Byte[] Temp = new Byte[1];
            Boolean Result = false;

            if (ReadDataType((Byte)DataTypes.INT8))
            {
                Result = Read(1, ref Temp);
                Output = (SByte)BitConverter.ToChar(Temp, 0);
            }

            return Result;
        }
        public Boolean ReadUInt8(ref Byte Output)
        {
            Byte[] Temp = new Byte[1];
            Boolean Result = false;

            if (ReadDataType((Byte)DataTypes.UINT8))
            {
                Result = Read(1, ref Temp);
                Output = Temp[0];
            }
            
            return Result;
        }
        public Boolean ReadInt16(ref Int16 Output)
        {
            Byte[] Temp = new Byte[2];
            Boolean Result = false;

            if (ReadDataType((Byte)DataTypes.INT16))
            {
                Result = Read(2, ref Temp);
                Output = BitConverter.ToInt16(Temp, 0);
            }

            return Result;
        }
        public Boolean ReadUInt16(ref UInt16 Output)
        {
            Byte[] Temp = new Byte[2];
            Boolean Result = false;

            if (ReadDataType((Byte)DataTypes.UINT16))
            {
                Result = Read(2, ref Temp);
                Output = BitConverter.ToUInt16(Temp, 0);
            }

            return Result;
        }
        public Boolean ReadInt32(ref Int32 Output)
        {
            Byte[] Temp = new Byte[4];
            Boolean Result = false;

            if (ReadDataType((Byte)DataTypes.INT32))
            {
                Result = Read(4, ref Temp);
                Output = BitConverter.ToInt32(Temp, 0);
            }
            
            return Result;
        }
        public Boolean ReadUInt32(ref UInt32 Output)
        {
            Byte[] Temp = new Byte[4];
            Boolean Result = false;

            if (ReadDataType((Byte)DataTypes.UINT32))
            {
                Result = Read(4, ref Temp);
                Output = BitConverter.ToUInt32(Temp, 0);
            }
            
            return Result;
        }
        public Boolean ReadInt64(ref Int64 Output)
        {
            Byte[] Temp = new Byte[8];
            Boolean Result = false;

            if (ReadDataType((Byte)DataTypes.INT64))
            {
                Result = Read(8, ref Temp);
                Output = BitConverter.ToInt64(Temp, 0);
            }
            
            return Result;
        }
        public Boolean ReadUInt64(ref UInt64 Output)
        {
            Byte[] Temp = new Byte[8];
            Boolean Result = false;

            if (ReadDataType((Byte)DataTypes.UINT64))
            {
                Result = Read(8, ref Temp);
                Output = BitConverter.ToUInt64(Temp, 0);
            }
            
            return Result;
        }
        public Boolean ReadFloat(ref Single Output)
        {
            Byte[] Temp = new Byte[4];
            Boolean Result = false;

            if (ReadDataType((Byte)DataTypes.FLOAT))
            {
                Result = Read(4, ref Temp);
                Output = BitConverter.ToSingle(Temp, 0);
            }

            return Result;
        }
        public Boolean ReadString(ref String Output)
        {
            Byte[] Temp = new Byte[4];
            UInt32 Length = 0;

            if (!ReadDataType((Byte)DataTypes.STRING) || !ReadUInt32(ref Length))
                return false;

            if (!Read(Length, ref Temp))
                return false;

            Output = Encoding.UTF8.GetString(Temp);
            return true;
        }
        public Boolean ReadBlob(ref Byte[] Output)
        {
            Byte[] Temp = new Byte[4];
            UInt32 Length = 0;

            if (!ReadDataType((Byte)DataTypes.BLOB) || !ReadUInt32(ref Length))
                return false;

            if (!Read(Length, ref Output))
                return false;

            return true;
        }
        public Boolean ReadBlob(ref List<Byte> Output)
        {
            Byte[] Temp = new Byte[4];
            UInt32 Length = 0;

            if (!ReadDataType((Byte)DataTypes.BLOB) || !ReadUInt32(ref Length))
                return false;

            if (!Read(Length, ref Temp))
                return false;

            for (UInt32 i = 0; i < Length; i++)
                Output.Add(Temp[i]);

            return true;
        }
        public Boolean ReadArray(Byte Type, ref UInt32 Count, ref UInt32 ElementSize, ref Byte[] Output)
        {
            Byte Storedtype = 0;
            UInt32 ArraySize = 0;
            UInt32 ArrayCount = 0;

            if (!ReadUInt8(ref Storedtype))
                return false;

            if (Storedtype != (Type + 100))
                return false;

            if (!ReadUInt32(ref ArraySize))
                return false;

            if (!ReadUInt32(ref ArrayCount))
                return false;

            Count = ArrayCount;

            if (ElementSize != null)
            {
                if (ArrayCount > 0)
                    ElementSize = ArraySize / ArrayCount;
                else
                    ElementSize = 0;
            }

            return Read(ArraySize, ref Output);
        }

        // Write.
        public Boolean WriteBoolean(Boolean Input)
        {
            return (Write(1, BitConverter.GetBytes((Byte)DataTypes.BOOL)) && Write(1, BitConverter.GetBytes(Input)));
        }
        public Boolean WriteInt8(SByte Input)
        {
            return (Write(1, BitConverter.GetBytes((Byte)DataTypes.INT8)) && Write(1, BitConverter.GetBytes(Input)));
        }
        public Boolean WriteUInt8(Byte Input)
        {
            return (Write(1, BitConverter.GetBytes((Byte)DataTypes.UINT8)) && Write(1, BitConverter.GetBytes(Input)));
        }
        public Boolean WriteInt16(Int16 Input)
        {
            return (Write(1, BitConverter.GetBytes((Byte)DataTypes.INT16)) && Write(2, BitConverter.GetBytes(Input)));
        }
        public Boolean WriteUInt16(UInt16 Input)
        {
            return (Write(1, BitConverter.GetBytes((Byte)DataTypes.UINT16)) && Write(2, BitConverter.GetBytes(Input)));
        }
        public Boolean WriteInt32(Int32 Input)
        {
            return (Write(1, BitConverter.GetBytes((Byte)DataTypes.INT32)) && Write(4, BitConverter.GetBytes(Input)));
        }
        public Boolean WriteUInt32(UInt32 Input)
        {
            return (Write(1, BitConverter.GetBytes((Byte)DataTypes.UINT32)) && Write(4, BitConverter.GetBytes(Input)));
        }
        public Boolean WriteInt64(Int64 Input)
        {
            return (Write(1, BitConverter.GetBytes((Byte)DataTypes.INT64)) && Write(8, BitConverter.GetBytes(Input)));
        }
        public Boolean WriteUInt64(UInt64 Input)
        {
            return (Write(1, BitConverter.GetBytes((Byte)DataTypes.UINT64)) && Write(8, BitConverter.GetBytes(Input)));
        }
        public Boolean WriteFloat(Single Input)
        {
            return (Write(1, BitConverter.GetBytes((Byte)DataTypes.FLOAT)) && Write(4, BitConverter.GetBytes(Input)));
        }
        public Boolean WriteString(String Input)
        {
            Byte[] Temp = Encoding.UTF8.GetBytes(Input);

            if(!Write(1, BitConverter.GetBytes((Byte)DataTypes.STRING)))
                return false;

           if(!WriteUInt32((UInt32)Temp.Length))
                return false;

            return Write((UInt32)Temp.Length, Temp);
        }
        public Boolean WriteBlob(Byte[] Data)
        {
            if (!Write(1, BitConverter.GetBytes((Byte)DataTypes.BLOB)))
                return false;

            if (!WriteUInt32((UInt32)Data.Length))
                return false;

            return Write((UInt32)Data.Length, Data);
        }
        public Boolean WriteArray(Byte Type, UInt32 Count, UInt32 ElementSize, Byte[] Data)
        {
            if (!WriteUInt8((Byte)(Type + 100)))
                return false;

            if (!WriteUInt32(Count * ElementSize))
                return false;

            if (!WriteUInt32(Count))
                return false;

            return Write(Count * ElementSize, Data);
        }
    }
}