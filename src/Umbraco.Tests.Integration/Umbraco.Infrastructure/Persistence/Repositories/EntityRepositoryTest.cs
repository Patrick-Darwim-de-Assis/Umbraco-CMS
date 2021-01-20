// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Mapper = true, Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class EntityRepositoryTest : UmbracoIntegrationTest
    {
        [Test]
        public void Get_Paged_Mixed_Entities_By_Ids()
        {
            // Create content
            IContentService contentService = GetRequiredService<IContentService>();
            IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();
            var createdContent = new List<IContent>();
            ContentType contentType = ContentTypeBuilder.CreateBasicContentType("blah");
            contentTypeService.Save(contentType);
            for (int i = 0; i < 10; i++)
            {
                Content c1 = ContentBuilder.CreateBasicContent(contentType);
                contentService.Save(c1);
                createdContent.Add(c1);
            }

            // Create media
            IMediaService mediaService = GetRequiredService<IMediaService>();
            IMediaTypeService mediaTypeService = GetRequiredService<IMediaTypeService>();
            var createdMedia = new List<IMedia>();
            MediaType imageType = MediaTypeBuilder.CreateImageMediaType("myImage");
            mediaTypeService.Save(imageType);
            for (int i = 0; i < 10; i++)
            {
                Media c1 = MediaBuilder.CreateMediaImage(imageType, -1);
                mediaService.Save(c1);
                createdMedia.Add(c1);
            }

            // Create members
            IMemberService memberService = GetRequiredService<IMemberService>();
            IMemberTypeService memberTypeService = GetRequiredService<IMemberTypeService>();
            MemberType memberType = MemberTypeBuilder.CreateSimpleMemberType("simple");
            memberTypeService.Save(memberType);
            var createdMembers = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10).ToList();
            memberService.Save(createdMembers);

            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                EntityRepository repo = CreateRepository((IScopeAccessor)provider);

                IEnumerable<int> ids = createdContent.Select(x => x.Id).Concat(createdMedia.Select(x => x.Id)).Concat(createdMembers.Select(x => x.Id));

                System.Guid[] objectTypes = new[] { Constants.ObjectTypes.Document, Constants.ObjectTypes.Media, Constants.ObjectTypes.Member };

                IQuery<IUmbracoEntity> query = provider.SqlContext.Query<IUmbracoEntity>()
                    .WhereIn(e => e.Id, ids);

                var entities = repo.GetPagedResultsByQuery(query, objectTypes, 0, 20, out long totalRecords, null, null).ToList();

                Assert.AreEqual(20, entities.Count);
                Assert.AreEqual(30, totalRecords);

                // add the next page
                entities.AddRange(repo.GetPagedResultsByQuery(query, objectTypes, 1, 20, out totalRecords, null, null));

                Assert.AreEqual(30, entities.Count);
                Assert.AreEqual(30, totalRecords);

                var contentEntities = entities.OfType<IDocumentEntitySlim>().ToList();
                var mediaEntities = entities.OfType<IMediaEntitySlim>().ToList();
                var memberEntities = entities.OfType<IMemberEntitySlim>().ToList();

                Assert.AreEqual(10, contentEntities.Count);
                Assert.AreEqual(10, mediaEntities.Count);
                Assert.AreEqual(10, memberEntities.Count);
            }
        }

        private EntityRepository CreateRepository(IScopeAccessor scopeAccessor) => new EntityRepository(scopeAccessor, AppCaches.Disabled);
    }
}