
// Copyright (C) 2006 by Nikola Paljetak

using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;

namespace NikeSoftChat
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IChatCallback))]
    interface IChat
    {
        [OperationContract(IsOneWay = false, IsInitiating = true, IsTerminating = false)]
        string[] Join(string name);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void Say(string msg);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void Whisper(string to, string msg);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = true)]
        void Leave();
    }

    interface IChatCallback
    {
        [OperationContract(IsOneWay = true)]
        void Receive(string senderName, string message);

        [OperationContract(IsOneWay = true)]
        void ReceiveWhisper(string senderName, string message);

        [OperationContract(IsOneWay = true)]
        void UserEnter(string name);

        [OperationContract(IsOneWay = true)]
        void UserLeave(string name);
    }

    //枚举客户端动作
    public enum MessageType { Receive, UserEnter, UserLeave, ReceiveWhisper };

    //自定义参数类
    public class ChatEventArgs : EventArgs
    {
        public MessageType msgType;
        public string name;
        public string message;
    }


    // InstanceContextMode.PerSession 服务器为每个客户会话创建一个新的上下文对象
    // ConcurrencyMode.Multiple      服务端可以进行多线程处理（同一时刻可以处理多个实例）
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ChatService : IChat  //继承IChat接口或者说IChat的实现类
    { 
        //Lock锁，注意最好用Object类型，引用类型可能会造成全局阻塞
        private static Object syncObj = new Object();
        //回调接口实例
        IChatCallback callback = null;
        //定义委托
        public delegate void ChatEventHandler(object sender, ChatEventArgs e);
        //定义静态委托事件
        public static event ChatEventHandler ChatEvent;
        //创建一个静态Dictionary，用于记录在线成员名称和与之对应的注册信息
        static Dictionary<string, ChatEventHandler> chatters = new Dictionary<string, ChatEventHandler>();
        //昵称
        private string name;

        //进入聊天室
        public string[] Join(string name)
        {
            bool userAdded = false;

            //由于服务端一次可以处理多个实例，所以需要锁定以便保证数据的一致性
            lock (syncObj)
            {
                //如果请求的昵称在成员字典中不存在并且昵称不空
                if (!chatters.ContainsKey(name) && name != "" && name != null)
                {
                    //全局昵称记录
                    this.name = name;
                    //保存昵称和ChatEventHandler的代理，这个代理只是一个指向而已
                    //如果想调用的话，可以通过同步调用：ChatEventHandler(object sender,ChatEventArgs e )
                    //也可以通过异步调用：ChatEventHandler.BeginInvoke(***)的方式来，本实例采用后一种。
                    chatters.Add(name, MyEventHandler);
                    userAdded = true;
                }
            }

            if (userAdded)
            {
                //获取当前操作客户端实例的通道给IChatCallback接口的实例callback,
                //此通道是一个定义为IChatCallback类型的泛类型
                //通道的类型是事先服务契约协定好的双工机制（见IChat前的ServiceContract）
                callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();
                //实例化事件消息类并赋值
                ChatEventArgs e = new ChatEventArgs();
                e.msgType = MessageType.UserEnter;
                e.name = name;
                
                //其实这里就是抛出事件，只不过利用了异步的方式来将事件抛出，同步抛出方式为：handler（this,e）
                //事件抛出后，需要接收方，下面一句就是将这个抛出的事件给接收住了。所以如果下面一条代码放到上面来，
                //就会收到自己进到聊天室的信息。
                BroadcastMessage(e);
                
                //这里也是处理回调信息的地方。将用户的加入信息回调出去发送给客户端。
                //这里也是注册事件的地方，将刚刚抛出的事件接收住，去处理。也就是当用户进入后，发送回调信息给客户端，告知用户已经进入。
                ChatEvent += MyEventHandler;  
                //以下代码返回当前进入聊天室成员的称列表
                string[] list = new string[chatters.Count];
                lock (syncObj)
                {
                    //从成员字典索引0 开始复制chatters成员字典的key 值到list 字符串数组
                    chatters.Keys.CopyTo(list, 0);
                }
                return list;
            }
            else
            {
                //当昵称重复或为空是，如果客户端做了为空检测，则可直接认为是名称重复，当前要在没有异常的情况下。
                return null;
            }
        }

        //群聊
        public void Say(string msg)
        {
            ChatEventArgs e = new ChatEventArgs();
            e.msgType = MessageType.Receive;
            e.name = this.name;
            e.message = msg;
            BroadcastMessage(e);  //抛出事件，事件可以多次抛出，只需要一个接收的地方就可以了。因为可以利用GetInvocationList来返回委托组。
        }

        //单聊
        public void Whisper(string to, string msg)
        {
            ChatEventArgs e = new ChatEventArgs();
            e.msgType = MessageType.ReceiveWhisper;
            e.name = this.name;
            e.message = msg;
            try
            {
                //创建一个临时委托实例
                ChatEventHandler chatterTo;
                lock (syncObj)
                {
                    //查找成员字典中，找到要接收者的委托调用
                    chatterTo = chatters[to];
                }
                //异步调用，由于这个委托对象是唯一的，所以直接调用即可。当然也ChatEvent += MyEventHandler;注册注册
                chatterTo.BeginInvoke(this, e, new AsyncCallback(EndAsync), null);
            }
            catch (KeyNotFoundException)
            {
                //访问集合中元素的键与集合中的任何键都不匹配时所引发的异常
            }
        }

        //成员离开聊天室
        public void Leave()
        {
            if (this.name == null)
                return;

            //删除成员字典中的当前会话的成员，及删除多路广播委托的调用列表中的当前调用
            //name 和myEventHandler 的生存周期是在当前会话中一直存在的，参考Session 周期
            lock (syncObj)
            {
                chatters.Remove(this.name);
            }
            ChatEvent -= MyEventHandler;
            ChatEventArgs e = new ChatEventArgs();
            e.msgType = MessageType.UserLeave;
            e.name = this.name;
            this.name = null;
            BroadcastMessage(e);
        }

        //回调
        //根据客户端动作通知对应客户端执行对应的操作
        private void MyEventHandler(object sender, ChatEventArgs e)
        {
            try
            {
                switch (e.msgType)
                {
                    case MessageType.Receive:
                        callback.Receive(e.name, e.message);
                        break;
                    case MessageType.ReceiveWhisper:
                        callback.ReceiveWhisper(e.name, e.message);
                        break;
                    case MessageType.UserEnter:
                        callback.UserEnter(e.name);
                        break;
                    case MessageType.UserLeave:
                        callback.UserLeave(e.name);
                        break;
                }
            }
            catch  //异常退出，或超时，或session过期
            {
                Leave();
            }
        }

        //发送广播信息
        //要点：根据上下文理解: 1 广播什么(what),2 为谁广播(who),3“谁”从哪来(where),4 如何来的(how)
        private void BroadcastMessage(ChatEventArgs e)
        {
            //创建回调委托事件实例ChatEvent的一个副本，之所以用副本是因为ChatEvent处于多线程并状态(？此处不知理解是否正确,因为我理解后面的handler 是一个引用，自相矛盾了)
            ChatEventHandler temp = ChatEvent;

            if (temp != null)
            {
                //GetInvocationList方法，按照调用顺序返回“多路广播委托(MulticastDelegate)”的调用列表
                foreach (ChatEventHandler handler in temp.GetInvocationList())
                {
                    //异步方式调用多路广播委托的调用列表中的ChatEventHandler 
                    //BeginInvoke方法异步调用，即不等等执行，详细说明则是：公共语言运行库(CLR) 将对请求进行排队并立即返回到调用方。将对来自线程池的线程调用该目标方法。
                    //EndAsync 为线程异步调用完成的回调方法，EndAsync 接收并操持着线程异步调用的操作状态,可通过此结果找到调用者，如此例handler，handler是一个委托实例的引用
                    //        此状态为调用者（委托）的事件声明类型此例为public event ChatEventHandler ChatEvent; 中的ChatEventHandler
                    //最后一个参数:包含的对象的状态信息，传递给委托;
                    handler.BeginInvoke(this, e, new AsyncCallback(EndAsync), null);
                }
            }
        }

        //广播中线程调用完成的回调方法
        //功能：清除异常多路广播委托的调用列表中异常对象（空对象）
        private void EndAsync(IAsyncResult ar)
        {
            ChatEventHandler d = null;

            try
            {
                //封装异步委托上的异步操作结果
                System.Runtime.Remoting.Messaging.AsyncResult asres = (System.Runtime.Remoting.Messaging.AsyncResult)ar;
                //asres.AsyncDelegate 获取在异步调用asres 的委托对象，asres 来自对ar 的AsyncResult 封装，ar 来自线程异步调用的操作状态
                d = ((ChatEventHandler)asres.AsyncDelegate);
                //EndInvoke 返回由异步操作ar 生成的结果Object
                d.EndInvoke(ar);
            }
            catch
            {
                ChatEvent -= d;
            }
        }
    }
}

