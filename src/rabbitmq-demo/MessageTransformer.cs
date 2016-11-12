using Autofac;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace rabbitmq_demo
{
    static class MessageTransformer
    {
        public static TContract ToObject<TContract>(this string content)
        {
            return JsonConvert.DeserializeObject<TContract>(content);
        }

        public static string ToContent(this byte[] body)
        {
            return Encoding.UTF8.GetString(body);
        }

        public static byte[] ToBody(this string message)
        {
            return Encoding.UTF8.GetBytes(message);
        }

        public static string ToMessage<T>(this T input)
        {
            return JsonConvert.SerializeObject(input);
        }
    }

}
