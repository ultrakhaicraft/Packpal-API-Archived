using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packpal.BLL.Utilities;

public static class ErrorMessage
{
	internal static string ValidatePassword = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt (@#$%^&*!_)";
	internal static string AccountExist = "Email đã được sử dụng, vui lòng sử dụng email khác";
	internal static string ConfirmPasswordNotMatch = "Xác nhận mật khẩu không khớp";
	internal static string InvalidPassword = "Mật khẩu không chính xác";
	internal static string AccountNotFound = "Tài khoản không tồn tại";
	internal static string UserNotFound = "Không tìm thấy người dùng. Vui lòng kiểm tra lại email";
	internal static string InvalidPhoneNumber = "Số điện thoại không hợp lệ. Vui lòng sử dụng định dạng số điện thoại Việt Nam (VD: 0901234567, +84901234567)";
	internal static string LockerNotAvailable = "Tủ đồ này đã được thuê bởi người dùng khác";
	internal static string LockerNotFound= "Không tìm thấy tủ đồ";
	internal static string LockerStationNotFound = "Không tìm thấy trạm tủ đồ";
	internal static string KeeperExist = "Người giữ đồ đã tồn tại với số CMND này, vui lòng sử dụng số CMND khác";
}
