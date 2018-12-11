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
                var release = new NewsRelease
                {
                    Id = Guid.NewGuid(),
                    Key = "2018PREM1234-123456",
                    Year = 2018,
                    Timestamp = DateTime.Now,
                    ReleaseDateTime = DateTime.Now,
                    PublishDateTime = DateTime.Now,
                    IsPublished = true,
                    IsActive = true,
                    IsCommitted = true,
                    Keywords = "lorem, ipsum, dolor",
                };
                release.NewsReleaseLog = new List<NewsReleaseLog>
                {
                    new NewsReleaseLog
                    {
                        Id = 1,
                        Description = "Created by Jane Doe",
                        DateTime = DateTime.Now,
                        Release = release
                    },
                    new NewsReleaseLog {
                        Id = 2,
                        Description = "Edited by John Doe",
                        DateTime = DateTime.Now,
                        Release = release
                    }
                };
                return release;
            }
        }
        public static Activity CreateDbActivity(string title, string details, int id)
        {
            return new Activity
            {
                Title = title,
                Details = details,
                IsActive = true,
                IsConfirmed = true,
                Id = id,
                StartDateTime = DateTime.Today.AddDays(3),
                ActivityCategories = new List<ActivityCategories>
                {
                    new ActivityCategories
                    {
                         ActivityId = id,
                         Category = new Category
                         {
                            Name = "Release Only (No Event)"
                         }
                    }
                }
            };
        }

        public static Message CreateDbMessage(string title, string description,
            int sortOrder, bool isPublished = false, bool isHighlighted = false)
        {
            return new Message
            {
                Id = Guid.Empty,
                Title = title,
                Description = description,
                SortOrder = sortOrder,
                IsPublished = isPublished,
                IsHighlighted = isHighlighted,
                Timestamp = DateTime.Now
            };
        }

        public static SocialMediaPost CreateDbSocialMediaPost(string url, int sortOrder = 0)
        {
            return new SocialMediaPost
            {
                Id = Guid.Empty,
                Url = url,
                SortOrder = sortOrder,
                Timestamp = DateTime.Now,
                IsActive = true
            };
        }
    }
}
