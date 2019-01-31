This repo contains a [CLR MD](https://github.com/microsoft/clrmd) analyzer to help identify a specific reliability bug in .NET Framework dumps.

The bug was fixed in .NET 4.8 preview [build 3694](https://github.com/Microsoft/dotnet-framework-early-access/blob/master/release-notes/NET48/build-3694/changes.md#networking):
* Fix a race condition that would sometimes cause all of the connections to a server to stop sending HTTP requests. [499777, System.dll, Bug, Build:3694]

## Disclaimers

* The code is released as is without beautifying to be used by motivated people.
* It was my first CLR MD usage ever, so take it as that. It is not meant as best practice for use of CLR MD.
* Use it at your own risk.

## Instructions

* Update hardcoded dump and DAC file names. (It was meant as one-time use tool, so it is not parsed from args or anything else fancy)
    * To find DAC, e.g. run `windbg` with correct symbol server. Grab it from `.chain` (SOS) and `.cordll` (DAC) ... my lazy method.
* Run it on your dump, check for high number of `m_NonKeepAliveRequestPipelined = true` in the printed statistics, e.g.:
    * `m_NonKeepAliveRequestPipelined: true = 254 / false = 2`
    * The field tracks a connection which has a request queued that cannot use KeepAlive (e.g. it is POST request, or explicitly asked by request, etc.). The connection is supposed to be closed once the request is processed. Due to a bug, the connection gets into "confused" state when it is not processing any request in queue, and is marked (incorrectly) with NonKeepAlive flag (i.e. nothing else will be queued there to kick off the request processing). As a result, the connection is stuck, not processing anything - the underlying socket is "leaked".

## Motivation

Motivated by tweet ask from community: https://twitter.com/KooKiz/status/1090673233605652481
