using System.Data;
using System.Globalization;
using ExcelDataReader;
using Pipeline.Worker.Configuration;

namespace Pipeline.Worker.Processing;

public sealed class ExcelFileDataReader : IDataReader
{
    private readonly FileStream _stream;
    private readonly IExcelDataReader _reader;
    private readonly IReadOnlyList<DatasetColumnMapping> _mappings;

    private bool _isClosed;

    public ExcelFileDataReader(
        string filePath,
        IReadOnlyList<DatasetColumnMapping> mappings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(mappings);

        _mappings = mappings;

        System.Text.Encoding.RegisterProvider(
            System.Text.CodePagesEncodingProvider.Instance);

        _stream = File.Open(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite);

        _reader = ExcelReaderFactory.CreateReader(
            _stream);
    }

    public int FieldCount => _mappings.Count;

    public int Depth => 0;

    public bool IsClosed => _isClosed;

    public int RecordsAffected => -1;

    public object this[int i] => GetValue(i);

    public object this[string name] => GetValue(GetOrdinal(name));

    public bool Read()
    {
        return _reader.Read();
    }

    public bool NextResult()
    {
        return false;
    }

    public object GetValue(int i)
    {
        ValidateIndex(i);

        var mapping = _mappings[i];

        var rawValue = _reader.GetValue(
            mapping.SourceColumnIndex);

        return ConvertValue(
            rawValue,
            mapping.TargetType,
            mapping.IsRequired,
            mapping.DestinationColumnName);
    }

    public int GetValues(object[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var count = Math.Min(
            values.Length,
            FieldCount);

        for (var i = 0; i < count; i++)
        {
            values[i] = GetValue(i);
        }

        return count;
    }

    public string GetName(int i)
    {
        ValidateIndex(i);

        return _mappings[i].DestinationColumnName;
    }

    public int GetOrdinal(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        for (var i = 0; i < _mappings.Count; i++)
        {
            if (string.Equals(
                _mappings[i].DestinationColumnName,
                name,
                StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        throw new IndexOutOfRangeException(
            $"Column '{name}' was not found.");
    }

    public Type GetFieldType(int i)
    {
        ValidateIndex(i);

        return GetTargetType(
            _mappings[i].TargetType);
    }

    public string GetDataTypeName(int i)
    {
        return GetFieldType(i).Name;
    }

    public bool IsDBNull(int i)
    {
        return GetValue(i) is DBNull;
    }

    public bool GetBoolean(int i)
    {
        return Convert.ToBoolean(
            GetValue(i),
            CultureInfo.InvariantCulture);
    }

    public byte GetByte(int i)
    {
        return Convert.ToByte(
            GetValue(i),
            CultureInfo.InvariantCulture);
    }

    public long GetBytes(
        int i,
        long fieldOffset,
        byte[]? buffer,
        int bufferoffset,
        int length)
    {
        var value = GetValue(i);

        if (value is DBNull)
        {
            return 0;
        }

        byte[] bytes;

        if (value is byte[] byteArray)
        {
            bytes = byteArray;
        }
        else
        {
            bytes = System.Text.Encoding.UTF8.GetBytes(
                Convert.ToString(
                    value,
                    CultureInfo.InvariantCulture) ?? string.Empty);
        }

        if (buffer is null)
        {
            return bytes.Length;
        }

        if (fieldOffset >= bytes.Length)
        {
            return 0;
        }

        var available = bytes.Length - (int)fieldOffset;

        var bytesToCopy = Math.Min(
            available,
            length);

        Array.Copy(
            bytes,
            fieldOffset,
            buffer,
            bufferoffset,
            bytesToCopy);

        return bytesToCopy;
    }

    public char GetChar(int i)
    {
        return Convert.ToChar(
            GetValue(i),
            CultureInfo.InvariantCulture);
    }

    public long GetChars(
        int i,
        long fieldOffset,
        char[]? buffer,
        int bufferoffset,
        int length)
    {
        var value = GetValue(i);

        if (value is DBNull)
        {
            return 0;
        }

        var chars = (
            Convert.ToString(
                value,
                CultureInfo.InvariantCulture) ?? string.Empty
        ).ToCharArray();

        if (buffer is null)
        {
            return chars.Length;
        }

        if (fieldOffset >= chars.Length)
        {
            return 0;
        }

        var available = chars.Length - (int)fieldOffset;

        var charsToCopy = Math.Min(
            available,
            length);

        Array.Copy(
            chars,
            fieldOffset,
            buffer,
            bufferoffset,
            charsToCopy);

        return charsToCopy;
    }

    public Guid GetGuid(int i)
    {
        var value = GetValue(i);

        if (value is Guid guid)
        {
            return guid;
        }

        return Guid.Parse(
            Convert.ToString(
                value,
                CultureInfo.InvariantCulture)!);
    }

    public short GetInt16(int i)
    {
        return Convert.ToInt16(
            GetValue(i),
            CultureInfo.InvariantCulture);
    }

    public int GetInt32(int i)
    {
        return Convert.ToInt32(
            GetValue(i),
            CultureInfo.InvariantCulture);
    }

    public long GetInt64(int i)
    {
        return Convert.ToInt64(
            GetValue(i),
            CultureInfo.InvariantCulture);
    }

    public float GetFloat(int i)
    {
        return Convert.ToSingle(
            GetValue(i),
            CultureInfo.InvariantCulture);
    }

    public double GetDouble(int i)
    {
        return Convert.ToDouble(
            GetValue(i),
            CultureInfo.InvariantCulture);
    }

    public string GetString(int i)
    {
        return Convert.ToString(
            GetValue(i),
            CultureInfo.InvariantCulture) ?? string.Empty;
    }

    public decimal GetDecimal(int i)
    {
        return Convert.ToDecimal(
            GetValue(i),
            CultureInfo.InvariantCulture);
    }

    public DateTime GetDateTime(int i)
    {
        return Convert.ToDateTime(
            GetValue(i),
            CultureInfo.InvariantCulture);
    }

    public IDataReader GetData(int i)
    {
        throw new NotSupportedException();
    }

    public DataTable? GetSchemaTable()
    {
        return null;
    }

    public void Close()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_isClosed)
        {
            return;
        }

        _reader.Dispose();
        _stream.Dispose();

        _isClosed = true;
    }

    private void ValidateIndex(int i)
    {
        if (i < 0 || i >= _mappings.Count)
        {
            throw new IndexOutOfRangeException(
                $"Column index {i} is outside the reader range.");
        }
    }

    private static object ConvertValue(
        object? rawValue,
        string? targetType,
        bool isRequired,
        string destinationColumnName)
    {
        if (rawValue is null ||
            rawValue is DBNull ||
            string.IsNullOrWhiteSpace(rawValue.ToString()))
        {
            if (isRequired)
            {
                throw new InvalidDataException(
                    $"Required column '{destinationColumnName}' contains an empty value.");
            }

            return DBNull.Value;
        }

        try
        {
            return targetType?.Trim().ToLowerInvariant() switch
            {
                "int" or "int32" =>
                    Convert.ToInt32(
                        rawValue,
                        CultureInfo.InvariantCulture),

                "bigint" or "long" or "int64" =>
                    Convert.ToInt64(
                        rawValue,
                        CultureInfo.InvariantCulture),

                "smallint" or "int16" =>
                    Convert.ToInt16(
                        rawValue,
                        CultureInfo.InvariantCulture),

                "tinyint" or "byte" =>
                    Convert.ToByte(
                        rawValue,
                        CultureInfo.InvariantCulture),

                "decimal" or "numeric" =>
                    Convert.ToDecimal(
                        rawValue,
                        CultureInfo.InvariantCulture),

                "float" or "double" =>
                    Convert.ToDouble(
                        rawValue,
                        CultureInfo.InvariantCulture),

                "real" or "single" =>
                    Convert.ToSingle(
                        rawValue,
                        CultureInfo.InvariantCulture),

                "bit" or "bool" or "boolean" =>
                    ConvertBoolean(rawValue),

                "datetime" or "datetime2" or "date" =>
                    Convert.ToDateTime(
                        rawValue,
                        CultureInfo.InvariantCulture),

                "uniqueidentifier" or "guid" =>
                    rawValue is Guid guid
                        ? guid
                        : Guid.Parse(rawValue.ToString()!),

                "nvarchar" or "varchar" or "string" or "text" =>
                    Convert.ToString(
                        rawValue,
                        CultureInfo.InvariantCulture) ?? string.Empty,

                null or "" =>
                    rawValue,

                _ =>
                    rawValue
            };
        }
        catch (Exception ex)
        {
            throw new InvalidDataException(
                $"Unable to convert value '{rawValue}' for column " +
                $"'{destinationColumnName}' to target type '{targetType}'.",
                ex);
        }
    }

    private static bool ConvertBoolean(object value)
    {
        if (value is bool boolValue)
        {
            return boolValue;
        }

        var text = value.ToString();

        if (bool.TryParse(
            text,
            out var parsed))
        {
            return parsed;
        }

        if (text == "1")
        {
            return true;
        }

        if (text == "0")
        {
            return false;
        }

        throw new FormatException(
            $"'{value}' is not a valid boolean value.");
    }

    private static Type GetTargetType(string? targetType)
    {
        return targetType?.Trim().ToLowerInvariant() switch
        {
            "int" or "int32" =>
                typeof(int),

            "bigint" or "long" or "int64" =>
                typeof(long),

            "smallint" or "int16" =>
                typeof(short),

            "tinyint" or "byte" =>
                typeof(byte),

            "decimal" or "numeric" =>
                typeof(decimal),

            "float" or "double" =>
                typeof(double),

            "real" or "single" =>
                typeof(float),

            "bit" or "bool" or "boolean" =>
                typeof(bool),

            "datetime" or "datetime2" or "date" =>
                typeof(DateTime),

            "uniqueidentifier" or "guid" =>
                typeof(Guid),

            _ =>
                typeof(string)
        };
    }
}