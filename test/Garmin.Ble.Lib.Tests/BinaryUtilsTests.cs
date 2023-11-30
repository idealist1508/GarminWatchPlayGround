using Garmin.Ble.Lib.Messages;
using Xunit;

namespace Garmin.Ble.Lib.Tests;

public class BinaryUtilsTests
{
    private readonly byte[] _data =
        { 0x76, 0xc3, 0xad, 0x76, 0x6f, 0x6d, 0x6f, 0x76, 0x65, 0x20, 0x48, 0x52, 0x03, 0x87, 0xd4, 0x00 };
    
    [Fact]
    public void ReadByteTest()
    {
       Assert.Equal(118, BinaryUtils.ReadByte(_data,0)); 
       Assert.Equal(195, BinaryUtils.ReadByte(_data,1)); 
    } 
    
    [Fact]
    public void WriteByteTest()
    {
       var data = new byte[1];
       BinaryUtils.WriteByte(data, 0, 118);
       Assert.Equal(new byte[]{0x76}, data); 
    } 
    
    [Fact]
    public void ReadShortTest()
    {
       Assert.Equal(50038, BinaryUtils.ReadShort(_data,0)); 
       Assert.Equal(44483, BinaryUtils.ReadShort(_data,1)); 
    } 
    
    [Fact]
    public void WriteShortTest()
    {
       var data = new byte[2];
       BinaryUtils.WriteShort(data, 0, 50038);
       Assert.Equal(new byte[]{0x76, 0xc3}, data); 
    } 
    
    [Fact]
    public void ReadIntTest()
    {
       Assert.Equal(1991099254, BinaryUtils.ReadInt(_data,0)); 
       Assert.Equal(1870048707, BinaryUtils.ReadInt(_data,1)); 
    } 
    
    [Fact]
    public void WriteIntTest()
    {
       var data = new byte[4];
       BinaryUtils.WriteInt(data, 0, 1991099254);
       Assert.Equal(new byte[]{0x76, 0xc3, 0xad, 0x76}, data); 
    } 
    
    [Fact]
    public void ReadLongTest()
    {
       Assert.Equal(8534160144390275958, BinaryUtils.ReadLong(_data,0)); 
       Assert.Equal(7311153560894746051, BinaryUtils.ReadLong(_data,1)); 
    } 
    
    [Fact]
    public void WriteLongTest()
    {
       var data = new byte[8];
       BinaryUtils.WriteLong(data, 0, 8534160144390275958);
       Assert.Equal(new byte[]{0x76, 0xc3, 0xad, 0x76, 0x6f, 0x6d, 0x6f, 0x76}, data); 
    } 
    
}