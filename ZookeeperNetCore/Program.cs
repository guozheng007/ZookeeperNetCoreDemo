using org.apache.zookeeper;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static org.apache.zookeeper.ZooDefs;

namespace ZookeeperNetCore
{
    class Program
    {
        public const int timeout = 5000;
        static async Task Main(string[] args)
        {
            var conf = new ZookeeperClient("139.198.13.12:5011,139.198.13.12:5012,139.198.13.12:5013", timeout);

            try
            {
                conf.QueryPath = "/UserName";

                Console.WriteLine("客户端开始连接zookeeper服务器...");
                Console.WriteLine($"连接状态：{conf.ZK.getState()}");
                Thread.Sleep(1000);//注意：为什么要加上这行代码，如果不加会出现什么问题
                Console.WriteLine($"连接状态：{conf.ZK.getState()}");

                if (await conf.ZK.existsAsync(conf.QueryPath, false) == null)
                {
                    conf.ConfigData = Encoding.Default.GetBytes("guozheng");
                    await conf.ZK.createAsync(conf.QueryPath, conf.ConfigData, Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                }

                string configData = await conf.ReadConfigDataAsync();
                Console.WriteLine("节点【{0}】目前的值为【{1}】。", conf.QueryPath, configData);
                Console.ReadLine();


                Random random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                conf.ConfigData = Encoding.UTF8.GetBytes(string.Format("Mike_{0}", random.Next(100)));

                await conf.ZK.setDataAsync(conf.QueryPath, conf.ConfigData, -1);

                Console.WriteLine("节点【{0}】的值已被修改为【{1}】。", conf.QueryPath, Encoding.UTF8.GetString(conf.ConfigData));

                Console.ReadLine();

                if (await conf.ZK.existsAsync(conf.QueryPath, false) != null)
                {
                    await conf.ZK.deleteAsync(conf.QueryPath, -1);

                    Console.WriteLine("已删除此【{0}】节点。{1}", conf.QueryPath, Environment.NewLine);
                }

            }
            catch (Exception ex)
            {
                if (conf.ZK == null)
                {
                    Console.WriteLine("已关闭ZooKeeper的连接。");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("抛出异常：{0}【{1}】。", Environment.NewLine, ex.ToString());
            }
            finally
            {
                await conf.Close();
                Console.WriteLine("已关闭ZooKeeper的连接。");
                Console.ReadLine();
            }



            ////开始会话重连
            //Console.WriteLine("开始会话重连...");

            //var conf2 = new ZookeeperClient("139.198.13.12:5011,139.198.13.12:5012,139.198.13.12:5013", timeout, sessionId, sessionPassword);

            //Console.WriteLine(conf2.ZK.getSessionId());
            //Console.WriteLine( Encoding.UTF8.GetString(conf2.ZK.getSessionPasswd()));

            //Console.WriteLine($"重新连接状态zkSession：{conf2.ZK.getState()}");
            //Thread.Sleep(1000);//注意：为什么要加上这行代码，如果不加会出现什么问题
            //Console.WriteLine($"重新连接状态zkSession：{conf2.ZK.getState()}");


            Console.ReadKey();
        }
    }
}
