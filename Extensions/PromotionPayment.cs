using System.Collections.Generic;

namespace Trinbago_MVC5.Extensions
{
    public class PromoDurationStruct
    {
        public int Days { get; set; }
        public int Weeks { get; set; }
        public string Duration { get; set; }
    }

    public static class PromotionStaticInfo
    {
        /// <summary>
        /// List of possible promotion durations min 2 weeks max 12 weeks
        /// </summary>
        public static class PromotionDuration
        {
            public static IReadOnlyCollection<PromoDurationStruct> DurationRange = new List<PromoDurationStruct>() {
                new PromoDurationStruct() { Days = 0, Duration = "-None-", Weeks = 0 },
                new PromoDurationStruct() { Days = 14, Duration = "2 Weeks", Weeks = 2 },
                new PromoDurationStruct() { Days = 28, Duration = "4 Weeks", Weeks = 4 },
                new PromoDurationStruct() { Days = 42, Duration = "6 Weeks", Weeks = 6 },
                new PromoDurationStruct() { Days = 56, Duration = "8 Weeks", Weeks = 8 },
                new PromoDurationStruct() { Days = 70, Duration = "10 Weeks", Weeks = 10 },
                new PromoDurationStruct() { Days = 84, Duration = "12 Weeks", Weeks = 12 }
            };
        }
        public static class BumpAdPaymentInfo
        {
            /// <summary>
            /// Price per week
            /// </summary>
            public struct BumpAdPrice
            {
                public const double Price = 10.00;
            }
        }
        public static class FeaturedAdPaymentInfo
        {
            /// <summary>
            /// Price per week
            /// </summary>
            public struct FeaturedAdPrice
            {
                public const double Price = 75.00;
            }
        }
        public static class TopAdPaymentInfo
        {
            /// <summary>
            /// Price per week
            /// </summary>
            public struct TopAdPrice
            {
                public const double Price = 50.00;
            }
        }
        public static class UrgentAdPaymentInfo
        {
            /// <summary>
            /// Price per week
            /// </summary>
            public struct UrgentAdPrice
            {
                public const double Price = 25.00;
            }
        }
    }
}