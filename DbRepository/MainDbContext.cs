using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using CommonClasses.DbClasses;

namespace DbLayer
{
    public class MainDbContext: DbContext
    {
        public MainDbContext(): base("Name=MainDbContext") {}

        public DbSet<User> Users { get; set; }
        public DbSet<Instance> Instances { get; set; }
        public DbSet<UserInstance> UserInstances { get; set; }
        public DbSet<InstanceUsage> InstanceUsages { get; set; }
        public DbSet<TemporaryCode> TemporaryCodes { get; set; }
        public DbSet<DataLog> DataLogs { get; set; }
        public DbSet<Component> Components { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ComponentRole> ComponentRoles { get; set; }

        #region Auxilliary Properties and Methods

        private Dictionary<Type, object> _dbSetDict;
        protected Dictionary<Type, object> DbSetDict
        {
            get
            {
                if (_dbSetDict == null) InitializeDbSetDict();
                return _dbSetDict;
            }
        }

        private void InitializeDbSetDict()
        {
            _dbSetDict = new Dictionary<Type, object>();
            var propertyInfos = GetType().GetProperties();
            foreach (var propertyInfo in propertyInfos)
            {
                if (propertyInfo.PropertyType.FullName != null && propertyInfo.PropertyType.FullName.StartsWith("System.Data.Entity.DbSet"))
                {
                    _dbSetDict.Add(propertyInfo.PropertyType.GenericTypeArguments[0], propertyInfo.GetValue(this));
                }
            }
        }

        public DbSet<T> GetDbSet<T>() where T: class, IMapping
        {
            object result;
            if (!DbSetDict.TryGetValue(typeof(T), out result))
                throw new Exception("There is no DbSet for class " + typeof(T).Name);
            return (DbSet<T>) result;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }

        #endregion
    }
}
