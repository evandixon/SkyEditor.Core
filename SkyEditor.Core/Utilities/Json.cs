﻿using SkyEditor.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyEditor.Core.Utilities
{
    /// <summary>
    /// Converts objects to and from JSON
    /// </summary>
    public static class Json
    {
        /// <summary>
        /// Serializes the specified object into a JSON string.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <returns>Json text that represents the given object.</returns>
        public static string Serialize(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object Deserialize(Type type, string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(json, type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="obj"></param>
        /// <param name="provider"></param>
        public static void SerializeToFile(string filename, object obj, IIOProvider provider)
        {
            provider.WriteAllText(filename, Serialize(obj));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static T DeserializeFromFile<T>(string filename, IIOProvider provider)
        {
            return Deserialize<T>(provider.ReadAllText(filename));
        }
    }
}
