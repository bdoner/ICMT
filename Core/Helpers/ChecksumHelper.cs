namespace ICMT.Core.Helpers
{
    public class ChecksumHelper
    {
        public static byte[] GetFileChecksum(string file)
        {
            uint checksum = 0;
            using (var br = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                uint r = 0;
                while (0 < (r = br.ReadUInt32()))
                {
                    checksum ^= br.ReadUInt32();
                }
            }
            return BitConverter.GetBytes(checksum);
        }
    }
}
