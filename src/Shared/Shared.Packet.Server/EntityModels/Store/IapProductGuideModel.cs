using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("iap_product_guide")]    
    public class IapProductGuideModel
    {
        [Key]
        [Column("product_id")]
        public string ProductId { get; set; }
                
        [Column("price")]
        public int Price { get; set; } 

        [Column("product_name")]
        public string ProductName { get; set; }

    }
}
