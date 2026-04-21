# 💰 MANUAL PAYOUT FLOW DOCUMENTATION

## 🔄 **LUỒNG MANUAL HANDLING VỚI SIGNALR**

### **Bước 1: Payment Success (Manual)**
```
Renter thanh toán → PayOS → Admin manually calls: PaymentController.UpdatePaymentStatus()
↓
Order.IsPaid = true
↓
SignalR notification → Keeper: "Order đã được thanh toán!"
```

### **Bước 2: Keeper Request Payout**
```
Keeper nhận notification → UI "Request Payout"
↓
POST /api/payout/create
{
  "orderId": "guid",
  "keeperId": "guid", 
  "bankAccount": "optional",
  "note": "optional"
}
↓
PayoutRequest created (status: NOTPAID)
↓
SignalR notification → Staff: "New payout request!"
```

### **Bước 3: Staff Processing**
```
Staff dashboard → Payout Requests table
↓
PATCH /api/payout/{id}/start-processing
(status: NOTPAID → BUSY)
↓
Staff chuyển tiền ngoài hệ thống
↓
PATCH /api/payout/upload-proof + certification image
↓
PUT /api/payout/complete-payout/{id}
(status: BUSY → PAID)
↓
SignalR notification → Keeper: "Payout completed!"
```

## 📋 **API ENDPOINTS**

### **Keeper APIs**
- `GET /api/payout/check-eligibility/{orderId}` - Check if order eligible for payout
- `POST /api/payout/create` - Create payout request
- `GET /api/payout/my-requests` - Get keeper's payout requests
- `GET /api/payout/{id}` - Get payout details

### **Staff APIs**  
- `GET /api/payout/requests` - Get all payout requests (with filters)
- `PATCH /api/payout/{id}/start-processing` - Start processing payout
- `PATCH /api/payout/upload-proof` - Upload payment proof
- `PUT /api/payout/complete-payout/{id}` - Complete payout
- `PATCH /api/payout/update-status` - Change payout status

### **Payment APIs**
- `POST /api/payment/update-payment-status` - Update payment status (manual)
- `POST /api/payment/payos-webhook` - PayOS webhook callback (auto)

## 🔔 **NOTIFICATION FLOW**

### **Payment Success**
```
PaymentController → NotificationService.NotifyKeeperOrderStatusChangeAsync()
→ SignalR → Keeper: "Order {orderId} đã được thanh toán!"
```

### **Payout Request Created**
```
PayoutController → NotificationService.NotifyStaffOfIncomingPayoutAsync()
→ SignalR → Staff: "New payout request from keeper!"
```

### **Payout Completed**
```
PayoutService.CompletePayout() → NotificationService.NotifyKeeperPayoutCompletedAsync()
→ SignalR → Keeper: "Payout ${amount} đã hoàn thành!"
```

## 💾 **DATABASE CHANGES**

### **PayoutRequest Table**
```sql
- Id (Guid) - Primary key
- OrderId (Guid) - FK to Order
- UserId (Guid?) - Staff who processes (null when created by keeper)
- Amount (double) - Payout amount after commission
- Status (string) - NOTPAID/BUSY/PAID
- ImageURL (string?) - Payment proof certification
- CreatedAt (DateTime) - Request creation time
- TransactionId (Guid?) - FK to Transaction when completed
```

### **Updated Models**
```csharp
CreatePayoutInfo {
  OrderId: Guid,
  KeeperId: Guid,  // Changed from StaffId
  BankAccount?: string,
  Note?: string
}
```

## ⚡ **COMMISSION CALCULATION**
```csharp
// In GeneralHelper.CalculateComissionFee()
Total Order Amount: 100,000 VND
Commission (20%): 20,000 VND  
Keeper Payout: 80,000 VND
```

## 🔐 **AUTHORIZATION**
- **Keeper**: Can create payout requests, view own requests
- **Staff**: Can view all requests, process payouts, upload proof
- **System**: Auto-notifications via SignalR

## 📱 **UI INTEGRATION POINTS**

### **Mobile App (Keeper)**
1. Order detail screen → "Request Payout" button (if eligible)
2. Payout history screen → List of requests with status
3. Notifications → Payment success, payout completed

### **Staff Dashboard**
1. Payout Requests table → Status filters, search
2. Payout detail modal → Order info, bank details, proof upload
3. Notifications → New payout requests

## 🚀 **DEPLOYMENT CHECKLIST**
- ✅ Backend APIs implemented
- ✅ SignalR notifications working
- ✅ Database schema updated
- ⏳ Frontend UI integration needed
- ⏳ PayOS webhook configuration needed
- ⏳ Staff dashboard payout section needed

---
*Generated on: ${new Date().toISOString()}*
