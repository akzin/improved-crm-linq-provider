using System;
using Microsoft.Xrm.Sdk.Query;

namespace Akzin.Crm.Linq.Query
{
    internal class LinkEntityOrQueryExpression
    {
        private readonly LinkEntity linkEntity;
        private readonly QueryExpression queryExpression;

        public LinkEntityOrQueryExpression(LinkEntity linkEntity)
        {
            this.linkEntity = linkEntity;
        }

        public LinkEntityOrQueryExpression(QueryExpression queryExpression)
        {
            this.queryExpression = queryExpression;
        }

        public void AddOrder(OrderExpression orderExpression)
        {
            if (linkEntity != null)
            {
                linkEntity.Orders.Add(orderExpression);
            }
            else
            {
                queryExpression.Orders.Add(orderExpression);
            }
        }

        public void AddColumn(string columnName)
        {
            if(columnName == NamingExtensions.SpecialRowVersion || columnName == NamingExtensions.SpecialId)
                return;
            
            if (linkEntity != null)
            {
                linkEntity.Columns.AddColumn(columnName);
            }
            else
            {
                queryExpression.ColumnSet.AddColumn(columnName);
            }
        }

        public void SetAllColumns()
        {
            if (linkEntity != null)
            {
                linkEntity.Columns = new ColumnSet(true);
            }
            else
            {
                queryExpression.ColumnSet = new ColumnSet(true);
            }
        }

        public void AddLinkEntity(LinkEntity subLinkEntity)
        {
            if (linkEntity != null)
            {
                linkEntity.LinkEntities.Add(subLinkEntity);
            }
            else
            {
                queryExpression.LinkEntities.Add(subLinkEntity);
            }
        }

        public void SetJoinOperator(JoinOperator joinOperator)
        {
            if (linkEntity != null)
            {
                linkEntity.JoinOperator = joinOperator;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public string GetEntityName()
        {
            if (linkEntity != null)
            {
                return linkEntity.LinkToEntityName;
            }
            else
            {
                return queryExpression.EntityName;
            }
        }
    }
}