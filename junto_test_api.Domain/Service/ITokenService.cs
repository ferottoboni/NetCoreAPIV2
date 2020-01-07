namespace junto_test_api.Domain.Service
{
    public interface ITokenService<TViewModel, TEntity> : IService<TViewModel, TEntity>
    {
        TokenViewModel CreateNewToken(CreateTokenViewModel createTokenViewModel);
        int GetAccountIdByToken(string token);
        bool ValidateToken(string token);
    }
}