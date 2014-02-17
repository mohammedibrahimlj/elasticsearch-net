using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using NUnit.Framework;
using Nest.Tests.Integration.Yaml;


namespace Nest.Tests.Integration.Yaml.IndicesGetTemplate1
{
	public partial class IndicesGetTemplate1YamlTests
	{	


		[NCrunch.Framework.ExclusivelyUses("ElasticsearchYamlTests")]
		public class Setup1Tests : YamlTestsBase
		{
			[Test]
			public void Setup1Test()
			{	

				//do indices.put_template 
				_body = new {
					template= "test-*",
					settings= new {
						number_of_shards= "1",
						number_of_replicas= "0"
					}
				};
				this.Do(()=> this._client.IndicesPutTemplateForAll("test", _body));

			}
		}

		[NCrunch.Framework.ExclusivelyUses("ElasticsearchYamlTests")]
		public class GetTemplate2Tests : YamlTestsBase
		{
			[Test]
			public void GetTemplate2Test()
			{	

				//do indices.get_template 
				this.Do(()=> this._client.IndicesGetTemplateForAll("test"));

				//match _response.test.template: 
				this.IsMatch(_response.test.template, @"test-*");

				//match _response.test.settings: 
				this.IsMatch(_response.test.settings, new Dictionary<string, object> {
					{ @"index.number_of_shards", @"1" },
					{ @"index.number_of_replicas", @"0" }
				});

			}
		}

		[NCrunch.Framework.ExclusivelyUses("ElasticsearchYamlTests")]
		public class GetAllTemplates3Tests : YamlTestsBase
		{
			[Test]
			public void GetAllTemplates3Test()
			{	

				//do indices.put_template 
				_body = new {
					template= "test2-*",
					settings= new {
						number_of_shards= "1"
					}
				};
				this.Do(()=> this._client.IndicesPutTemplateForAll("test2", _body));

				//do indices.get_template 
				this.Do(()=> this._client.IndicesGetTemplateForAll());

				//match _response.test.template: 
				this.IsMatch(_response.test.template, @"test-*");

				//match _response.test2.template: 
				this.IsMatch(_response.test2.template, @"test2-*");

			}
		}

		[NCrunch.Framework.ExclusivelyUses("ElasticsearchYamlTests")]
		public class GetTemplateWithLocalFlag4Tests : YamlTestsBase
		{
			[Test]
			public void GetTemplateWithLocalFlag4Test()
			{	

				//do indices.get_template 
				this.Do(()=> this._client.IndicesGetTemplateForAll("test", nv=>nv
					.Add("local", @"true")
				));

				//is_true _response.test; 
				this.IsTrue(_response.test);

			}
		}
	}
}

