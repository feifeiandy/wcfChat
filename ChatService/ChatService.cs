
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

    //ö�ٿͻ��˶���
    public enum MessageType { Receive, UserEnter, UserLeave, ReceiveWhisper };

    //�Զ��������
    public class ChatEventArgs : EventArgs
    {
        public MessageType msgType;
        public string name;
        public string message;
    }


    // InstanceContextMode.PerSession ������Ϊÿ���ͻ��Ự����һ���µ������Ķ���
    // ConcurrencyMode.Multiple      ����˿��Խ��ж��̴߳���ͬһʱ�̿��Դ�����ʵ����
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ChatService : IChat  //�̳�IChat�ӿڻ���˵IChat��ʵ����
    { 
        //Lock����ע�������Object���ͣ��������Ϳ��ܻ����ȫ������
        private static Object syncObj = new Object();
        //�ص��ӿ�ʵ��
        IChatCallback callback = null;
        //����ί��
        public delegate void ChatEventHandler(object sender, ChatEventArgs e);
        //���徲̬ί���¼�
        public static event ChatEventHandler ChatEvent;
        //����һ����̬Dictionary�����ڼ�¼���߳�Ա���ƺ���֮��Ӧ��ע����Ϣ
        static Dictionary<string, ChatEventHandler> chatters = new Dictionary<string, ChatEventHandler>();
        //�ǳ�
        private string name;

        //����������
        public string[] Join(string name)
        {
            bool userAdded = false;

            //���ڷ����һ�ο��Դ�����ʵ����������Ҫ�����Ա㱣֤���ݵ�һ����
            lock (syncObj)
            {
                //���������ǳ��ڳ�Ա�ֵ��в����ڲ����ǳƲ���
                if (!chatters.ContainsKey(name) && name != "" && name != null)
                {
                    //ȫ���ǳƼ�¼
                    this.name = name;
                    //�����ǳƺ�ChatEventHandler�Ĵ����������ֻ��һ��ָ�����
                    //�������õĻ�������ͨ��ͬ�����ã�ChatEventHandler(object sender,ChatEventArgs e )
                    //Ҳ����ͨ���첽���ã�ChatEventHandler.BeginInvoke(***)�ķ�ʽ������ʵ�����ú�һ�֡�
                    chatters.Add(name, MyEventHandler);
                    userAdded = true;
                }
            }

            if (userAdded)
            {
                //��ȡ��ǰ�����ͻ���ʵ����ͨ����IChatCallback�ӿڵ�ʵ��callback,
                //��ͨ����һ������ΪIChatCallback���͵ķ�����
                //ͨ�������������ȷ�����ԼЭ���õ�˫�����ƣ���IChatǰ��ServiceContract��
                callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();
                //ʵ�����¼���Ϣ�ಢ��ֵ
                ChatEventArgs e = new ChatEventArgs();
                e.msgType = MessageType.UserEnter;
                e.name = name;
                
                //��ʵ��������׳��¼���ֻ�����������첽�ķ�ʽ�����¼��׳���ͬ���׳���ʽΪ��handler��this,e��
                //�¼��׳�����Ҫ���շ�������һ����ǽ�����׳����¼�������ס�ˡ������������һ������ŵ���������
                //�ͻ��յ��Լ����������ҵ���Ϣ��
                BroadcastMessage(e);
                
                //����Ҳ�Ǵ���ص���Ϣ�ĵط������û��ļ�����Ϣ�ص���ȥ���͸��ͻ��ˡ�
                //����Ҳ��ע���¼��ĵط������ո��׳����¼�����ס��ȥ����Ҳ���ǵ��û�����󣬷��ͻص���Ϣ���ͻ��ˣ���֪�û��Ѿ����롣
                ChatEvent += MyEventHandler;  
                //���´��뷵�ص�ǰ���������ҳ�Ա�ĳ��б�
                string[] list = new string[chatters.Count];
                lock (syncObj)
                {
                    //�ӳ�Ա�ֵ�����0 ��ʼ����chatters��Ա�ֵ��key ֵ��list �ַ�������
                    chatters.Keys.CopyTo(list, 0);
                }
                return list;
            }
            else
            {
                //���ǳ��ظ���Ϊ���ǣ�����ͻ�������Ϊ�ռ�⣬���ֱ����Ϊ�������ظ�����ǰҪ��û���쳣������¡�
                return null;
            }
        }

        //Ⱥ��
        public void Say(string msg)
        {
            ChatEventArgs e = new ChatEventArgs();
            e.msgType = MessageType.Receive;
            e.name = this.name;
            e.message = msg;
            BroadcastMessage(e);  //�׳��¼����¼����Զ���׳���ֻ��Ҫһ�����յĵط��Ϳ����ˡ���Ϊ��������GetInvocationList������ί���顣
        }

        //����
        public void Whisper(string to, string msg)
        {
            ChatEventArgs e = new ChatEventArgs();
            e.msgType = MessageType.ReceiveWhisper;
            e.name = this.name;
            e.message = msg;
            try
            {
                //����һ����ʱί��ʵ��
                ChatEventHandler chatterTo;
                lock (syncObj)
                {
                    //���ҳ�Ա�ֵ��У��ҵ�Ҫ�����ߵ�ί�е���
                    chatterTo = chatters[to];
                }
                //�첽���ã��������ί�ж�����Ψһ�ģ�����ֱ�ӵ��ü��ɡ���ȻҲChatEvent += MyEventHandler;ע��ע��
                chatterTo.BeginInvoke(this, e, new AsyncCallback(EndAsync), null);
            }
            catch (KeyNotFoundException)
            {
                //���ʼ�����Ԫ�صļ��뼯���е��κμ�����ƥ��ʱ���������쳣
            }
        }

        //��Ա�뿪������
        public void Leave()
        {
            if (this.name == null)
                return;

            //ɾ����Ա�ֵ��еĵ�ǰ�Ự�ĳ�Ա����ɾ����·�㲥ί�еĵ����б��еĵ�ǰ����
            //name ��myEventHandler �������������ڵ�ǰ�Ự��һֱ���ڵģ��ο�Session ����
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

        //�ص�
        //���ݿͻ��˶���֪ͨ��Ӧ�ͻ���ִ�ж�Ӧ�Ĳ���
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
            catch  //�쳣�˳�����ʱ����session����
            {
                Leave();
            }
        }

        //���͹㲥��Ϣ
        //Ҫ�㣺�������������: 1 �㲥ʲô(what),2 Ϊ˭�㲥(who),3��˭��������(where),4 �������(how)
        private void BroadcastMessage(ChatEventArgs e)
        {
            //�����ص�ί���¼�ʵ��ChatEvent��һ��������֮�����ø�������ΪChatEvent���ڶ��̲߳�״̬(���˴���֪����Ƿ���ȷ,��Ϊ���������handler ��һ�����ã�����ì����)
            ChatEventHandler temp = ChatEvent;

            if (temp != null)
            {
                //GetInvocationList���������յ���˳�򷵻ء���·�㲥ί��(MulticastDelegate)���ĵ����б�
                foreach (ChatEventHandler handler in temp.GetInvocationList())
                {
                    //�첽��ʽ���ö�·�㲥ί�еĵ����б��е�ChatEventHandler 
                    //BeginInvoke�����첽���ã������ȵ�ִ�У���ϸ˵�����ǣ������������п�(CLR) ������������ŶӲ��������ص����÷������������̳߳ص��̵߳��ø�Ŀ�귽����
                    //EndAsync Ϊ�߳��첽������ɵĻص�������EndAsync ���ղ��ٳ����߳��첽���õĲ���״̬,��ͨ���˽���ҵ������ߣ������handler��handler��һ��ί��ʵ��������
                    //        ��״̬Ϊ�����ߣ�ί�У����¼��������ʹ���Ϊpublic event ChatEventHandler ChatEvent; �е�ChatEventHandler
                    //���һ������:�����Ķ����״̬��Ϣ�����ݸ�ί��;
                    handler.BeginInvoke(this, e, new AsyncCallback(EndAsync), null);
                }
            }
        }

        //�㲥���̵߳�����ɵĻص�����
        //���ܣ�����쳣��·�㲥ί�еĵ����б����쳣���󣨿ն���
        private void EndAsync(IAsyncResult ar)
        {
            ChatEventHandler d = null;

            try
            {
                //��װ�첽ί���ϵ��첽�������
                System.Runtime.Remoting.Messaging.AsyncResult asres = (System.Runtime.Remoting.Messaging.AsyncResult)ar;
                //asres.AsyncDelegate ��ȡ���첽����asres ��ί�ж���asres ���Զ�ar ��AsyncResult ��װ��ar �����߳��첽���õĲ���״̬
                d = ((ChatEventHandler)asres.AsyncDelegate);
                //EndInvoke �������첽����ar ���ɵĽ��Object
                d.EndInvoke(ar);
            }
            catch
            {
                ChatEvent -= d;
            }
        }
    }
}

