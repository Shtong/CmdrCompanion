using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CmdrCompanion.Interface.Modules
{
    public sealed class BackgroundStreamWrapper
    {
        // This lock is used so that two instances of BackgroundStreamWrapper
        // can never read their streams at the same time. This is because
        // it seems there are concurrency problems when accessing the output
        // and error streams of the same process at the same time.
        private static object _streamLock = new object();

        private ReaderWriterLockSlim _messagesLock;
        private Queue<string> _messages;
        private StringBuilder _buffer;
        private StreamReader _targetStream;
        private bool _done;

        public BackgroundStreamWrapper(StreamReader reader)
        {
            _messagesLock = new ReaderWriterLockSlim();
            _messages = new Queue<string>();
            _buffer = new StringBuilder();

            _targetStream = reader;

            ThreadPool.QueueUserWorkItem(Run);
        }

        public bool Done
        {
            get { return _done; }
        }

        public bool HasMessages 
        {
            get
            {
                _messagesLock.EnterReadLock();
                try
                {
                    return _messages.Count > 0;
                }
                finally
                {
                    _messagesLock.ExitReadLock();
                }
            }
        }

        public string GetNextMessage()
        {
            _messagesLock.EnterUpgradeableReadLock();
            try
            {
                if (_messages.Count > 0)
                {
                    _messagesLock.EnterWriteLock();
                    try
                    {
                        return _messages.Dequeue();
                    }
                    finally
                    {
                        _messagesLock.ExitWriteLock();
                    }
                }
                else
                    return null;
            }
            finally
            {
                _messagesLock.ExitUpgradeableReadLock();
            }
        }

        private void Run(object state)
        {
            int c = 0;
            while((c = _targetStream.Read()) >= 0)
            {
                if(c == '\n' || c == '\r')
                {
                    if(_buffer.Length > 0)
                    {
                        _messagesLock.EnterWriteLock();
                        try
                        {
                            _messages.Enqueue(_buffer.ToString());
                        }
                        finally
                        {
                            _messagesLock.ExitWriteLock();
                        }
                        _buffer.Clear();
                    }
                }
                else
                {
                    _buffer.Append((char)c);
                }
            }

            _done = true;
        }
    }
}
