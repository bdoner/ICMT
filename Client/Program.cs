namespace ICMT
{
    class Client
    {
        public static void Main(string[] args)
        {
            //Thread.Sleep(2000);
            Console.WriteLine("Hello, World!");

            var photo = args[0]; // @"photo.jpg";
            var host = args[1]; // "hurtig.ninja";

            ICMTClient client = new(host);
            try
            {
                client.Send(photo);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

            Console.WriteLine("----------------------");

            //Console.ReadLine();



        }
    }
}