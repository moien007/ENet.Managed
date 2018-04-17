## ENet.Managed
I was looking for a reliable UDP implementation, after a lots of searchs i found [ENet][enet-repo].<br>
Unlike other libraries, ENet is lightwight, high perfomance and written in C, take a look at [Benchmarks][benchmark].<br>
ENet.Managed (or Managed ENet) is managed wrapper for [ENet][enet-repo] written in C# and it tries to:
* Keep the perfomance same as possible 
* Keep the flexibility 
* Providing more features
* Providing managed interface for ENet
* And more...

This wrapper <b>only supportes Windows</b> and without a contribution it will not support any other operation systems because Im noob.
Also <b>I didn't test it with Unity</b> so please tell me if it works with Unity.

#### [Available on NuGet][nuget]
---
### Quick start (Usage)
TODO, for now please take a look at sample chat [server](ChatServer) and [client](ChatClient).

### Contribution
Contributions are welcome.

### License
nope.

[enet-repo]: www.github.com/lsalzman/enet
[benchmark]: www.github.com/nxrighthere/BenchmarkNet/wiki/Benchmark-Results
[nuget]: www.nuget.org/packages/ENet.Managed

