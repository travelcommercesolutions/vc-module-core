﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Cart.Model;
using VirtoCommerce.Domain.Common;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Domain.Store.Model;
using System.Collections.Specialized;

namespace VirtoCommerce.Domain.Payment.Model
{
	public class PostProcessPaymentEvaluationContext : PaymentEvaluationContextBase
    {
		public PostProcessPaymentEvaluationContext()
		{
		}

		public PostProcessPaymentEvaluationContext(PaymentIn payment)
		{
			this.Payment = payment;
		}

		public Store.Model.Store Store { get; set; }

		public string OuterId { get; set; }
	}
}
