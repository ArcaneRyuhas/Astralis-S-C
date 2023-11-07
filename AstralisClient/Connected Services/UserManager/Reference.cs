﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Astralis.UserManager {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="User", Namespace="http://schemas.datacontract.org/2004/07/MessageService.Contracts")]
    [System.SerializableAttribute()]
    public partial class User : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int ImageIdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string MailField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NicknameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string PasswordField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int ImageId {
            get {
                return this.ImageIdField;
            }
            set {
                if ((this.ImageIdField.Equals(value) != true)) {
                    this.ImageIdField = value;
                    this.RaisePropertyChanged("ImageId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Mail {
            get {
                return this.MailField;
            }
            set {
                if ((object.ReferenceEquals(this.MailField, value) != true)) {
                    this.MailField = value;
                    this.RaisePropertyChanged("Mail");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Nickname {
            get {
                return this.NicknameField;
            }
            set {
                if ((object.ReferenceEquals(this.NicknameField, value) != true)) {
                    this.NicknameField = value;
                    this.RaisePropertyChanged("Nickname");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Password {
            get {
                return this.PasswordField;
            }
            set {
                if ((object.ReferenceEquals(this.PasswordField, value) != true)) {
                    this.PasswordField = value;
                    this.RaisePropertyChanged("Password");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="UserManager.IUserManager")]
    public interface IUserManager {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUserManager/AddUser", ReplyAction="http://tempuri.org/IUserManager/AddUserResponse")]
        int AddUser(Astralis.UserManager.User user);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUserManager/AddUser", ReplyAction="http://tempuri.org/IUserManager/AddUserResponse")]
        System.Threading.Tasks.Task<int> AddUserAsync(Astralis.UserManager.User user);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUserManager/ConfirmUser", ReplyAction="http://tempuri.org/IUserManager/ConfirmUserResponse")]
        int ConfirmUser(string nickname, string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUserManager/ConfirmUser", ReplyAction="http://tempuri.org/IUserManager/ConfirmUserResponse")]
        System.Threading.Tasks.Task<int> ConfirmUserAsync(string nickname, string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUserManager/FindUserByNickname", ReplyAction="http://tempuri.org/IUserManager/FindUserByNicknameResponse")]
        bool FindUserByNickname(string nickname);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUserManager/FindUserByNickname", ReplyAction="http://tempuri.org/IUserManager/FindUserByNicknameResponse")]
        System.Threading.Tasks.Task<bool> FindUserByNicknameAsync(string nickname);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUserManager/GetUserByNickname", ReplyAction="http://tempuri.org/IUserManager/GetUserByNicknameResponse")]
        Astralis.UserManager.User GetUserByNickname(string nickname);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUserManager/GetUserByNickname", ReplyAction="http://tempuri.org/IUserManager/GetUserByNicknameResponse")]
        System.Threading.Tasks.Task<Astralis.UserManager.User> GetUserByNicknameAsync(string nickname);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUserManager/UpdateUser", ReplyAction="http://tempuri.org/IUserManager/UpdateUserResponse")]
        int UpdateUser(Astralis.UserManager.User user);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IUserManager/UpdateUser", ReplyAction="http://tempuri.org/IUserManager/UpdateUserResponse")]
        System.Threading.Tasks.Task<int> UpdateUserAsync(Astralis.UserManager.User user);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IUserManagerChannel : Astralis.UserManager.IUserManager, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class UserManagerClient : System.ServiceModel.ClientBase<Astralis.UserManager.IUserManager>, Astralis.UserManager.IUserManager {
        
        public UserManagerClient() {
        }
        
        public UserManagerClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public UserManagerClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public UserManagerClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public UserManagerClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public int AddUser(Astralis.UserManager.User user) {
            return base.Channel.AddUser(user);
        }
        
        public System.Threading.Tasks.Task<int> AddUserAsync(Astralis.UserManager.User user) {
            return base.Channel.AddUserAsync(user);
        }
        
        public int ConfirmUser(string nickname, string password) {
            return base.Channel.ConfirmUser(nickname, password);
        }
        
        public System.Threading.Tasks.Task<int> ConfirmUserAsync(string nickname, string password) {
            return base.Channel.ConfirmUserAsync(nickname, password);
        }
        
        public bool FindUserByNickname(string nickname) {
            return base.Channel.FindUserByNickname(nickname);
        }
        
        public System.Threading.Tasks.Task<bool> FindUserByNicknameAsync(string nickname) {
            return base.Channel.FindUserByNicknameAsync(nickname);
        }
        
        public Astralis.UserManager.User GetUserByNickname(string nickname) {
            return base.Channel.GetUserByNickname(nickname);
        }
        
        public System.Threading.Tasks.Task<Astralis.UserManager.User> GetUserByNicknameAsync(string nickname) {
            return base.Channel.GetUserByNicknameAsync(nickname);
        }
        
        public int UpdateUser(Astralis.UserManager.User user) {
            return base.Channel.UpdateUser(user);
        }
        
        public System.Threading.Tasks.Task<int> UpdateUserAsync(Astralis.UserManager.User user) {
            return base.Channel.UpdateUserAsync(user);
        }

    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="UserManager.ILobbyManager", CallbackContract=typeof(Astralis.UserManager.ILobbyManagerCallback))]
    public interface ILobbyManager {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILobbyManager/CreateLobby", ReplyAction="http://tempuri.org/ILobbyManager/CreateLobbyResponse")]
        string CreateLobby(Astralis.UserManager.User user);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILobbyManager/CreateLobby", ReplyAction="http://tempuri.org/ILobbyManager/CreateLobbyResponse")]
        System.Threading.Tasks.Task<string> CreateLobbyAsync(Astralis.UserManager.User user);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/ILobbyManager/ConnectLobby")]
        void ConnectLobby(Astralis.UserManager.User user, string gameId);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/ILobbyManager/ConnectLobby")]
        System.Threading.Tasks.Task ConnectLobbyAsync(Astralis.UserManager.User user, string gameId);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/ILobbyManager/DisconnectLobby")]
        void DisconnectLobby(Astralis.UserManager.User user);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/ILobbyManager/DisconnectLobby")]
        System.Threading.Tasks.Task DisconnectLobbyAsync(Astralis.UserManager.User user);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/ILobbyManager/ChangeLobbyUserTeam")]
        void ChangeLobbyUserTeam(Astralis.UserManager.User user, int team);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/ILobbyManager/ChangeLobbyUserTeam")]
        System.Threading.Tasks.Task ChangeLobbyUserTeamAsync(Astralis.UserManager.User user, int team);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/ILobbyManager/SendMessage")]
        void SendMessage(string message, string gameId);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/ILobbyManager/SendMessage")]
        System.Threading.Tasks.Task SendMessageAsync(string message, string gameId);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ILobbyManagerCallback {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILobbyManager/ShowConnectionInLobby", ReplyAction="http://tempuri.org/ILobbyManager/ShowConnectionInLobbyResponse")]
        void ShowConnectionInLobby(Astralis.UserManager.User user);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILobbyManager/ShowUsersInLobby", ReplyAction="http://tempuri.org/ILobbyManager/ShowUsersInLobbyResponse")]
        void ShowUsersInLobby(Astralis.UserManager.User[] userList);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILobbyManager/ShowDisconnectionInLobby", ReplyAction="http://tempuri.org/ILobbyManager/ShowDisconnectionInLobbyResponse")]
        void ShowDisconnectionInLobby(Astralis.UserManager.User user);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILobbyManager/UpdateLobbyUserTeam", ReplyAction="http://tempuri.org/ILobbyManager/UpdateLobbyUserTeamResponse")]
        void UpdateLobbyUserTeam(Astralis.UserManager.User user, int team);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILobbyManager/GiveLobbyId", ReplyAction="http://tempuri.org/ILobbyManager/GiveLobbyIdResponse")]
        void GiveLobbyId(string gameId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILobbyManager/ReceiveMessage", ReplyAction="http://tempuri.org/ILobbyManager/ReceiveMessageResponse")]
        void ReceiveMessage(string message);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ILobbyManagerChannel : Astralis.UserManager.ILobbyManager, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class LobbyManagerClient : System.ServiceModel.DuplexClientBase<Astralis.UserManager.ILobbyManager>, Astralis.UserManager.ILobbyManager {
        
        public LobbyManagerClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance) {
        }
        
        public LobbyManagerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName) {
        }
        
        public LobbyManagerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public LobbyManagerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public LobbyManagerClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress) {
        }
        
        public string CreateLobby(Astralis.UserManager.User user) {
            return base.Channel.CreateLobby(user);
        }
        
        public System.Threading.Tasks.Task<string> CreateLobbyAsync(Astralis.UserManager.User user) {
            return base.Channel.CreateLobbyAsync(user);
        }
        
        public void ConnectLobby(Astralis.UserManager.User user, string gameId) {
            base.Channel.ConnectLobby(user, gameId);
        }
        
        public System.Threading.Tasks.Task ConnectLobbyAsync(Astralis.UserManager.User user, string gameId) {
            return base.Channel.ConnectLobbyAsync(user, gameId);
        }
        
        public void DisconnectLobby(Astralis.UserManager.User user) {
            base.Channel.DisconnectLobby(user);
        }
        
        public System.Threading.Tasks.Task DisconnectLobbyAsync(Astralis.UserManager.User user) {
            return base.Channel.DisconnectLobbyAsync(user);
        }
        
        public void ChangeLobbyUserTeam(Astralis.UserManager.User user, int team) {
            base.Channel.ChangeLobbyUserTeam(user, team);
        }
        
        public System.Threading.Tasks.Task ChangeLobbyUserTeamAsync(Astralis.UserManager.User user, int team) {
            return base.Channel.ChangeLobbyUserTeamAsync(user, team);
        }
        
        public void SendMessage(string message, string gameId) {
            base.Channel.SendMessage(message, gameId);
        }
        
        public System.Threading.Tasks.Task SendMessageAsync(string message, string gameId) {
            return base.Channel.SendMessageAsync(message, gameId);
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="UserManager.IOnlineUserManager", CallbackContract=typeof(Astralis.UserManager.IOnlineUserManagerCallback))]
    public interface IOnlineUserManager {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IOnlineUserManager/ConectUser")]
        void ConectUser(string nickname);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IOnlineUserManager/ConectUser")]
        System.Threading.Tasks.Task ConectUserAsync(string nickname);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IOnlineUserManager/DisconectUser")]
        void DisconectUser(string nickname);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IOnlineUserManager/DisconectUser")]
        System.Threading.Tasks.Task DisconectUserAsync(string nickname);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IOnlineUserManagerCallback {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IOnlineUserManager/ShowUserConected", ReplyAction="http://tempuri.org/IOnlineUserManager/ShowUserConectedResponse")]
        void ShowUserConected(string nickname);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IOnlineUserManager/ShowUserDisconected", ReplyAction="http://tempuri.org/IOnlineUserManager/ShowUserDisconectedResponse")]
        void ShowUserDisconected(string nickname);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IOnlineUserManager/ShowOnlineFriends", ReplyAction="http://tempuri.org/IOnlineUserManager/ShowOnlineFriendsResponse")]
        void ShowOnlineFriends(System.Collections.Generic.Dictionary<string, bool> onlineFriends);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IOnlineUserManagerChannel : Astralis.UserManager.IOnlineUserManager, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class OnlineUserManagerClient : System.ServiceModel.DuplexClientBase<Astralis.UserManager.IOnlineUserManager>, Astralis.UserManager.IOnlineUserManager {
        
        public OnlineUserManagerClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance) {
        }
        
        public OnlineUserManagerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName) {
        }
        
        public OnlineUserManagerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public OnlineUserManagerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public OnlineUserManagerClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress) {
        }
        
        public void ConectUser(string nickname) {
            base.Channel.ConectUser(nickname);
        }
        
        public System.Threading.Tasks.Task ConectUserAsync(string nickname) {
            return base.Channel.ConectUserAsync(nickname);
        }
        
        public void DisconectUser(string nickname) {
            base.Channel.DisconectUser(nickname);
        }
        
        public System.Threading.Tasks.Task DisconectUserAsync(string nickname) {
            return base.Channel.DisconectUserAsync(nickname);
        }
    }
}
