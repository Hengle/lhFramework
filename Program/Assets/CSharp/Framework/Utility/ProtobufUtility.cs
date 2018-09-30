using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;

namespace lhFramework.Infrastructure.Utility
{
    public class ProtobufUtility
    {
        public static  byte[] Serialize<T>(T instance)
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, instance);
                bytes = ms.ToArray();// new byte[ms.Position];
                //var fullBytes = ms.GetBuffer();
                //Array.Copy(fullBytes, bytes, bytes.Length);
            }
            return bytes;
        }
        public static T Deserialize<T>(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return Serializer.Deserialize<T>(ms);
            }
        }
    }
}
