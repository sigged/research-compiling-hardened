using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Sigged.CodeHost.Core.Serialization
{
    public static class ChSerializer
    {
        //public static void Serialize<T>(Stream stream, T subject)
        //{
        //    BinaryFormatter bf = new BinaryFormatter();
        //    byte[] array;
        //    using (var ms = new MemoryStream())
        //    {
        //        bf.Serialize(ms, subject);
        //        array = ms.ToArray();
        //    }

        //    byte[] lengthPrefix = BitConverter.GetBytes(array.Length);
        //    if (BitConverter.IsLittleEndian)
        //        Array.Reverse(lengthPrefix);

        //    stream.Write(lengthPrefix, 0, lengthPrefix.Length);
        //    stream.Write(array, 0, array.Length);
        //}

        //public static T Deserialize<T>(Stream stream)
        //{
        //    object subject;

        //    byte[] lengthPrefix = new byte[4];
        //    stream.Read(lengthPrefix, 0, lengthPrefix.Length);
        //    if (BitConverter.IsLittleEndian)
        //        Array.Reverse(lengthPrefix);
        //    int length = BitConverter.ToInt32(lengthPrefix);

        //    byte[] data = new byte[length];
        //    stream.Read(data, 0, data.Length);

        //    BinaryFormatter bf = new BinaryFormatter();
        //    using (var ms = new MemoryStream(data))
        //    {
        //        subject = bf.Deserialize(ms);
        //    }
        //    return (T)subject;
        //}

        public static void Serialize<T>(Stream stream, T subject)
        {
            string json = JsonConvert.SerializeObject(subject);
            //BinaryFormatter bf = new BinaryFormatter();
            //byte[] array;
            //using (var ms = new MemoryStream())
            //{
            //    bf.Serialize(ms, subject);
            //    array = ms.ToArray();
            //}

            
            byte[] array = Encoding.Unicode.GetBytes(json);

            byte[] lengthPrefix = BitConverter.GetBytes(array.Length);
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(lengthPrefix);

            stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            stream.Flush();
            stream.Write(array, 0, array.Length);
            stream.Flush();
        }

        public static T Deserialize<T>(Stream stream)
        {
            byte[] lengthPrefix = new byte[4];
            int read = stream.Read(lengthPrefix, 0, lengthPrefix.Length);
            stream.Flush();
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(lengthPrefix);
            int length = BitConverter.ToInt32(lengthPrefix);

            byte[] data = new byte[length];

             read = stream.Read(data, 0, data.Length);
            stream.Flush();

            string json = Encoding.Unicode.GetString(data);

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
