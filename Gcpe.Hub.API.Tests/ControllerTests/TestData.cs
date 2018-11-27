using System;
using System.Collections.Generic;
using Gcpe.Hub.Data.Entity;

namespace Gcpe.Hub.API.Tests.ControllerTests
{
    public static class TestData
    {
        public static List<NewsRelease> TestNewsReleases
        {
            get
            {
                var releases = new List<NewsRelease>();

                for (var i = 0; i < 10; i++)
                {
                    var releaseId = Guid.NewGuid();

                    var release = new NewsRelease
                    {
                        Id = releaseId,
                        Key = $"2018PREM{i}-{i}00000",
                        Year = 2018,
                        Timestamp = DateTime.Now,
                        ReleaseDateTime = DateTime.Now,
                        PublishDateTime = DateTime.Now,
                        IsPublished = true,
                        IsActive = true,
                        IsCommitted = true,
                        Keywords = "lorem, ipsum, dolor",
                        NewsReleaseLog = new List<NewsReleaseLog>
                    {
                        new NewsReleaseLog
                        {
                            Id = 1,
                            Description = "Created by Jane Doe",
                            DateTime = DateTime.Now,
                            ReleaseId = releaseId
                        },
                        new NewsReleaseLog {
                            Id = 2,
                            Description = "Edited by John Doe",
                            DateTime = DateTime.Now,
                            ReleaseId = releaseId
                        }
                    }
                    };

                    releases.Add(release);
                }

                return releases;
            }
        }

        public static NewsRelease TestNewsRelease
        {
            get
            {
                var releaseId = Guid.NewGuid();

                return new NewsRelease
                {
                    Id = releaseId,
                    Key = "2018PREM1234-123456",
                    Year = 2018,
                    Timestamp = DateTime.Now,
                    ReleaseDateTime = DateTime.Now,
                    PublishDateTime = DateTime.Now,
                    IsPublished = true,
                    IsActive = true,
                    IsCommitted = true,
                    Keywords = "lorem, ipsum, dolor",
                    NewsReleaseLog = new List<NewsReleaseLog>
                    {
                        new NewsReleaseLog
                        {
                            Id = 1,
                            Description = "Created by Jane Doe",
                            DateTime = DateTime.Now,
                            ReleaseId = releaseId
                        },
                        new NewsReleaseLog {
                            Id = 2,
                            Description = "Edited by John Doe",
                            DateTime = DateTime.Now,
                            ReleaseId = releaseId
                        }
                    }
                };
            }
        }

        public static Message TestMessage
        {
            get
            {
                var id = Guid.NewGuid();

                var message = new Message
                {
                    Id = id,
                    Title = "2018MESSAGE-1",
                    Description = $"DESCRIPTION-1",
                    SortOrder = 0,
                    Timestamp = DateTime.Now,
                    IsPublished = true,
                    IsHighlighted = false
                };

                return message;
            }
        }

    }
}
