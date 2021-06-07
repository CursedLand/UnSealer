# UnSealer
- UnSealer Is Deobfuscator Written In C# And Using [AsmResolver](https://github.com/Washi1337/AsmResolver) To Read Assemblies.
- UnSealer Is Open Sourced & Licensed Under GPLv3.

# How To Use
- `UnSealer.CLI.exe filepath -protectionid -protectionid`
- Example Executing Confuser String Decrypter `UnSealer.CLI.exe unpackme.exe -cfexconst`

# Compiling
- Using `dotnet`
```cmd
dotnet restore
dotnet build
```
- Using IDE (e.g. Visual Studio, Jetbrains Rider)
- Open Solution.
- Restore Nuget Packages.
- Rebuild Solution.

# Features
- Devirualizing Code.
- Decrypting Constants.
- Fix Outlined Methods.
- Fix Call Proxies.
- Restoring Fields From Global Type without Module Corrupting.
- And Many More in Future!

# CLI-Preview
![alt text](https://github.com/CursedLand/UnSealer/blob/master/CLIPreview.PNG)
