using System;
using System.Collections.Generic;
using System.Linq;
using CommonClasses;
using CommonClasses.DbClasses;
using CommonClasses.DbRepositoryInterface;
using CommonClasses.Helpers;
using CommonClasses.MethodArguments;
using CommonClasses.MethodResults;
using CommonClasses.Models;
using Interfaces.Enums;

namespace BusinessLayer.Managers
{
    public class CompanyManager: CommonManager
    {
        #region Properties

        public int CompanyIdForTransactionTypes { get; set; }
        private IList<ExpressionDb> _expressions;
        public IList<ExpressionDb> Expressions
        {
            get { return _expressions ?? (_expressions = Db.GetCompanyExpressions(CompanyIdForTransactionTypes)); }
        }

        private IList<ValidationDb> _validations;
        public IList<ValidationDb> Validations
        {
            get { return _validations ?? (_validations = Db.GetCompanyValidations(CompanyIdForTransactionTypes)); }
        }

        #endregion
        #region Constructors

        public CompanyManager() {}

        public CompanyManager(IDbRepository repository): base(repository) {}

        #endregion

        #region Create Save Company

        public MethodResult<int> CreateCompany(NewCompanyArg arg)
        {
            var dbTran = Db.BeginTransaction();
            try
            {
                var companyDb = new CompanyDb
                                    {
                                        CompanyName = arg.CompanyName,
                                        NationalCurrencyId = AppConfiguration.NationalCurrencyId
                                    };
                var validateError = ValidateCompany(Db.UserId, companyDb);
                if(!string.IsNullOrEmpty(validateError))
                    return new MethodResult<int>{ ErrorMessage = validateError};

                Db.SaveCompany(companyDb);
                Db.SetCompanyId(companyDb.CompanyId);

                var adminRoleId = InsertSystemRoles();
                InsertUserCompanies(Db.UserId, adminRoleId);
                InsertTransactionTypes(arg.CompanyIdForTransactionTypes, Db.CompanyId);
                InsertDuties(arg.CompanyIdForDuties, Db.CompanyId);
                InsertCurrencyClass(companyDb);

                dbTran.Commit();
                return new MethodResult<int>(companyDb.CompanyId);
            }
            catch
            {
                dbTran.Rollback();
                throw;
            }
        }

        public string ValidateCompany(int userId, CompanyDb company)
        {
            if (string.IsNullOrEmpty(company.CompanyName))
                return Messages.EmptyCompanyName;
            if (IsExistCompanyName(userId, company.CompanyName))
                return Messages.ExistsCompanyName;
            return null;
        }

        public bool IsExistCompanyName(int userId, string companyName)
        {
            return Db.IsExistCompanyName(userId, companyName);
        }

        private int InsertSystemRoles()
        {
            var adminRoleId = InsertAdminRoleAndAccess();
            InsertGuestRoleAndAccess();
            return adminRoleId;
        }
        
        private int InsertAdminRoleAndAccess()
        {
            var adminRole = new RoleDb
                                {
                                    CompanyId = Db.CompanyId,
                                    Type = (int) SystemRoles.Administrator,
                                    Name = SystemRoles.Administrator.GetDescription()
                                };
            Db.SaveRole(adminRole);
            foreach (var component in Enum.GetValues(typeof (AccessComponent)))
            {
                if((int)component == (int)AccessComponent.None)
                    continue;
                var componentsToRole = new ComponentsToRoleDb
                                           {
                                               CompanyId = Db.CompanyId,
                                               AccessLevel = (int) AccessLevel.ReadWrite,
                                               ComponentId = (int) component,
                                               RoleId = adminRole.RoleId
                                           };
                Db.SaveComponentsToRole(componentsToRole);
            }
            return adminRole.RoleId;
        }
        
        private void InsertGuestRoleAndAccess()
        {
            var guestRole = new RoleDb
            {
                CompanyId = Db.CompanyId,
                Type = (int)SystemRoles.Guest,
                Name = SystemRoles.Guest.GetDescription()
            };
            Db.SaveRole(guestRole);
            foreach (var componentId in Constants.ComponentsForGuest)
            {
                var componentsToRole = new ComponentsToRoleDb
                {
                    CompanyId = Db.CompanyId,
                    AccessLevel = (int)AccessLevel.Read,
                    ComponentId = componentId,
                    RoleId = guestRole.RoleId
                };
                Db.SaveComponentsToRole(componentsToRole);
            }
        }

        private int InsertUserCompanies(int userId, int roleId)
        {
            var userCompany = new UserCompanyDb{LinkedCompanyId = Db.CompanyId, UserId = userId};
            Db.SaveUserCompany(userCompany);
            var userRole = new UsersToRoleDb {CompanyId = Db.CompanyId, RoleId = roleId, UserId = userId};
            Db.SaveUsersToRole(userRole);
            return userCompany.UserCompanyId;
        }

        private int GetSystemRole(SystemRoles roleType)
        {
            if (roleType == SystemRoles.None)
                throw new Exception();
            var roleId = Db.GetSystemRoleId(roleType);
            if(!roleId.HasValue)
                throw new Exception();
            return roleId.Value;
        }

        private void InsertTransactionTypes(int companyIdForTransactionTypes, int newCompanyId)
        {
            CompanyIdForTransactionTypes = companyIdForTransactionTypes;
            foreach (var transactionType in Db.GetCompanyTransactionTypes(companyIdForTransactionTypes))
            {
                var oldTransactionTypeId = transactionType.TransactionTypeId;
                transactionType.ExpressionId = InsertExpression(newCompanyId, transactionType.ExpressionId);
                transactionType.CompanyId = newCompanyId;
                transactionType.TransactionTypeId = 0;
                Db.SaveTransactionType(transactionType);

                InsertTransactionTypeValidations(newCompanyId, transactionType.TransactionTypeId, oldTransactionTypeId);
            }

            //insert additional functions defined in expressions and used as inner expressions
            foreach (var expression in Expressions.Where(e=>e.CompanyId != newCompanyId))
            {
                InsertExpression(newCompanyId, expression.ExpressionId);
            }
        }

        private int? InsertExpression(int newCompanyId, int? oldExpressionId)
        {
            var expression = Expressions.FirstOrDefault(e => e.ExpressionId == oldExpressionId);
            if (expression != null)
            {
                expression.CompanyId = newCompanyId;
                expression.ExpressionId = 0;
                Db.SaveExpression(expression);
                return expression.ExpressionId;
            }
            return null;
        }

        private void InsertTransactionTypeValidations(int newCompanyId, int newTransactionTypeId, int oldTransactionTypeId)
        {
            foreach (var validation in Validations.Where(v => v.TransactionTypeId == oldTransactionTypeId))
            {
                validation.ExpressionId = InsertExpression(newCompanyId, validation.ExpressionId).Value;
                validation.TransactionTypeId = newTransactionTypeId;
                validation.ValidationId = 0;
                validation.CompanyId = newCompanyId;
                Db.SaveValidation(validation);
            }
        }

        private void InsertDuties(int companyIdForDuties, int newCompanyId)
        {
            foreach (var duty in Db.GetCompanyDuties(companyIdForDuties))
            {
                duty.CompanyId = newCompanyId;
                duty.DutyId = 0;
                Db.SaveDuty(duty);
            }
        }

        private void InsertCurrencyClass(CompanyDb company)
        {
            var currencyShortName = Db.GetCurrencyById(company.NationalCurrencyId).CurrencyShortName;
            var nationalCurrencyClass = new CurrencyClassDb
                                    {
                                        CompanyId = company.CompanyId,
                                        CurrencyId = AppConfiguration.NationalCurrencyId,
                                        Description = "Национальная валюта",
                                        Code = currencyShortName,
                                        IsActive = true,
                                        IsCashless = false
                                    };
            company.DefaultCurrencyClassId = Db.SaveCurrencyClass(nationalCurrencyClass);
            Db.SaveCompany(company);
        }

        public MethodResult<Company> GetCompany(int companyId)
        {
            if(companyId == 0) return new MethodResult<Company> { ResultType = ResultTypeEnum.Error };
            Company company = Db.GetCompanyById(companyId);

            company.CurrencyIsUsed = Db.CurrencyHasTransactions(company.NationalCurrencyId);
            company.CurrencyHasRates = Db.CurrencyHasRates(company.NationalCurrencyId);

            company.Currencies = Db.GetAllCurrencies();
            company.CurrencyClasses = Db.GetAllCurrencyClasses();
            return new MethodResult<Company>(company);
        }

        public BaseResult SaveCompany(Company company)
        {
            if (company == null) return new BaseResult {ResultType = ResultTypeEnum.Error};

            if (company.NationalCurrencyId != CurrentCompany.NationalCurrencyId)
            {
                var manager = new CurrencyClassManager(Db);
                var result = manager.UpdateNationalCurrency(company.NationalCurrencyId);
                if (result.IsError()) return result;
            }

            Db.SaveCompany(company);
            return new BaseResult();
        }

        #endregion
        
        #region Other Methods
        public LoginToCompanyResult CheckFinanceKey(string salt, string financeKey)
        {
            var secretKey = Db.GetCompanySecretKey();
            var isNeeded = secretKey != null;
            var isEntered = !string.IsNullOrEmpty(financeKey);
            bool isCorrect;
            if (isNeeded && isEntered)
            {
                var financeKeyHash = CryptHelper.GetSha512Base64Hash(salt + CryptHelper.GetSha512Base64Hash(Db.CompanyId + financeKey));
                isCorrect = CryptHelper.IsHashEqualsWithStored(salt, secretKey.KeyHash, financeKeyHash);
            }
            else
            {
                isCorrect = !isNeeded && !isEntered;
            }
            return new LoginToCompanyResult { FinanceKeyIsCorrect = isCorrect, FinanceKeyIsEntered = isEntered, FinanceKeyIsNeeded = isNeeded };
        }

        public LoginToCompanyResult VerifyAccessToCompany(CompanyArg arg)
        {
            if (!Db.CheckIfUserLinkedToCompany(Db.UserId, Db.CompanyId))
            {
                return new LoginToCompanyResult { ErrorMessage = Messages.UserCompanyDoesntMatch };
            }

            var result = CheckFinanceKey(arg.Salt, arg.FinanceKey);
            result.CompanyIsLinked = true;
            if (result.FinanceKeyIsEntered &&
                (!result.FinanceKeyIsNeeded || !result.FinanceKeyIsCorrect))
            {
                result.ErrorMessage = Messages.WrongFinanceKey;
            }
            return result;
        }

        public LoginToCompanyResult TryLoginToCompany(CompanyArg arg)
        {
            var result = VerifyAccessToCompany(arg);
            if (result.IsError())
                return result;
            result.CompanyName = CurrentCompany.CompanyName;
            var logResult = LogUsageToDb(Db.UserId, Db.CompanyId);
            if (logResult.IsError())
            {
                result.ErrorMessage = logResult.ErrorMessage;
            }
            return result;
        }

        private BaseResult LogUsageToDb(int userId, int companyId)
        {
            var usageLog = new UserCompanyUsageDb { Date = DateTime.Now, UsedCompanyId = companyId, UserId = userId };
            Db.SaveUserCompanyUsage(usageLog);
            return new BaseResult { ResultType = ResultTypeEnum.Success };
        }

        #endregion

        #region Manage CompanyUser
        public InsertValueResult SaveUserToCompany(string loginOrEmail)
        {
            var isEmailEntered = loginOrEmail.Contains("@");
            var user = isEmailEntered ? Db.GetUserByEmail(loginOrEmail) : Db.GetUserByLogin(loginOrEmail);
            if(user == null)
                return new InsertValueResult { ErrorMessage = isEmailEntered ? Messages.UserNotFoundByEmail : Messages.UserNotFoundByLogin };
            if(Db.CheckIfUserLinkedToCompany(user.UserId, Db.CompanyId))
                return new InsertValueResult { ErrorMessage = Messages.UserCompanyAlreadyExist };
            var id = InsertUserCompanies(user.UserId, GetSystemRole(SystemRoles.Guest));
            return new InsertValueResult { Id = id, Value = user.Login };
        }

        public DeleteUserCompanyResult DeleteUserToCompany(DeleteArg arg)
        {
            var userCompany = Db.GetUserCompanyById(arg.Id);
            if(userCompany == null)
                return new DeleteUserCompanyResult { ErrorMessage = Messages.UserCompanyNotFound };
            Db.DeleteUserRoles(userCompany.UserId, string.Empty);
            Db.DeleteUserCompany(arg.Id, arg.Reason);
            return new DeleteUserCompanyResult { CompanyId = userCompany.LinkedCompanyId, UserId = userCompany.UserId};
        }

        #endregion
    }
}