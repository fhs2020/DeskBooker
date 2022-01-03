namespace DeskBooker.Core.Domain
{
    public class DeskbookingResult : DeskBookingBase
    {
        public DeskBookingResultCode Code { get; set; }
        public int? DeskBookingId { get; set; }
    }
}