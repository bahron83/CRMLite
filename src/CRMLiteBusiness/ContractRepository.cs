using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Westwind.BusinessObjects;
using Westwind.Utilities;

namespace CRMLiteBusiness
{
    public class ContractRepository : EntityFrameworkRepository<CRMLiteContext, Contract>
    {
        public ContractRepository(CRMLiteContext context) : base(context) { }

        public override async Task<Contract> Load(object contractId)
        {
            Contract contract = null;
            try
            {
                int id = (int)contractId;
                contract = await Context.Contracts
                    .Include(ctx => ctx.Company)
                    .Include(ctx => ctx.ContractItems)
                    .FirstOrDefaultAsync(con => con.ContractID == id);

                if (contract != null)
                    OnAfterLoaded(contract);
            }
            catch (InvalidOperationException)
            {
                // Handles errors where an invalid Id was passed, but SQL is valid                
                SetError("Couldn't load album - invalid album id specified.");
                return null;
            }
            catch (Exception ex)
            {
                // handles Sql errors                                
                SetError(ex);
            }

            return contract;
        }

        public async Task<List<Contract>> GetAllContracts(int page = 0, int pageSize = 25)
        {
            IQueryable<Contract> contracts = Context.Contracts
                .Include(ctx => ctx.Company)
                .Include(ctx => ctx.ContractItems)
                .OrderBy(con => con.Title);

            if (page > 0)
            {
                contracts = contracts
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);
            }

            return await contracts.ToListAsync();
        }

        public async Task<Contract> SaveContract(Contract postedContract)
        {
            int id = postedContract.CompanyID;

            Contract contract;

            if (id < 1)
                contract = Create();
            else
            {
                contract = await Load(id);
                if (contract == null)
                    contract = Create();
            }


            // add or udpate contacts
            foreach (var contractItem in postedContract.ContractItems)
            {
                var item = contract.ContractItems.FirstOrDefault(i => i.ProductID == contractItem.ProductID);
                if (contractItem.ProductID > 0 && item != null)
                    DataUtils.CopyObjectData(contractItem, item);
                else
                {
                    item = new ContractItem();
                    Context.ContractItems.Add(item);
                    DataUtils.CopyObjectData(contractItem, item, "Id,Contracts");
                    contract.ContractItems.Add(item);
                }
            }

            // then find all deleted contacts not in new contacts
            var deletedItems = contract.ContractItems
                .Where(i => i.ProductID > 0 &&
                                !postedContract.ContractItems
                                    .Where(ite => ite.ProductID > 0)
                                    .Select(ite => ite.ProductID)
                                .Contains(i.ProductID))
                .ToList();

            foreach (var dcontact in deletedItems)
                contract.ContractItems.Remove(dcontact);            

            //now lets save it all
            if (!await SaveAsync())
                return null;

            return contract;
        }

        public async Task<bool> DeleteContract(int id)
        {
            using (var tx = Context.Database.BeginTransaction())
            {
                // manually delete tracks
                var items = await Context.ContractItems.Where(i => i.ContractID == id).ToListAsync();
                for (int i = items.Count - 1; i > -1; i--)
                {
                    var item = items[i];
                    items.Remove(item);
                    Context.ContractItems.Remove(item);
                }

                var contract = await Context.Contracts
                    .FirstOrDefaultAsync(c => c.ContractID == id);

                if (contract == null)
                {
                    SetError("Invalid contract id.");
                    return false;
                }

                Context.Contracts.Remove(contract);

                var result = await SaveAsync();
                if (result)
                    tx.Commit();

                return result;
            }
        }

        protected override bool OnValidate(Contract entity)
        {
            if (entity == null)
            {
                ValidationErrors.Add("No item was passed.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.Title))
                ValidationErrors.Add("Please enter a title for this contract.", "Title");
            else if (string.IsNullOrEmpty(entity.Description) || entity.Description.Length < 30)
                ValidationErrors.Add("Please provide a description of at least 30 characters.");
            else if (entity.ContractItems.Count < 1)
                ValidationErrors.Add("Contract must have at least one item associated.");

            return ValidationErrors.Count < 1;
        }
    }
}
