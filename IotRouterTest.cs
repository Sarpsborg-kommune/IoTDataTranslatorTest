using System;
using System.Data;
using System.Text;

namespace sarpsborgkommune.iot
{
    class IotRouterTest
    {
        static void Main(string[] args)
        {
            if (args[0] == "" || args[1] == "")
            {
                Console.WriteLine("ERROR: decoder and/or data arguments is missing from the command line.");
                System.Environment.Exit(1);
            }
         
            switch(args[0])
            {
                case "elsys":
                    ElsysMessage testmessage = new ElsysMessage(helperfunctions.StringToByteArray(args[0]));
                    Console.WriteLine(testmessage.toJson());
                    break;
                case "default":
                    Console.WriteLine("ERROR: Decoder is not recognized.\nThe follwing decoders are avaliable:");
                    Console.WriteLine("[\"elsys\"]");
                    break;
            }
            
        }   
    }
}
