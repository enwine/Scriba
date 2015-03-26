using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace ScribaService
{
    class Engine
    {
        TcpClient client;
        public Engine (TcpClient _client)
        {
            client = _client;
            client.ReceiveBufferSize = 2048;
        }

        public void EngineLoop()
        {
            byte[] input = new byte[2048];
            byte[] output = null;
            bool closeConnection = false;
            NetworkStream stream = null;

            string command, response;
            
            while (true)
            {
                try
                {
                    if (closeConnection || !client.Connected)
                    {
                        stream.Close();
                        ShutdownEngine();
                        break;
                    }

                    stream = client.GetStream();
                    stream.Read(input, 0, (int)client.ReceiveBufferSize);

                    command = System.Text.Encoding.UTF8.GetString(input);
                    response = ReadCommand(command, out closeConnection);
                    output = Encoding.UTF8.GetBytes(response);

                    stream.Write(output, 0, output.Length);
                    stream.Flush();
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine(ex.ToString());
#endif
                }
            }
        }
        public string ReadCommand(string commandString, out bool closeConnection)
        {
            closeConnection = false;
            try
            {
                string command = commandString.Substring(0, commandString.IndexOf(" ")).ToUpperInvariant(),
                token = commandString.Substring(commandString.IndexOf(" ") + 1, 32),
                payload = commandString.Substring(commandString.IndexOf(" ") + 34);
                switch (command)
                {
                    case "GET":
                        return "GET Reads a unique event by APP_TOKEN + EVENT_ID.";
                    case "PUT":
                        return PUT(token, payload);
                    case "ALL":
                        return "ALL Gets all events by APP_TOKEN, and a filter object. Sorts by descending date, and serves up to 1,000 events (within 1.96 MBytes).";
                    case "BYE":
                        closeConnection = true;
                        return "BYE";
                    default:
                        return command + " is an unknown command.";
                }
            }
            catch (Exception ex)
            {
                return "Scriba could not understand your message.";
            }
            
        }


        private string PUT(string token, string payload)
        {
            EventRow row = EventRow.FromJson(payload);
            row.ApplicationId = token;
            Server.Gate.PushEventRow(row);
            return "OK";
        }
        private void ShutdownEngine()
        {
            client.Close();
        }
    }
}
