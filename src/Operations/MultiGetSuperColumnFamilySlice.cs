﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentCassandra.Types;

namespace FluentCassandra.Operations
{
	public class MultiGetSuperColumnFamilySlice<CompareWith, CompareSubcolumnWith> : QueryableColumnFamilyOperation<IFluentSuperColumnFamily<CompareWith, CompareSubcolumnWith>>
		where CompareWith : CassandraType
		where CompareSubcolumnWith : CassandraType
	{
		/*
		 * map<string,list<ColumnOrSuperColumn>> multiget_slice(keyspace, keys, column_parent, predicate, consistency_level)
		 */

		public List<BytesType> Keys { get; private set; }

		public override IEnumerable<IFluentSuperColumnFamily<CompareWith, CompareSubcolumnWith>> Execute()
		{
			CassandraSession _localSession = null;
			if (CassandraSession.Current == null)
				_localSession = new CassandraSession();

			try
			{
				var parent = new CassandraColumnParent {
					ColumnFamily = ColumnFamily.FamilyName
				};

				var output = CassandraSession.Current.GetClient().multiget_slice(
					Keys,
					parent,
					SlicePredicate,
					CassandraSession.Current.ReadConsistency
				);

				foreach (var result in output)
				{
					var key = CassandraType.FromBigEndian<BytesType>(result.Key);

					var superColumns = result.Value.Select(col => {
						var superCol = Helper.ConvertSuperColumnToFluentSuperColumn<CompareWith, CompareSubcolumnWith>(col.Super_column);
						ColumnFamily.Context.Attach(superCol);
						superCol.MutationTracker.Clear();

						return superCol;
					});

					var familyName = ColumnFamily.FamilyName;
					var r = new FluentSuperColumnFamily<CompareWith, CompareSubcolumnWith>(key, familyName, superColumns);
					ColumnFamily.Context.Attach(r);
					r.MutationTracker.Clear();

					yield return r;
				}
			}
			finally
			{
				if (_localSession != null)
					_localSession.Dispose();
			}
		}

		public MultiGetSuperColumnFamilySlice(IEnumerable<BytesType> keys, CassandraSlicePredicate columnSlicePredicate)
		{
			Keys = keys.ToList();
			SlicePredicate = columnSlicePredicate;
		}
	}
}
