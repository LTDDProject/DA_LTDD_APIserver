namespace QLTours.Models
{
	public class PaymentRequest
	{
		public int TourId { get; set; }
		public int UserId { get; set; }
		public decimal Total { get; set; }
	}

}
