using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Portfolio.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogCategories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "BlogTags",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    organization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    issue_date = table.Column<DateTime>(type: "date", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "date", nullable: true),
                    credential_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    credential_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_certificates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ContactMessages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    subject = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Education",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    school_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    major = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    degree = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    start_year = table.Column<int>(type: "int", nullable: true),
                    end_year = table.Column<int>(type: "int", nullable: true),
                    gpa = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    logo_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_education", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Experiences",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    position = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    company_logo_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    start_date = table.Column<DateTime>(type: "date", nullable: false),
                    end_date = table.Column<DateTime>(type: "date", nullable: true),
                    is_current = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    technologies = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_experiences", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    job_title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    short_bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    about_me = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    banner_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    cv_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    github_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    linkedin_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    facebook_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    project_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    short_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    full_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    project_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    thumbnail_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    github_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    demo_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    start_date = table.Column<DateTime>(type: "date", nullable: true),
                    end_date = table.Column<DateTime>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    is_featured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    site_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    logo_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    favicon_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    theme_color = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    seo_title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    seo_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    contact_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    footer_text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "SkillCategories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_skill_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    last_login_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    slug = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    thumbnail_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    published_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    view_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_featured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blogs", x => x.id);
                    table.ForeignKey(
                        name: "FK_Blogs_BlogCategories_CategoryId",
                        column: x => x.category_id,
                        principalTable: "BlogCategories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectImages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    project_id = table.Column<int>(type: "int", nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    caption = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_thumbnail = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_ProjectImages_Projects_ProjectId",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    level = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    icon_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_featured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_skills", x => x.id);
                    table.ForeignKey(
                        name: "FK_Skills_SkillCategories_CategoryId",
                        column: x => x.category_id,
                        principalTable: "SkillCategories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    entity_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    entity_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    old_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UploadedFiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    original_file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    stored_file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    content_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_uploaded_files", x => x.id);
                    table.ForeignKey(
                        name: "FK_UploadedFiles_Users_UploadedBy",
                        column: x => x.uploaded_by,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.role_id,
                        principalTable: "Roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogTagMappings",
                columns: table => new
                {
                    blog_id = table.Column<int>(type: "int", nullable: false),
                    tag_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_tag_mappings", x => new { x.blog_id, x.tag_id });
                    table.ForeignKey(
                        name: "FK_BlogTagMappings_BlogTags_TagId",
                        column: x => x.tag_id,
                        principalTable: "BlogTags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogTagMappings_Blogs_BlogId",
                        column: x => x.blog_id,
                        principalTable: "Blogs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViewStatistics",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    blog_id = table.Column<int>(type: "int", nullable: true),
                    project_id = table.Column<int>(type: "int", nullable: true),
                    page_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    viewed_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_view_statistics", x => x.id);
                    table.ForeignKey(
                        name: "FK_ViewStatistics_Blogs_BlogId",
                        column: x => x.blog_id,
                        principalTable: "Blogs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ViewStatistics_Projects_ProjectId",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSkills",
                columns: table => new
                {
                    project_id = table.Column<int>(type: "int", nullable: false),
                    skill_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_skills", x => new { x.project_id, x.skill_id });
                    table.ForeignKey(
                        name: "FK_ProjectSkills_Projects_ProjectId",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectSkills_Skills_SkillId",
                        column: x => x.skill_id,
                        principalTable: "Skills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BlogCategories",
                columns: new[] { "id", "description", "is_active", "name", "slug" },
                values: new object[,]
                {
                    { 1, "C#, ASP.NET Core, API và kiến trúc backend.", true, "Backend", "backend" },
                    { 2, "ReactJS, TypeScript và UI/UX.", true, "Frontend", "frontend" },
                    { 3, "Kinh nghiệm học tập và phát triển nghề nghiệp.", true, "Career", "career" }
                });

            migrationBuilder.InsertData(
                table: "BlogTags",
                columns: new[] { "id", "name", "slug" },
                values: new object[,]
                {
                    { 1, "ASP.NET Core", "aspnet-core" },
                    { 2, "Entity Framework Core", "entity-framework-core" },
                    { 3, "ReactJS", "reactjs" },
                    { 4, "JWT", "jwt" },
                    { 5, "SQL Server", "sql-server" }
                });

            migrationBuilder.InsertData(
                table: "Education",
                columns: new[] { "id", "degree", "description", "end_year", "gpa", "is_active", "logo_url", "major", "school_name", "start_year" },
                values: new object[] { 1, "College", "Tập trung vào lập trình web, cơ sở dữ liệu, phân tích và thiết kế hệ thống.", 2026, "7.3/10", true, null, "Information Technology", "Ho Chi Minh City Industry and Trade College", 2023 });

            migrationBuilder.InsertData(
                table: "Experiences",
                columns: new[] { "id", "company", "company_logo_url", "description", "display_order", "end_date", "is_active", "is_current", "location", "position", "start_date", "technologies" },
                values: new object[] { 1, "Personal Projects", null, "Phân tích nghiệp vụ, thiết kế cơ sở dữ liệu, xây dựng RESTful API và phát triển giao diện web responsive.", 1, null, true, true, "TP. Hồ Chí Minh", "Full-Stack Developer", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "C#, ASP.NET Core, EF Core, SQL Server, ReactJS, TypeScript" });

            migrationBuilder.InsertData(
                table: "Experiences",
                columns: new[] { "id", "company", "company_logo_url", "description", "display_order", "end_date", "is_active", "location", "position", "start_date", "technologies" },
                values: new object[] { 2, "Inter K", null, "Tìm hiểu và triển khai quy trình DMS, theo dõi đơn hàng, nhập kho, trả hàng và báo cáo phục vụ nghiệp vụ.", 2, new DateTime(2026, 7, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "TP. Hồ Chí Minh", "IT Intern", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "DMS, SQL, Business Analysis, Technical Support" });

            migrationBuilder.InsertData(
                table: "Profiles",
                columns: new[] { "id", "about_me", "address", "avatar_url", "banner_url", "cv_url", "email", "facebook_url", "full_name", "github_url", "is_active", "job_title", "linkedin_url", "phone", "short_bio" },
                values: new object[] { 1, "Tôi tập trung xây dựng Web API, thiết kế cơ sở dữ liệu và giao diện web hiện đại. Tôi có kinh nghiệm thực hành với ASP.NET Core, Entity Framework Core, SQL Server, ReactJS và TypeScript.", "TP. Hồ Chí Minh, Việt Nam", null, null, null, "nthanhvu1411@gmail.com", null, "Nguyễn Thanh Vũ", "https://github.com/nguyenthanhvu1411", true, "Fresher Full-Stack Developer", null, "+84374797826", "Full-Stack Developer định hướng C#, ASP.NET Core và ReactJS." });

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "id", "demo_url", "end_date", "full_description", "github_url", "is_active", "is_featured", "project_name", "project_type", "role", "short_description", "slug", "start_date", "status", "thumbnail_url" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 7, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "WMS xây dựng bằng ASP.NET Core, Entity Framework Core, SQL Server, ReactJS và TypeScript; hỗ trợ FIFO/FEFO, lot/serial, JWT, RBAC và audit log.", null, true, true, "Enterprise Warehouse Management System", "Full-stack Web Application", "Full-Stack Developer", "Hệ thống quản lý kho với các luồng nhập, xuất, tồn kho, điều chuyển, kiểm kê và trả hàng.", "enterprise-warehouse-management-system", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", null },
                    { 2, null, new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hệ thống gồm các nghiệp vụ giỏ hàng, đơn hàng, thanh toán, voucher và hoàn tiền; sử dụng JWT, API Gateway, Eureka và VNPay.", null, true, true, "E-Commerce Microservices", "Microservices Web Application", "Backend / Full-Stack Developer", "Nền tảng thương mại điện tử theo kiến trúc microservices.", "ecommerce-microservices", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", null },
                    { 3, null, null, "Portfolio sử dụng ASP.NET Core Web API, SQL Server, ReactJS và TypeScript; hỗ trợ quản lý hồ sơ, kỹ năng, dự án, blog, tin nhắn liên hệ và thống kê lượt xem.", null, true, true, "Developer Portfolio", "Full-stack Web Application", "Full-Stack Developer", "Website Portfolio có trang public và hệ thống quản trị nội dung.", "developer-portfolio", new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "InProgress", null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "id", "description", "name" },
                values: new object[,]
                {
                    { 1, "Toàn quyền quản trị hệ thống Portfolio.", "SuperAdmin" },
                    { 2, "Quản lý nội dung Portfolio.", "Admin" }
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "id", "contact_email", "favicon_url", "footer_text", "logo_url", "seo_description", "seo_title", "site_name", "theme_color" },
                values: new object[] { 1, "nthanhvu1411@gmail.com", null, "© 2026 Nguyễn Thanh Vũ. All rights reserved.", null, "Portfolio cá nhân giới thiệu kỹ năng, dự án và kinh nghiệm phát triển phần mềm.", "Nguyễn Thanh Vũ - Full-Stack Developer", "Nguyễn Thanh Vũ Portfolio", "#2563EB" });

            migrationBuilder.InsertData(
                table: "SkillCategories",
                columns: new[] { "id", "description", "display_order", "is_active", "name" },
                values: new object[,]
                {
                    { 1, "Ngôn ngữ và framework phía máy chủ.", 1, true, "Backend" },
                    { 2, "Công nghệ xây dựng giao diện web.", 2, true, "Frontend" },
                    { 3, "Hệ quản trị và thiết kế cơ sở dữ liệu.", 3, true, "Database" },
                    { 4, "Công cụ phát triển, quản lý mã nguồn và triển khai.", 4, true, "Tools & DevOps" },
                    { 5, "Kiến trúc phần mềm, xác thực và phân quyền.", 5, true, "Architecture & Security" }
                });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "id", "category_id", "description", "display_order", "icon_url", "is_active", "is_featured", "level", "name" },
                values: new object[,]
                {
                    { 1, 1, null, 1, null, true, true, "Advanced", "C#" },
                    { 2, 1, null, 2, null, true, true, "Advanced", "ASP.NET Core" },
                    { 3, 1, null, 3, null, true, true, "Advanced", "Entity Framework Core" },
                    { 4, 1, null, 4, null, true, true, "Advanced", "RESTful API" }
                });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "id", "category_id", "description", "display_order", "icon_url", "is_active", "level", "name" },
                values: new object[] { 5, 1, null, 5, null, true, "Intermediate", "Java Spring Boot" });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "id", "category_id", "description", "display_order", "icon_url", "is_active", "is_featured", "level", "name" },
                values: new object[,]
                {
                    { 6, 2, null, 1, null, true, true, "Advanced", "ReactJS" },
                    { 7, 2, null, 2, null, true, true, "Advanced", "TypeScript" },
                    { 8, 2, null, 3, null, true, true, "Advanced", "JavaScript" }
                });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "id", "category_id", "description", "display_order", "icon_url", "is_active", "level", "name" },
                values: new object[] { 9, 2, null, 4, null, true, "Intermediate", "Tailwind CSS" });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "id", "category_id", "description", "display_order", "icon_url", "is_active", "is_featured", "level", "name" },
                values: new object[] { 10, 3, null, 1, null, true, true, "Advanced", "SQL Server" });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "id", "category_id", "description", "display_order", "icon_url", "is_active", "level", "name" },
                values: new object[] { 11, 3, null, 2, null, true, "Intermediate", "MySQL" });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "id", "category_id", "description", "display_order", "icon_url", "is_active", "is_featured", "level", "name" },
                values: new object[] { 12, 4, null, 1, null, true, true, "Advanced", "Git" });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "id", "category_id", "description", "display_order", "icon_url", "is_active", "level", "name" },
                values: new object[] { 13, 4, null, 2, null, true, "Intermediate", "Docker" });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "id", "category_id", "description", "display_order", "icon_url", "is_active", "is_featured", "level", "name" },
                values: new object[,]
                {
                    { 14, 5, null, 1, null, true, true, "Intermediate", "Clean Architecture" },
                    { 15, 5, null, 2, null, true, true, "Advanced", "JWT & RBAC" }
                });

            migrationBuilder.InsertData(
                table: "ProjectSkills",
                columns: new[] { "project_id", "skill_id" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 1, 3 },
                    { 1, 6 },
                    { 1, 7 },
                    { 1, 10 },
                    { 1, 14 },
                    { 1, 15 },
                    { 2, 5 },
                    { 2, 6 },
                    { 2, 7 },
                    { 2, 11 },
                    { 2, 13 },
                    { 2, 15 },
                    { 3, 1 },
                    { 3, 2 },
                    { 3, 3 },
                    { 3, 6 },
                    { 3, 7 },
                    { 3, 10 },
                    { 3, 14 },
                    { 3, 15 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "entity_name", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "UX_BlogCategories_Name",
                table: "BlogCategories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_BlogCategories_Slug",
                table: "BlogCategories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_CategoryId",
                table: "Blogs",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_Status_PublishedAt",
                table: "Blogs",
                columns: new[] { "status", "published_at" });

            migrationBuilder.CreateIndex(
                name: "UX_Blogs_Slug",
                table: "Blogs",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogTagMappings_TagId",
                table: "BlogTagMappings",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "UX_BlogTags_Name",
                table: "BlogTags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_BlogTags_Slug",
                table: "BlogTags",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_IsActive",
                table: "Certificates",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_Status_CreatedAt",
                table: "ContactMessages",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_Education_IsActive",
                table: "Education",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_Experiences_IsActive_DisplayOrder",
                table: "Experiences",
                columns: new[] { "is_active", "display_order" });

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_IsActive",
                table: "Profiles",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectImages_ProjectId_DisplayOrder",
                table: "ProjectImages",
                columns: new[] { "project_id", "display_order" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_PublicDisplay",
                table: "Projects",
                columns: new[] { "is_active", "is_featured", "status" });

            migrationBuilder.CreateIndex(
                name: "UX_Projects_Slug",
                table: "Projects",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSkills_SkillId",
                table: "ProjectSkills",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_ExpiresAt",
                table: "RefreshTokens",
                columns: new[] { "user_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "UX_Roles_Name",
                table: "Roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkillCategories_IsActive_DisplayOrder",
                table: "SkillCategories",
                columns: new[] { "is_active", "display_order" });

            migrationBuilder.CreateIndex(
                name: "UX_SkillCategories_Name",
                table: "SkillCategories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_CategoryId",
                table: "Skills",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_PublicDisplay",
                table: "Skills",
                columns: new[] { "is_active", "is_featured", "display_order" });

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_StoredFileName",
                table: "UploadedFiles",
                column: "stored_file_name");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_UploadedBy",
                table: "UploadedFiles",
                column: "uploaded_by");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "UX_Users_Email",
                table: "Users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ViewStatistics_BlogId_ViewedAt",
                table: "ViewStatistics",
                columns: new[] { "blog_id", "viewed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_ViewStatistics_PagePath_ViewedAt",
                table: "ViewStatistics",
                columns: new[] { "page_path", "viewed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_ViewStatistics_ProjectId_ViewedAt",
                table: "ViewStatistics",
                columns: new[] { "project_id", "viewed_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BlogTagMappings");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "ContactMessages");

            migrationBuilder.DropTable(
                name: "Education");

            migrationBuilder.DropTable(
                name: "Experiences");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "ProjectImages");

            migrationBuilder.DropTable(
                name: "ProjectSkills");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "UploadedFiles");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "ViewStatistics");

            migrationBuilder.DropTable(
                name: "BlogTags");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "SkillCategories");

            migrationBuilder.DropTable(
                name: "BlogCategories");
        }
    }
}
