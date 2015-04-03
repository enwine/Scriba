using System;
using System.Net.Sockets;
using System.Text;

namespace ScribaService
{
    internal class Engine
    {
        private readonly TcpClient _client;

        public Engine(TcpClient client)
        {
            _client = client;
            _client.ReceiveBufferSize = 2048;
        }

        public void EngineLoop()
        {
            var input = new byte[2048];
            var closeConnection = false;

            while (true)
            {
                try
                {
                    var stream = _client.GetStream();

                    if (closeConnection || !_client.Connected)
                    {
                        stream.Close();
                        ShutdownEngine();
                        break;
                    }

                    stream.Read(input, 0, _client.ReceiveBufferSize);

                    var command = Encoding.UTF8.GetString(input);
                    var response = ReadCommand(command, out closeConnection);
                    var output = Encoding.UTF8.GetBytes(response);

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
                string command =
                    commandString.Substring(0, commandString.IndexOf(" ", StringComparison.Ordinal)).ToUpperInvariant(),
                    token = commandString.Substring(commandString.IndexOf(" ", StringComparison.Ordinal) + 1, 32),
                    payload = commandString.Substring(commandString.IndexOf(" ", StringComparison.Ordinal) + 34);
                switch (command)
                {
                    case "GET":
                        return Get(token, payload);
                    case "PUT":
                        return Put(token, payload);
                    case "ALL":
                        return All(token, payload);
                    case "BYE":
                        closeConnection = true;
                        return "BYE";
                    default:
                        return command + " is an unknown command.";
                }
            }
            catch
            {
                return "Scriba could not understand your message.";
            }
        }

        private string Get(string token, string payload)
        {
            var row = EventRow.FromJson(payload);
            row.ApplicationId = token;
            Server.Gate.PushEventRow(row);
            return "OK";
        }

        private string Put(string token, string payload)
        {
            var row = EventRow.FromJson(payload);
            row.ApplicationId = token;
            Server.Gate.PushEventRow(row);
            return "OK";
        }

        /// <summary>
        ///     ALL Gets all events by APP_TOKEN, and a filter object. Sorts by descending date, and serves up to 1,000 events
        ///     (within 1.96 MBytes).
        /// </summary>
        /// <param name="token"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        private string All(string token, string payload)
        {
            var row = EventRow.FromJson(payload);
            row.ApplicationId = token;
            Server.Gate.PushEventRow(row);
            return "OK";
        }

        private void ShutdownEngine()
        {
            _client.Close();
        }
    }
}