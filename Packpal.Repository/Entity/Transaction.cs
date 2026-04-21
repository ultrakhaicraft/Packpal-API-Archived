using Packpal.DAL.Enum;

namespace Packpal.DAL.Entity;

public class Transaction
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public Guid OrderId { get; set; }  //Order ID associated with the transaction, regardless of the transaction type
	public string TransactionCode { get; set; } = string.Empty; // Order or Payment Code from PayOS payment gateway
	public double Amount { get; set; }
	public string Description { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public string Status { get; set; } = TransactionStatus.PENDING.ToString(); // Possible values: PENDING, COMPLETED, FAILED, CANCELLED
	public string TransactionType { get; set; } = TransactionTypeEnum.IN.ToString(); // IN/OUT (IN là renter chuyển cho app, OUT là staff chuyển cho keeper)

	//Navigation properties
	public virtual Order? Order { get; set; }

}
