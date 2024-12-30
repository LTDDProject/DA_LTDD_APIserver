namespace QLTours.Models
{
	public class BookingDTO
	{
		public int BookingId { get; set; }
		public string Status { get; set; }
		public int? UserId { get; set; }
		public int? TourId { get; set; }
		public decimal Total { get; set; }
		public DateTime BookingDate { get; set; }

		// New properties to hold the names
		public string? UserName { get; set; }
		public string? TourName { get; set; }


	}

}
