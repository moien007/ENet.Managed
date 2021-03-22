[![Discord](https://img.shields.io/discord/728246944765313075?label=discord)](https://discord.gg/38UqCVC)
[![ENet version](https://img.shields.io/badge/enet-1.3.17-green)](https://github.com/moien007/enet)
[![Nuget](https://img.shields.io/nuget/dt/ENet.Managed?label=downloads)][nuget]
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/ENet.Managed?label=version)][nuget]
[![Build status](https://img.shields.io/github/workflow/status/moien007/ENet.Managed/.NET)](https://github.com/moien007/ENet.Managed/actions)

## ENet.Managed
**ENet** is cross-platform reliable UDP networking library written in **C** and **ENet.Managed** is an unofficial, managed wrapper for **ENet** available for **.NET** and it supports specific set of platforms. You can checkout ENet's repo **[here][enet-repo]**.

# Quick start (Usage)
Take a look at **examples** folder.

# Features
* Supports **AnyCPU** targets
* Supports IPv6
* Provides async-await interface.
* It's cross-platform via .NET Standard
* It's available via NuGet package manager. ([Here][nuget])
* You can set custom:
  * compression method (not enabled by default, recommended to specify a compressor using <code>CompressWith*</code>)
  * checksum algorithm (not enabled by default, recommended to specify a checksum using <code>ChecksumWith*</code>)
  * heap allocator (by default ManagedENet forces ENet to use a custom allocator which is faster than malloc, take a look at <code>ENetManagedAllocator.cs</code>)
* Takes advantage of <code>Span\<byte></code> and friends to reduce GC allocations.
* Provides nearly all features of ENet via managed API.

# Benchmarks
You can see how ENet performs compared to other libraries by taking look at **[these][benchmark]** benchmarks.<br/>
Benchmarks for the wrapper itself are not available yet but it should have near native performance when optimizations are enabled.

# Supported platfroms
| Platform\Arch | X86 | X86_64 | ARM32 | ARM64 |
|:-------------:|:---:|:------:|:-----:|:-----:|
|    Windows    | Yes |   Yes  |  Yes  |   -   |
|     Linux     |  -  |   Yes  |  Yes  |  Yes  |
|      Mac      |  -  |   Yes  |   -   |   -   |

> Linux binaries are built and statically linked against **[MUSL](https://www.musl-libc.org/faq.html)**.

# Notes
* This wrapper deploys ENet binaries to OS's temp folder and dynamically loads them, you can alter this behavior using <code>ENetStartupOptions</code> or by manually initializing ENet using <code>LibENet</code> class.
* ENet binaries are built from https://github.com/moien007/enet
* ENet's checksum feature is disabled by default, it is highly recommended to use checksum to avoid receiving damaged packets. 
  * <code>ChecksumWithCRC32</code> enables ENet's builtin checksum feature.
* In case of consuming custom version of ENet you have to sync the data structures offsets with the wrapper. 
* <code>ENetPeer.DisconnectNow</code> method will not generate <code>ENetEventType.Disconnect</code>.
* If you use <code>SetUserData</code> extension methods you have to call <code>UnsetUserData</code> when you done, otherwise memory leak will happen.
* You can use <code>ENetPooledAllocator</code> in case if you want to make ENet use pooled buffers to reduce allocations.

# Contribution
You can contribute by reporting bugs and making pull requests.

# License
MIT open-source license.

[enet-repo]: http://www.github.com/lsalzman/enet
[benchmark]: http://www.github.com/nxrighthere/BenchmarkNet/wiki/Benchmark-Results
[nuget]: http://www.nuget.org/packages/ENet.Managed

