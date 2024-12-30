using System;
using System.Collections.Generic;

namespace QLTours.Models;

public partial class TourDetail
{
    public int TourDetailId { get; set; }

    public int? TourId { get; set; }

    public int? VehicleId { get; set; }

    public int? HotelId { get; set; }

    public virtual Hotel? Hotel { get; set; }

    public virtual Tour? Tour { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
	// Hàm này sẽ lấy thông tin TourName, VehicleName, HotelName từ các khóa phụ
	public string GetTourDetailNames()
	{
		string tourName = Tour?.TourName ?? "No Tour";
		string vehicleName = Vehicle?.VehicleName ?? "No Vehicle";
		string hotelName = Hotel?.HotelName ?? "No Hotel";

		return $"Tour: {tourName}, Vehicle: {vehicleName}, Hotel: {hotelName}";
	}
}
