using System.Text;

namespace Garmin.Ble.Lib;

    public class ByteArrayOutputStream : MemoryStream
    {

        public ByteArrayOutputStream(int size)
        : base()
        {
        }

        public void WriteAsByte(int b)
        {
            Write(new []{(byte)b});
        }
    }