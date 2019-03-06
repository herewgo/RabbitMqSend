using RabbitMQ.Client;
using System;

namespace RabbitMqSend
{
    class Program
    {
        //send
        static void Main(string[] args)
        {
            DirectExchange();

            //DirectExchangeSendMsg();
            //Console.ReadKey();
        }
        //<1>RabbitMQ的direct类型Exchange
        //<2> RabbitMQ的Topic类型Exchange
        private static readonly ConnectionFactory rabbitFactory = new ConnectionFactory()
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "luf",
            Password = "123456"
        };
        private static string exchangeName = "luf.exchange";
        private static string queueName = "luf.queue";
        //private static string routeKey = "luf.routeKey";

        public static bool ArgsInput(out string exchangeName, out string queueName, out string routeKey)
        {
            exchangeName = queueName = routeKey = null;
            bool flag = true;
            while (flag)
            {
                Console.Write("请输入(Exchange,Queue,RouteKey)(输入exit退出)：");
                string str = Console.ReadLine();
                if (str == "exit")
                {
                    flag = false;
                    return false;
                }
                if (!str.Contains(","))
                {
                    Console.WriteLine("格式不正确");
                    continue;
                }
                string[] arr = str.Split(',');
                if (arr.Length == 2)
                {
                    exchangeName = arr[0];
                    queueName = arr[1];
                    routeKey = queueName;
                    flag = false;
                    return true;
                }
                else if (arr.Length == 3)
                {
                    exchangeName = arr[0];
                    queueName = arr[1];
                    routeKey = arr[2];
                    flag = false;
                    return true;
                }
                Console.WriteLine("格式不正确");
            }

            return true;
        }
        public enum ExchangeType
        {
            direct = 1, fanout = 2, topic = 3
        }
        public static void DirectExchange()
        {
            Console.Write("请输入Exchange类型(direct = 1, fanout = 2, topic = 3)：");
            string exchangeTypeString = null;
            int exchangeType = Convert.ToInt32(Console.ReadLine());
            if (exchangeType < 1 || exchangeType > 3)
            {
                Console.WriteLine("格式不正确");
            }
            else
            {
                exchangeTypeString = ((ExchangeType)exchangeType).ToString();
                Console.WriteLine($"Exchange:{exchangeTypeString}类型");
                string exchangeName, queueName, routeKey = null;
                if (ArgsInput(out exchangeName, out queueName, out routeKey))
                {
                    using (IConnection conn = rabbitFactory.CreateConnection())
                    {
                        using (IModel channel = conn.CreateModel())
                        {
                            //direct,fanout,topic
                            channel.ExchangeDeclare(exchangeName, exchangeTypeString, durable: true, autoDelete: false, arguments: null);
                            channel.QueueDeclare(queueName, true, false, false, null);
                            channel.QueueBind(queueName, exchangeName, routingKey: routeKey);

                            var props = channel.CreateBasicProperties();
                            props.Persistent = true;

                            string text = Console.ReadLine();
                            while (text != "exit")
                            {
                                var bytes = System.Text.Encoding.UTF8.GetBytes(text);
                                channel.BasicPublish(exchange: exchangeName, routingKey: routeKey, basicProperties: props, body: bytes);
                                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}-{exchangeTypeString}-{exchangeName}-{queueName}-{routeKey}:{text}");
                                text = Console.ReadLine();
                            }
                        }
                    }
                }
            }
        }

    }
}
