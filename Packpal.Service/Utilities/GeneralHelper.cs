using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Packpal.DAL.ModelViews.PayoutInfoModel;

namespace Packpal.BLL.Utilities;

public class GeneralHelper
{

	//Return both commission fee and amount left after deducting the fee
	public static (double CommissionFee, double AmountLeft) CalculateComissionFee(double amount, int percentage)
	{
		if (amount <= 0 || percentage < 0)
		{
			throw new ArgumentException("Amount must be greater than 0 and percentage must be non-negative.");
		}

		double fee = (amount * percentage) / 100;
		double amountLeft = amount - fee;

		return (fee, amountLeft);
	}

	
}
