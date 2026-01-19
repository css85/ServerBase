namespace WebTool.Base.Item
{
    public class CostumeSelectItemData : SelectItemData
    {
        public string Gender { get; set; }
        public string[] CostumeSlotTypes { get; set; }
        public string CostumeSlot { get; set; }
        public string CostumeType { get; set; }
        public string CostumeGrade { get; set; }
        public int StoreView { get; set; }
        public string PriceType { get; set; }
        public int Price { get; set; }
    }
}
