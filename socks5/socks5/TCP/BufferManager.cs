using System;
using System.Collections.Generic;

namespace Socona.Fiveocks.TCP
{
    public class BufferManager
    {
        private readonly int m_ByteSize;

        private readonly Stack<byte[]> m_Buffers;
        private readonly object m_LockObject = new Object();

        public static readonly BufferManager DefaultManager = new BufferManager(8192, 1000);

        #region constructors

        public BufferManager(int _byteSize, int _poolCount)
        {
            lock (m_LockObject)
            {
                m_ByteSize = _byteSize;
                m_Buffers = new Stack<Byte[]>(_poolCount);
                for (int i = 0; i < _poolCount; i++)
                {
                    CreateNewSegment();
                }
            }
        }

        #endregion //constructors

        public int AvailableBuffers
        {
            get { return m_Buffers.Count; }
        }


        public System.Int64 TotalBufferSizeInBytes
        {
            get { return m_Buffers.Count * m_ByteSize; }
        }

        public System.Int64 TotalBufferSizeInKBs
        {
            get { return (m_Buffers.Count * m_ByteSize / 1024); }
        }

        public System.Int64 TotalBufferSizeInMBs
        {
            get { return (m_Buffers.Count * m_ByteSize / 1024 / 1024); }
        }



        private void CreateNewSegment()
        {
            byte[] bytes = new byte[m_ByteSize];
            m_Buffers.Push(bytes);
        }



        /// <summary>
        /// Checks out a buffer from the manager
        /// </summary>        
        public Byte[] CheckOut()
        {
            lock (m_LockObject)
            {
                if (m_Buffers.Count == 0)
                {
                    CreateNewSegment();

                }
                return m_Buffers.Pop();
            }
        }


        /// <summary>
        /// Returns a buffer to the control of the manager
        /// </summary>
        ///<remarks>
        /// It is the client’s responsibility to return the buffer to the manger by
        /// calling Checkin on the buffer
        ///</remarks>
        public void CheckIn(Byte[] _Buffer)
        {
            lock (m_LockObject)
            {
                m_Buffers.Push(_Buffer);
            }
        }


    }
}
