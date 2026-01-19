using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SampleGame;
using Shared.CsvData;
using Shared.CsvParser;
//using Shared.CsvParser.Data;
using Shared.Server.Define;
using Shared.ServerModel;
using Shared.Clock;
using Shared.Server.Packet.Internal;
using System.Net.Sockets;
using System.Net;
using System.Numerics;

namespace Shared.ServerApp.Services
{
    public class CsvStoreContextData
    {        
        
        public List<StoreGachaRewardGroupCSVData> StoreGachaRewardGroupListData;
        public List<StoreGachaProbGroupCSVData> StoreGachaProbGroupListData;
        public List<StoreGachaPickupCSVData> StoreGachaPickupListData;
        public List<StorePackageRewardGroupCSVData> StorePackageRewardGroupListData;
        public List<StorePackageSpecialRewardGroupCSVData> StorePackageSpecialRewardGroupListData;
        public List<MaterialCSVData> MaterialListData;  
        public List<MaterialCombineCSVData> MaterialCombineListData;
        public List<RankingCSVData> RankingRewardInfoListData;
        public List<RankingRewardGroupCSVData> RankingRewardGroupListData;  
        public List<ShoppingmallGradeRewardGroupCSVData> ShoppingmallGradeRewardGroupListData;
        public List<MissionAchievementCSVData> MissionAchievementListData;
        public List<MissionNavigationCSVData> MissionNavigationListData;
        public List<MissionTwoDayBonusCSVData> MissionTwoDayBonusListData;
        public List<CouponRewardGroupCSVData> CouponRewardListData;
        public List<AttendanceRewardGroupCSVData> AttendanceRewardGroupListData;        
        public List<SpecialBuffCSVData> SpecialBuffListData;        
        public List<FairyRewardGroupCSVData> FairyRewardGroupListData;        
        public List<BoxRewardGroupCSVData> BoxRewardGroupListData;
     
        public List<PointCSVData> PointListData;
        
        //VIP 작업
        public List<VipCSVData> VipListData;
        public List<VipBuffListGroupCSVData> VipBuffListGroupListData;
        public List<VipDailyRewardGroupCSVData> VipDailyRewardGroupListData;
        public List<VipAttendanceRewardCSVData> VipAttendanceRewardListData;
        public List<VipUnlockPaymentCSVData> VipUnlockPaymentListData;
        public List<VipShopCSVData> VipShopListData;
        public List<VipShopItemRewardGroupCSVData> VipShopItemRewardGroupListData;
        public List<VipShopPartsRewardGroupCSVData> VipShopPartsRewardGroupListData;
        

        public Dictionary<long, CurrencyCSVData> CurrencyDicData;
        public Dictionary<long, StoreGachaCSVData> StoreGachaDicData;
        public Dictionary<long, StorePackageCSVData> StorePackageDicData;
        public Dictionary<long, StorePackageSpecialCSVData> StorePackageSpecialDicData;
        public Dictionary<int, ShoppingmallGradeCSVData> ShoppingmallGradeDicData;        
        public Dictionary<long, MissionDailyCSVData> MissionDailyDicData;
        public Dictionary<long, RecommendCSVData> RecommendDicData;        
        public Dictionary<string, CouponCSVData> CouponDicData;
        public Dictionary<long, AttendanceCSVData> AttendanceDicData;
        public Dictionary<long, BoxCSVData> BoxDicData;
        public Dictionary<long, ItemIntimacyCSVData> ItemIntimacyDicData;
        
        


        public Dictionary<CsvItemType, Dictionary<long, (object, string)>> AllItemData;

        public readonly bool IsTest;
        public readonly string CsvRootPath;

        public Dictionary<string, Func<string, bool>> ValidateFuncMap = new();
        private List<BaseCSVData> _csvDataList = new();

        private static Dictionary<Type, BaseCSVData> _dummyDataMap = new();

        public CsvStoreContextData(bool isTest, string csvRootPath)
        {
            IsTest = isTest;
            CsvRootPath = csvRootPath;
            LoadCsvDataAll();
        }

        public void LoadCsvDataAll()
        {
            _csvDataList.Clear();
            LoadStoreData();
            LoadShoppingmallData();
            LoadItemData();
            LoadMissionData();
            LoadFriendsData();
            LoadCouponData();
            LoadAttendanceData();

            var errorStringBuilder = new StringBuilder();
            foreach (var csvData in _csvDataList)
            {
                csvData.InitAfter(this);
                if (csvData.CheckValidationAfter(this, out _) == false)
                {
                    // 검증 에러
                    errorStringBuilder.AppendLine(
                        $"{csvData.GetFileName()}:{csvData.LineNumber}({csvData.KeyString}) CheckValidationAfter failed.");
                }
            }

            if (errorStringBuilder.Length > 0)
            {
                throw new Exception(errorStringBuilder.ToString());
            }
        }

        
        private void LoadStoreData()
        {
            StoreGachaDicData = LoadDictionary<long, StoreGachaCSVData>();
            StoreGachaRewardGroupListData = LoadList<StoreGachaRewardGroupCSVData>();
            StoreGachaProbGroupListData = LoadList<StoreGachaProbGroupCSVData>();
            StoreGachaPickupListData = LoadList<StoreGachaPickupCSVData>();
            StorePackageDicData = LoadDictionary<long, StorePackageCSVData>();
            StorePackageRewardGroupListData = LoadList<StorePackageRewardGroupCSVData>();
            StorePackageSpecialDicData = LoadDictionary<long, StorePackageSpecialCSVData>();
            StorePackageSpecialRewardGroupListData = LoadList<StorePackageSpecialRewardGroupCSVData>();
        }

        private void LoadShoppingmallData()
        {   
            RankingRewardInfoListData = LoadList<RankingCSVData>();
            RankingRewardGroupListData = LoadList<RankingRewardGroupCSVData>();
            ShoppingmallGradeDicData = LoadDictionary<int,  ShoppingmallGradeCSVData>();
            ShoppingmallGradeRewardGroupListData = LoadList<ShoppingmallGradeRewardGroupCSVData>();
            FairyRewardGroupListData = LoadList<FairyRewardGroupCSVData>();

            VipListData = LoadList<VipCSVData>();
            VipBuffListGroupListData = LoadList<VipBuffListGroupCSVData>();
            VipDailyRewardGroupListData = LoadList<VipDailyRewardGroupCSVData>();
            VipAttendanceRewardListData = LoadList<VipAttendanceRewardCSVData>();
            VipUnlockPaymentListData = LoadList<VipUnlockPaymentCSVData>();
            VipShopListData = LoadList<VipShopCSVData>();
            VipShopItemRewardGroupListData = LoadList<VipShopItemRewardGroupCSVData>();
            VipShopPartsRewardGroupListData = LoadList<VipShopPartsRewardGroupCSVData>();
        }

        private void LoadItemData()
        {
            CurrencyDicData = LoadDictionary<long, CurrencyCSVData>();
            MaterialListData = LoadList<MaterialCSVData>();
            MaterialCombineListData = LoadList<MaterialCombineCSVData>();
            SpecialBuffListData = LoadList<SpecialBuffCSVData>();   
            BoxDicData = LoadDictionary<long, BoxCSVData>();
            BoxRewardGroupListData = LoadList<BoxRewardGroupCSVData>();
            ItemIntimacyDicData = LoadDictionary<long, ItemIntimacyCSVData>();
            PointListData = LoadList<PointCSVData>();

        }

        private void LoadMissionData()
        {
            MissionDailyDicData = LoadDictionary<long, MissionDailyCSVData>(); 
            MissionAchievementListData = LoadList<MissionAchievementCSVData>();
            MissionNavigationListData = LoadList<MissionNavigationCSVData>();
            MissionTwoDayBonusListData = LoadList<MissionTwoDayBonusCSVData>();
        }

        private void LoadFriendsData()
        {
            RecommendDicData = LoadDictionary<long, RecommendCSVData>();
        }
      

        private void LoadCouponData()
        {
            CouponDicData = LoadDictionary<string, CouponCSVData>();
            CouponRewardListData = LoadList<CouponRewardGroupCSVData>();
        }
        private void LoadAttendanceData()
        {
            AttendanceDicData = LoadDictionary<long, AttendanceCSVData>();
            AttendanceRewardGroupListData = LoadList<AttendanceRewardGroupCSVData>();
        }
       
        private Dictionary<TKey, TValue> LoadDictionary<TKey, TValue>(Predicate<KeyValuePair<TKey, TValue>> where = null)
            where TValue : BaseCSVData, new()
        {
            var dummyData = LoadDummyData<TValue>();
            var filename = GetCsvFileName(dummyData.GetFileName());

            var result = where != null
                ? CSVUtility.Load<TKey, TValue>(Path.Combine(CsvRootPath, filename))
                    .Where(p => where(p))
                    .ToDictionary(p => p.Key, p => p.Value)
                : CSVUtility.Load<TKey, TValue>(Path.Combine(CsvRootPath, filename));

            ValidateFuncMap.TryAdd(dummyData.GetFileName(),
                dataString =>
                {
                    try
                    {
                        CSVUtility.LoadDictionary<TKey, TValue>(filename, dataString);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                });

            LoadData(result.Values);
            _csvDataList.AddRange(result.Values);

            return result;
        }

        private List<TValue> LoadList<TValue>(Predicate<TValue> where = null)
            where TValue : BaseCSVData, new()
        {
            var dummyData = LoadDummyData<TValue>();
            var filename = GetCsvFileName(dummyData.GetFileName());

            var result = where != null
                ? CSVUtility.Load<TValue>(Path.Combine(CsvRootPath, filename)).Where(p => where(p)).ToList()
                : CSVUtility.Load<TValue>(Path.Combine(CsvRootPath, filename));

            ValidateFuncMap.TryAdd(dummyData.GetFileName(),
                dataString =>
                {
                    try
                    {
                        CSVUtility.LoadList<TValue>(dummyData.GetFileName(), dataString);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                });

            LoadData(result);
            _csvDataList.AddRange(result);

            return result;
        }

        private void LoadData(IEnumerable<BaseCSVData> data)
        {
            foreach (var baseCSVData in data)
            {
                baseCSVData.Init();
            }
        }

        private BaseCSVData LoadDummyData<T>() where T : BaseCSVData, new()
        {
            if (_dummyDataMap.TryGetValue(typeof(T), out var dummyData) == false)
            {
                dummyData = new T();
                _dummyDataMap.TryAdd(typeof(T), dummyData);
            }
            return dummyData;
        }

        private string GetCsvFileName(string filename)
        {
            if (IsTest)
            {
                var testFilename = filename.Replace(".csv", ".test.csv");
                if (File.Exists(Path.Combine(CsvRootPath, testFilename)))
                {
                    filename = testFilename;
                }
            }

            return filename;
        }

    }
}
