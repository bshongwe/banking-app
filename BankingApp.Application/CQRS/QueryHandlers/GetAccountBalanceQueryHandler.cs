using BankingApp.Infrastructure.Repositories;
using BankingApp.Application.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankingApp.Application.CQRS.QueryHandlers;

public class GetAccountBalanceQueryHandler
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountBalanceQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<decimal> HandleAsync(Queries.GetAccountBalanceQuery query)
    {
        var account = await _accountRepository.GetByIdAsync(query.AccountId);
        if (account == null)
            throw new ResourceNotFoundException("Account", query.AccountId);

        var balance = await _accountRepository.GetBalanceAsync(query.AccountId);
        return balance;
    }
}
