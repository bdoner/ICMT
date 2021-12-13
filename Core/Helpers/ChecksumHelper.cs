namespace ICMT.Core.Helpers
{
    public class ChecksumHelper
    {
        public static byte[] GetFileChecksum(string file)
        {
            uint checksum = 0;
            using (var br = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                int its = 0;
                byte[] b = new byte[4];
                int r = 0;
                while (0 < (r = br.Read(b)))
                {
                    its++;
                    uint c = BitConverter.ToUInt32(b);
                    checksum ^= c;

                    b = new byte[4] { 0, 0, 0, 0 };

                    //Console.WriteLine($"iterations: {its++}\nr: {r}\nc: {c}\nchecksum: {checksum}\n\n");

                }
            }
            return BitConverter.GetBytes(checksum);
        }
    }
}
