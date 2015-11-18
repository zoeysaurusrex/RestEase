﻿using Moq;
using RestEase;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RestEaseUnitTests.ImplementationBuilderTests
{
    public class QueryParamTests
    {
        public interface ISingleParameterWithQueryParamAttributeNoReturn
        {
            [Get("boo")]
            Task BooAsync([Query("bar")] string foo);
        }

        public interface IQueryParamWithImplicitName
        {
            [Get("foo")]
            Task FooAsync([Query] string foo);
        }

        public interface ITwoQueryParametersWithTheSameName
        {
            [Get("foo")]
            Task FooAsync([Query("bar")] string foo, [Query] string bar);
        }

        public interface INullableQueryParameters
        {
            [Get("foo")]
            Task FooAsync(object foo, int? bar, int? baz, int yay);
        }

        public interface IArrayQueryParam
        {
            [Get("foo")]
            Task FooAsync(IEnumerable<int> intArray);
        }

        public interface ISerializedQueryParam
        {
            [Get("foo")]
            Task FooAsync([Query(QuerySerialializationMethod.Serialized)] object foo);
        }

        private readonly Mock<IRequester> requester = new Mock<IRequester>(MockBehavior.Strict);
        private readonly ImplementationBuilder builder = new ImplementationBuilder();

        [Fact]
        public void SingleParameterWithQueryParamAttributeNoReturnCallsCorrectly()
        {
            var implementation = this.builder.CreateImplementation<ISingleParameterWithQueryParamAttributeNoReturn>(this.requester.Object);

            var expectedResponse = Task.FromResult(false);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(expectedResponse)
                .Verifiable();

            var response = implementation.BooAsync("the value");

            Assert.Equal(expectedResponse, response);
            this.requester.Verify();
            Assert.Equal(CancellationToken.None, requestInfo.CancellationToken);
            Assert.Equal(HttpMethod.Get, requestInfo.Method);
            Assert.Equal(1, requestInfo.QueryParams.Count);

            var queryParam0 = requestInfo.QueryParams[0].SerializeToString().First();
            Assert.Equal("bar", queryParam0.Key);
            Assert.Equal("the value", queryParam0.Value);
            Assert.Equal("boo", requestInfo.Path);
        }

        [Fact]
        public void QueryParamWithImplicitNameCallsCorrectly()
        {
            var implementation = this.builder.CreateImplementation<IQueryParamWithImplicitName>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("the value");

            Assert.Equal(1, requestInfo.QueryParams.Count);

            var queryParam0 = requestInfo.QueryParams[0].SerializeToString().First();
            Assert.Equal("foo", queryParam0.Key);
            Assert.Equal("the value", queryParam0.Value);
        }

        [Fact]
        public void HandlesMultipleQueryParamsWithTheSameName()
        {
            var implementation = this.builder.CreateImplementation<ITwoQueryParametersWithTheSameName>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("foo value", "bar value");

            Assert.Equal(2, requestInfo.QueryParams.Count);

            var queryParam0 = requestInfo.QueryParams[0].SerializeToString().First();
            Assert.Equal("bar", queryParam0.Key);
            Assert.Equal("foo value", queryParam0.Value);

            var queryParam1 = requestInfo.QueryParams[1].SerializeToString().First();
            Assert.Equal("bar", queryParam1.Key);
            Assert.Equal("bar value", queryParam1.Value);
        }

        [Fact]
        public void ExcludesNullQueryParams()
        {
            var implementation = this.builder.CreateImplementation<INullableQueryParameters>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync(null, null, 0, 0);

            Assert.Equal(4, requestInfo.QueryParams.Count);

            Assert.Equal(0, requestInfo.QueryParams[0].SerializeToString().Count());

            Assert.Equal(0, requestInfo.QueryParams[1].SerializeToString().Count());

            var queryParam2 = requestInfo.QueryParams[2].SerializeToString().First();
            Assert.Equal("baz", queryParam2.Key);
            Assert.Equal("0", queryParam2.Value);

            var queryParam3 = requestInfo.QueryParams[3].SerializeToString().First();
            Assert.Equal("yay", queryParam3.Key);
            Assert.Equal("0", queryParam3.Value);
        }

        [Fact]
        public void HandlesQueryParamArays()
        {
            var implementation = this.builder.CreateImplementation<IArrayQueryParam>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync(new int[] { 1, 2, 3 });

            Assert.Equal(1, requestInfo.QueryParams.Count);

            var queryParams = requestInfo.QueryParams[0].SerializeToString().ToArray();

            Assert.Equal("intArray", queryParams[0].Key);
            Assert.Equal("1", queryParams[0].Value);

            Assert.Equal("intArray", queryParams[1].Key);
            Assert.Equal("2", queryParams[1].Value);

            Assert.Equal("intArray", queryParams[2].Key);
            Assert.Equal("3", queryParams[2].Value);
        }

        [Fact]
        public void RecordsToStringSerializationMethod()
        {
            var implementation = this.builder.CreateImplementation<ISingleParameterWithQueryParamAttributeNoReturn>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.BooAsync("yay");

            Assert.Equal(1, requestInfo.QueryParams.Count);
            Assert.Equal(QuerySerialializationMethod.ToString, requestInfo.QueryParams[0].SerializationMethod);
        }

        [Fact]
        public void RecordsSerializedSerializationMethod()
        {
            var implementation = this.builder.CreateImplementation<ISerializedQueryParam>(this.requester.Object);
            IRequestInfo requestInfo = null;

            this.requester.Setup(x => x.RequestVoidAsync(It.IsAny<IRequestInfo>()))
                .Callback((IRequestInfo r) => requestInfo = r)
                .Returns(Task.FromResult(false));

            implementation.FooAsync("boom");

            Assert.Equal(1, requestInfo.QueryParams.Count);
            Assert.Equal(QuerySerialializationMethod.Serialized, requestInfo.QueryParams[0].SerializationMethod);
        }
    }
}
