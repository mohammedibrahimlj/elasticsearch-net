﻿using System;
using FluentAssertions;
using Nest;
using Tests.Framework.Integration;
using Tests.Framework.MockData;
using Tests.Framework;

namespace Tests.Aggregations.Metric.GeoCentroid
{
	/**
	 * A metric aggregation that computes the weighted centroid from all coordinate values
	 * for a Geo-point datatype field.
	 *
	 * Be sure to read the Elasticsearch documentation on {ref_current}/search-aggregations-metrics-geocentroid-aggregation.html[Geo Centroid Aggregation]
	 */
	[SkipVersion("<2.1.0", "")]
	public class GeoCentroidAggregationUsageTests : AggregationUsageTestBase
	{
		public GeoCentroidAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override object ExpectJson => new
		{
			aggs = new
			{
				centroid = new
				{
					geo_centroid = new
					{
						field = "location"
					}
				}
			}
		};

		protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
			.Aggregations(a => a
				.GeoCentroid("centroid", gb => gb
					.Field(p => p.Location)
				)
			);

		protected override SearchRequest<Project> Initializer =>
			new SearchRequest<Project>
			{
				Aggregations = new GeoCentroidAggregation("centroid", Infer.Field<Project>(p => p.Location))
			};

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.IsValid.Should().BeTrue();
			var centroid = response.Aggs.GeoCentroid("centroid");
			centroid.Should().NotBeNull();
			centroid.Location.Should().NotBeNull();

			centroid.Location.Latitude.Should().NotBe(0);
			centroid.Location.Longitude.Should().NotBe(0);
		}
	}

	/**
	 *[float]
	 *[[geo-centroid-sub-aggregation]]
	 *== Geo Centroid Sub Aggregation
	 *
	 * The `geo_centroid` aggregation is more interesting when combined as a sub-aggregation to other bucket aggregations
	 */
	[SkipVersion("<2.1.0", "")]
	public class NestedGeoCentroidAggregationUsageTests : AggregationUsageTestBase
	{
		public NestedGeoCentroidAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override object ExpectJson => new
		{
			aggs = new
			{
				projects = new
				{
					terms = new
					{
						field = "name"
					},
					aggs = new
					{
						centroid = new
						{
							geo_centroid = new
							{
								field = "location"
							}
						}
					}
				}
			}
		};

		protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
			.Aggregations(a => a
				.Terms("projects", t => t
					.Field(p => p.Name)
					.Aggregations(sa => sa
						.GeoCentroid("centroid", gb => gb
							.Field(p => p.Location)
						)
					)
				)
			);

		protected override SearchRequest<Project> Initializer =>
			new SearchRequest<Project>
			{
				Aggregations = new TermsAggregation("projects")
				{
					Field = Infer.Field<Project>(p => p.Name),
					Aggregations = new GeoCentroidAggregation("centroid", Infer.Field<Project>(p => p.Location))
				}
			};

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.IsValid.Should().BeTrue();

			var projects = response.Aggs.Terms("projects");

			foreach (var bucket in projects.Buckets)
			{
				var centroid = bucket.GeoCentroid("centroid");
				centroid.Should().NotBeNull();
				centroid.Location.Should().NotBeNull();

				centroid.Location.Latitude.Should().NotBe(0);
				centroid.Location.Longitude.Should().NotBe(0);
			}
		}
	}
}
