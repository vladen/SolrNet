using System.Collections.Generic;
using SolrNet.Commands.Parameters;
using SolrNet.Exceptions;
using SolrNet.Utils;

namespace SolrNet {
	/// <summary>
	/// Executable query
	/// </summary>
	/// <typeparam name="T">Document type</typeparam>
	public class SolrQueryExecuter<T> : ISolrQueryExecuter<T> where T : ISolrDocument, new() {
		/// <summary>
		/// Solr response parser, default is XML response parser
		/// </summary>
		public ISolrQueryResultParser<T> ResultParser { get; set; }

		/// <summary>
		/// Connection to use
		/// </summary>
		public ISolrConnection Connection { get; set; }

		public ISolrQuery Query { get; set; }

		public QueryOptions Options { get; set; }

		public IListRandomizer ListRandomizer { get; set; }

		public IUniqueKeyFinder<T> UniqueKeyFinder { get; set; }

		public int DefaultRows { get; set; }

		public SolrQueryExecuter(ISolrConnection connection, ISolrQuery query) {
			Query = query;
			Connection = connection;
			ListRandomizer = new ListRandomizer();
			ResultParser = new SolrQueryResultParser<T>();
			UniqueKeyFinder = new UniqueKeyFinder<T>();
			DefaultRows = 1000000000;
		}

		public SolrQueryExecuter(ISolrConnection connection, string query) {
			Query = new SolrQuery(query);
			Connection = connection;
			ListRandomizer = new ListRandomizer();
			ResultParser = new SolrQueryResultParser<T>();
			UniqueKeyFinder = new UniqueKeyFinder<T>();
			DefaultRows = 1000000000;
		}

		/// <summary>
		/// Executes the query and returns results
		/// </summary>
		/// <returns>query results</returns>
		public ISolrQueryResults<T> Execute() {
			var param = new Dictionary<string, string>();
			param["q"] = Query.Query;
			if (Options != null) {
				if (Options.Start.HasValue)
					param["start"] = Options.Start.ToString();
				var rows = Options.Rows.HasValue ? Options.Rows.Value : DefaultRows;
				param["rows"] = rows.ToString();
				if (Options.OrderBy != null && Options.OrderBy.Count > 0)
					if (Options.OrderBy == SortOrder.Random) {
						var pk = UniqueKeyFinder.UniqueKeyFieldName;
						var pkProp = UniqueKeyFinder.UniqueKeyProperty;
						if (pk == null)
							throw new NoUniqueKeyException();
						var executer = new SolrQueryExecuter<T>(Connection, Query) {
							ListRandomizer = ListRandomizer,
							ResultParser = ResultParser,
							UniqueKeyFinder = UniqueKeyFinder,
							Options = new QueryOptions {
								Fields = new[] {pk},
							}
						};
						var nr = executer.Execute();
						ListRandomizer.Randomize(nr);
						var idListQuery = new SolrQueryInList(pk, Func.Select(Func.Take(nr, rows), x => pkProp.GetValue(x, null)));
						param["q"] = idListQuery.Query;
					} else {
						param["sort"] = Func.Join(",", Options.OrderBy);
					}
				if (Options.Fields != null && Options.Fields.Count > 0)
					param["fl"] = Func.Join(",", Options.Fields);
			}
			string r = Connection.Get("/select", param);
			var qr = ResultParser.Parse(r);
			return qr;
		}
	}
}