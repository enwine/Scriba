using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace ScribaService
{
    internal class Server
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<Server> _instance = new Lazy<Server>(() => new Server());
        private readonly ConcurrentQueue<EventRow> _setEventsQueue;
        private TcpListener _server;

        private Server()
        {
            _setEventsQueue = new ConcurrentQueue<EventRow>();
        }

        public static Server Gate
        {
            get { return _instance.Value; }
        }

        /// <summary>
        ///     Makes the server listen to a given port.
        /// </summary>
        /// <param name="port">Port number to listen, by default: 27422.</param>
        public void OpenPort(int port = 27422)
        {
            // ToDo
            // Needs to be reviewd as this constructor is dreprecated.
            _server = new TcpListener(port);
            _server.Start();
            ProcessQueue();

            Console.WriteLine("\nScriba server is now running...\n");

            while (true)
            {
                try
                {
                    var client = _server.AcceptTcpClient();
                    ForkClient(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        /// <summary>
        ///     Pushes an element to the end of the processing FIFO queue.
        /// </summary>
        /// <param name="ev">Item to push.</param>
        public void PushEventRow(EventRow ev)
        {
            _setEventsQueue.Enqueue(ev);
        }

        /// <summary>
        ///     Pops the oldest element, following a FIFO structure.
        /// </summary>
        /// <remarks>Before returning the object, it is processed and saved to disk.</remarks>
        /// <returns>An EventRow object, null if there's no more objects.</returns>
        public EventRow PopEventRow()
        {
            EventRow row;
            if (_setEventsQueue.TryDequeue(out row))
            {
                // insert to disk
            }
            return row;
        }

        private void ForkClient(TcpClient client)
        {
            var engine = new Engine(client);
            var thread = new Thread(engine.EngineLoop);
            thread.Start();
        }

        private void ProcessQueue(object obj = null)
        {
            while (!_setEventsQueue.IsEmpty)
            {
                PopEventRow();
            }
            // ReSharper disable once ObjectCreationAsStatement
            new Timer(ProcessQueue, null, 200, Timeout.Infinite);
        }
    }
}