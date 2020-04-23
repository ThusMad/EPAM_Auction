﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAM_DataAccessLayer.Entities
{
    [Table("Bids")]
    public class Bid
    {
        [Index(IsUnique = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string PlacerId { get; set; }
        public Guid AuctionId { get; set; }
        public Auction Auction { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }
        public long Time { get; set; }
    }
}
