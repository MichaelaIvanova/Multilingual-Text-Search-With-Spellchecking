using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Profiling;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Example.UnitTesting.Utilities
{
    public class ContextMocker
    {
        public ContextMocker(string[] qKeys = null)
        {
            ILogger loggerMock = Mock.Of<ILogger>();
            IProfiler profilerMock = Mock.Of<IProfiler>();
            IDictionary dictionary = Mock.Of<IDictionary>();// this is related to injecting the dictionary into the mocked UmbracoHelper

            var session = new Mock<HttpSessionStateBase>();
            var contextBaseMock = new Mock<HttpContextBase>();
            contextBaseMock.Setup(ctx => ctx.Session).Returns(session.Object);
            contextBaseMock.SetupGet(ctx => ctx.Items).Returns(dictionary);//// this is related to injecting the dictionary into the mocked UmbracoHelper

            if (qKeys != null)
            {
                var requestMock = new Mock<HttpRequestBase>();
                requestMock.SetupGet(r => r.Url).Returns(new Uri("http://imaginary.com"));
                var mockedQstring = new Mock<NameValueCollection>();

                foreach (var k in qKeys)
                {
                    mockedQstring.Setup(r => r.Get(It.Is<string>(s => s.Contains(k)))).Returns("example_value_" + k);
                }

                requestMock.SetupGet(r => r.QueryString).Returns(mockedQstring.Object);
                contextBaseMock.SetupGet(p => p.Request).Returns(requestMock.Object);
            }

            WebSecurity webSecurityMock = new Mock<WebSecurity>(null, null).Object;
            IUmbracoSettingsSection umbracoSettingsSectionMock = Mock.Of<IUmbracoSettingsSection>();

            HttpContextBaseMock = contextBaseMock.Object;
            ApplicationContextMock = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper(), new ProfilingLogger(loggerMock, profilerMock));
            UmbracoContextMock = UmbracoContext.EnsureContext(contextBaseMock.Object, ApplicationContextMock, webSecurityMock, umbracoSettingsSectionMock, Enumerable.Empty<IUrlProvider>(), true);
        }

        public ApplicationContext ApplicationContextMock { get; }
        public UmbracoContext UmbracoContextMock { get; }
        public HttpContextBase HttpContextBaseMock { get; }

        //TODO: add mockings as params
        public UmbracoHelper GetMockedUmbracoHelper(ICultureDictionary dictionary)
        {
            return new UmbracoHelper(
                UmbracoContextMock,
                Mock.Of<IPublishedContent>(),
                Mock.Of<ITypedPublishedContentQuery>(query => query.TypedContent(It.IsAny<int>()) == Mock.Of<IPublishedContent>(content => content.Id == 7)),
                Mock.Of<IDynamicPublishedContentQuery>(),
                Mock.Of<ITagQuery>(),
                Mock.Of<IDataTypeService>(),
                new UrlProvider(UmbracoContextMock, Enumerable.Empty<IUrlProvider>()),
                dictionary,
                Mock.Of<IUmbracoComponentRenderer>(),
                new MembershipHelper(UmbracoContextMock, Mock.Of<MembershipProvider>(), Mock.Of<RoleProvider>()));
        }

        public ViewContext GetViewContext()
        {
            var controllecContext = new Mock<ControllerContext>();
            controllecContext.SetupGet(x => x.HttpContext).Returns(HttpContextBaseMock);

            return new ViewContext(controllecContext.Object, new Mock<IView>().Object, new ViewDataDictionary(), new TempDataDictionary(), new StreamWriter(new MemoryStream()));
        }
    }
}