using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SampleGame.Shared.Common;
using Integration.Tests.Utils;
using Shared;
using Shared.CsvData;
using Shared.Models;
using Shared.Packet.Models;
using Shared.PacketModel;
using Shared.PacketModel.Test;
using Shared.Server.Define;


namespace Integration.Tests.Client
{
    public partial class TestUserContext
    {
       
        
        public async Task<ItemInfo> AddItemAsync(RewardPaymentType obtainType, long itemId, string uniqueKey, int itemQty, int period)
        {
            var result = await SendPacketAsync<TestObtainItemRes>(new TestObtainItemReq
            {
                ObtainType = obtainType,
                ItemId = itemId,
                UniqueKey = uniqueKey,
                ItemQty = itemQty,
                Period = period,
            });
            AssertEx.EqualResult(ResultCode.Success, result.ResultCode);

            return result.Body.ItemInfo;
        }

        
 
        public async Task<ItemInfo> AddMoneyAsync(CurrencyType currencyType, int itemQty)
        {
            var result = await SendPacketAsync<TestObtainItemRes>(new TestObtainItemReq
            {
                ObtainType = RewardPaymentType.Currency,
                ItemId = (long) currencyType,
                UniqueKey = "",
                ItemQty = itemQty,
                Period = 0,
            });
            AssertEx.EqualResult(ResultCode.Success, result.ResultCode);

            return result.Body.ItemInfo;
        }

        public async Task UseCurrencyMoneyAsync(CurrencyType type, int amount)
        {
            var result = await SendPacketAsync<TestUseCurrencyMoneyRes>(new TestUseCurrencyMoneyReq
            {
                CurrencyType = type,
                Amount = amount,
            });
            AssertEx.EqualResult(ResultCode.Success, result.ResultCode);
        }

      
        public async Task ChargeCurrencyMoneyAsync()
        {
            await AddItemAsync(RewardPaymentType.Currency, 1, "", 10000000, 0);
            await AddItemAsync(RewardPaymentType.Currency, 2, "", 10000000, 0);
        }

    }
}