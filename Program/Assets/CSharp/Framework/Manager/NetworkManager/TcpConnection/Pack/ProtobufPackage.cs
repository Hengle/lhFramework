﻿using System;
using System.Collections.Generic;

namespace lhFramework.Infrastructure.Managers
{
    public class ProtobufPackage : IPackage
    {
      /// <summary>
       /// Pack your message here(this is only an example)
       /// </summary>
       /// <param name="source"></param>
       /// <param name="unpackedHandler"></param>
        void IPackage.Unpack(IByteArray source, Action<byte[]> unpackedHandler)
        {
            // Unpack your message(use int, 4 byte as head)
            while (source.Length >= 4)
            {
                var head = source.Read(4);
                int bodyLength = BitConverter.ToInt32(head, 0);// get body's length
                if (source.Length >= bodyLength)
                {
                    var unpacked = source.Read(bodyLength);// get body
                    unpackedHandler(unpacked);
                }
                else
                {
                    source.Insert(0, head);// rewrite in, used for next time
                }
            }
        }

        /// <summary>
        /// Unpack your message here(this is only an example)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="packedHandler"></param>
        void IPackage.Pack(IByteArray source, Action<byte[]> packedHandler)
        {
            // Add head length to your message(use int, 4 byte as head)
            var length = source.Length;
            var head = BitConverter.GetBytes(length);
            source.Insert(0, head);// add head bytes
            var packed = source.Read(source.Length);
            packedHandler(packed);
        }
    }
}
