using System.Buffers.Binary;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// ReSharper disable once CheckNamespace
namespace EFCore.SqlServer.VectorSearch.Storage.Internal;

public class VectorValueConverter() : ValueConverter<float[], byte[]>(
    f => ToBytes(f),
    b => FromBytes(b),
    mappingHints: new ConverterMappingHints(size: 8000))
{
    private const byte MagicByte = 0xA9;
    private const byte Version = 0x01;

    private static byte[] ToBytes(float[] data)
    {
        if (data.Length > short.MaxValue)
        {
            throw new InvalidOperationException("Vector size is too large.");
        }

        var bytes = new byte[8 + data.Length * sizeof(float)];
        bytes[0] = MagicByte;
        bytes[1] = Version;
        BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(2), (short)data.Length);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), 0);

        var span = bytes.AsSpan(8);
        foreach (var f in data)
        {
            BinaryPrimitives.WriteSingleLittleEndian(span, f);
            span = span[sizeof(float)..];
        }

        return bytes;
    }

    private static float[] FromBytes(byte[] data)
    {
        if (data[0] != MagicByte)
        {
            throw new InvalidOperationException("Invalid data, header magic byte does not match.");
        }

        if (data[1] != Version)
        {
            throw new InvalidOperationException("Invalid data, header version does not match.");
        }

        var dimensions = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(2));
        var floats = new float[dimensions];
        ReadOnlySpan<byte> span = data.AsSpan(8);

        for (var i = 0; i < dimensions; i++)
        {
            floats[i] = BinaryPrimitives.ReadSingleLittleEndian(span);
            span = span[sizeof(float)..];
        }

        return floats;
    }
}