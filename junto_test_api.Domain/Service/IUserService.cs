namespace junto_test_api.Domain.Service
{
    public interface IUserService<TViewModel, TEntity> : IService<TViewModel, TEntity>
    {
        bool ChangePassword(ChangePasswordViewModel newPass);
    }
}