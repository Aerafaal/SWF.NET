using SWF.NET.Utils;

namespace SWF.NET.Tags;

/// <summary>
///     FlvScriptTag
/// </summary>
public class FlvScriptTag : FlvBaseTag
{
    public FlvScriptType Type { get; set; }

    public override void ReadData(byte version, BufferedBinaryReader binaryReader)
    {
        base.ReadData(version, binaryReader);

        Type = (FlvScriptType)binaryReader.ReadUBits(8);

        var count = 0;
        if (Type == FlvScriptType.Number)
            count = 8;

        for (var i = 0; i < count; i++) binaryReader.ReadByte();
    }
}

public enum FlvScriptType
{
    Number = 0,
    Boolean = 1,
    String = 2,
    Object = 3,

    /// <summary>
    ///     (reserved, not supported)
    /// </summary>
    MovieClip = 4,
    Null = 5,
    Undefined = 6,
    Reference = 7,
    ECMAarray = 8,
    ObjectEndMarker = 9,
    StrictArray = 10,
    Date = 11,
    LongString = 12

    //DOUBLE = 0,
    //UI8 = 1,
    //SCRIPTDATASTRING = 2,
    //SCRIPTDATAOBJECT = 3,
    //UI16 = 7,
    //SCRIPTDATAECMAARRAY = 8,
    //SCRIPTDATASTRICTARRAY = 10,
    //SCRIPTDATADATE = 11,
    //SCRIPTDATALONGSTRING = 12,
}