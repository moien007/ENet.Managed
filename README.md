## ENet.Managed
I was looking for a reliable UDP implementation, after a lots of searchs i found [ENet][enet-repo].<br>
Unlike other libraries, ENet is lightwight, high perfomance and written in C, take a look at [Benchmarks][benchmark].<br>
ENet.Managed (or Managed ENet) is managed wrapper for [ENet][enet-repo] written in C# and it tries to:
* Keep the perfomance same as possible 
* Keep the flexibility 
* Providing more features
* Providing managed interface for ENet
* And more...


#### [Available on NuGet][nuget]
---
### Quick start (Usage)
TODO, for now please take a look at ENetChatSample

### Features
* Supports Any CPU
* Supports Windows and Linux (.NET Core 2.0 and .NET 4.5+)
* Multicast
* Custom compression (Not enabled by default, recommended to specify a compressor using <code>CompressWith*</code>)
* Custom checksum (Not enabled by default, recommended to specify a checksum using <code>ChecksumWith*</code>)
* Custom allocator (by default ManagedENet forces ENet to use a custom allocator which is faster than malloc, take a look at <code>ENetManagedAllocator.cs</code>)
* And more

### TODO
* Add summaries.

### Notes
* This wrapper holds x86 and x64 release versions of ENet from [this][enet-repo] repo in its resources and will extracted to temp folder by default. Anyway you can change path by setting <Code>LibENet.DllPath</code> before calling <code>ManagedENet.Startup</code> but keep in mind you have to seperate x86 and x64 path by checking <code>Environment.Is64BitProcess</code>.
* You can set compressor to <code>ENetDeflateCompressor</code> which uses <code>System.IO.Compression.DeflateStream</code>
* Don't forget to enable checksum (ENet library provides CRC32 but not enabled by default, enable it using <code>ChecksumWithCRC32</code>) 
* <code>Service</code> method may takes more than specified timeout, in my experience it burns CPU if specify a timeout more 10ms, also it should be noted that a single call to <code>Service</code> method may raise multiple events.

### Contribution
Contributions are welcome.

### License
MIT

[enet-repo]: http://www.github.com/lsalzman/enet
[benchmark]: http://www.github.com/nxrighthere/BenchmarkNet/wiki/Benchmark-Results
[nuget]: http://www.nuget.org/packages/ENet.Managed

