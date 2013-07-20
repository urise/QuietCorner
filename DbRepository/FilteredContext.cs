using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonClasses.DbClasses;
using System.Linq.Expressions;

namespace DbLayer
{
    public class FilteredContext: IDisposable
    {
        #region Properties and Constructors

        private int? _instanceId;
        public int? InstanceId { get { return _instanceId; } }
        private readonly MainDbContext _context;

        public FilteredContext(int? instanceId)
        {
            _instanceId = instanceId;
            _context = new MainDbContext();
        }

        public void SetInstanceId(int? instanceId)
        {
            _instanceId = instanceId;
        }

        private Dictionary<Type, object> _filteredTableDict;
        protected Dictionary<Type, object> FilteredTableDict
        {
            get
            {
                if (_filteredTableDict == null) InitializeFilteredTableDict();
                return _filteredTableDict;
            }
        }

        private void InitializeFilteredTableDict()
        {
            _filteredTableDict = new Dictionary<Type, object>();
            var propertyInfos = GetType().GetProperties();
            foreach (var propertyInfo in propertyInfos)
            {
                if (propertyInfo.PropertyType.FullName != null && propertyInfo.PropertyType.FullName.StartsWith("System.Linq.IQueryable"))
                {
                    _filteredTableDict.Add(propertyInfo.PropertyType.GenericTypeArguments[0], propertyInfo.GetValue(this));
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _context.Dispose();
        }

        #endregion

        #region Tables

        public IQueryable<User> Users
        {
            get { return _context.Users; }
        }

        public IQueryable<Instance> Instances
        {
            get { return _context.Instances; }
        }

        public IQueryable<UserInstance> UserInstances
        {
            get { return _context.UserInstances; }
        }

        public IQueryable<InstanceUsage> InstanceUsages
        {
            get { return _context.InstanceUsages; }
        }

        public IQueryable<TemporaryCode> TemporaryCodes
        {
            get { return _context.TemporaryCodes; }
        }

        public IQueryable<DataLog> DataLogs
        {
            get { return _context.DataLogs.Where(r => r.InstanceId == InstanceId); }
        }

        public IQueryable<UserRole> UserRoles
        {
            get { return _context.UserRoles.Where(r => r.InstanceId == InstanceId); }
        }

        public IQueryable<ComponentRole> ComponentRoles
        {
            get { return _context.ComponentRoles.Where(r => r.InstanceId == InstanceId); }
        }

        public IQueryable<Role> Roles
        {
            get { return _context.Roles.Where(r => r.InstanceId == InstanceId); }
        }

        public IQueryable<Component> Components
        {
            get { return _context.Components; }
        }

        #endregion

        #region Other Methods

        public void Add<T>(T record) where T: class, IMapping
        {
            _context.GetDbSet<T>().Add(record);
        }

        public void Remove<T>(T record) where T : class, IMapping
        {
            _context.GetDbSet<T>().Remove(record);
        }

        public T GetById<T>(int id) where T : class, IMapping
        {
            var param = Expression.Parameter(typeof(T), "e");
            var predicate =
                Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Property(param, typeof(T).Name + "Id"),
                                                                  Expression.Constant(id)), param);
            var table = GetFilteredTable<T>();
            return table.SingleOrDefault(predicate);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public IQueryable<T> GetFilteredTable<T>() where T : class, IMapping
        {
            object result;
            if (!FilteredTableDict.TryGetValue(typeof(T), out result))
                throw new Exception("There is no IQueryable<" + typeof(T).Name + "> property");
            return (IQueryable<T>)result;
        }

        #endregion
    }
}
