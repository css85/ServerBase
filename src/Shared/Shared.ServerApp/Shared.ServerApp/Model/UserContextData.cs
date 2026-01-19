using System.Collections.Generic;
using Shared.Entities.Models;

namespace Shared.ServerApp.Model
{
    public class UserContextData
    {
        public readonly long UserSeq;
        public readonly int SessionId;
//        public readonly Wallet UserWallet;
        //public UserContextData(Wallet userWallet)
        //{
        //    UserSeq = userWallet.UserSeq;
        //    UserWallet = userWallet;
        //}

        public UserContextData()
        {

        }
        
    }
}