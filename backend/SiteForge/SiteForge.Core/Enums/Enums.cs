namespace SiteForge.Core.Enums;

/// <summary>
/// 網站狀態
/// </summary>
public enum SiteStatus
{
    Draft = 0,
    Published = 1,
    Suspended = 2
}

/// <summary>
/// 頁面類型
/// </summary>
public enum PageType
{
    Home = 0,
    About = 1,
    Product = 2,
    Blog = 3,
    Contact = 4,
    Custom = 5
}

/// <summary>
/// 使用者角色
/// </summary>
public enum UserRole
{
    Admin = 0,
    User = 1
}

/// <summary>
/// 發佈任務狀態
/// </summary>
public enum PublishStatus
{
    Pending = 0,
    Publishing = 1,
    Done = 2,
    Failed = 3
}

/// <summary>
/// 發佈任務類型
/// </summary>
public enum PublishTaskType
{
    FullPublish = 0,
    Incremental = 1,
    Rollback = 2
}

/// <summary>
/// AI 動作類型
/// </summary>
public enum AiActionType
{
    GenerateSection = 0,
    GeneratePage = 1,
    GenerateComponent = 2,
    RewriteContent = 3,
    Translate = 4,
    GenerateImage = 5,
    ChangeStyle = 6,
    ChangeTheme = 7,
    SuggestLayout = 8
}

/// <summary>
/// 資產來源
/// </summary>
public enum AssetSource
{
    Upload = 0,
    AiGenerated = 1,
    Unsplash = 2
}

/// <summary>
/// DNS 配置狀態
/// </summary>
public enum DnsStatus
{
    Pending = 0,
    Configured = 1,
    Error = 2
}
