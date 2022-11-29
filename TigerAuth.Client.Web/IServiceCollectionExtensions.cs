using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Sustainsys.Saml2;
using Sustainsys.Saml2.AspNetCore2;
using Sustainsys.Saml2.Metadata;

namespace TigerAuth.Client.Web;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddTigerAuthentication(
        this IServiceCollection services,
        AuthenticationOptions options
    )
    {
        _ = options ?? throw new ArgumentNullException(nameof(options));

        var optionsConfig = CreateConfigurationForOptions(options);

        // services.AddCors(x =>
        // {
        //     x.AddDefaultPolicy(o =>
        //     {
        //         o.WithOrigins(
        //                 "https://login.microsoftonline.com",
        //                 "https://localhost:44318",
        //                 "https://localhost:44373",
        //                 "https://localhost:44301",
        //                 "https://localhost:44476"
        //             )
        //             .AllowCredentials()
        //             .AllowAnyMethod()
        //             .SetIsOriginAllowedToAllowWildcardSubdomains()
        //             .WithExposedHeaders("location")
        //             .AllowAnyHeader();
        //     });
        // });

        services
            .AddAuthentication(o =>
            {
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                // o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //o.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //o.DefaultChallengeScheme = Saml2Defaults.Scheme;
                //o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie(
                CookieAuthenticationDefaults.AuthenticationScheme,
                o =>
                {
                    o.Cookie.SameSite = SameSiteMode.Unspecified;
                    o.ForwardChallenge = Saml2Defaults.Scheme;
                    // o.ForwardDefaultSelector = ctx =>
                    //     ctx.Request.Path.StartsWithSegments("/api/Authorization")
                    //     ? JwtBearerDefaults.AuthenticationScheme
                    //     : null;
                }
            )
            // .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
            // {
            //     o.TokenValidationParameters = new TokenValidationParameters()
            //     {
            //         ValidateIssuer = false,
            //         ValidateAudience = false,
            //         ValidateLifetime = true,
            //         ValidateIssuerSigningKey = true,
            //         ClockSkew = TimeSpan.FromMinutes(1440),
            //         // ValidIssuer = builder.Configuration["Jwt:Issuer"],
            //         // IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            //     };
            // })
            .AddSaml2(
                Saml2Defaults.Scheme,
                o =>
                {
                    o.SPOptions.EntityId = new EntityId("https://localhost:44330");
                    o.SPOptions.PublicOrigin = new Uri("https://localhost:44362");

                    // o.SPOptions.ReturnUrl = new Uri("https://localhost:44301");
                    o.IdentityProviders.Add(
                        new IdentityProvider(
                            new EntityId(
                                "https://sts.windows.net/a35017a7-9bde-499f-97a9-bc27534e628e/"
                            ),
                            o.SPOptions
                        )
                        {
                            MetadataLocation =
                                "https://login.microsoftonline.com/a35017a7-9bde-499f-97a9-bc27534e628e/federationmetadata/2007-06/federationmetadata.xml?appid=17faab92-7694-4a4c-8e51-d92e3b539d6d",
                            LoadMetadata = true,
                            // AllowUnsolicitedAuthnResponse = true,
                            // SingleSignOnServiceUrl = new Uri("https://login.microsoftonline.com/a35017a7-9bde-499f-97a9-bc27534e628e/saml2"),
                        }
                    );
                }
            );

        services.AddHttpContextAccessor().TryAddSingleton(options);

         services
             // .AddSingleton<IAccessTokenClient, AccessTokenClient>()
             // .AddTransient<ITicketStore, DistributedCacheTicketStore>()
             // .AddSingleton<AuthenticationFilter>()
             // .AddSingleton<AuthenticationMiddleware>()
             .AddSingleton<Authenticator>();
        // .AddTransient<IAuthenticationStore, AuthenticationStore>();

        // services.TryAddSingleton<IMemoryCache, MemoryCache>();
        //
        services
            .Configure<AuthenticationOptions>(optionsConfig.GetSection(nameof(AuthenticationOptions)));
        // .Configure<AccessTokenClientOptions>(optionsConfig.GetSection(nameof(AccessTokenClientOptions)));

        return services;
    }

    /// <summary>
    /// Return an IConfiguration object when calling AddTigerAuthentication(AuthenticationOptions)
    /// Since IOptions object have been implemented, they wont be injected into controllers unless added through configuration
    /// This method will be obsolete until all apps that need AddTigerAuthentication(AuthenticationOptions) are using AddTigerAuthentication()
    /// </summary>
    /// <returns></returns>
    private static IConfiguration CreateConfigurationForOptions(AuthenticationOptions options)
    {
        var optionsConfig = new Dictionary<string, string?>
             {
                 {"AuthenticationOptions:AuthorizationServiceUrl", options.AuthorizationServiceUrl},
                 {"AuthenticationOptions:IsLiveData", options.IsLiveData.ToString().ToLower()},
                 {"AuthenticationOptions:ApplicationName", options.ApplicationName},
                 {"AuthenticationOptions:ApplicationSecret", options.ApplicationSecret},
                 {"AuthenticationOptions:Scope", options.Scope},
                 {"AuthenticationOptions:ClientId", options.ClientId},
                 {"AuthenticationOptions:ClientSecret", options.ClientSecret},
                 {"AuthenticationOptions:ResourceOwnerType", options.ResourceOwnerType?.ToString() ?? null},
                 {"AccessTokenClientOptions:AuthorizationServiceUrl", options.AuthorizationServiceUrl},
                 {"AccessTokenClientOptions:IsLiveData", options.IsLiveData.ToString().ToLower()},
                 {"AccessTokenClientOptions:ClientId", options.ClientId},
                 {"AccessTokenClientOptions:ClientSecret", options.ClientSecret},
                 {"AccessTokenClientOptions:ApiVersion", options.ApiVersion?.ToString().ToLower() ?? null},
             };

        var builder = new ConfigurationBuilder();
        builder.AddInMemoryCollection(optionsConfig);
        return builder.Build();
    }
}
