using BankingApp.Application.CQRS.CommandHandlers;
using BankingApp.Application.CQRS.QueryHandlers;

namespace BankingApp.Api.Handlers;

/// <summary>
/// Aggregates account command handlers (Create, Update, Freeze, Unfreeze)
/// </summary>
public class AccountCommandHandlers
{
    public CreateAccountCommandHandler Create { get; }
    public UpdateAccountCommandHandler Update { get; }
    public FreezeAccountCommandHandler Freeze { get; }
    public UnfreezeAccountCommandHandler Unfreeze { get; }

    public AccountCommandHandlers(
        CreateAccountCommandHandler create,
        UpdateAccountCommandHandler update,
        FreezeAccountCommandHandler freeze,
        UnfreezeAccountCommandHandler unfreeze)
    {
        Create = create;
        Update = update;
        Freeze = freeze;
        Unfreeze = unfreeze;
    }
}

/// <summary>
/// Aggregates account query handlers (Get, List, Balance, Transactions)
/// </summary>
public class AccountQueryHandlers
{
    public GetAccountDetailQueryHandler GetDetail { get; }
    public ListAccountsQueryHandler List { get; }
    public GetAccountBalanceQueryHandler GetBalance { get; }
    public GetAccountTransactionHistoryQueryHandler GetTransactions { get; }

    public AccountQueryHandlers(
        GetAccountDetailQueryHandler getDetail,
        ListAccountsQueryHandler list,
        GetAccountBalanceQueryHandler getBalance,
        GetAccountTransactionHistoryQueryHandler getTransactions)
    {
        GetDetail = getDetail;
        List = list;
        GetBalance = getBalance;
        GetTransactions = getTransactions;
    }
}
