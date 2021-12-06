namespace ICMT.Core.Helpers
{
    public class ChecksumHelper
    {
        public static byte[] GetFileChecksum(string file)
        {
            uint checksum = 0;
            using (var br = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                byte[] b = new byte[4];
                while (0 < br.Read(b))
                {
                    uint r = BitConverter.ToUInt32(b);
                    checksum ^= r;
                }
            }
            return BitConverter.GetBytes(checksum);
        }
    }
}
