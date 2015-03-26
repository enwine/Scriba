using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace ScribaService
{
    class Server
    {
        ConcurrentQueue<EventRow> setEventsQueue;
        TcpListener server;
        private static readonly Lazy<Server> _instance = new Lazy<Server>(() => new Server());
        private Server()
        {
            setEventsQueue = new ConcurrentQueue<EventRow>();
        }
        public static Server Gate
        {
            get
            {
                return _instance.Value;
            }
        }


        /// <summary>
        /// Makes the server listen to a given port.
        /// </summary>
        /// <param name="port">Port number to listen, by default: 27422.</param>
        public void OpenPort(int port = 27422)
        {
            // ToDo
            // Needs to be reviewd as this constructor is dreprecated.
            server = new TcpListener(port);
            server.Start();
            ProcessQueue();
            
            TcpClient client = default(TcpClient);

            while (true)
            {
                try
                {
                    client = server.AcceptTcpClient();
                    ForkClient(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

        }
        /// <summary>
        /// Pushes an element to the end of the processing FIFO queue.
        /// </summary>
        /// <param name="ev">Item to push.</param>
        public void PushEventRow(EventRow ev)
        {
            setEventsQueue.Enqueue(ev);
        }
        /// <summary>
        /// Pops the oldest element, following a FIFO structure.
        /// </summary>
        /// <remarks>Before returning the object, it is processed and saved to disk.</remarks>
        /// <returns>An EventRow object, null if there's no more objects.</returns>
        public EventRow PopEventRow()
        {
            EventRow row = null;
            if (setEventsQueue.TryDequeue(out row))
            {
                // insert to disk
            }
            return row;
        }


        private void ForkClient(TcpClient client)
        {
            Engine engine = new Engine(client);
            Thread thread = new Thread(engine.EngineLoop);
            thread.Start();
        }
        private void ProcessQueue(object obj = null)
        {
            while (!setEventsQueue.IsEmpty)
            {
                PopEventRow();
            }
            new Timer(ProcessQueue, null, 200, Timeout.Infinite);
        }
    }
}
