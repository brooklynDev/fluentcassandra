﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentCassandra.Types;

namespace FluentCassandra.Operations
{
	public class CassandraIndexClause<CompareWith> : CassandraIndexClause
		where CompareWith : CassandraType
	{
		public CassandraIndexClause(BytesType startKey, int count, Expression<Func<IFluentRecordHasFluentColumns<CompareWith>, bool>> expression)
			: base(startKey, count, expression) { }
	}

	public abstract class CassandraIndexClause
	{
		protected internal CassandraIndexClause(BytesType startKey, int count, Expression expression)
		{
			StartKey = startKey;
			Count = count;
			Expression = expression;
		}

		private List<Apache.Cassandra.IndexExpression> _compiledExpressions;

		public BytesType StartKey { get; private set; }
		public int Count { get; private set; }
		public Expression Expression { get; private set; }

		public List<Apache.Cassandra.IndexExpression> CompiledExpressions
		{
			get
			{
				if (_compiledExpressions == null)
					_compiledExpressions = CassandraIndexClauseBuilder.Evaluate(Expression);

				return _compiledExpressions;
			}
		}
	}
}
