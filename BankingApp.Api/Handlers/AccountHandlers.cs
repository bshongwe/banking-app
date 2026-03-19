using BankingApp.Application.CQRS.CommandHandlers;
using BankingApp.Application.CQRS.QueryHandlers;

namespace BankingApp.Api.Handlers;

/// <summary>
/// Aggregates account command handlers (Create, Update, Freeze, Unfreeze)
/// </summary>
public class AccountCommandHandlers(
    CreateAccountCommandHandler create,
    UpdateAccountCommandHandler update,
    FreezeAccountCommandHandler freeze,
    UnfreezeAccountCommandHandler unfreeze)
{
    public CreateAccountCommandHandler Create { get; } = create;
    public UpdateAccountCommandHandler Update { get; } = update;
    public FreezeAccountCommandHandler Freeze { get; } = freeze;
    public UnfreezeAccountCommandHandler Unfreeze { get; } = unfreeze;
}

/// <summary>
/// Aggregates account query handlers (Get, List, Balance, Transactions)
/// </summary>
public class AccountQueryHandlers(
    GetAccountDetailQueryHandler getDetail,
    ListAccountsQueryHandler list,
    GetAccountBalanceQueryHandler getBalance,
    GetAccountTransactionHistoryQueryHandler getTransactions)
{
    public GetAccountDetailQueryHandler GetDetail { get; } = getDetail;
    public ListAccountsQueryHandler List { get; } = list;
    public GetAccountBalanceQueryHandler GetBalance { get; } = getBalance;
    public GetAccountTransactionHistoryQueryHandler GetTransactions { get; } = getTransactions;
}
