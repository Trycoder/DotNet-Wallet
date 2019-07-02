using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto_Services.ExternalService.BlockCypher.BlockCypherModel.Enums
{
    public enum HookEvent
    {
        UnconfirmedTransaction,
        NewBlock,
        ConfirmedTransaction,
        TransactionConfirmation,
        DoubleSpendTransaction
    }
}
