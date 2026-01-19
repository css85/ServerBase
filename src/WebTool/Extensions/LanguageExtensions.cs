using System;
using Shared;
using Shared.CsvData;
using Shared.Packet.Models;
using Shared.Server.Define;
using Shared.ServerApp.Services;
using Shared.ServerModel;
using WebTool.Base.Item;

namespace WebTool.Extensions
{
    public static class LanguageExtensions
    {
        public static string GetNameLanguageKey(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string GetNameLanguageKey(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
        }

        
      

        public static string GetNameLanguageKey(this SessionLocation location)
        {
            return (SessionLocationType) location.Type switch
            {
                SessionLocationType.None => "오프라인",
                SessionLocationType.Disconnected => "연결끊김",
                SessionLocationType.Frontend => "메인메뉴",
                SessionLocationType.Lobby => "채널선택",
                SessionLocationType.Channel => $"채널: {location.ValueString}",
                SessionLocationType.GameRoom => $"채널: {location.ValueString}, 게임룸: {location.Value2}",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        
        public static string GetNameLanguageKey(this SelectItemType selectItemType)
        {
            return selectItemType switch
            {
                SelectItemType.Ruby => "루비",
                SelectItemType.Den => "덴",
                SelectItemType.Costume => "코스튬",
                SelectItemType.Couple_Costume => "커플 코스튬",
                SelectItemType.Item_Global_Functional => "기능성 아이템",
                SelectItemType.Item_Global_Buff => "버프 아이템",
                SelectItemType.Item_ShoppingMall_Coupon => "쇼핑몰 쿠폰 아이템",
                SelectItemType.Item_ShoppingMall_Functional => "쇼핑몰 기능성 아이템",
                SelectItemType.Item_ShowDancer_Functional => "쇼댄서 기능성 아이템",
                SelectItemType.Item_Event_Box => "이벤트 박스",
                SelectItemType.Item_Costume_Deco => "코스튬 장식",
                SelectItemType.Item_UserProfile_Frame => "유저 프로필 프레임",
                SelectItemType.Item_Costume_DecoPiece => "코스튬 장식 조각",
                SelectItemType.Item_ShowPuzzle_Functional => "쇼퍼즐 아이템",
                SelectItemType.Item_ShoppingMall_Ticket => "쇼핑몰 티켓 아이템",
                SelectItemType.Item_Global_Material => "재료 아이템",
                SelectItemType.Item_DanceMaster => "댄스마스터 댄스",
                SelectItemType.Item_DanceMaster_Jewel => "댄스마스터 댄스 주얼",
                SelectItemType.Item_Collection_Material => "컬렉션 재료 아이템",
                _ => throw new ArgumentOutOfRangeException(nameof(selectItemType), selectItemType, null)
            };
        }


    }
}