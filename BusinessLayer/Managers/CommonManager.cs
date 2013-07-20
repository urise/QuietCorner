using CommonClasses.DbRepositoryInterface;

namespace BusinessLayer.Managers
{
    public class CommonManager: IDbManager
    {
        #region Constructors

        public CommonManager() { }

        public CommonManager(IDbRepository repository)
        {
            Db = repository;
        }

        #endregion

        public IDbRepository Db { get; set; }
    }
}
