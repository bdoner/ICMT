namespace ICMT
{
    class Client
    {
        public static void Main()
        {
            //Thread.Sleep(2000);
            Console.WriteLine("Hello, World!");

            var photo = @"photo.jpg";
            var host = "127.0.0.1";

            ICMTClient client = new(host);
            client.Send(photo);


            Console.WriteLine("----------------------");

            //Console.ReadLine();



        }
    }
}