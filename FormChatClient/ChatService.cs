// This code was generated by a svcutil tool
// Modified by Nikola Paljetak

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(CallbackContract = typeof(IChatCallback), SessionMode = System.ServiceModel.SessionMode.Required)]
public interface IChat
{
    

    [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://tempuri.org/IChat/Join", ReplyAction = "http://tempuri.org/IChat/JoinResponse")]
    System.IAsyncResult BeginJoin(string name, System.AsyncCallback callback, object asyncState);

    string[] EndJoin(System.IAsyncResult result);

    [System.ServiceModel.OperationContractAttribute(IsOneWay = true, IsInitiating = false, Action = "http://tempuri.org/IChat/Leave")]
    void Leave();

    [System.ServiceModel.OperationContractAttribute(IsOneWay = true, IsInitiating = false, Action = "http://tempuri.org/IChat/Say")]
    void Say(string msg);

    [System.ServiceModel.OperationContractAttribute(IsOneWay = true, IsInitiating = false, Action = "http://tempuri.org/IChat/Whisper")]
    void Whisper(string to, string msg);
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public interface IChatCallback
{

    [System.ServiceModel.OperationContractAttribute(IsOneWay = true, Action = "http://tempuri.org/IChat/Receive")]
    void Receive(string senderName, string message);

    [System.ServiceModel.OperationContractAttribute(IsOneWay = true, Action = "http://tempuri.org/IChat/ReceiveWhisper")]
    void ReceiveWhisper(string senderName, string message);

    [System.ServiceModel.OperationContractAttribute(IsOneWay = true, Action = "http://tempuri.org/IChat/UserEnter")]
    void UserEnter(string name);

    [System.ServiceModel.OperationContractAttribute(IsOneWay = true, Action = "http://tempuri.org/IChat/UserLeave")]
    void UserLeave(string name);
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public interface IChatChannel : IChat, System.ServiceModel.IClientChannel
{
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public partial class ChatProxy : System.ServiceModel.DuplexClientBase<IChat>, IChat
{

    public ChatProxy(System.ServiceModel.InstanceContext callbackInstance)
        :
            base(callbackInstance)
    {
    }

    public ChatProxy(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName)
        :
            base(callbackInstance, endpointConfigurationName)
    {
    }

    public ChatProxy(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress)
        :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
    {
    }

    public ChatProxy(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress)
        :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
    {
    }

    public ChatProxy(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
        :
            base(callbackInstance, binding, remoteAddress)
    {
    }

    public System.IAsyncResult BeginJoin(string name, System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginJoin(name, callback, asyncState);
    }

    public string[] EndJoin(System.IAsyncResult result)
    {
        return base.Channel.EndJoin(result);
    }

    public void Leave()
    {
        base.Channel.Leave();
    }

    public void Say(string msg)
    {
        base.Channel.Say(msg);
    }

    public void Whisper(string to, string msg)
    {
        base.Channel.Whisper(to, msg);
    }
}

