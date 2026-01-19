using System;
using System.Collections.Generic;
using System.Linq;
using Shared.CsvParser;
using Shared.Packet.Models;
using Shared.ServerApp.Services;

namespace Shared.CsvData
{
    public class AttendanceCSVData : BaseCSVData
    {
        public override string GetFileName() => "Attendance.csv";

        [CSVColumn("index", primaryKey: true)]
        public long Index { get; private set; }

        [CSVColumn("fr_dt")]
        public DateTime FrDt { get; private set; }

        [CSVColumn("to_dt")]
        public DateTime ToDt { get; private set; }

        [CSVColumn("attendance_type")]
        public AttendanceType AttendanceType { get; private set; }

        [CSVColumn("reward_group_id")]
        public long RewardGroupId { get; private set; }

        public List<AttendanceRewardGroupCSVData> RewardDatas { get; private set; }

        public bool IsValidTime(DateTime now)
        {
            return (FrDt <= now && now <= ToDt);
        }

        public override void InitAfter(CsvStoreContextData csvData)
        {
            base.InitAfter(csvData);
            RewardDatas = csvData.AttendanceRewardGroupListData.Where(p => p.RewardGroupId == RewardGroupId).ToList();
        }
    }

}
