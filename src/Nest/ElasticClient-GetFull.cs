﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Nest.Resolvers;
using Nest.Domain;

namespace Nest
{

	public partial class ElasticClient
	{
		/// <summary>
		/// Gets a document of T by id in the default index and the inferred typename for T
		/// </summary>
		/// <returns>an instance of T</returns>
		public IGetResponse<T> GetFull<T>(int id) where T : class
		{
			return this.GetFull<T>(id.ToString());
		}
		/// <summary>
		/// Gets a document of T by id in the default index and the inferred typename for T
		/// </summary>
		/// <returns>an instance of T</returns>
		public IGetResponse<T> GetFull<T>(string id) where T : class
		{
			var index = this.Infer.IndexName<T>();
			index.ThrowIfNullOrEmpty("Cannot infer default index for current connection.");

			var typeName = this.Infer.TypeName<T>();
			var path = this.PathResolver.CreateIndexTypeIdPath(index, typeName, id);
			return this._GetFull<T>(path);
		}
		/// <summary>
		/// Gets a document of T by id in the specified index and the specified typename
		/// </summary>
		/// <returns>an instance of T</returns>
		public IGetResponse<T> GetFull<T>(string index, string type, string id) where T : class
		{
			var path = this.PathResolver.CreateIndexTypeIdPath(index, type, id);
			return this._GetFull<T>(path);
		}
		/// <summary>
		/// Gets a document of T by id in the specified index and the specified typename
		/// </summary>
		/// <returns>an instance of T</returns>
		public IGetResponse<T> GetFull<T>(string index, string type, int id) where T : class
		{
			var path = this.PathResolver.CreateIndexTypeIdPath(index, type, id.ToString());
			return this._GetFull<T>(path);
		}
		

		public IGetResponse<T> GetFull<T>(Action<GetDescriptor<T>> getSelector) where T : class
		{
			getSelector.ThrowIfNull("getSelector");
			var d = new GetDescriptor<T>();
			getSelector(d);

			d._Id.ThrowIfNullOrEmpty("Id on getselector");

			var p = new PathResolver(this._connectionSettings);
			var path = p.CreateGetPath<T>(d);
			return this._GetFull<T>(path);
		}

		private IGetResponse<T> _GetFull<T>(string path) where T : class
		{
			var response = this.Connection.GetSync(path);
			var getResponse = this.Deserialize<GetResponse<T>>(response);

			if (response.Result != null)
			{
				var f = new FieldSelection<T>();
				var o = JObject.Parse(response.Result);
				var source = o["fields"];
				if (source != null)
				{
					var json = source.ToString();
					f.Document = getResponse.Source;
					f.FieldValues = this.Deserialize<Dictionary<string, object>>(json);

				}
				getResponse.Fields = f;
			}

			return getResponse;
		}

	
		
	}
}
