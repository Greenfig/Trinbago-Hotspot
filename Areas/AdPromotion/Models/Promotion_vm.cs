using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Trinbago_MVC5.Areas.Promotion.Models
{
    public class Promo
    {
        [DataType(DataType.Currency)]
        public double Price { get; set; }
        public int Days { get; set; }
        public int Interval { get; set; }
        public string FormatOutput {
            get
            {
                return Days != 0 && Interval != 0 ? string.Format("Every {0} Days For {1} Month(s) {2:C}", Interval, Days/31, Price) : Days == -1 ? string.Format("{0:C0}", Price) : string.Format("{0} Days {1:C}", Days, Price);
            }
        }
    }

    public class PromotionSelectList
    {
        public string Name { get; set; }
        public ICollection<Promo> Promo { get; set; }
    }

    public static class PromotionListData
    { 
        /*public static PromotionSelectList BumpAd = new PromotionSelectList() { Name = "BumpAd", Promo = new List<Promo>() { new Promo() { Interval = (int)BumpAdPaymentInfo.BumpAdInterval.FIRST, Days = (int)BumpAdPaymentInfo.BumpAdDuration.FIRST, Price = BumpAdPaymentInfo.BumpAdPrice.FIRST }, new Promo() { Interval = (int)BumpAdPaymentInfo.BumpAdInterval.SECOND, Days = (int)BumpAdPaymentInfo.BumpAdDuration.SECOND, Price = BumpAdPaymentInfo.BumpAdPrice.SECOND }, new Promo() { Interval = (int)BumpAdPaymentInfo.BumpAdInterval.THIRD, Days = (int)BumpAdPaymentInfo.BumpAdDuration.THIRD, Price = BumpAdPaymentInfo.BumpAdPrice.THIRD } } };
        public static PromotionSelectList FeaturedAd = new PromotionSelectList() { Name = "FeaturedAd", Promo = new List<Promo>() { new Promo() { Days = (int)FeaturedAdPaymentInfo.FeaturedAdDays.FIRST, Price = FeaturedAdPaymentInfo.FeaturedAdPrice.FIRST }, new Promo() { Days = (int)FeaturedAdPaymentInfo.FeaturedAdDays.SECOND, Price = FeaturedAdPaymentInfo.FeaturedAdPrice.SECOND }, new Promo() { Days = (int)FeaturedAdPaymentInfo.FeaturedAdDays.THIRD, Price = FeaturedAdPaymentInfo.FeaturedAdPrice.THIRD } } };
        public static PromotionSelectList TopAd = new PromotionSelectList() { Name = "TopAd", Promo = new List<Promo>() { new Promo() { Days = (int)TopAdPaymentInfo.TopAdDays.FIRST, Price = TopAdPaymentInfo.TopAdPrice.FIRST }, new Promo() { Days = (int)TopAdPaymentInfo.TopAdDays.SECOND, Price = TopAdPaymentInfo.TopAdPrice.SECOND }, new Promo() { Days = (int)TopAdPaymentInfo.TopAdDays.THIRD, Price = TopAdPaymentInfo.TopAdPrice.THIRD } } }; 
        public static PromotionSelectList UrgentAd = new PromotionSelectList() { Name = "UrgentAd", Promo = new List<Promo>() { new Promo() { Days = (int)UrgentAdPaymentInfo.UrgentdAdDays.FIRST, Price = UrgentAdPaymentInfo.UrgentAdPrice.FIRST } } };
    */
    }
}