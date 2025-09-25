![.NET Core](https://github.com/jason-kerney/whats-your-version/workflows/.NET%20Core/badge.svg)
[![License](https://img.shields.io/github/license/jason-kerney/whats-your-version)](https://github.com/jason-kerney/whats-your-version/blob/main/LICENSE)
[![Nuget](https://img.shields.io/nuget/v/whats-your-version.svg)](https://www.nuget.org/packages/whats-your-version/)

# Introduction 
I simple way to make an assembly's version information availible to consumers within that assembly.

# Getting Started
To get build date add this to your project file
```xml
  <ItemGroup>
    <AssemblyAttribute Include="WhatsYourVersion.BuildDateAttribute">
      <_Parameter1>$([System.DateTime]::UtcNow.ToString("yyyyMMddHHmmss"))</_Parameter1>
    </AssemblyAttribute>
  </ItemGroup>
```

To consume your version you would use code simular to the following:

```c#
public class MyVersionConsumer 
{
    private readonly VersionInfo _versionInfo;
    public MyVersionConsumer(IVersionRetriever versionGetter) 
    {
        _versionInfo = versionGetter.GetVersion();
    }
    
    // Code removed for brevity.
} 
```

This relies on the `IVersionRetriever` being injected in. So where ever you build your injectables you would then have to build your retriever.

Here is an example in the Dot Net WebApi Startup file:
```c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Surrounding code removed for brevity 
        services.AddScoped<IVersionRetriever>(() => 
            new VersionRetriever(AssemblyWrapper.From<Startup>())
            // Assembly Wrapper, is a way to gain a pointer to the assembly
            // you wish to read the version of. As such, the type given in 
            // the "From" method needs to be any valid type from that 
            // assembly.
        );
        // Surrounding code removed for brevity   
    }
}
```
## Contributors ✨

Thanks goes to these wonderful people ([emoji key](https://allcontributors.org/docs/en/emoji-key)):

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tr>
    <td align="center"><a href="https://github.com/edf-re"><img src="https://avatars.githubusercontent.com/u/13739273?v=4?s=100" width="100px;" alt=""/><br /><sub><b>EDF Renewables</b></sub></a><br /><a href="#financial-edf-re" title="Financial">💵</a></td>
  </tr>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->

This project follows the [all-contributors](https://github.com/all-contributors/all-contributors) specification. Contributions of any kind welcome!