using System;
using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.Infrastructure.Persistence.Seed;

public static class ModelBuilderSeedExtensions
{
    public static void SeedPortfolioData(this ModelBuilder modelBuilder)
    {
        SeedRoles(modelBuilder);
        SeedProfile(modelBuilder);
        SeedSkillCategories(modelBuilder);
        SeedSkills(modelBuilder);
        SeedProjects(modelBuilder);
        SeedProjectSkills(modelBuilder);
        SeedExperience(modelBuilder);
        SeedEducation(modelBuilder);
        SeedBlogMetadata(modelBuilder);
        SeedSettings(modelBuilder);
    }

    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = PortfolioSeedIds.Roles.SuperAdmin,
                Name = "SuperAdmin",
                Description = "Toàn quyền quản trị hệ thống Portfolio."
            },
            new Role
            {
                Id = PortfolioSeedIds.Roles.Admin,
                Name = "Admin",
                Description = "Quản lý nội dung Portfolio."
            });
    }

    private static void SeedProfile(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profile>().HasData(
            new Profile
            {
                Id = 1,
                FullName = "Nguyễn Thanh Vũ",
                JobTitle = "Fresher Full-Stack Developer",
                ShortBio = "Full-Stack Developer định hướng C#, ASP.NET Core và ReactJS.",
                AboutMe = "Tôi tập trung xây dựng Web API, thiết kế cơ sở dữ liệu và giao diện web hiện đại. Tôi có kinh nghiệm thực hành với ASP.NET Core, Entity Framework Core, SQL Server, ReactJS và TypeScript.",
                Email = "nthanhvu1411@gmail.com",
                Phone = "+84374797826",
                Address = "TP. Hồ Chí Minh, Việt Nam",
                GithubUrl = "https://github.com/nguyenthanhvu1411",
                IsActive = true
            });
    }

    private static void SeedSkillCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SkillCategory>().HasData(
            new SkillCategory { Id = 1, Name = "Backend", Description = "Ngôn ngữ và framework phía máy chủ.", DisplayOrder = 1, IsActive = true },
            new SkillCategory { Id = 2, Name = "Frontend", Description = "Công nghệ xây dựng giao diện web.", DisplayOrder = 2, IsActive = true },
            new SkillCategory { Id = 3, Name = "Database", Description = "Hệ quản trị và thiết kế cơ sở dữ liệu.", DisplayOrder = 3, IsActive = true },
            new SkillCategory { Id = 4, Name = "Tools & DevOps", Description = "Công cụ phát triển, quản lý mã nguồn và triển khai.", DisplayOrder = 4, IsActive = true },
            new SkillCategory { Id = 5, Name = "Architecture & Security", Description = "Kiến trúc phần mềm, xác thực và phân quyền.", DisplayOrder = 5, IsActive = true });
    }

    private static void SeedSkills(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Skill>().HasData(
            new Skill { Id = 1, CategoryId = 1, Name = "C#", Level = SkillLevel.Advanced, DisplayOrder = 1, IsFeatured = true, IsActive = true },
            new Skill { Id = 2, CategoryId = 1, Name = "ASP.NET Core", Level = SkillLevel.Advanced, DisplayOrder = 2, IsFeatured = true, IsActive = true },
            new Skill { Id = 3, CategoryId = 1, Name = "Entity Framework Core", Level = SkillLevel.Advanced, DisplayOrder = 3, IsFeatured = true, IsActive = true },
            new Skill { Id = 4, CategoryId = 1, Name = "RESTful API", Level = SkillLevel.Advanced, DisplayOrder = 4, IsFeatured = true, IsActive = true },
            new Skill { Id = 5, CategoryId = 1, Name = "Java Spring Boot", Level = SkillLevel.Intermediate, DisplayOrder = 5, IsFeatured = false, IsActive = true },
            new Skill { Id = 6, CategoryId = 2, Name = "ReactJS", Level = SkillLevel.Advanced, DisplayOrder = 1, IsFeatured = true, IsActive = true },
            new Skill { Id = 7, CategoryId = 2, Name = "TypeScript", Level = SkillLevel.Advanced, DisplayOrder = 2, IsFeatured = true, IsActive = true },
            new Skill { Id = 8, CategoryId = 2, Name = "JavaScript", Level = SkillLevel.Advanced, DisplayOrder = 3, IsFeatured = true, IsActive = true },
            new Skill { Id = 9, CategoryId = 2, Name = "Tailwind CSS", Level = SkillLevel.Intermediate, DisplayOrder = 4, IsFeatured = false, IsActive = true },
            new Skill { Id = 10, CategoryId = 3, Name = "SQL Server", Level = SkillLevel.Advanced, DisplayOrder = 1, IsFeatured = true, IsActive = true },
            new Skill { Id = 11, CategoryId = 3, Name = "MySQL", Level = SkillLevel.Intermediate, DisplayOrder = 2, IsFeatured = false, IsActive = true },
            new Skill { Id = 12, CategoryId = 4, Name = "Git", Level = SkillLevel.Advanced, DisplayOrder = 1, IsFeatured = true, IsActive = true },
            new Skill { Id = 13, CategoryId = 4, Name = "Docker", Level = SkillLevel.Intermediate, DisplayOrder = 2, IsFeatured = false, IsActive = true },
            new Skill { Id = 14, CategoryId = 5, Name = "Clean Architecture", Level = SkillLevel.Intermediate, DisplayOrder = 1, IsFeatured = true, IsActive = true },
            new Skill { Id = 15, CategoryId = 5, Name = "JWT & RBAC", Level = SkillLevel.Advanced, DisplayOrder = 2, IsFeatured = true, IsActive = true });
    }

    private static void SeedProjects(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>().HasData(
            new Project
            {
                Id = 1,
                ProjectName = "Enterprise Warehouse Management System",
                Slug = "enterprise-warehouse-management-system",
                ShortDescription = "Hệ thống quản lý kho với các luồng nhập, xuất, tồn kho, điều chuyển, kiểm kê và trả hàng.",
                FullDescription = "WMS xây dựng bằng ASP.NET Core, Entity Framework Core, SQL Server, ReactJS và TypeScript; hỗ trợ FIFO/FEFO, lot/serial, JWT, RBAC và audit log.",
                Role = "Full-Stack Developer",
                ProjectType = "Full-stack Web Application",
                StartDate = new DateTime(2026, 5, 1),
                EndDate = new DateTime(2026, 7, 31),
                Status = ProjectStatus.Completed,
                IsFeatured = true,
                IsActive = true
            },
            new Project
            {
                Id = 2,
                ProjectName = "E-Commerce Microservices",
                Slug = "ecommerce-microservices",
                ShortDescription = "Nền tảng thương mại điện tử theo kiến trúc microservices.",
                FullDescription = "Hệ thống gồm các nghiệp vụ giỏ hàng, đơn hàng, thanh toán, voucher và hoàn tiền; sử dụng JWT, API Gateway, Eureka và VNPay.",
                Role = "Backend / Full-Stack Developer",
                ProjectType = "Microservices Web Application",
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 4, 30),
                Status = ProjectStatus.Completed,
                IsFeatured = true,
                IsActive = true
            },
            new Project
            {
                Id = 3,
                ProjectName = "Developer Portfolio",
                Slug = "developer-portfolio",
                ShortDescription = "Website Portfolio có trang public và hệ thống quản trị nội dung.",
                FullDescription = "Portfolio sử dụng ASP.NET Core Web API, SQL Server, ReactJS và TypeScript; hỗ trợ quản lý hồ sơ, kỹ năng, dự án, blog, tin nhắn liên hệ và thống kê lượt xem.",
                Role = "Full-Stack Developer",
                ProjectType = "Full-stack Web Application",
                StartDate = new DateTime(2026, 7, 1),
                Status = ProjectStatus.InProgress,
                IsFeatured = true,
                IsActive = true
            });
    }

    private static void SeedProjectSkills(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectSkill>().HasData(
            new ProjectSkill { ProjectId = 1, SkillId = 1 },
            new ProjectSkill { ProjectId = 1, SkillId = 2 },
            new ProjectSkill { ProjectId = 1, SkillId = 3 },
            new ProjectSkill { ProjectId = 1, SkillId = 6 },
            new ProjectSkill { ProjectId = 1, SkillId = 7 },
            new ProjectSkill { ProjectId = 1, SkillId = 10 },
            new ProjectSkill { ProjectId = 1, SkillId = 14 },
            new ProjectSkill { ProjectId = 1, SkillId = 15 },

            new ProjectSkill { ProjectId = 2, SkillId = 5 },
            new ProjectSkill { ProjectId = 2, SkillId = 6 },
            new ProjectSkill { ProjectId = 2, SkillId = 7 },
            new ProjectSkill { ProjectId = 2, SkillId = 11 },
            new ProjectSkill { ProjectId = 2, SkillId = 13 },
            new ProjectSkill { ProjectId = 2, SkillId = 15 },

            new ProjectSkill { ProjectId = 3, SkillId = 1 },
            new ProjectSkill { ProjectId = 3, SkillId = 2 },
            new ProjectSkill { ProjectId = 3, SkillId = 3 },
            new ProjectSkill { ProjectId = 3, SkillId = 6 },
            new ProjectSkill { ProjectId = 3, SkillId = 7 },
            new ProjectSkill { ProjectId = 3, SkillId = 10 },
            new ProjectSkill { ProjectId = 3, SkillId = 14 },
            new ProjectSkill { ProjectId = 3, SkillId = 15 });
    }

    private static void SeedExperience(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Experience>().HasData(
            new Experience
            {
                Id = 1,
                Position = "Full-Stack Developer",
                Company = "Personal Projects",
                Location = "TP. Hồ Chí Minh",
                StartDate = new DateTime(2024, 1, 1),
                IsCurrent = true,
                Description = "Phân tích nghiệp vụ, thiết kế cơ sở dữ liệu, xây dựng RESTful API và phát triển giao diện web responsive.",
                Technologies = "C#, ASP.NET Core, EF Core, SQL Server, ReactJS, TypeScript",
                DisplayOrder = 1,
                IsActive = true
            },
            new Experience
            {
                Id = 2,
                Position = "IT Intern",
                Company = "Inter K",
                Location = "TP. Hồ Chí Minh",
                StartDate = new DateTime(2026, 5, 1),
                EndDate = new DateTime(2026, 7, 31),
                IsCurrent = false,
                Description = "Tìm hiểu và triển khai quy trình DMS, theo dõi đơn hàng, nhập kho, trả hàng và báo cáo phục vụ nghiệp vụ.",
                Technologies = "DMS, SQL, Business Analysis, Technical Support",
                DisplayOrder = 2,
                IsActive = true
            });
    }

    private static void SeedEducation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Education>().HasData(
            new Education
            {
                Id = 1,
                SchoolName = "Ho Chi Minh City Industry and Trade College",
                Major = "Information Technology",
                Degree = "College",
                StartYear = 2023,
                EndYear = 2026,
                GPA = "7.3/10",
                Description = "Tập trung vào lập trình web, cơ sở dữ liệu, phân tích và thiết kế hệ thống.",
                IsActive = true
            });
    }

    private static void SeedBlogMetadata(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogCategory>().HasData(
            new BlogCategory { Id = 1, Name = "Backend", Slug = "backend", Description = "C#, ASP.NET Core, API và kiến trúc backend.", IsActive = true },
            new BlogCategory { Id = 2, Name = "Frontend", Slug = "frontend", Description = "ReactJS, TypeScript và UI/UX.", IsActive = true },
            new BlogCategory { Id = 3, Name = "Career", Slug = "career", Description = "Kinh nghiệm học tập và phát triển nghề nghiệp.", IsActive = true });

        modelBuilder.Entity<BlogTag>().HasData(
            new BlogTag { Id = 1, Name = "ASP.NET Core", Slug = "aspnet-core" },
            new BlogTag { Id = 2, Name = "Entity Framework Core", Slug = "entity-framework-core" },
            new BlogTag { Id = 3, Name = "ReactJS", Slug = "reactjs" },
            new BlogTag { Id = 4, Name = "JWT", Slug = "jwt" },
            new BlogTag { Id = 5, Name = "SQL Server", Slug = "sql-server" });
    }

    private static void SeedSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Setting>().HasData(
            new Setting
            {
                Id = 1,
                SiteName = "Nguyễn Thanh Vũ Portfolio",
                ThemeColor = "#2563EB",
                SeoTitle = "Nguyễn Thanh Vũ - Full-Stack Developer",
                SeoDescription = "Portfolio cá nhân giới thiệu kỹ năng, dự án và kinh nghiệm phát triển phần mềm.",
                ContactEmail = "nthanhvu1411@gmail.com",
                FooterText = "© 2026 Nguyễn Thanh Vũ. All rights reserved."
            });
    }
}
