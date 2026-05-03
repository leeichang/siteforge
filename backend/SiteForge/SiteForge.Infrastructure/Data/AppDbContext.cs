using SqlSugar;
using SiteForge.Core.Entities;

namespace SiteForge.Infrastructure.Data;

/// <summary>
/// SqlSugar 資料庫上下文配置
/// </summary>
public class AppDbContext
{
    private readonly string _connectionString;

    public AppDbContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    /// <summary>
    /// 建立 SqlSugarClient 實例
    /// </summary>
    public ISqlSugarClient CreateClient()
    {
        var db = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = _connectionString,
            DbType = DbType.PostgreSQL,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
            ConfigureExternalServices = new ConfigureExternalServices
            {
                EntityService = (type, column) =>
                {
                    // 處理 JSONB 欄位
                    if (column.DbType == null && 
                        (column.PropertyInfo?.PropertyType == typeof(string) || 
                         column.PropertyInfo?.PropertyType == typeof(string?)))
                    {
                        var attr = column.PropertyInfo?.GetCustomAttributes(typeof(SugarColumn), false)
                            ?.FirstOrDefault() as SugarColumn;
                        if (attr?.ColumnDataType == "jsonb")
                        {
                            column.IsJson = true;
                        }
                    }
                }
            }
        });

        // 設置 SQL 日誌
        db.Aop.OnLogExecuting = (sql, parameters) =>
        {
            Console.WriteLine($"[SQL] {sql}");
            if (parameters?.Length > 0)
            {
                Console.WriteLine($"[SQL Parameters] {string.Join(", ", parameters.Select(p => $"{p.ParameterName}={p.Value}"))}");
            }
        };

        return db;
    }

    /// <summary>
    /// 初始化資料庫（自動建立表格）
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        using var db = CreateClient();
        
        // 自動建立所有表格（如果不存在）
        db.CodeFirst.InitTables(
            typeof(User),
            typeof(Site),
            typeof(Page),
            typeof(WidgetTemplate),
            typeof(WidgetBase),
            typeof(Theme),
            typeof(Layout),
            typeof(LayoutZone),
            typeof(SiteDomain),
            typeof(AiConversation),
            typeof(AiMessage),
            typeof(Asset),
            typeof(PublishTask)
        );

        // 種子資料：系統內建主題
        await SeedSystemThemesAsync(db);
        
        // 種子資料：系統內建區塊模板
        await SeedWidgetTemplatesAsync(db);
        
        // 種子資料：系統內建佈局
        await SeedLayoutsAsync(db);
        
        Console.WriteLine("[DB] Database initialization complete.");
    }

    private async Task SeedSystemThemesAsync(ISqlSugarClient db)
    {
        var exists = await db.Queryable<Theme>().AnyAsync(t => t.IsSystem);
        if (exists) return;

        var themes = new List<Theme>
        {
            new()
            {
                Name = "Minimal Light",
                Description = "簡約明亮的白色主題",
                IsSystem = true,
                Colors = """{"primary":"#4F46E5","secondary":"#7C3AED","accent":"#F59E0B","background":"#FFFFFF","text":"#111827","success":"#10B981","warning":"#F59E0B","danger":"#EF4444"}""",
                Fonts = """{"heading":"Inter","body":"Inter"}""",
                FontImportUrl = "https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap",
                Spacing = """{"unit":"rem","baseSize":1}""",
                BorderRadius = """{"sm":"0.25rem","md":"0.5rem","lg":"1rem","full":"9999px"}""",
                Shadows = """{"sm":"0 1px 2px 0 rgba(0,0,0,0.05)","md":"0 4px 6px -1px rgba(0,0,0,0.1)","lg":"0 10px 15px -3px rgba(0,0,0,0.1)"}"""
            },
            new()
            {
                Name = "Dark Neon",
                Description = "暗色霓虹風格主題",
                IsSystem = true,
                Colors = """{"primary":"#06B6D4","secondary":"#8B5CF6","accent":"#F43F5E","background":"#0F172A","text":"#F1F5F9","success":"#10B981","warning":"#F59E0B","danger":"#EF4444"}""",
                Fonts = """{"heading":"Inter","body":"Inter"}""",
                FontImportUrl = "https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap",
                Spacing = """{"unit":"rem","baseSize":1}""",
                BorderRadius = """{"sm":"0.25rem","md":"0.5rem","lg":"1rem","full":"9999px"}""",
                Shadows = """{"sm":"0 1px 2px 0 rgba(0,0,0,0.3)","md":"0 4px 6px -1px rgba(0,0,0,0.4)","lg":"0 10px 15px -3px rgba(0,0,0,0.5)"}"""
            },
            new()
            {
                Name = "Warm Earth",
                Description = "暖色大地色系主題",
                IsSystem = true,
                Colors = """{"primary":"#D97706","secondary":"#92400E","accent":"#059669","background":"#FFFBEB","text":"#451A03","success":"#059669","warning":"#D97706","danger":"#DC2626"}""",
                Fonts = """{"heading":"Playfair Display","body":"Inter"}""",
                FontImportUrl = "https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&family=Playfair+Display:wght@400;500;600;700&display=swap",
                Spacing = """{"unit":"rem","baseSize":1}""",
                BorderRadius = """{"sm":"0.25rem","md":"0.5rem","lg":"1rem","full":"9999px"}""",
                Shadows = """{"sm":"0 1px 2px 0 rgba(0,0,0,0.05)","md":"0 4px 6px -1px rgba(0,0,0,0.1)","lg":"0 10px 15px -3px rgba(0,0,0,0.1)"}"""
            },
            new()
            {
                Name = "Ocean Blue",
                Description = "海洋藍色清新主題",
                IsSystem = true,
                Colors = """{"primary":"#2563EB","secondary":"#0891B2","accent":"#14B8A6","background":"#F0F9FF","text":"#0F172A","success":"#10B981","warning":"#F59E0B","danger":"#EF4444"}""",
                Fonts = """{"heading":"Inter","body":"Inter"}""",
                FontImportUrl = "https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap",
                Spacing = """{"unit":"rem","baseSize":1}""",
                BorderRadius = """{"sm":"0.375rem","md":"0.75rem","lg":"1.25rem","full":"9999px"}""",
                Shadows = """{"sm":"0 1px 3px 0 rgba(37,99,235,0.1)","md":"0 4px 6px -1px rgba(37,99,235,0.15)","lg":"0 10px 15px -3px rgba(37,99,235,0.2)"}"""
            }
        };

        await db.Insertable(themes).ExecuteCommandAsync();
        Console.WriteLine("[DB] Seeded 4 system themes.");
    }

    private async Task SeedWidgetTemplatesAsync(ISqlSugarClient db)
    {
        var exists = await db.Queryable<WidgetTemplate>().AnyAsync(w => w.IsSystem);
        if (exists) return;

        var templates = new List<WidgetTemplate>
        {
            new()
            {
                Name = "Hero",
                Category = "Layout",
                Description = "首頁英雄區塊 — 大標題 + 副標題 + 行動呼籲按鈕 + 背景圖片",
                IsSystem = true,
                DefaultContent = """
                    <section id="hero" class="relative min-h-[600px] flex items-center justify-center bg-cover bg-center" style="background-image: linear-gradient(rgba(0,0,0,0.5), rgba(0,0,0,0.5)), url('https://images.unsplash.com/photo-1497366216548-37526070297c?w=1920');">
                      <div class="container mx-auto px-4 text-center">
                        <h1 class="text-5xl md:text-6xl lg:text-7xl font-bold text-white mb-6">Your Compelling Headline</h1>
                        <p class="text-xl md:text-2xl text-gray-200 mb-8 max-w-3xl mx-auto">Engaging subtitle that captures attention and explains your value proposition.</p>
                        <a href="#get-started" class="inline-block px-8 py-4 bg-white text-gray-900 font-semibold rounded-lg hover:bg-gray-100 transition-all transform hover:scale-105">Get Started</a>
                      </div>
                    </section>
                """,
                DefaultStyle = """
                    .hero-section { position: relative; overflow: hidden; }
                    .hero-overlay { position: absolute; inset: 0; background: linear-gradient(rgba(0,0,0,0.4), rgba(0,0,0,0.4)); }
                """,
                EditableProps = """{"headline":{"type":"text","label":"Headline"},"subtitle":{"type":"textarea","label":"Subtitle"},"ctaText":{"type":"text","label":"Button Text"},"ctaLink":{"type":"text","label":"Button Link"},"background":{"type":"image","label":"Background"}}""",
                DisplayOrder = 1
            },
            new()
            {
                Name = "Features",
                Category = "Content",
                Description = "特色功能展示 — 3/4/6 欄功能卡片網格",
                IsSystem = true,
                DefaultContent = """
                    <section id="features" class="py-20 bg-gray-50">
                      <div class="container mx-auto px-4">
                        <h2 class="text-3xl md:text-4xl font-bold text-center mb-4">Why Choose Us</h2>
                        <p class="text-xl text-gray-600 text-center mb-12 max-w-2xl mx-auto">Discover what sets us apart from the competition.</p>
                        <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
                          <div class="bg-white rounded-xl p-8 shadow-md hover:shadow-lg transition-shadow">
                            <div class="w-12 h-12 bg-indigo-100 rounded-lg flex items-center justify-center mb-4">
                              <svg class="w-6 h-6 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"/></svg>
                            </div>
                            <h3 class="text-xl font-semibold mb-2">Lightning Fast</h3>
                            <p class="text-gray-600">Optimized for speed and performance.</p>
                          </div>
                          <div class="bg-white rounded-xl p-8 shadow-md hover:shadow-lg transition-shadow">
                            <div class="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center mb-4">
                              <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/></svg>
                            </div>
                            <h3 class="text-xl font-semibold mb-2">Secure by Default</h3>
                            <p class="text-gray-600">Enterprise-grade security built-in.</p>
                          </div>
                          <div class="bg-white rounded-xl p-8 shadow-md hover:shadow-lg transition-shadow">
                            <div class="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center mb-4">
                              <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6z"/></svg>
                            </div>
                            <h3 class="text-xl font-semibold mb-2">Easy Integration</h3>
                            <p class="text-gray-600">Seamlessly connects with your tools.</p>
                          </div>
                        </div>
                      </div>
                    </section>
                """,
                EditableProps = """{"title":{"type":"text","label":"Section Title"},"subtitle":{"type":"textarea","label":"Section Subtitle"},"columns":{"type":"select","label":"Columns","options":[{"value":3,"label":"3 Columns"},{"value":4,"label":"4 Columns"},{"value":6,"label":"6 Columns"}]}}""",
                DisplayOrder = 2
            },
            new()
            {
                Name = "About",
                Category = "Content",
                Description = "關於我們 — 左文右圖或左圖右文的雙欄區塊",
                IsSystem = true,
                DefaultContent = """
                    <section id="about" class="py-20">
                      <div class="container mx-auto px-4">
                        <div class="flex flex-col md:flex-row items-center gap-12">
                          <div class="md:w-1/2">
                            <h2 class="text-3xl md:text-4xl font-bold mb-6">Our Story</h2>
                            <p class="text-lg text-gray-600 mb-4">We started with a simple idea: make website building accessible to everyone. Our platform combines the power of AI with intuitive design tools.</p>
                            <p class="text-lg text-gray-600 mb-6">With thousands of happy customers worldwide, we continue to innovate and push boundaries.</p>
                            <a href="#learn-more" class="inline-flex items-center text-indigo-600 font-semibold hover:text-indigo-700">
                              Learn More
                              <svg class="w-4 h-4 ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3"/></svg>
                            </a>
                          </div>
                          <div class="md:w-1/2">
                            <img src="https://images.unsplash.com/photo-1522071820081-009f0129c71c?w=800" alt="About us" class="rounded-xl shadow-lg w-full">
                          </div>
                        </div>
                      </div>
                    </section>
                """,
                EditableProps = """{"title":{"type":"text","label":"Title"},"content":{"type":"textarea","label":"Content"},"imagePosition":{"type":"select","label":"Image Position","options":[{"value":"left","label":"Image Left"},{"value":"right","label":"Image Right"}]}}""",
                DisplayOrder = 3
            },
            new()
            {
                Name = "Team",
                Category = "Content",
                Description = "團隊成員卡片展示",
                IsSystem = true,
                DefaultContent = """
                    <section id="team" class="py-20 bg-gray-50">
                      <div class="container mx-auto px-4">
                        <h2 class="text-3xl md:text-4xl font-bold text-center mb-4">Meet Our Team</h2>
                        <p class="text-xl text-gray-600 text-center mb-12 max-w-2xl mx-auto">The creative minds behind our success.</p>
                        <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-8">
                          <div class="bg-white rounded-xl p-6 text-center shadow-md">
                            <img src="https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=200&h=200&fit=crop&crop=face" alt="Team member" class="w-24 h-24 rounded-full mx-auto mb-4 object-cover">
                            <h3 class="text-lg font-semibold">John Doe</h3>
                            <p class="text-gray-500 text-sm">CEO & Founder</p>
                          </div>
                          <div class="bg-white rounded-xl p-6 text-center shadow-md">
                            <img src="https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=200&h=200&fit=crop&crop=face" alt="Team member" class="w-24 h-24 rounded-full mx-auto mb-4 object-cover">
                            <h3 class="text-lg font-semibold">Jane Smith</h3>
                            <p class="text-gray-500 text-sm">CTO</p>
                          </div>
                        </div>
                      </div>
                    </section>
                """,
                EditableProps = """{"title":{"type":"text","label":"Section Title"},"subtitle":{"type":"textarea","label":"Section Subtitle"}}""",
                DisplayOrder = 4
            },
            new()
            {
                Name = "Contact",
                Category = "Content",
                Description = "聯絡表單 — 姓名、Email、訊息欄位",
                IsSystem = true,
                DefaultContent = """
                    <section id="contact" class="py-20">
                      <div class="container mx-auto px-4 max-w-2xl">
                        <h2 class="text-3xl md:text-4xl font-bold text-center mb-4">Get In Touch</h2>
                        <p class="text-xl text-gray-600 text-center mb-12">Have a question? We'd love to hear from you.</p>
                        <form class="space-y-6">
                          <div>
                            <label class="block text-sm font-medium text-gray-700 mb-2">Name</label>
                            <input type="text" class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none transition" placeholder="Your name">
                          </div>
                          <div>
                            <label class="block text-sm font-medium text-gray-700 mb-2">Email</label>
                            <input type="email" class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none transition" placeholder="your@email.com">
                          </div>
                          <div>
                            <label class="block text-sm font-medium text-gray-700 mb-2">Message</label>
                            <textarea rows="4" class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none transition" placeholder="Your message"></textarea>
                          </div>
                          <button type="submit" class="w-full px-8 py-4 bg-indigo-600 text-white font-semibold rounded-lg hover:bg-indigo-700 transition">Send Message</button>
                        </form>
                      </div>
                    </section>
                """,
                EditableProps = """{"title":{"type":"text","label":"Section Title"},"subtitle":{"type":"textarea","label":"Section Subtitle"},"formFields":{"type":"json","label":"Form Fields"}}""",
                DisplayOrder = 5
            },
            new()
            {
                Name = "FAQ",
                Category = "Content",
                Description = "常見問題 — 可折疊的問答區塊",
                IsSystem = true,
                DefaultContent = """
                    <section id="faq" class="py-20 bg-gray-50">
                      <div class="container mx-auto px-4 max-w-3xl">
                        <h2 class="text-3xl md:text-4xl font-bold text-center mb-4">Frequently Asked Questions</h2>
                        <p class="text-xl text-gray-600 text-center mb-12">Everything you need to know.</p>
                        <div class="space-y-4">
                          <div class="bg-white rounded-lg shadow-sm p-6">
                            <h3 class="text-lg font-semibold mb-2">How does it work?</h3>
                            <p class="text-gray-600">Simply describe what you want, and our AI generates a beautiful website for you. You can then customize it with our drag-and-drop editor.</p>
                          </div>
                          <div class="bg-white rounded-lg shadow-sm p-6">
                            <h3 class="text-lg font-semibold mb-2">Is there a free trial?</h3>
                            <p class="text-gray-600">Yes! We offer a 14-day free trial with full features. No credit card required.</p>
                          </div>
                          <div class="bg-white rounded-lg shadow-sm p-6">
                            <h3 class="text-lg font-semibold mb-2">Can I use my own domain?</h3>
                            <p class="text-gray-600">Absolutely. You can connect your custom domain and we'll handle the SSL certificate automatically.</p>
                          </div>
                        </div>
                      </div>
                    </section>
                """,
                EditableProps = """{"title":{"type":"text","label":"Section Title"},"subtitle":{"type":"textarea","label":"Section Subtitle"}}""",
                DisplayOrder = 6
            },
            new()
            {
                Name = "Footer",
                Category = "Layout",
                Description = "頁尾 — 版權資訊、導航連結、社群圖示",
                IsSystem = true,
                DefaultContent = """
                    <footer id="footer" class="py-12 bg-gray-900 text-white">
                      <div class="container mx-auto px-4">
                        <div class="grid grid-cols-1 md:grid-cols-4 gap-8">
                          <div class="md:col-span-2">
                            <h3 class="text-xl font-bold mb-4">SiteForge</h3>
                            <p class="text-gray-400">Building the web, one block at a time.</p>
                          </div>
                          <div>
                            <h4 class="font-semibold mb-4">Quick Links</h4>
                            <ul class="space-y-2 text-gray-400">
                              <li><a href="#" class="hover:text-white transition">About</a></li>
                              <li><a href="#" class="hover:text-white transition">Features</a></li>
                              <li><a href="#" class="hover:text-white transition">Pricing</a></li>
                            </ul>
                          </div>
                          <div>
                            <h4 class="font-semibold mb-4">Contact</h4>
                            <ul class="space-y-2 text-gray-400">
                              <li>hello@siteforge.dev</li>
                              <li>Twitter: @siteforge</li>
                            </ul>
                          </div>
                        </div>
                        <hr class="border-gray-700 my-8">
                        <p class="text-center text-gray-500">© 2026 SiteForge. All rights reserved.</p>
                      </div>
                    </section>
                """,
                EditableProps = """{"companyName":{"type":"text","label":"Company Name"},"copyright":{"type":"text","label":"Copyright Text"}}""",
                DisplayOrder = 99
            }
        };

        await db.Insertable(templates).ExecuteCommandAsync();
        Console.WriteLine($"[DB] Seeded {templates.Count} system widget templates.");
    }

    private async Task SeedLayoutsAsync(ISqlSugarClient db)
    {
        var exists = await db.Queryable<Layout>().AnyAsync(l => l.IsSystem);
        if (exists) return;

        var layouts = new List<Layout>
        {
            new()
            {
                Name = "Full Width",
                Description = "全寬佈局 — 主內容區域佔滿整頁寬度",
                IsSystem = true,
                IsActive = true,
                BodyHtml = @"<div class=""layout-full-width"">{main}</div>",
                Zones = new List<LayoutZone>
                {
                    new()
                    {
                        Name = "main",
                        Title = "主內容區",
                        Width = 12,
                        Order = 1,
                        CssClass = "main-content",
                        PlaceholderHtml = "<div class=\"p-8 border-2 border-dashed border-gray-300 rounded-lg text-center text-gray-400\">拖拽區塊到這裡</div>",
                        IsEditable = true
                    }
                }
            },
            new()
            {
                Name = "With Sidebar",
                Description = "帶側邊欄佈局 — 主要內容 + 右側邊欄",
                IsSystem = true,
                IsActive = true,
                BodyHtml = @"<div class=""layout-sidebar flex flex-col md:flex-row gap-8""><div class=""md:w-2/3"">{main}</div><div class=""md:w-1/3"">{sidebar}</div></div>",
                Zones = new List<LayoutZone>
                {
                    new()
                    {
                        Name = "main",
                        Title = "主內容區",
                        Width = 8,
                        Order = 1,
                        CssClass = "main-content md:w-2/3",
                        PlaceholderHtml = "<div class=\"p-8 border-2 border-dashed border-gray-300 rounded-lg text-center text-gray-400\">拖拽區塊到這裡（主內容）</div>",
                        IsEditable = true
                    },
                    new()
                    {
                        Name = "sidebar",
                        Title = "側邊欄",
                        Width = 4,
                        Order = 2,
                        CssClass = "sidebar md:w-1/3",
                        PlaceholderHtml = "<div class=\"p-8 border-2 border-dashed border-blue-300 rounded-lg text-center text-gray-400\">拖拽區塊到這裡（側邊欄）</div>",
                        IsEditable = true
                    }
                }
            }
        };

        // 先插入 Layouts 取得 ID
        var layoutIds = await db.Insertable(layouts).ExecuteReturnSnowflakeIdListAsync();
        
        // 再插入 LayoutZones（已透過 Navigation 自動處理）
        foreach (var layout in layouts)
        {
            foreach (var zone in layout.Zones!)
            {
                zone.LayoutId = layout.Id;
            }
            await db.Insertable(layout.Zones).ExecuteCommandAsync();
        }
        
        Console.WriteLine($"[DB] Seeded {layouts.Count} system layouts.");
    }
}
