using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Example.UnitTesting.Utilities
{
    public class ModelMocks
    {
        public const string Document = "C66BA18E-EAF3-4CFF-8A22-41B16D66A972";
        public static readonly Guid DocumentGuid = new Guid(Document);
        public static string[] TestPropertyAliases = {"TestProperty1", "TestProperty2"};
        public static string TestPropertyEditorAlias = "testEditor";

        [PropertyEditor("simple", "Simple", "STRING", "simple")]
        public class SimplePropertyEditor : PropertyEditor
        {

        }

        public static IContent SimpleMockedContent(int id = 123, int parentId = 456)
        {
            var c = Mock.Of<IContent>(
                content => content.Id == id
                           && content.Published
                           && content.CreateDate == DateTime.Now.AddDays(-2)
                           && content.CreatorId == 0
                           && content.HasIdentity
                           && content.ContentType == Mock.Of<IContentType>(ct => ct.Id == 99 && ct.Alias == "testType")
                           && content.ContentTypeId == 10
                           && content.Level == 1
                           && content.Name == "Home"
                           && content.Path == $"-1,{parentId},{id}"
                           && content.ParentId == parentId
                           && content.SortOrder == 1
                           && content.Template == Mock.Of<ITemplate>(te => te.Id == 9 && te.Alias == "home")
                           && content.UpdateDate == DateTime.Now.AddDays(-1)
                           && content.WriterId == 1
                           && content.PropertyTypes == GetIPublishedContentPropertyTypesMock()
                           && content.Properties == GetIPublishedContentPropertiesMock());

            var mock = Mock.Get(c);
            mock.Setup(content => content.HasProperty(It.IsAny<string>()))
                .Returns((string alias) => alias == TestPropertyAliases[0] || alias == TestPropertyAliases[1]);

            return mock.Object;
        }

        public static IMedia SimpleMockedMedia(int id = 123, int parentId = 456)
        {
            var c = Mock.Of<IMedia>(
                content => content.Id == id
                           && content.CreateDate == DateTime.Now.AddDays(-2)
                           && content.CreatorId == 0
                           && content.HasIdentity
                           && content.ContentType == Mock.Of<IMediaType>(ct => ct.Id == 99 && ct.Alias == "testType")
                           && content.ContentTypeId == 10
                           && content.Level == 1
                           && content.Name == "Home"
                           && content.Path == "-1,123"
                           && content.ParentId == parentId
                           && content.SortOrder == 1
                           && content.UpdateDate == DateTime.Now.AddDays(-1)
                           && content.PropertyTypes == GetIPublishedContentPropertyTypesMock()
                           && content.Properties == GetIPublishedContentPropertiesMock());

            var mock = Mock.Get(c);
            mock.Setup(content => content.HasProperty(It.IsAny<string>()))
                .Returns((string alias) => alias == TestPropertyAliases[0] || alias == TestPropertyAliases[1]);

            return mock.Object;
        }

        public static IMember SimpleMockedMember(int id = 123, int parentId = 456)
        {
            var c = Mock.Of<IMember>(
                content => content.Id == id
                           && content.CreateDate == DateTime.Now.AddDays(-2)
                           && content.CreatorId == 0
                           && content.HasIdentity
                           && content.ContentType == Mock.Of<IMemberType>(ct => ct.Id == 99 && ct.Alias == "testType")
                           && content.ContentTypeId == 10
                           && content.Level == 1
                           && content.Name == "John Johnson"
                           && content.Path == "-1,123"
                           && content.ParentId == parentId
                           && content.SortOrder == 1
                           && content.UpdateDate == DateTime.Now.AddDays(-1)
                           && content.PropertyTypes == GetIPublishedContentPropertyTypesMock()
                           && content.Properties == GetIPublishedContentPropertiesMock());

            var mock = Mock.Get(c);
            mock.Setup(content => content.HasProperty(It.IsAny<string>()))
                .Returns((string alias) => alias == TestPropertyAliases[0] || alias == TestPropertyAliases[1]);

            return mock.Object;
        }

        public static IContentType SimpleMockedContentType()
        {
            return Mock.Of<IContentType>();
        }

        public static IMediaType SimpleMockedMediaType()
        {
            return Mock.Of<IMediaType>();
        }

        public static IMemberType SimpleMockedMemberType()
        {
            return Mock.Of<IMemberType>();
        }

        public static IRelationType SimpleMockedRelationType()
        {
            return Mock.Of<IRelationType>(
                type => type.ChildObjectType == DocumentGuid &&
                        type.ParentObjectType == DocumentGuid &&
                        type.Alias == "testType");
        }

        public static IRelation SimpleMockedRelation(int id, int child, int parent, IRelationType relType)
        {
            return Mock.Of<IRelation>(content =>
                content.ChildId == child &&
                content.ParentId == parent &&
                content.Id == id &&
                content.RelationType == relType &&
                content.CreateDate == DateTime.Now);
        }

        public static IPublishedContent SimpleMockedPublishedContent(int id = 123, string documentTypeAlias = null, int templateId = 9, int? parentId = null, int? childId = null)
        {
            return Mock.Of<IPublishedContent>(
                content => content.Id == id
                           && content.IsDraft == false
                           && content.CreateDate == DateTime.Now.AddDays(-2)
                           && content.CreatorId == 0
                           && content.CreatorName == "admin"
                           && content.DocumentTypeAlias == documentTypeAlias
                           && content.DocumentTypeId == 10
                           && content.ItemType == PublishedItemType.Content
                           && content.Level == 1
                           && content.Name == "Home"
                           && content.Path == "-1,123"
                           && content.SortOrder == 1
                           && content.TemplateId == templateId
                           && content.UpdateDate == DateTime.Now.AddDays(-1)
                           && content.Url == "/home"
                           && content.UrlName == "home"
                           && content.WriterId == 1
                           && content.WriterName == "Editor"
                           && content.Properties == new List<IPublishedProperty>(new[]
                           {
                               Mock.Of<IPublishedProperty>(property => property.HasValue
                                                                       && property.PropertyTypeAlias == TestPropertyAliases[0]
                                                                       && (string)property.DataValue == "raw value"
                                                                       && (string)property.Value == "Property Value"),
                               Mock.Of<IPublishedProperty>(property => property.HasValue
                                                                       && property.PropertyTypeAlias == TestPropertyAliases[1]
                                                                       && (string)property.DataValue == "raw value"
                                                                       && (string)property.Value == "Property Value")
                           })
                           && content.Parent == (parentId.HasValue ? SimpleMockedPublishedContent(parentId.Value, null, 9, null, null) : null)
                           && content.Children == (childId.HasValue ? new[] { SimpleMockedPublishedContent(childId.Value, null, 9, null, null) } : Enumerable.Empty<IPublishedContent>()));
        }

        public static Mock<IPublishedContent> MockOfPublishContentWithProperty<T>(string propertyAlias, T propertyValue, bool populateTheOtherPropertiesOfSameType)
        {
            var propertyMock = GetPropertyWithValue(propertyValue);
            var content = new Mock<IPublishedContent>();
            content.Setup(y => y.GetProperty(propertyAlias, false)).Returns(propertyMock.Object);

            if (populateTheOtherPropertiesOfSameType)
            {
                Type propertyValueType = typeof(T);

                //fix to prevent String other calls throwing null ref exception ToLower() and ToUpper()
                if (propertyValueType == typeof(string))
                {
                    SetAllOtherProperties(propertyAlias, content, "some non empty string");
                }

                if (propertyValueType == typeof(int))
                {
                    SetAllOtherProperties(propertyAlias, content, 12345);
                }

                //Add more cases here if needed
            }

            return content;
        }

        public static void SetAllOtherProperties<T>(string propertyAlias, Mock<IPublishedContent> content, T value)
        {
            var someNonEmptyProperty = GetPropertyWithValue(value);

            content.Setup(y => y.GetProperty(It.Is<string>(s => s != propertyAlias), false))
                .Returns(someNonEmptyProperty.Object);
        }

        public static Mock<IPublishedProperty> GetPropertyWithValue<T>(T value)
        {
            var someNonEmptyProperty = new Mock<IPublishedProperty>();
            someNonEmptyProperty.SetupGet(p => p.HasValue).Returns(true);
            someNonEmptyProperty.Setup(p => p.Value).Returns(value);
            return someNonEmptyProperty;
        }

        private static List<PropertyType> GetIPublishedContentPropertyTypesMock()
        {
            return new List<PropertyType>(new[]
            {
                new PropertyType(TestPropertyEditorAlias, DataTypeDatabaseType.Nvarchar, TestPropertyAliases[0])
                {
                    Name = "Test Property1",
                    Mandatory = true,
                    ValidationRegExp = ""
                },
                new PropertyType(TestPropertyEditorAlias, DataTypeDatabaseType.Nvarchar, TestPropertyAliases[1])
                {
                    Name = "Test Property2",
                    Mandatory = false,
                    ValidationRegExp = ""
                }
            });
        }

        private static PropertyCollection GetIPublishedContentPropertiesMock()
        {
            return new PropertyCollection(new[]
            {
                new Property(3, Guid.NewGuid(),
                    new PropertyType(TestPropertyEditorAlias, DataTypeDatabaseType.Nvarchar, TestPropertyAliases[0]), "property value1"),
                new Property(3, Guid.NewGuid(),
                    new PropertyType(TestPropertyEditorAlias, DataTypeDatabaseType.Nvarchar, TestPropertyAliases[1]), "property value2")
            });
        }
    }
}
