﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Packages.MarkdownIt;
using Volo.Abp.AspNetCore.Mvc.UI.Packages.Prismjs;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.PageToolbars;
using Volo.Abp.AutoMapper;
using Volo.Abp.Http.ProxyScripting.Generators.JQuery;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.ObjectExtending.Modularity;
using Volo.Abp.SettingManagement.Web.Pages.SettingManagement;
using Volo.Abp.Threading;
using Volo.Abp.UI.Navigation;
using Volo.Abp.VirtualFileSystem;
using Volo.CmsKit.Admin.MediaDescriptors;
using Volo.CmsKit.Admin.Web.Menus;
using Volo.CmsKit.Admin.Web.Pages.CmsKit.Comments.Approve;
using Volo.CmsKit.Admin.Web.Pages.CmsKit.Shared.Components.Comments;
using Volo.CmsKit.Localization;
using Volo.CmsKit.Permissions;
using Volo.CmsKit.Web;

namespace Volo.CmsKit.Admin.Web;

[DependsOn(
    typeof(CmsKitAdminApplicationContractsModule),
    typeof(CmsKitCommonWebModule)
    )]
public class CmsKitAdminWebModule : AbpModule
{
    private readonly static OneTimeRunner OneTimeRunner = new OneTimeRunner();
    
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(CmsKitResource),
                typeof(CmsKitAdminWebModule).Assembly,
                typeof(CmsKitAdminApplicationContractsModule).Assembly,
                typeof(CmsKitCommonApplicationContractsModule).Assembly
            );
        });

        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(CmsKitAdminWebModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new CmsKitAdminMenuContributor());
        });
        
        Configure<AbpBundlingOptions>(options =>
        {
            options.ScriptBundles
                .Configure(typeof(Abp.SettingManagement.Web.Pages.SettingManagement.IndexModel).FullName,
                    configuration =>
                    {
                        configuration.AddFiles("/client-proxies/cms-kit-admin-proxy.js");
                    })
                .Configure(StandardBundles.Scripts.Global,
                    configuration =>
                    {
                        configuration.AddContributors(typeof(MarkdownItScriptContributor));
                    });
        });
        
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<CmsKitAdminWebModule>("Volo.CmsKit.Admin.Web");
        });

        context.Services.AddAutoMapperObjectMapper<CmsKitAdminWebModule>();
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<CmsKitAdminWebModule>(validate: true); });

        Configure<RazorPagesOptions>(options =>
        {
            options.Conventions.AuthorizeFolder("/CmsKit/Tags/", CmsKitAdminPermissions.Tags.Default);
            options.Conventions.AuthorizeFolder("/CmsKit/Tags/CreateModal", CmsKitAdminPermissions.Tags.Create);
            options.Conventions.AuthorizeFolder("/CmsKit/Tags/UpdateModal", CmsKitAdminPermissions.Tags.Update);
            options.Conventions.AuthorizeFolder("/CmsKit/Pages", CmsKitAdminPermissions.Pages.Default);
            options.Conventions.AuthorizeFolder("/CmsKit/Pages/Create", CmsKitAdminPermissions.Pages.Create);
            options.Conventions.AuthorizeFolder("/CmsKit/Pages/Update", CmsKitAdminPermissions.Pages.Update);
            options.Conventions.AuthorizeFolder("/CmsKit/Pages/SetAsHomePage", CmsKitAdminPermissions.Pages.SetAsHomePage);
            options.Conventions.AuthorizeFolder("/CmsKit/Blogs", CmsKitAdminPermissions.Blogs.Default);
            options.Conventions.AuthorizeFolder("/CmsKit/Blogs/Create", CmsKitAdminPermissions.Blogs.Create);
            options.Conventions.AuthorizeFolder("/CmsKit/Blogs/Update", CmsKitAdminPermissions.Blogs.Update);
            options.Conventions.AuthorizeFolder("/CmsKit/BlogPosts", CmsKitAdminPermissions.BlogPosts.Default);
            options.Conventions.AuthorizeFolder("/CmsKit/BlogPosts/Create", CmsKitAdminPermissions.BlogPosts.Create);
            options.Conventions.AuthorizeFolder("/CmsKit/BlogPosts/Update", CmsKitAdminPermissions.BlogPosts.Update);
            options.Conventions.AuthorizeFolder("/CmsKit/Comments/", CmsKitAdminPermissions.Comments.Default);
            options.Conventions.AuthorizeFolder("/CmsKit/Comments/Details", CmsKitAdminPermissions.Comments.Default);
            options.Conventions.AuthorizeFolder("/CmsKit/Menus", CmsKitAdminPermissions.Menus.Default);
            options.Conventions.AuthorizePage("/CmsKit/Menus/MenuItems/CreateModal", CmsKitAdminPermissions.Menus.Create);
            options.Conventions.AuthorizePage("/CmsKit/Menus/MenuItems/UpdateModal", CmsKitAdminPermissions.Menus.Update);
            options.Conventions.AuthorizeFolder("/CmsKit/Menus/MenuItems", CmsKitAdminPermissions.Menus.Update);
            options.Conventions.AuthorizeFolder("/CmsKit/GlobalResources", CmsKitAdminPermissions.GlobalResources.Default);
            // TODO: Add /CmsKit/Comments/Approve/Index page
        });

        Configure<RazorPagesOptions>(options =>
        {
            options.Conventions.AddPageRoute("/CmsKit/Tags/Index", "/Cms/Tags");
            options.Conventions.AddPageRoute("/CmsKit/Pages/Index", "/Cms/Pages");
            options.Conventions.AddPageRoute("/CmsKit/Pages/Create", "/Cms/Pages/Create");
            options.Conventions.AddPageRoute("/CmsKit/Pages/Update", "/Cms/Pages/Update/{Id}");
            options.Conventions.AddPageRoute("/CmsKit/Blogs/Index", "/Cms/Blogs");
            options.Conventions.AddPageRoute("/CmsKit/BlogPosts/Index", "/Cms/BlogPosts");
            options.Conventions.AddPageRoute("/CmsKit/BlogPosts/Create", "/Cms/BlogPosts/Create");
            options.Conventions.AddPageRoute("/CmsKit/BlogPosts/Update", "/Cms/BlogPosts/Update/{Id}");
            options.Conventions.AddPageRoute("/CmsKit/Comments/Index", "/Cms/Comments");
            options.Conventions.AddPageRoute("/CmsKit/Comments/Details", "/Cms/Comments/{Id}");
            options.Conventions.AddPageRoute("/CmsKit/Menus/MenuItems/Index", "/Cms/Menus/Items");
            options.Conventions.AddPageRoute("/CmsKit/GlobalResources/Index", "/Cms/GlobalResources");
            options.Conventions.AddPageRoute("/CmsKit/Comments/Approve/Index", "/Cms/Comments/Approve");

        });

        Configure<AbpPageToolbarOptions>(options =>
        {

            options.Configure<Pages.CmsKit.Tags.IndexModel>(
                toolbar =>
                {
                    toolbar.AddButton(
                        LocalizableString.Create<CmsKitResource>("NewTag"),
                        icon: "plus",
                        name: "NewButton",
                        requiredPolicyName: CmsKitAdminPermissions.Tags.Create
                    );
                }
            );

            options.Configure<Pages.CmsKit.Pages.IndexModel>(
                toolbar =>
                {
                    toolbar.AddButton(
                        LocalizableString.Create<CmsKitResource>("NewPage"),
                        icon: "plus",
                        name: "CreatePage",
                        requiredPolicyName: CmsKitAdminPermissions.Pages.Create
                    );
                });

            options.Configure<Pages.CmsKit.Blogs.IndexModel>(
                toolbar =>
                {
                    toolbar.AddButton(
                        LocalizableString.Create<CmsKitResource>("NewBlog"),
                        icon: "plus",
                        name: "CreateBlog",
                        id: "CreateBlog",
                        requiredPolicyName: CmsKitAdminPermissions.Blogs.Create
                        );
                });

            options.Configure<Pages.CmsKit.BlogPosts.IndexModel>(
                toolbar =>
                {
                    toolbar.AddButton(
                        LocalizableString.Create<CmsKitResource>("NewBlogPost"),
                        icon: "plus",
                        name: "CreateBlogPost",
                        id: "CreateBlogPost",
                        requiredPolicyName: CmsKitAdminPermissions.BlogPosts.Create
                        );
                });

            options.Configure<Pages.CmsKit.Menus.MenuItems.IndexModel>(
                toolbar =>
                {
                    toolbar.AddButton(
                        LocalizableString.Create<CmsKitResource>("NewMenuItem"),
                        icon: "plus",
                        name: "CreateMenuItem",
                        id: "CreateMenuItem",
                        requiredPolicyName: CmsKitAdminPermissions.Menus.Update
                        );
                });

        });
       
        Configure<DynamicJavaScriptProxyOptions>(options =>
        {
            options.DisableModule(CmsKitAdminRemoteServiceConsts.ModuleName);
        });

        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.FormBodyBindingIgnoredTypes.Add(typeof(CreateMediaInputWithStream));
        });

        Configure<SettingManagementPageOptions>(options =>
        {
            options.Contributors.Add(new CommentSettingPageContributor());
        });

    }

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        OneTimeRunner.Run(() =>
        {
            ModuleExtensionConfigurationHelper
                .ApplyEntityConfigurationToUi(
                    CmsKitModuleExtensionConsts.ModuleName,
                    CmsKitModuleExtensionConsts.EntityNames.Blog,
                    createFormTypes: new[] { typeof(Volo.CmsKit.Admin.Web.Pages.CmsKit.Blogs.CreateModalModel.CreateBlogViewModel) },
                    editFormTypes: new[] { typeof(Volo.CmsKit.Admin.Web.Pages.CmsKit.Blogs.UpdateModalModel.UpdateBlogViewModel) }
                );
            
            ModuleExtensionConfigurationHelper
                .ApplyEntityConfigurationToUi(
                    CmsKitModuleExtensionConsts.ModuleName,
                    CmsKitModuleExtensionConsts.EntityNames.BlogPost,
                    createFormTypes: new[] { typeof(Volo.CmsKit.Admin.Web.Pages.CmsKit.BlogPosts.CreateModel.CreateBlogPostViewModel) },
                    editFormTypes: new[] { typeof(Volo.CmsKit.Admin.Web.Pages.CmsKit.BlogPosts.UpdateModel.UpdateBlogPostViewModel) }
                );
            
            ModuleExtensionConfigurationHelper
                .ApplyEntityConfigurationToUi(
                    CmsKitModuleExtensionConsts.ModuleName,
                    CmsKitModuleExtensionConsts.EntityNames.MenuItem,
                    createFormTypes: new[] { typeof(Volo.CmsKit.Admin.Web.Pages.CmsKit.Menus.MenuItems.CreateModalModel.MenuItemCreateViewModel) },
                    editFormTypes: new[] { typeof(Volo.CmsKit.Admin.Web.Pages.CmsKit.Menus.MenuItems.UpdateModalModel.MenuItemUpdateViewModel) }
                );
            
            ModuleExtensionConfigurationHelper
                .ApplyEntityConfigurationToUi(
                    CmsKitModuleExtensionConsts.ModuleName,
                    CmsKitModuleExtensionConsts.EntityNames.Page,
                    createFormTypes: new[] { typeof(Volo.CmsKit.Admin.Web.Pages.CmsKit.Pages.CreateModel.CreatePageViewModel) },
                    editFormTypes: new[] { typeof(Volo.CmsKit.Admin.Web.Pages.CmsKit.Pages.UpdateModel.UpdatePageViewModel) }
                );
            
            ModuleExtensionConfigurationHelper
                .ApplyEntityConfigurationToUi(
                    CmsKitModuleExtensionConsts.ModuleName,
                    CmsKitModuleExtensionConsts.EntityNames.Tag,
                    createFormTypes: new[] { typeof(Volo.CmsKit.Admin.Web.Pages.CmsKit.Tags.CreateModalModel.TagCreateViewModel) },
                    editFormTypes: new[] { typeof(Volo.CmsKit.Admin.Web.Pages.CmsKit.Tags.EditModalModel.TagEditViewModel) }
                );

        });
    }
}
