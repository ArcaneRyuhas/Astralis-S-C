using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessProject.Contracts
{
    [ServiceContract(CallbackContract = typeof(IMessageManagerCallback))]
    public interface IMessageManager
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(string message, string nickname);
    }

    [ServiceContract]
    public interface IMessageManagerCallback
    {
        [OperationContract]
        void ReceiveMessage(string message);
    }
}
