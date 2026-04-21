namespace Packpal.DAL.Enum
{
    public enum OrderStatusEnum
    {
        PENDING,     // Vừa tạo, chờ keeper confirm
        CONFIRMED,   // Keeper đã xác nhận
        IN_STORAGE,  // Nhận được gói hàng và bắt đầu tính giờ
        COMPLETED,   // Hoàn thành và đã thanh toán
        CANCELLED    // Đã hủy
    }
}
