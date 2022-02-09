using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Enums
{
    public enum OrderStatus
    {
        Pending = 0,
        AwaitingPayment = 1,
        AwaitingFulfillment = 2,
        AwaitingShipment = 3,
        AwaitingPickup = 4,
        PartiallyShipped = 5,
        Completed = 6,
        Shipped = 7,
        Cancelled = 8,
        Declined = 9,
        Refunded = 10,
        Disputed = 11,
        ManualVerificationRequired = 12,
        PartiallyRefunded = 13
    }
}
